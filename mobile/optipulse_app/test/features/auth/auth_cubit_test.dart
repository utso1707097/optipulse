import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:optipulse_app/features/auth/domain/auth_failure.dart';
import 'package:optipulse_app/features/auth/domain/auth_repository.dart';
import 'package:optipulse_app/features/auth/domain/auth_session.dart';
import 'package:optipulse_app/features/auth/presentation/auth_cubit.dart';

class _MockAuthRepository extends Mock implements AuthRepository {}

AuthSession _session({String role = 'Admin', Duration ttl = const Duration(hours: 1)}) =>
    AuthSession(
      accessToken: 'access-token',
      refreshToken: 'refresh-token',
      role: role,
      expiresAt: DateTime(2026).add(ttl),
    );

void main() {
  late _MockAuthRepository repository;

  setUp(() {
    repository = _MockAuthRepository();
    registerFallbackValue(_session());
  });

  group('restoreSession', () {
    blocTest<AuthCubit, AuthState>(
      'reports unauthenticated when the keystore holds nothing',
      setUp: () => when(repository.restore).thenAnswer((_) async => null),
      build: () => AuthCubit(repository),
      act: (cubit) => cubit.restoreSession(),
      expect: () => [const AuthState(status: AuthStatus.unauthenticated)],
    );

    blocTest<AuthCubit, AuthState>(
      'restores a stored session without asking for a password',
      setUp: () => when(repository.restore).thenAnswer((_) async => _session()),
      build: () => AuthCubit(repository),
      act: (cubit) => cubit.restoreSession(),
      expect: () => [
        AuthState(status: AuthStatus.authenticated, session: _session()),
      ],
    );

    test('starts in unknown, never unauthenticated', () {
      // Guards the cold-start flash: rendering the login form before the keystore has been read
      // would sign-in-prompt a user who is already signed in.
      expect(AuthCubit(repository).state.status, AuthStatus.unknown);
    });
  });

  group('logIn', () {
    blocTest<AuthCubit, AuthState>(
      'emits submitting, then authenticated',
      setUp: () => when(
        () => repository.logIn(email: any(named: 'email'), password: any(named: 'password')),
      ).thenAnswer((_) async => _session()),
      build: () => AuthCubit(repository),
      act: (cubit) => cubit.logIn(email: 'admin@optipulse.dev', password: 'correct'),
      expect: () => [
        const AuthState(isSubmitting: true),
        AuthState(status: AuthStatus.authenticated, session: _session()),
      ],
    );

    blocTest<AuthCubit, AuthState>(
      'surfaces the failure and holds no session on bad credentials',
      setUp: () => when(
        () => repository.logIn(email: any(named: 'email'), password: any(named: 'password')),
      ).thenThrow(const AuthFailure.invalidCredentials()),
      build: () => AuthCubit(repository),
      act: (cubit) => cubit.logIn(email: 'admin@optipulse.dev', password: 'wrong'),
      expect: () => [
        const AuthState(isSubmitting: true),
        const AuthState(
          status: AuthStatus.unauthenticated,
          failure: AuthFailure.invalidCredentials(),
        ),
      ],
      verify: (cubit) => expect(cubit.state.session, isNull),
    );
  });

  group('logOut', () {
    blocTest<AuthCubit, AuthState>(
      'clears local state before the network call, and regardless of it',
      setUp: () {
        when(repository.restore).thenAnswer((_) async => _session());
        // A server that refuses the revocation must not keep the user signed in on the device.
        when(() => repository.logOut(any())).thenThrow(const AuthFailure.network());
      },
      build: () => AuthCubit(repository),
      seed: () => AuthState(status: AuthStatus.authenticated, session: _session()),
      act: (cubit) async {
        try {
          await cubit.logOut();
        } on AuthFailure {
          // The repository's contract is to clear locally even on failure; this test only
          // asserts the Cubit does not retain a session either way.
        }
      },
      expect: () => [const AuthState(status: AuthStatus.unauthenticated)],
    );
  });

  group('role', () {
    test('isAdmin reflects the role the SERVER reported', () {
      final cubit = AuthCubit(repository)
        ..emit(AuthState(status: AuthStatus.authenticated, session: _session(role: 'Admin')));
      expect(cubit.isAdminForTest, isTrue);
    });

    test('a non-admin role does not grant admin affordances', () {
      final cubit = AuthCubit(repository)
        ..emit(AuthState(status: AuthStatus.authenticated, session: _session(role: 'Manager')));
      expect(cubit.isAdminForTest, isFalse);
    });
  });

  group('session lifecycle callbacks', () {
    test('onSessionRefreshed replaces the session in place', () {
      final cubit = AuthCubit(repository)
        ..emit(AuthState(status: AuthStatus.authenticated, session: _session()));
      final rotated = _session().copyWith(accessToken: 'rotated', refreshToken: 'rotated-r');

      cubit.onSessionRefreshed(rotated);

      expect(cubit.state.session?.accessToken, 'rotated');
      expect(cubit.state.status, AuthStatus.authenticated);
    });

    test('onSessionRefreshed is ignored once signed out', () {
      // A refresh that resolves after the user signed out must not resurrect the session.
      final cubit = AuthCubit(repository)
        ..emit(const AuthState(status: AuthStatus.unauthenticated));

      cubit.onSessionRefreshed(_session());

      expect(cubit.state.status, AuthStatus.unauthenticated);
      expect(cubit.state.session, isNull);
    });

    test('onSessionLost signs out with an explanation', () {
      final cubit = AuthCubit(repository)
        ..emit(AuthState(status: AuthStatus.authenticated, session: _session()));

      cubit.onSessionLost();

      expect(cubit.state.status, AuthStatus.unauthenticated);
      expect(cubit.state.failure?.kind, AuthFailureKind.sessionExpired);
    });
  });
}

extension on AuthCubit {
  bool get isAdminForTest => state.isAdmin;
}
