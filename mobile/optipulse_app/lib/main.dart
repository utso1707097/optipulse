import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'core/di/injection.dart';
import 'features/auth/presentation/auth_cubit.dart';
import 'features/auth/presentation/login_screen.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
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
            return const _HomePlaceholder();
        }
      },
    );
  }
}

/// Stands in until the telemetry, alerts and kill-switch features land (T074-T077). It shows
/// the signed-in identity so the auth path can be verified end to end rather than assumed.
class _HomePlaceholder extends StatelessWidget {
  const _HomePlaceholder();

  @override
  Widget build(BuildContext context) {
    final state = context.watch<AuthCubit>().state;
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('OptiPulse'),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            tooltip: 'Sign out',
            onPressed: () => context.read<AuthCubit>().logOut(),
          ),
        ],
      ),
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(Icons.check_circle, size: 64, color: theme.colorScheme.primary),
              const SizedBox(height: 16),
              Text('Signed in', style: theme.textTheme.headlineSmall),
              const SizedBox(height: 8),
              Text(
                'Role: ${state.session?.role ?? 'unknown'}',
                style: theme.textTheme.bodyLarge,
              ),
              const SizedBox(height: 24),
              Text(
                state.isAdmin
                    ? 'Kill-switch controls will appear here (T076).'
                    : 'Telemetry will appear here (T074). Kill-switch is Admin-only.',
                textAlign: TextAlign.center,
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
