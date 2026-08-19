import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:hydrated_bloc/hydrated_bloc.dart';
import 'package:path_provider/path_provider.dart';

import 'core/di/injection.dart';
import 'features/auth/presentation/auth_cubit.dart';
import 'features/auth/presentation/login_screen.dart';
import 'features/alerts/data/push_registrar.dart';
import 'features/alerts/presentation/alerts_cubit.dart';
import 'features/alerts/presentation/alerts_screen.dart';
import 'features/killswitch/presentation/kill_switch_cubit.dart';
import 'features/killswitch/presentation/kill_switch_screen.dart';
import 'features/telemetry/presentation/telemetry_cubit.dart';
import 'features/telemetry/presentation/telemetry_screen.dart';

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

/// The signed-in shell: telemetry, alerts and the kill switch.
class _Home extends StatefulWidget {
  const _Home();

  @override
  State<_Home> createState() => _HomeState();
}

class _HomeState extends State<_Home> {
  int _tab = 0;

  @override
  void initState() {
    super.initState();
    // Fire-and-forget: registration is an enhancement, and an app that blocked on it would be
    // trading the features that work for the one that does not. With no push provider
    // configured this resolves to false immediately.
    unawaited(getIt<PushRegistration>().registerIfAvailable());
  }

  @override
  Widget build(BuildContext context) {
    final role = context.select<AuthCubit, String>((c) => c.state.session?.role ?? 'unknown');

    return MultiBlocProvider(
      providers: [
        BlocProvider<TelemetryCubit>(create: (_) => getIt<TelemetryCubit>()),
        BlocProvider<AlertsCubit>(create: (_) => getIt<AlertsCubit>()..load()),
        BlocProvider<KillSwitchCubit>(create: (_) => getIt<KillSwitchCubit>()..loadFlags()),
      ],
      child: Scaffold(
        appBar: AppBar(
          title: Text(switch (_tab) {
            0 => 'Telemetry',
            1 => 'Alerts',
            _ => 'Kill switch',
          }),
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
        body: IndexedStack(
          index: _tab,
          children: const [TelemetryScreen(), AlertsScreen(), KillSwitchScreen()],
        ),
        bottomNavigationBar: BlocBuilder<AlertsCubit, AlertsState>(
          builder: (context, alerts) => NavigationBar(
            selectedIndex: _tab,
            onDestinationSelected: (index) => setState(() => _tab = index),
            destinations: [
              const NavigationDestination(
                icon: Icon(Icons.monitor_heart_outlined),
                selectedIcon: Icon(Icons.monitor_heart),
                label: 'Telemetry',
              ),
              NavigationDestination(
                // The badge counts UNACKNOWLEDGED alerts, so it clears by responding rather than
                // by opening the tab — an ops badge that disappears on a glance stops meaning
                // "something still needs attention".
                icon: Badge(
                  isLabelVisible: alerts.unacknowledgedCount > 0,
                  label: Text('${alerts.unacknowledgedCount}'),
                  child: const Icon(Icons.notifications_outlined),
                ),
                selectedIcon: const Icon(Icons.notifications),
                label: 'Alerts',
              ),
              const NavigationDestination(
                icon: Icon(Icons.block_outlined),
                selectedIcon: Icon(Icons.block),
                label: 'Kill switch',
              ),
            ],
          ),
        ),
      ),
    );
  }
}
