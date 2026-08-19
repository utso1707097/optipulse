import 'dart:async';

import 'package:equatable/equatable.dart';
import 'package:hydrated_bloc/hydrated_bloc.dart';

import '../../../core/reconcile/connectivity_monitor.dart';
import '../../../core/reconcile/reconciler.dart';
import '../domain/flag.dart';
import '../domain/flag_repository.dart';
import '../domain/kill_switch_intent.dart';

enum FlagsStatus { initial, loading, ready, failed }

class KillSwitchState extends Equatable {
  const KillSwitchState({
    this.status = FlagsStatus.initial,
    this.flags = const [],
    this.intents = const {},
    this.error,
    this.isOnline = true,
    this.conflicts = const [],
  });

  final FlagsStatus status;
  final List<Flag> flags;

  /// Keyed by flag key. An entry survives failure on purpose — see [KillSwitchIntent].
  final Map<String, KillSwitchIntent> intents;

  final String? error;

  /// Last known network state. Drives the offline banner, and nothing else — the app does not
  /// pre-emptively refuse to send when it believes it is offline. A device's connectivity
  /// report is a hint, not a fact, and an admin engaging a kill switch should get a real
  /// attempt rather than a refusal based on a stale radio state.
  final bool isOnline;

  /// Flags where reconciliation found the server disagreeing with a pending release.
  final List<String> conflicts;

  /// What the toggle should show: the admin's intent if one is outstanding, otherwise the
  /// server's truth. An unconfirmed intent must render as the requested position, or the UI
  /// tells the admin their action did not happen while it is still in flight.
  bool displayedEngagement(Flag flag) {
    final intent = intents[flag.key];
    if (intent != null && intent.state != IntentState.confirmed) return intent.engage;
    return flag.killSwitchEngaged;
  }

  bool isUnconfirmed(String key) {
    final intent = intents[key];
    return intent != null && intent.state != IntentState.confirmed;
  }

  List<KillSwitchIntent> get failedIntents =>
      intents.values.where((intent) => intent.isFailed).toList();

  KillSwitchState copyWith({
    FlagsStatus? status,
    List<Flag>? flags,
    Map<String, KillSwitchIntent>? intents,
    String? error,
    bool clearError = false,
    bool? isOnline,
    List<String>? conflicts,
  }) =>
      KillSwitchState(
        status: status ?? this.status,
        flags: flags ?? this.flags,
        intents: intents ?? this.intents,
        error: clearError ? null : (error ?? this.error),
        isOnline: isOnline ?? this.isOnline,
        conflicts: conflicts ?? this.conflicts,
      );

  @override
  List<Object?> get props => [status, flags, intents, error, isOnline, conflicts];
}

/// The kill switch, and the promise that an admin's decision is never silently dropped.
///
/// The ordinary optimistic-UI pattern is: flip the control, send the request, revert on error.
/// That is wrong for a safety control. Reverting because the network hiccuped leaves an admin
/// believing a broken feature is disabled while it is still serving traffic — and the only
/// evidence was a toast that has already disappeared. So a failed intent is KEPT, the toggle
/// continues to show what was asked for, and the failure stays on screen until it is retried or
/// explicitly dismissed.
class KillSwitchCubit extends HydratedCubit<KillSwitchState> {
  KillSwitchCubit(
    this._repository, {
    ConnectivityMonitor? connectivity,
    DateTime Function()? now,
  })  : _now = now ?? DateTime.now,
        super(const KillSwitchState()) {
    if (connectivity != null) _watch(connectivity);
  }

  final FlagRepository _repository;
  final DateTime Function() _now;
  StreamSubscription<bool>? _connectivity;

  void _watch(ConnectivityMonitor monitor) {
    _connectivity = monitor.onConnectivityChanged.listen((online) {
      final wasOffline = !state.isOnline;
      emit(state.copyWith(isOnline: online));

      // Reconcile only on the OFFLINE -> ONLINE edge, not on every report. Connectivity plugins
      // emit repeatedly on a flaky link, and re-running a full list-and-replay on each one would
      // hammer the API precisely when the network is least able to carry it.
      if (online && wasOffline) unawaited(reconcileWithServer());
    });
  }

  @override
  Future<void> close() {
    _connectivity?.cancel();
    return super.close();
  }

