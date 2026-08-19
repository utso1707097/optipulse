import 'dart:async';

import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:optipulse_app/core/reconcile/connectivity_monitor.dart';
import 'package:optipulse_app/features/killswitch/domain/flag.dart';
import 'package:optipulse_app/features/killswitch/domain/flag_repository.dart';
import 'package:optipulse_app/features/killswitch/domain/kill_switch_intent.dart';
import 'package:optipulse_app/features/killswitch/presentation/kill_switch_cubit.dart';

import '../../support/memory_storage.dart';

class _MockFlagRepository extends Mock implements FlagRepository {}

/// Drives offline -> online transitions on demand. The whole point of putting connectivity
/// behind an interface: a plugin cannot be made to reconnect from a unit test.
class _FakeConnectivity implements ConnectivityMonitor {
  final _controller = StreamController<bool>.broadcast();
  bool _online = true;

  void goOffline() {
    _online = false;
    _controller.add(false);
  }

  void goOnline() {
    _online = true;
    _controller.add(true);
  }

  @override
  Stream<bool> get onConnectivityChanged => _controller.stream;

  @override
  Future<bool> get isOnline async => _online;
}

Flag _flag(String key, {bool killed = false}) => Flag(
      key: key,
      name: key,
      status: 'Active',
      killSwitchEngaged: killed,
      version: 1,
    );

void main() {
  late _MockFlagRepository repository;
  late _FakeConnectivity connectivity;

  setUp(() {
    repository = _MockFlagRepository();
    connectivity = _FakeConnectivity();
    useMemoryStorage();
  });

  test('an intent taken offline is REPLAYED when the connection returns', () async {
    // The requirement in one test: an action taken with no network is not a note in the UI, it
    // is a decision that still has to reach the server.
    when(() => repository.setKillSwitch(key: any(named: 'key'), engaged: any(named: 'engaged')))
        .thenThrow(const FlagRepositoryFailure.network());
    when(repository.listFlags).thenAnswer((_) async => [_flag('checkout')]);

    final cubit = KillSwitchCubit(repository, connectivity: connectivity);
    cubit.emit(KillSwitchState(status: FlagsStatus.ready, flags: [_flag('checkout')]));

    connectivity.goOffline();
    await cubit.setKillSwitch(flagKey: 'checkout', engage: true);
    expect(cubit.state.intents['checkout']?.isFailed, isTrue);

    // The network comes back and the write now succeeds.
    when(() => repository.setKillSwitch(key: any(named: 'key'), engaged: any(named: 'engaged')))
        .thenAnswer((_) async => _flag('checkout', killed: true));

    connectivity.goOnline();
    await Future<void>.delayed(const Duration(milliseconds: 50));

    verify(() => repository.setKillSwitch(key: 'checkout', engaged: true)).called(2);
    expect(cubit.state.intents, isEmpty, reason: 'the replay confirmed it');
    expect(cubit.state.flags.single.killSwitchEngaged, isTrue);

    await cubit.close();
  });

  test('reconnect does NOT auto-release when the server engaged the switch meanwhile', () async {
    // Kill-switch precedence, end to end. Replaying this release automatically would revive
    // whatever the other admin killed.
    when(repository.listFlags).thenAnswer((_) async => [_flag('checkout', killed: true)]);

    final cubit = KillSwitchCubit(repository, connectivity: connectivity)
      ..emit(KillSwitchState(
        status: FlagsStatus.ready,
        flags: [_flag('checkout', killed: true)],
        intents: {
          'checkout': KillSwitchIntent(
            flagKey: 'checkout',
            engage: false,
            state: IntentState.failed,
            requestedAt: DateTime(2026, 8, 19),
          ),
        },
      ));

    connectivity.goOffline();
    connectivity.goOnline();
    await Future<void>.delayed(const Duration(milliseconds: 50));

    verifyNever(
      () => repository.setKillSwitch(key: any(named: 'key'), engaged: any(named: 'engaged')),
    );
    expect(cubit.state.conflicts, ['checkout']);
    expect(cubit.state.flags.single.killSwitchEngaged, isTrue);

    await cubit.close();
  });

  test('reconciliation failing does not discard intents', () async {
    when(repository.listFlags).thenThrow(const FlagRepositoryFailure.network());

    final cubit = KillSwitchCubit(repository, connectivity: connectivity)
      ..emit(KillSwitchState(
        status: FlagsStatus.ready,
        intents: {
          'checkout': KillSwitchIntent(
            flagKey: 'checkout',
            engage: true,
            state: IntentState.failed,
            requestedAt: DateTime(2026, 8, 19),
          ),
        },
      ));

    await cubit.reconcileWithServer();

    expect(cubit.state.intents.containsKey('checkout'), isTrue);
    await cubit.close();
  });

  test('an unsent intent survives the app being killed', () async {
    // The scenario: engage on a train, lose signal, the OS reclaims the app. Without
    // persistence the decision is simply gone.
    when(() => repository.setKillSwitch(key: any(named: 'key'), engaged: any(named: 'engaged')))
        .thenThrow(const FlagRepositoryFailure.network());

    final first = KillSwitchCubit(repository)
      ..emit(KillSwitchState(status: FlagsStatus.ready, flags: [_flag('checkout')]));
    await first.setKillSwitch(flagKey: 'checkout', engage: true);
    await first.close();

    // A brand new instance, as after a cold start, reading the same storage.
    final restored = KillSwitchCubit(repository);

    expect(restored.state.intents['checkout']?.engage, isTrue);
    expect(
      restored.state.intents['checkout']?.state,
      IntentState.failed,
      reason: 'restored as failed, not pending — no request is actually in flight',
    );
    expect(
      restored.state.status,
      FlagsStatus.initial,
      reason: 'cached flags are last-known, not freshly fetched',
    );

    await restored.close();
  });

  test('acceptServerState is the only way a conflict clears', () async {
    when(repository.listFlags).thenAnswer((_) async => [_flag('checkout', killed: true)]);

    final cubit = KillSwitchCubit(repository)
      ..emit(KillSwitchState(
        status: FlagsStatus.ready,
        intents: {
          'checkout': KillSwitchIntent(
            flagKey: 'checkout',
            engage: false,
            state: IntentState.failed,
            requestedAt: DateTime(2026, 8, 19),
          ),
        },
      ));

    await cubit.reconcileWithServer();
    expect(cubit.state.conflicts, ['checkout']);

    cubit.acceptServerState('checkout');

    expect(cubit.state.conflicts, isEmpty);
    expect(cubit.state.intents, isEmpty);
    await cubit.close();
  });
}
