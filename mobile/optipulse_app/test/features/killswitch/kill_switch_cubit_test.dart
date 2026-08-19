import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:optipulse_app/features/killswitch/domain/flag.dart';
import 'package:optipulse_app/features/killswitch/domain/flag_repository.dart';
import 'package:optipulse_app/features/killswitch/domain/kill_switch_intent.dart';
import 'package:optipulse_app/features/killswitch/presentation/kill_switch_cubit.dart';

class _MockFlagRepository extends Mock implements FlagRepository {}

Flag _flag({bool killed = false, int version = 1}) => Flag(
      key: 'checkout.new-flow',
      name: 'Checkout — new flow',
      status: 'Active',
      killSwitchEngaged: killed,
      version: version,
    );

void main() {
  late _MockFlagRepository repository;

  setUp(() => repository = _MockFlagRepository());

  group('loadFlags', () {
    blocTest<KillSwitchCubit, KillSwitchState>(
      'clears the list on failure rather than leaving it stale',
      // A list of kill-switch states that silently stopped updating is worse than no list: it
      // looks authoritative and is not, and this is the screen opened during an incident.
      setUp: () => when(repository.listFlags).thenThrow(const FlagRepositoryFailure.network()),
      build: () => KillSwitchCubit(repository),
      seed: () => KillSwitchState(status: FlagsStatus.ready, flags: [_flag()]),
      act: (cubit) => cubit.loadFlags(),
      verify: (cubit) {
        expect(cubit.state.status, FlagsStatus.failed);
        expect(cubit.state.flags, isEmpty);
        expect(cubit.state.error, isNotNull);
      },
    );
  });

  group('setKillSwitch', () {
    blocTest<KillSwitchCubit, KillSwitchState>(
      'goes pending, then confirms and drops the intent',
      setUp: () {
        when(() => repository.setKillSwitch(key: any(named: 'key'), engaged: any(named: 'engaged')))
            .thenAnswer((_) async => _flag(killed: true, version: 2));
      },
      build: () => KillSwitchCubit(repository),
      seed: () => KillSwitchState(status: FlagsStatus.ready, flags: [_flag()]),
      act: (cubit) => cubit.setKillSwitch(flagKey: 'checkout.new-flow', engage: true),
      verify: (cubit) {
        expect(cubit.state.intents, isEmpty, reason: 'a confirmed intent is no longer pending');
        expect(cubit.state.flags.single.killSwitchEngaged, isTrue);
        expect(cubit.state.flags.single.version, 2);
      },
    );

    blocTest<KillSwitchCubit, KillSwitchState>(
      'KEEPS a failed intent and keeps showing the requested position',
      // THE REQUIREMENT THIS ENFORCES: a kill-switch action is never silently lost. Reverting
      // the toggle on error would leave an admin believing a broken feature is off while it is
      // still serving traffic, with the only evidence a toast that has already gone.
      setUp: () {
        when(() => repository.setKillSwitch(key: any(named: 'key'), engaged: any(named: 'engaged')))
            .thenThrow(const FlagRepositoryFailure.network());
      },
      build: () => KillSwitchCubit(repository),
      seed: () => KillSwitchState(status: FlagsStatus.ready, flags: [_flag()]),
      act: (cubit) => cubit.setKillSwitch(flagKey: 'checkout.new-flow', engage: true),
      verify: (cubit) {
        final intent = cubit.state.intents['checkout.new-flow'];
        expect(intent, isNotNull, reason: 'the intent must survive the failure');
        expect(intent!.state, IntentState.failed);
        expect(intent.engage, isTrue);
        expect(
          cubit.state.displayedEngagement(_flag()),
          isTrue,
          reason: 'the toggle keeps showing what the admin asked for, not the server state',
        );
        expect(cubit.state.isUnconfirmed('checkout.new-flow'), isTrue);
      },
    );

    blocTest<KillSwitchCubit, KillSwitchState>(
      'does not confirm when the server reports a different state than requested',
      // Someone else changed the flag in between. Treating the 200 as success would tell the
      // admin their action landed when the opposite is live.
      setUp: () {
        when(() => repository.setKillSwitch(key: any(named: 'key'), engaged: any(named: 'engaged')))
            .thenAnswer((_) async => _flag(killed: false, version: 7));
      },
      build: () => KillSwitchCubit(repository),
      seed: () => KillSwitchState(status: FlagsStatus.ready, flags: [_flag()]),
      act: (cubit) => cubit.setKillSwitch(flagKey: 'checkout.new-flow', engage: true),
      verify: (cubit) {
        final intent = cubit.state.intents['checkout.new-flow'];
        expect(intent?.state, IntentState.failed);
        expect(intent?.error, contains('Someone else'));
      },
    );

    blocTest<KillSwitchCubit, KillSwitchState>(
      'a forbidden response is surfaced as such, not as a network blip',
      setUp: () {
        when(() => repository.setKillSwitch(key: any(named: 'key'), engaged: any(named: 'engaged')))
            .thenThrow(const FlagRepositoryFailure('Only Admins can operate the kill switch.',
                isForbidden: true));
      },
      build: () => KillSwitchCubit(repository),
      seed: () => KillSwitchState(status: FlagsStatus.ready, flags: [_flag()]),
      act: (cubit) => cubit.setKillSwitch(flagKey: 'checkout.new-flow', engage: true),
      verify: (cubit) => expect(
        cubit.state.intents['checkout.new-flow']?.error,
        contains('Only Admins'),
      ),
    );
  });

  group('retry and dismiss', () {
    blocTest<KillSwitchCubit, KillSwitchState>(
      'retry re-sends the ORIGINAL intent, not the current server state',
      setUp: () {
        var attempts = 0;
        when(() => repository.setKillSwitch(key: any(named: 'key'), engaged: any(named: 'engaged')))
            .thenAnswer((invocation) async {
          attempts++;
          if (attempts == 1) throw const FlagRepositoryFailure.network();
          return _flag(killed: invocation.namedArguments[#engaged] as bool, version: 3);
        });
      },
      build: () => KillSwitchCubit(repository),
      seed: () => KillSwitchState(status: FlagsStatus.ready, flags: [_flag()]),
      act: (cubit) async {
        await cubit.setKillSwitch(flagKey: 'checkout.new-flow', engage: true);
        await cubit.retry('checkout.new-flow');
      },
      verify: (cubit) {
        expect(cubit.state.intents, isEmpty);
        expect(cubit.state.flags.single.killSwitchEngaged, isTrue);
        verify(() => repository.setKillSwitch(key: 'checkout.new-flow', engaged: true)).called(2);
      },
    );

    blocTest<KillSwitchCubit, KillSwitchState>(
      'dismiss is the only way an unsucceeded intent leaves the screen',
      setUp: () {
        when(() => repository.setKillSwitch(key: any(named: 'key'), engaged: any(named: 'engaged')))
            .thenThrow(const FlagRepositoryFailure.network());
      },
      build: () => KillSwitchCubit(repository),
      seed: () => KillSwitchState(status: FlagsStatus.ready, flags: [_flag()]),
      act: (cubit) async {
        await cubit.setKillSwitch(flagKey: 'checkout.new-flow', engage: true);
        cubit.dismiss('checkout.new-flow');
      },
      verify: (cubit) {
        expect(cubit.state.intents, isEmpty);
        expect(
          cubit.state.displayedEngagement(_flag()),
          isFalse,
          reason: 'with the intent abandoned, the server state is shown again',
        );
      },
    );

    test('retry on an unknown flag is a no-op, not a crash', () async {
      final cubit = KillSwitchCubit(repository);
      await cubit.retry('nothing.here');
      verifyNever(
        () => repository.setKillSwitch(key: any(named: 'key'), engaged: any(named: 'engaged')),
      );
    });
  });
}
