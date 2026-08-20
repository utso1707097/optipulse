import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:optipulse_app/core/network/auth_interceptor.dart';
import 'package:optipulse_app/features/auth/domain/auth_failure.dart';
import 'package:optipulse_app/features/auth/domain/auth_repository.dart';
import 'package:optipulse_app/features/auth/domain/auth_session.dart';

/// Counts refreshes and can be made to hang, so the single-flight behaviour is observable
/// rather than inferred. A mock that returns instantly would let a broken implementation pass:
/// the second caller would arrive after the first had already finished.
class _RecordingRepository implements AuthRepository {
  _RecordingRepository({this.failRefresh = false, this.failureKind});

  final bool failRefresh;
  final AuthFailureKind? failureKind;
  int refreshCalls = 0;
  AuthSession? storedSession;
  final _gate = Completer<void>();

  void releaseRefresh() => _gate.complete();

  @override
  Future<AuthSession> refresh(AuthSession session) async {
    refreshCalls++;
    await _gate.future;
    if (failRefresh) {
      throw switch (failureKind ?? AuthFailureKind.sessionExpired) {
        AuthFailureKind.network => const AuthFailure.network(),
        _ => const AuthFailure.sessionExpired(),
      };
    }
    return session.copyWith(
      accessToken: 'refreshed-${session.accessToken}',
      refreshToken: 'rotated',
      expiresAt: DateTime.now().add(const Duration(hours: 1)),
    );
  }

  @override
  Future<AuthSession> logIn({required String email, required String password}) async =>
      throw UnimplementedError();
  @override
  Future<void> logOut(AuthSession? session) async {}
  @override
  Future<AuthSession?> restore() async => storedSession;
  @override
  Future<void> persist(AuthSession session) async {}
}

AuthSession _session({Duration ttl = const Duration(hours: 1)}) => AuthSession(
      accessToken: 'access',
      refreshToken: 'refresh',
      role: 'Admin',
      expiresAt: DateTime.now().add(ttl),
    );

