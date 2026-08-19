import 'package:dio/dio.dart';
import 'package:openapi/openapi.dart';

import '../domain/auth_failure.dart';
import '../domain/auth_repository.dart';
import '../domain/auth_session.dart';
import 'token_store.dart';

/// [AuthRepository] over the generated client.
///
/// Every Dio failure is translated into an [AuthFailure] here. That is the whole point of the
/// boundary: a `DioException` reaching a Cubit would drag the transport into the presentation
/// layer, and a screen would end up switching on HTTP status codes to decide what to tell a
/// person.
class AuthRepositoryImpl implements AuthRepository {
  AuthRepositoryImpl({
    required AuthenticationApi api,
    required TokenStore store,
    DateTime Function()? now,
  })  : _api = api,
        _store = store,
        // Injected so tests can pin the clock: expiry is computed from `expiresInSeconds`
        // relative to now, which would otherwise make every assertion time-dependent.
        _now = now ?? DateTime.now;

  final AuthenticationApi _api;
  final TokenStore _store;
  final DateTime Function() _now;

  @override
  Future<AuthSession> logIn({required String email, required String password}) async {
    try {
      final response = await _api.login(
        loginRequest: LoginRequest(
          (b) => b
            ..email = email
            ..password = password,
        ),
      );
      final session = _toSession(response.data!);
      await _store.write(session);
      return session;
    } on DioException catch (error) {
      throw _translate(error, onUnauthorized: const AuthFailure.invalidCredentials());
    }
  }

  @override
  Future<AuthSession> refresh(AuthSession session) async {
    try {
      final response = await _api.refresh(
        refreshRequest: RefreshRequest((b) => b..refreshToken = session.refreshToken),
      );
      final refreshed = _toSession(response.data!);
      await _store.write(refreshed);
      return refreshed;
    } on DioException catch (error) {
      // A rejected refresh token is terminal, not retryable. The backend rotates refresh tokens
      // and revokes the whole family on reuse, so a 401 here can also mean "this token was
      // already redeemed by someone else" — in which case retrying is exactly wrong.
      throw _translate(error, onUnauthorized: const AuthFailure.sessionExpired());
    }
  }

  @override
  Future<void> logOut(AuthSession? session) async {
    try {
      if (session != null) {
        await _api.logout(
          refreshRequest: RefreshRequest((b) => b..refreshToken = session.refreshToken),
        );
      }
    } on DioException {
      // Deliberately swallowed. Local credentials are cleared below regardless: a sign-out that
      // leaves tokens on the device because the network was down is not a sign-out.
    } finally {
      await _store.clear();
    }
  }

  @override
  Future<AuthSession?> restore() => _store.read();

  @override
  Future<void> persist(AuthSession session) => _store.write(session);

  AuthSession _toSession(LoginResponse response) => AuthSession(
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
        role: response.role,
        expiresAt: _now().add(Duration(seconds: response.expiresInSeconds)),
      );

  AuthFailure _translate(DioException error, {required AuthFailure onUnauthorized}) {
    switch (error.type) {
      case DioExceptionType.connectionError:
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.sendTimeout:
      case DioExceptionType.receiveTimeout:
        return const AuthFailure.network();
      case DioExceptionType.badResponse:
        final status = error.response?.statusCode;
        if (status == 401 || status == 403) return onUnauthorized;
        return AuthFailure(
          AuthFailureKind.server,
          'OptiPulse returned an unexpected error (${status ?? 'no status'}).',
        );
      default:
        return const AuthFailure.network();
    }
  }
}
