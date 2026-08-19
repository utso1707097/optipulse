import 'package:flutter_test/flutter_test.dart';
import 'package:optipulse_app/core/reconcile/reconciler.dart';
import 'package:optipulse_app/features/killswitch/domain/flag.dart';
import 'package:optipulse_app/features/killswitch/domain/kill_switch_intent.dart';

Flag _flag(String key, {bool killed = false}) => Flag(
      key: key,
      name: key,
      status: 'Active',
      killSwitchEngaged: killed,
      version: 1,
    );

KillSwitchIntent _intent(String key, {required bool engage}) => KillSwitchIntent(
      flagKey: key,
      engage: engage,
      state: IntentState.failed,
      requestedAt: DateTime(2026, 8, 19, 12),
    );

void main() {
  group('kill-switch precedence (FR-028)', () {
    test('a pending ENGAGE survives a server that still reports the flag live', () {
      final result = reconcile(
        serverFlags: [_flag('checkout', killed: false)],
        pendingIntents: {'checkout': _intent('checkout', engage: true)},
      );

      expect(result.intents.containsKey('checkout'), isTrue, reason: 'must still be sent');
      expect(
        result.displayFlags.single.killSwitchEngaged,
        isTrue,
        reason: 'the admin asked for a kill; do not show the feature as live meanwhile',
      );
      expect(result.replayed, isEmpty);
    });

    test('a server-side ENGAGE beats a pending release, and is flagged as a conflict', () {
      // The dangerous direction. Something killed this flag after the admin asked to release
      // it; applying the release implicitly would revive whatever the kill was stopping.
      final result = reconcile(
        serverFlags: [_flag('checkout', killed: true)],
        pendingIntents: {'checkout': _intent('checkout', engage: false)},
      );

      expect(result.displayFlags.single.killSwitchEngaged, isTrue, reason: 'the kill stands');
      expect(result.conflicts, ['checkout'], reason: 'a person must see this');
      expect(
        result.intents.containsKey('checkout'),
        isTrue,
        reason: 'the release is still the admin\'s decision to make, not ours to discard',
      );
    });

    test('an intent the server already satisfies is dropped, not re-sent', () {
      final result = reconcile(
        serverFlags: [_flag('checkout', killed: true)],
        pendingIntents: {'checkout': _intent('checkout', engage: true)},
      );

      expect(result.replayed, ['checkout']);
      expect(result.intents, isEmpty);
      expect(result.conflicts, isEmpty);
    });

    test('an intent for a flag the server did not return is KEPT', () {
      // A list request that omitted the flag must never be the reason a kill-switch action
      // disappears.
      final result = reconcile(
        serverFlags: [_flag('other')],
        pendingIntents: {'checkout': _intent('checkout', engage: true)},
      );

      expect(result.intents.containsKey('checkout'), isTrue);
      expect(result.displayFlags.map((f) => f.key), ['other']);
    });
  });

  group('determinism', () {
    test('the result does not depend on input ordering', () {
      final flags = [
        _flag('c', killed: true),
        _flag('a'),
        _flag('b', killed: true),
      ];
      final intents = {
        'b': _intent('b', engage: false),
        'a': _intent('a', engage: true),
      };

      final forward = reconcile(serverFlags: flags, pendingIntents: intents);
      final reversed = reconcile(
        serverFlags: flags.reversed.toList(),
        pendingIntents: Map.fromEntries(intents.entries.toList().reversed),
      );

      expect(
        forward.displayFlags.map((f) => '${f.key}:${f.killSwitchEngaged}'),
        reversed.displayFlags.map((f) => '${f.key}:${f.killSwitchEngaged}'),
      );
      expect(forward.conflicts, reversed.conflicts);
      expect(forward.intents.keys.toList()..sort(), reversed.intents.keys.toList()..sort());
    });

    test('re-running against the same server truth changes nothing', () {
      // Idempotence. Reconnects fire repeatedly — a flaky connection produces several in a row
      // — and each pass must reach the same answer.
      //
      // Note what is fed back in: the ORIGINAL server flags, not `displayFlags`. The projection
      // already asserts the pending intent, so passing it back would make every intent look
      // satisfied. That is a misuse the field name now warns about.
      final serverFlags = [_flag('a'), _flag('b', killed: true)];
      final first = reconcile(
        serverFlags: serverFlags,
        pendingIntents: {'a': _intent('a', engage: true)},
      );
      final second = reconcile(serverFlags: serverFlags, pendingIntents: first.intents);

      expect(
        second.displayFlags.map((f) => '${f.key}:${f.killSwitchEngaged}'),
        first.displayFlags.map((f) => '${f.key}:${f.killSwitchEngaged}'),
      );
      expect(second.intents.keys, first.intents.keys);
      expect(second.conflicts, first.conflicts);
    });

    test('the display projection must NOT be fed back as server truth', () {
      // Documents the failure mode directly, so the reason for the naming survives.
      final serverFlags = [_flag('a')];
      final first = reconcile(
        serverFlags: serverFlags,
        pendingIntents: {'a': _intent('a', engage: true)},
      );

      final misused = reconcile(
        serverFlags: first.displayFlags,
        pendingIntents: first.intents,
      );

      expect(first.intents.containsKey('a'), isTrue);
      expect(
        misused.intents,
        isEmpty,
        reason: 'the projection asserts the intent, so it always looks already satisfied',
      );
    });

    test('no intents and no flags is not an error', () {
      final result = reconcile(serverFlags: const [], pendingIntents: const {});
      expect(result.displayFlags, isEmpty);
      expect(result.intents, isEmpty);
      expect(result.conflicts, isEmpty);
    });
  });
}
