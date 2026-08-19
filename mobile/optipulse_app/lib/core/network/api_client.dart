import 'package:dio/dio.dart';
import 'package:openapi/openapi.dart';

import '../../features/auth/domain/auth_repository.dart';
import '../../features/auth/domain/auth_session.dart';
import 'auth_interceptor.dart';

/// Where the app points. Supplied at build time:
///
///   flutter run --dart-define=OPTIPULSE_API_URL=https://optipulse-api.onrender.com
///
/// The default is the deployed API rather than localhost, because the common case for this app
/// is an on-call admin opening it on a real phone, not a developer with a backend running.
const apiBaseUrl = String.fromEnvironment(
  'OPTIPULSE_API_URL',
  defaultValue: 'https://optipulse-api.onrender.com',
);

/// Builds the configured client.
///
/// The retry client is a SEPARATE Dio with no interceptors. Replaying a request through the
/// same instance would send it back through [AuthInterceptor], which on another 401 would try
/// to refresh again — the recursion the `retried` marker exists to stop, reintroduced one layer
/// down.
Openapi buildApiClient({
  required AuthRepository repository,
  required AuthSession? Function() readSession,
  required void Function(AuthSession) onRefreshed,
  required void Function() onSessionLost,
}) {
  // Render's free tier suspends idle instances; the first request after a quiet period pays a
  // cold start of up to a minute. A 30s timeout would report that healthy service as unreachable.
  final options = BaseOptions(
    baseUrl: apiBaseUrl,
    connectTimeout: const Duration(seconds: 90),
    receiveTimeout: const Duration(seconds: 90),
  );

  final retryClient = Dio(options);

  final client = Openapi(dio: Dio(options));
  client.dio.interceptors.add(
    AuthInterceptor(
      repository: repository,
      readSession: readSession,
      onRefreshed: onRefreshed,
      onSessionLost: onSessionLost,
      retryClient: retryClient,
    ),
  );
  return client;
}