void main() {
  test('concurrent expiring requests trigger exactly ONE refresh', () async {
    // THE BUG THIS GUARDS. With rotating refresh tokens, refreshing per failed request is not
    // merely wasteful: the first refresh invalidates the token the others still hold, the
    // backend sees a reused token, and it revokes the whole family — signing the user out via
    // the mechanism meant to keep them signed in.
    final repository = _RecordingRepository();
    var session = _session(ttl: const Duration(seconds: 5)); // inside the 30s refresh margin

    final interceptor = AuthInterceptor(
      repository: repository,
      readSession: () => session,
      onRefreshed: (refreshed) => session = refreshed,
      onSessionLost: () {},
    );

    final handlers = List.generate(5, (_) => _CapturingHandler());
    final requests = [
      for (var i = 0; i < 5; i++) RequestOptions(path: '/api/v1/flags')
    ];

    final pending = [
      for (var i = 0; i < 5; i++) interceptor.onRequest(requests[i], handlers[i]),
    ];

    await Future<void>.delayed(Duration.zero);
    repository.releaseRefresh();
    await Future.wait(pending);

    expect(repository.refreshCalls, 1, reason: 'all five must share one refresh');
    for (final handler in handlers) {
      expect(handler.options!.headers['Authorization'], 'Bearer refreshed-access');
    }
  });

  test('a valid token is used as-is, with no refresh', () async {
    final repository = _RecordingRepository();
    final interceptor = AuthInterceptor(
      repository: repository,
      readSession: _session,
      onRefreshed: (_) {},
      onSessionLost: () {},
    );

    final handler = _CapturingHandler();
    await interceptor.onRequest(RequestOptions(path: '/api/v1/flags'), handler);

    expect(repository.refreshCalls, 0);
    expect(handler.options!.headers['Authorization'], 'Bearer access');
  });

  test('login and refresh calls never carry a bearer token', () async {
    // Sending a stale token to /auth/refresh would be a 401 that triggers another refresh.
    final repository = _RecordingRepository();
    final interceptor = AuthInterceptor(
      repository: repository,
      readSession: () => _session(ttl: const Duration(seconds: 1)),
      onRefreshed: (_) {},
      onSessionLost: () {},
    );

    for (final path in ['/api/v1/auth/login', '/api/v1/auth/refresh']) {
      final handler = _CapturingHandler();
      await interceptor.onRequest(RequestOptions(path: path), handler);
      expect(handler.options!.headers.containsKey('Authorization'), isFalse, reason: path);
    }
    expect(repository.refreshCalls, 0);
  });

  test('a rejected refresh token signs the user out instead of retrying', () async {
    final repository = _RecordingRepository(failRefresh: true)..releaseRefresh();
    var lost = false;

    final interceptor = AuthInterceptor(
      repository: repository,
      readSession: () => _session(ttl: const Duration(seconds: 1)),
      onRefreshed: (_) {},
      onSessionLost: () => lost = true,
    );

    final handler = _CapturingHandler();
    await interceptor.onRequest(RequestOptions(path: '/api/v1/flags'), handler);

    expect(lost, isTrue);
    expect(handler.rejected, isTrue);
    expect(repository.refreshCalls, 1, reason: 'terminal failure must not be retried');
  });

  test('a NETWORK failure during refresh does not sign the user out', () async {
    // THE BUG THIS GUARDS. Every AuthFailure used to end the session, so a refresh that never
    // reached the server — flaky signal, a tunnel, a cold-starting free-tier instance — logged
    // the user out. The token was almost certainly still valid; nobody had said otherwise.
    final repository = _RecordingRepository(
      failRefresh: true,
      failureKind: AuthFailureKind.network,
    )..releaseRefresh();
    var lost = false;

    final interceptor = AuthInterceptor(
      repository: repository,
      readSession: () => _session(ttl: const Duration(seconds: 1)),
      onRefreshed: (_) {},
      onSessionLost: () => lost = true,
    );

    final handler = _CapturingHandler();
    await interceptor.onRequest(RequestOptions(path: '/api/v1/flags'), handler);

    expect(lost, isFalse, reason: 'a dropped packet is not proof the session ended');
    expect(handler.rejected, isTrue, reason: 'the request still fails');
  });

  test('a stale in-memory token is recovered from the keystore before giving up', () async {
    // The server rotated the token and the device persisted it, but the in-memory copy is
    // older — the process died between the two, or this interceptor captured the session
    // first. Presenting the old one trips reuse detection and revokes the whole family.
    var attempts = 0;
    final repository = _StaleTokenRepository(
      stored: _session().copyWith(refreshToken: 'newer-token'),
      onRefresh: () => attempts++,
    );
    var lost = false;

    final interceptor = AuthInterceptor(
      repository: repository,
      readSession: () => _session(ttl: const Duration(seconds: 1)),
      onRefreshed: (_) {},
      onSessionLost: () => lost = true,
    );

    final handler = _CapturingHandler();
    await interceptor.onRequest(RequestOptions(path: '/api/v1/flags'), handler);

    expect(attempts, 2, reason: 'one failed attempt, then one with the stored token');
    expect(lost, isFalse);
    expect(handler.options!.headers['Authorization'], 'Bearer recovered');
  });

  test('recovery does NOT retry when the keystore holds the same token', () async {
    // Retrying with the identical token would be genuine reuse, and would revoke the family
    // for real. Recovery is only valid when the stored token actually differs.
    // Inside the 30s margin, so a proactive refresh actually fires.
    final same = _session(ttl: const Duration(seconds: 1));
    final repository = _RecordingRepository(failRefresh: true)
      ..releaseRefresh()
      ..storedSession = same;
    var lost = false;

    final interceptor = AuthInterceptor(
      repository: repository,
      readSession: () => same,
      onRefreshed: (_) {},
      onSessionLost: () => lost = true,
    );

    await interceptor.onRequest(RequestOptions(path: '/api/v1/flags'), _CapturingHandler());

    expect(repository.refreshCalls, 1, reason: 'must not present the same token twice');
    expect(lost, isTrue, reason: 'nothing left to recover with');
  });

  test('an already-retried request is not retried again', () async {
    // Without this, a server returning 401 for any token loops forever.
    final repository = _RecordingRepository()..releaseRefresh();
    final interceptor = AuthInterceptor(
      repository: repository,
      readSession: _session,
      onRefreshed: (_) {},
      onSessionLost: () {},
    );

    final options = RequestOptions(path: '/api/v1/flags')
      ..extra['optipulse.retried'] = true;
    final handler = _CapturingErrorHandler();

    await interceptor.onError(
      DioException(
        requestOptions: options,
        response: Response(requestOptions: options, statusCode: 401),
        type: DioExceptionType.badResponse,
      ),
      handler,
    );

    expect(repository.refreshCalls, 0);
    expect(handler.passedThrough, isTrue);
  });
}

/// Fails the first refresh as expired, then succeeds with whatever the keystore holds.
class _StaleTokenRepository implements AuthRepository {
  _StaleTokenRepository({required this.stored, required this.onRefresh});

  final AuthSession stored;
  final void Function() onRefresh;
  var _attempts = 0;

  @override
  Future<AuthSession> refresh(AuthSession session) async {
    onRefresh();
    _attempts++;
    if (_attempts == 1) throw const AuthFailure.sessionExpired();
    return session.copyWith(accessToken: 'recovered');
  }

  @override
  Future<AuthSession?> restore() async => stored;

  @override
  Future<AuthSession> logIn({required String email, required String password}) async =>
      throw UnimplementedError();
  @override
  Future<void> logOut(AuthSession? session) async {}
  @override
  Future<void> persist(AuthSession session) async {}
}

class _CapturingHandler extends RequestInterceptorHandler {
  RequestOptions? options;
  bool rejected = false;

  @override
  void next(RequestOptions requestOptions) => options = requestOptions;

  @override
  void reject(DioException error, [bool callFollowingErrorInterceptor = false]) {
    rejected = true;
    options = error.requestOptions;
  }
}

class _CapturingErrorHandler extends ErrorInterceptorHandler {
  bool passedThrough = false;

  @override
  void next(DioException err) => passedThrough = true;
}
