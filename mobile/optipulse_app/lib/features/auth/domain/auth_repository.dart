import 'auth_session.dart';

/// The auth operations the app depends on, stated without reference to Dio, to the generated
/// client, or to where tokens are stored. Presentation talks to this; only `data/` knows there
/// is an HTTP call behind it.
abstract class AuthRepository {
  /// Throws [AuthFailure] on any non-success.
  Future<AuthSession> logIn({required String email, required String password});

  /// Exchanges the refresh token for a new pair. Throws [AuthFailure] when the session cannot
  /// be recovered, which the caller must treat as "signed out" rather than "try again".
  Future<AuthSession> refresh(AuthSession session);

  /// Best-effort server-side revocation, then local clearing. Local state is cleared even if
  /// the network call fails: a sign-out that leaves credentials on the device because the API
  /// was unreachable is a sign-out that did not happen.
  Future<void> logOut(AuthSession? session);

  /// The session persisted on this device, if any is still usable.
  Future<AuthSession?> restore();

  Future<void> persist(AuthSession session);
}
