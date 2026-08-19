import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

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
  });

  final FlagsStatus status;
  final List<Flag> flags;

  /// Keyed by flag key. An entry survives failure on purpose — see [KillSwitchIntent].
  final Map<String, KillSwitchIntent> intents;

  final String? error;

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
  }) =>
      KillSwitchState(
        status: status ?? this.status,
        flags: flags ?? this.flags,
        intents: intents ?? this.intents,
        error: clearError ? null : (error ?? this.error),
      );

  @override
  List<Object?> get props => [status, flags, intents, error];
}

/// The kill switch, and the promise that an admin's decision is never silently dropped.
///
/// The ordinary optimistic-UI pattern is: flip the control, send the request, revert on error.
/// That is wrong for a safety control. Reverting because the network hiccuped leaves an admin
/// believing a broken feature is disabled while it is still serving traffic — and the only
/// evidence was a toast that has already disappeared. So a failed intent is KEPT, the toggle
/// continues to show what was asked for, and the failure stays on screen until it is retried or
/// explicitly dismissed.
class KillSwitchCubit extends Cubit<KillSwitchState> {
  KillSwitchCubit(this._repository, {DateTime Function()? now})
      : _now = now ?? DateTime.now,
        super(const KillSwitchState());

  final FlagRepository _repository;
  final DateTime Function() _now;

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
