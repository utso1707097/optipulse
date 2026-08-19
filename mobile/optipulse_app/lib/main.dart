import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:hydrated_bloc/hydrated_bloc.dart';
import 'package:path_provider/path_provider.dart';

import 'core/di/injection.dart';
import 'features/auth/presentation/auth_cubit.dart';
import 'features/auth/presentation/login_screen.dart';
import 'features/killswitch/presentation/kill_switch_cubit.dart';
import 'features/killswitch/presentation/kill_switch_screen.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Backs KillSwitchCubit's persistence. Application-support rather than temporary storage:
  // an unsent kill-switch decision must not be something the OS can reclaim under disk
  // pressure. It is not secret — it is a flag key and a boolean — so it does not belong in the
  // keystore alongside tokens.
  HydratedBloc.storage = await HydratedStorage.build(
    storageDirectory: HydratedStorageDirectory(
      (await getApplicationSupportDirectory()).path,
    ),
  );

  await configureDependencies();
  runApp(const OptiPulseApp());
}

class OptiPulseApp extends StatelessWidget {
  const OptiPulseApp({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider<AuthCubit>(
      // Resolved from the locator rather than constructed here: the Dio interceptor reads the
      // session from this same instance, so a second one would leave requests authenticating
      // with a session the UI does not know about.
      create: (_) => getIt<AuthCubit>()..restoreSession(),
      child: MaterialApp(
        title: 'OptiPulse',
        debugShowCheckedModeBanner: false,
        theme: ThemeData(
          colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF4F46E5)),
        ),
        darkTheme: ThemeData(
          colorScheme: ColorScheme.fromSeed(
            seedColor: const Color(0xFF4F46E5),
            brightness: Brightness.dark,
          ),
        ),
        home: const _AuthGate(),
      ),
    );
  }
}

/// Decides between the login screen and the app, and renders NEITHER until the keystore has
/// been read. Treating "unknown" as "signed out" would flash a sign-in form at every already
/// authenticated user on every cold start.
class _AuthGate extends StatelessWidget {
  const _AuthGate();

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<AuthCubit, AuthState>(
      builder: (context, state) {
        switch (state.status) {
          case AuthStatus.unknown:
            return const Scaffold(body: Center(child: CircularProgressIndicator()));
          case AuthStatus.unauthenticated:
            return const LoginScreen();
          case AuthStatus.authenticated:
            return const _Home();
        }
      },
    );
  }
}

/// The signed-in shell. Telemetry and alerts (T074, T075) join the kill switch here.
class _Home extends StatelessWidget {
  const _Home();

  @override
  Widget build(BuildContext context) {
    final role = context.select<AuthCubit, String>((c) => c.state.session?.role ?? 'unknown');

    return BlocProvider<KillSwitchCubit>(
      create: (_) => getIt<KillSwitchCubit>()..loadFlags(),
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Kill switch'),
          actions: [
            Center(
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 12),
                child: Text(role, style: Theme.of(context).textTheme.labelMedium),
              ),
            ),
            IconButton(
              icon: const Icon(Icons.logout),
              tooltip: 'Sign out',
              onPressed: () => context.read<AuthCubit>().logOut(),
            ),
          ],
        ),
        body: const KillSwitchScreen(),
      ),
    );
  }
}