  /// Re-reads server truth and merges it with whatever this device is still holding (FR-028).
  ///
  /// Any intent that survives the merge is REPLAYED. That is the point of the whole mechanism:
  /// an action taken offline is not a note in the UI, it is a decision that still has to reach
  /// the server.
  Future<void> reconcileWithServer() async {
    final List<Flag> serverFlags;
    try {
      serverFlags = await _repository.listFlags();
    } on FlagRepositoryFailure catch (failure) {
      // Reconciliation failing must not discard intents. The device stays where it was and
      // tries again on the next reconnect.
      emit(state.copyWith(status: FlagsStatus.failed, flags: const [], error: failure.message));
      return;
    }

    final result = reconcile(serverFlags: serverFlags, pendingIntents: state.intents);

    emit(state.copyWith(
      status: FlagsStatus.ready,
      flags: result.displayFlags,
      intents: result.intents,
      conflicts: result.conflicts,
      clearError: true,
    ));

    // Replayed in sorted order so a device with several outstanding intents produces the same
    // sequence of writes every time — the "deterministic" in FR-028 covers what is SENT, not
    // only what is displayed.
    final replayable = result.intents.values
        .where((intent) => !result.conflicts.contains(intent.flagKey))
        .toList()
      ..sort((a, b) => a.flagKey.compareTo(b.flagKey));

    for (final intent in replayable) {
      await setKillSwitch(flagKey: intent.flagKey, engage: intent.engage);
    }
  }

  /// Drops the pending release and accepts the server's engagement. The only way a conflict is
  /// resolved, and deliberately a person's decision.
  void acceptServerState(String flagKey) {
    final intents = {...state.intents}..remove(flagKey);
    emit(state.copyWith(
      intents: intents,
      conflicts: state.conflicts.where((key) => key != flagKey).toList(),
    ));
  }

  @override
  KillSwitchState? fromJson(Map<String, dynamic> json) {
    final flags = (json['flags'] as List<dynamic>? ?? const [])
        .whereType<Map<String, dynamic>>()
        .map(Flag.fromJson)
        .whereType<Flag>()
        .toList();

    final intents = <String, KillSwitchIntent>{};
    for (final raw in (json['intents'] as List<dynamic>? ?? const [])) {
      if (raw is! Map<String, dynamic>) continue;
      final intent = KillSwitchIntent.fromJson(raw);
      if (intent != null) intents[intent.flagKey] = intent;
    }

    // Restored as `initial`, never `ready`: the cached flags are last-known, not current, and
    // marking them ready would present stale kill-switch state as though it had just been
    // fetched. loadFlags() runs on open and replaces them.
    return KillSwitchState(
      status: FlagsStatus.initial,
      flags: flags,
      intents: intents,
    );
  }

  @override
  Map<String, dynamic>? toJson(KillSwitchState state) => {
        'flags': state.flags.map((flag) => flag.toJson()).toList(),
        'intents': state.intents.values.map((intent) => intent.toJson()).toList(),
      };

  Future<void> loadFlags() async {
    emit(state.copyWith(status: FlagsStatus.loading, clearError: true));
    try {
      final flags = await _repository.listFlags();
      emit(state.copyWith(status: FlagsStatus.ready, flags: flags));
    } on FlagRepositoryFailure catch (failure) {
      // The flag list is CLEARED on failure rather than left stale. A list of kill-switch
      // states that silently stopped updating is worse than no list: it looks authoritative
      // and is not, and this is the screen someone opens during an incident.
      emit(state.copyWith(
        status: FlagsStatus.failed,
        flags: const [],
        error: failure.message,
      ));
    }
  }

  Future<void> setKillSwitch({required String flagKey, required bool engage}) async {
    final intent = KillSwitchIntent(
      flagKey: flagKey,
      engage: engage,
      state: IntentState.pending,
      requestedAt: _now(),
    );
    emit(state.copyWith(intents: {...state.intents, flagKey: intent}));

    try {
      final updated = await _repository.setKillSwitch(key: flagKey, engaged: engage);

      // Confirmation is the SERVER's reading of the flag, not an assumption that the write
      // did what was asked. If the returned flag disagrees with the intent, the intent is not
      // confirmed — something else changed it, and the admin needs to see that.
      if (updated.killSwitchEngaged != engage) {
        emit(state.copyWith(
          flags: _replace(updated),
          intents: {
            ...state.intents,
            flagKey: intent.copyWith(
              state: IntentState.failed,
              error: 'The server reports this flag as '
                  '${updated.killSwitchEngaged ? 'killed' : 'live'}. Someone else may have '
                  'changed it.',
            ),
          },
        ));
        return;
      }

      final intents = {...state.intents}..remove(flagKey);
      emit(state.copyWith(flags: _replace(updated), intents: intents));
    } on FlagRepositoryFailure catch (failure) {
      emit(state.copyWith(
        intents: {
          ...state.intents,
          flagKey: intent.copyWith(state: IntentState.failed, error: failure.message),
        },
      ));
    }
  }

  Future<void> retry(String flagKey) async {
    final intent = state.intents[flagKey];
    if (intent == null) return;
    await setKillSwitch(flagKey: flagKey, engage: intent.engage);
  }

  /// Explicit abandonment. The only way an unconfirmed intent leaves the screen without having
  /// succeeded — deliberately a deliberate act, so nothing disappears on its own.
  void dismiss(String flagKey) {
    final intents = {...state.intents}..remove(flagKey);
    emit(state.copyWith(intents: intents));
  }

  List<Flag> _replace(Flag updated) =>
      state.flags.map((flag) => flag.key == updated.key ? updated : flag).toList();
}
