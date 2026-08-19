import 'dart:async';

import 'package:dio/dio.dart';

import '../../features/auth/domain/auth_failure.dart';
import '../../features/auth/domain/auth_repository.dart';
import '../../features/auth/domain/auth_session.dart';

/// Attaches the bearer token and recovers from expiry, once.
///
/// THE PROBLEM THIS EXISTS TO SOLVE, stated plainly: a dashboard screen typically fires several
/// requests at once. If the access token has expired, every one of them comes back 401 at
/// roughly the same moment. The naive interceptor refreshes per failed request — so five
/// requests trigger five refreshes. With rotating refresh tokens that is not merely wasteful,
/// it is fatal: the first refresh invalidates the token the other four are still holding, the
/// backend sees a reused token, and it revokes the entire family. The user is signed out by
/// the very mechanism meant to keep them signed in.
///
/// So refresh is funnelled through a single in-flight future ([_inFlight]). The first 401 starts
/// it; the rest await the same one and then retry with whatever token it produced.
class AuthInterceptor extends QueuedInterceptor {
  AuthInterceptor({
    required AuthRepository repository,
    required AuthSession? Function() readSession,
    required void Function(AuthSession) onRefreshed,
    required void Function() onSessionLost,
    Dio? retryClient,
  })  : _repository = repository,
        _readSession = readSession,
        _onRefreshed = onRefreshed,
        _onSessionLost = onSessionLost,
        _retryClient = retryClient;

  final AuthRepository _repository;
  final AuthSession? Function() _readSession;
  final void Function(AuthSession) _onRefreshed;
  final void Function() _onSessionLost;
  final Dio? _retryClient;

  Future<AuthSession>? _inFlight;

  /// Endpoints that must never carry a stale bearer token or trigger a refresh loop: refreshing
  /// is itself an unauthenticated exchange, and a 401 from login means "wrong password", not
  /// "token expired".
  static const _unauthenticatedPaths = {
    '/api/v1/auth/login',
    '/api/v1/auth/refresh',
  };

  bool _isUnauthenticated(String path) =>
      _unauthenticatedPaths.any((candidate) => path.endsWith(candidate));

  @override
  Future<void> onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    if (_isUnauthenticated(options.path)) return handler.next(options);

    var session = _readSession();
    if (session == null) return handler.next(options);

    // Refresh proactively when the token is about to die. This is not an optimisation: without
    // it, a long-running screen reliably pays a 401-then-retry on its first request after the
    // token lapses, which shows up to the user as a stutter.
    if (session.expiresWithin(const Duration(seconds: 30), now: DateTime.now())) {
      try {
        session = await _refreshOnce(session);
      } on AuthFailure {
        _onSessionLost();
        return handler.reject(
          DioException(
            requestOptions: options,
            type: DioExceptionType.badResponse,
            response: Response(requestOptions: options, statusCode: 401),
          ),
        );
      }
    }

    options.headers['Authorization'] = 'Bearer ${session.accessToken}';
    handler.next(options);
  }

  @override
  Future<void> onError(DioException err, ErrorInterceptorHandler handler) async {
    final status = err.response?.statusCode;
    final path = err.requestOptions.path;

    // `_retried` marks a request that has already been through this recovery once. Without it a
    // server that returns 401 no matter what token it is given would loop forever.
    final alreadyRetried = err.requestOptions.extra['optipulse.retried'] == true;

    if (status != 401 || alreadyRetried || _isUnauthenticated(path)) {
      return handler.next(err);
    }

    final session = _readSession();
    if (session == null) return handler.next(err);

    final AuthSession refreshed;
    try {
      refreshed = await _refreshOnce(session);
    } on AuthFailure {
      _onSessionLost();
      return handler.next(err);
    }

    final options = err.requestOptions
      ..extra['optipulse.retried'] = true
      ..headers['Authorization'] = 'Bearer ${refreshed.accessToken}';

    try {
      final client = _retryClient;
      if (client == null) return handler.next(err);
      final response = await client.fetch<dynamic>(options);
      handler.resolve(response);
    } on DioException catch (retryError) {
      handler.next(retryError);
    }
  }

  /// One refresh at a time, shared by every caller that arrives while it is running.
  Future<AuthSession> _refreshOnce(AuthSession session) {
    final existing = _inFlight;
    if (existing != null) return existing;

    final future = _repository.refresh(session).then((refreshed) {
      _onRefreshed(refreshed);
      return refreshed;
    }).whenComplete(() {
      _inFlight = null;
    });

    _inFlight = future;
    return future;
  }
}
