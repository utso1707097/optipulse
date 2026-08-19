import '../../features/killswitch/domain/flag.dart';
import '../../features/killswitch/domain/kill_switch_intent.dart';

/// The outcome of merging what the device believes with what the server reports.
class ReconcileResult {
  const ReconcileResult({
    required this.displayFlags,
    required this.intents,
    required this.replayed,
    required this.conflicts,
  });

  /// What the UI should SHOW: server truth with kill-switch precedence applied.
  ///
  /// Named `displayFlags`, not `flags`, because it is a projection and not a new server truth.
  /// Feeding it back into [reconcile] as `serverFlags` would report outstanding intents as
  /// already satisfied — the projection asserts the intent, so comparing the intent against it
  /// always matches. The first test written against this API made exactly that mistake, which
  /// is reason enough for the name to carry the warning.
  final List<Flag> displayFlags;

  /// Intents still outstanding — these must be sent.
  final Map<String, KillSwitchIntent> intents;

  /// Intents the server has already satisfied; they can be dropped.
  final List<String> replayed;

  /// Flags where the device and the server disagreed in a way a person should see.
  final List<String> conflicts;
}

/// Merges offline state with server truth (FR-028).
///
/// A PURE FUNCTION on purpose. "Deterministic" is the requirement, and the only way to hold a
/// system to it is to make the merge depend on nothing but its arguments — no clock, no
/// network, no ambient state. Every rule below is therefore testable by calling this directly.
///
/// THE PRECEDENCE RULE, and why it is asymmetric:
///
/// When the device and the server disagree about a kill switch, the ENGAGED state wins. This is
/// deliberately not "last write wins" and not "server always wins". A kill switch is a safety
/// control, and the two possible mistakes are not equal:
///
///   - Wrongly leaving a feature killed costs a feature being off for a few minutes.
///   - Wrongly bringing a killed feature back costs whatever the admin killed it to stop —
///     during an incident, while they are holding a phone that told them it was handled.
///
/// So a pending "engage" is preserved even if the server currently reports the flag live, and a
/// server-side engagement is honoured even if the device has a pending "release". A release is
/// never applied implicitly by reconciliation; it must be issued deliberately.
ReconcileResult reconcile({
  required List<Flag> serverFlags,
  required Map<String, KillSwitchIntent> pendingIntents,
}) {
  final resolved = <Flag>[];
  final remaining = <String, KillSwitchIntent>{};
  final replayed = <String>[];
  final conflicts = <String>[];

  // Sorted so the result cannot depend on map iteration order. Two devices holding the same
  // intents must reconcile to the same answer.
  final keys = serverFlags.map((flag) => flag.key).toList()..sort();
  final byKey = {for (final flag in serverFlags) flag.key: flag};

  for (final key in keys) {
    final flag = byKey[key]!;
    final intent = pendingIntents[key];

    if (intent == null) {
      resolved.add(flag);
      continue;
    }

    if (flag.killSwitchEngaged == intent.engage) {
      // The server already reflects what was asked. The intent is satisfied — whether by this
      // device's earlier attempt actually landing, or by another admin doing the same thing.
      // Either way there is nothing left to send.
      resolved.add(flag);
      replayed.add(key);
      continue;
    }

    if (intent.engage) {
      // Pending engage, server says live. The intent stands and must be sent. The flag is shown
      // as ENGAGED in the meantime so the admin is not told a feature is live when they have
      // asked for it to be killed and the request has not yet been rejected.
      resolved.add(flag.copyWith(killSwitchEngaged: true));
      remaining[key] = intent;
      continue;
    }

    // Pending release, server says killed. Precedence applies: the kill stands. The intent is
    // KEPT rather than discarded — the admin's decision is still theirs to make — but it is
    // surfaced as a conflict, because something engaged this after they asked for a release and
    // that is exactly the case where silently releasing would be dangerous.
    resolved.add(flag);
    remaining[key] = intent;
    conflicts.add(key);
  }

  // Intents for flags the server did not return at all: the flag was deleted, or this device
  // has never successfully listed. Kept, not dropped — an unsent kill-switch action is never
  // discarded because a list request happened to omit it.
  for (final entry in pendingIntents.entries) {
    if (!byKey.containsKey(entry.key)) {
      remaining[entry.key] = entry.value;
    }
  }

  return ReconcileResult(
    displayFlags: resolved,
    intents: remaining,
    replayed: replayed,
    conflicts: conflicts,
  );
}
