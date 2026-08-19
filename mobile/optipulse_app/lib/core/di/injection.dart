import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:get_it/get_it.dart';
import 'package:openapi/openapi.dart';

import '../../features/auth/data/auth_repository_impl.dart';
import '../../features/auth/data/token_store.dart';
import '../../features/auth/domain/auth_repository.dart';
import '../../features/auth/domain/auth_session.dart';
import '../../features/auth/presentation/auth_cubit.dart';
import '../../features/killswitch/data/flag_repository_impl.dart';
import '../../features/killswitch/domain/flag_repository.dart';
import '../../features/killswitch/presentation/kill_switch_cubit.dart';
import '../network/api_client.dart';
import '../reconcile/connectivity_monitor.dart';

final getIt = GetIt.instance;

/// Registers the object graph.
///
/// Wired by hand rather than with `injectable`'s code generation. This graph has six
/// registrations and one genuinely awkward edge — the API client needs the auth repository,
/// which needs the API client — and expressing that circularity through annotations is harder
/// to read than the twenty lines below. A second build_runner pipeline in the app, on top of
/// the one the generated client already runs, is not worth it at this size.
///
/// THE CIRCULARITY, and how it is broken: the repository calls the API, and the API's
/// interceptor calls the repository to refresh. It is resolved by giving the interceptor its
/// own client — one whose AuthenticationApi is built on a bare Dio with no interceptors — so
/// refresh never re-enters the thing that triggered it.
Future<void> configureDependencies() async {
  getIt.registerLazySingleton<FlutterSecureStorage>(
    // Android defaults are left alone deliberately: as of flutter_secure_storage 11 the default
    // is AES-GCM with RSA-OAEP key wrapping in the Android Keystore. The older
    // `encryptedSharedPreferences: true` knob was removed because the Jetpack Security library
    // behind it is deprecated — asking for it now would be asking for the weaker option.
    //
    // On iOS, `first_unlock` rather than the default `unlocked`: this app receives push alerts
    // about critical events and needs to read its token while the phone is locked in a pocket.
    // It still requires the device to have been unlocked at least once since boot.
    () => const FlutterSecureStorage(
      iOptions: IOSOptions(accessibility: KeychainAccessibility.first_unlock),
    ),
  );

  getIt.registerLazySingleton<TokenStore>(() => TokenStore(getIt()));

  // The repository the INTERCEPTOR uses, built on a plain client. See the note above.
  getIt.registerLazySingleton<AuthRepository>(
    () => AuthRepositoryImpl(
      api: Openapi(basePathOverride: apiBaseUrl).getAuthenticationApi(),
      store: getIt(),
    ),
  );

  getIt.registerLazySingleton<AuthCubit>(() => AuthCubit(getIt()));

  getIt.registerLazySingleton<Openapi>(
    () {
      final cubit = getIt<AuthCubit>();
      return buildApiClient(
        repository: getIt(),
        readSession: () => cubit.state.session,
        onRefreshed: cubit.onSessionRefreshed,
        onSessionLost: cubit.onSessionLost,
      );
    },
  );

  // Built on the SESSION-AWARE client (getIt<Openapi>()), unlike the auth repository above:
  // these calls carry a bearer token and must go through the refresh interceptor.
  getIt.registerLazySingleton<FlagRepository>(
    () => FlagRepositoryImpl(getIt<Openapi>().getFlagsApi()),
  );

  getIt.registerLazySingleton<ConnectivityMonitor>(ConnectivityPlusMonitor.new);

  getIt.registerFactory<KillSwitchCubit>(
    () => KillSwitchCubit(getIt(), connectivity: getIt<ConnectivityMonitor>()),
  );
}

/// Convenience for features that only need a session-aware client.
Openapi get apiClient => getIt<Openapi>();

AuthSession? get currentSession => getIt<AuthCubit>().state.session;
