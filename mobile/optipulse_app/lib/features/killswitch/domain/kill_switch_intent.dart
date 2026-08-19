import 'package:equatable/equatable.dart';

/// An admin's decision that has not yet been confirmed by the server.
///
/// This type exists because of one requirement: a kill-switch action must NEVER be silently
/// lost. The obvious optimistic-UI pattern — flip the toggle, fire the request, revert on
/// error — is exactly wrong here. Reverting a safety control because the network hiccuped
/// leaves an admin believing a broken feature is off while it is still serving traffic, and the
/// only signal was a toast that has already gone.
///
/// So an intent is recorded, kept, and shown as unconfirmed until the server acknowledges it or
/// the admin explicitly abandons it.
enum IntentState {
  /// Sent, no answer yet.
  pending,

  /// The server acknowledged it. The flag now reads back what was asked for.
  confirmed,

  /// The attempt failed. The intent is RETAINED, not discarded.
  failed,
}

class KillSwitchIntent extends Equatable {
  const KillSwitchIntent({
    required this.flagKey,
    required this.engage,
    required this.state,
    required this.requestedAt,
    this.error,
  });

  final String flagKey;

  /// What the admin asked for: true = engage the kill switch, false = release it.
  final bool engage;

  final IntentState state;
  final DateTime requestedAt;
  final String? error;

  bool get isPending => state == IntentState.pending;
  bool get isFailed => state == IntentState.failed;

  KillSwitchIntent copyWith({IntentState? state, String? error}) => KillSwitchIntent(
        flagKey: flagKey,
        engage: engage,
        state: state ?? this.state,
        requestedAt: requestedAt,
        error: error,
      );

  @override
  List<Object?> get props => [flagKey, engage, state, requestedAt, error];
}
