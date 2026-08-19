import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../domain/auth_failure.dart';
import '../domain/auth_repository.dart';
import '../domain/auth_session.dart';

enum AuthStatus { unknown, authenticated, unauthenticated }

class AuthState extends Equatable {
  const AuthState({
    this.status = AuthStatus.unknown,
    this.session,
    this.failure,
    this.isSubmitting = false,
  });

  /// Starts [AuthStatus.unknown], not `unauthenticated`. The difference is visible to the user:
  /// "we have not looked in the keystore yet" must not render the login screen, or every cold
  /// start flashes a sign-in form at someone who is already signed in.
  final AuthStatus status;
  final AuthSession? session;
  final AuthFailure? failure;
  final bool isSubmitting;

  bool get isAdmin => session?.isAdmin ?? false;

  AuthState copyWith({
    AuthStatus? status,
    AuthSession? session,
    AuthFailure? failure,
    bool? isSubmitting,
    bool clearFailure = false,
    bool clearSession = false,
  }) =>
      AuthState(
        status: status ?? this.status,
        session: clearSession ? null : (session ?? this.session),
        failure: clearFailure ? null : (failure ?? this.failure),
        isSubmitting: isSubmitting ?? this.isSubmitting,
      );

  @override
  List<Object?> get props => [status, session, failure, isSubmitting];
}

/// Owns the signed-in/signed-out question for the whole app.
///
/// It does NOT own token refresh. That happens in the Dio interceptor, because a token can
/// expire during any request from any screen, and routing every call through here would mean
/// the presentation layer scheduling transport concerns. The Cubit is told after the fact via
/// [onSessionRefreshed] / [onSessionLost], which keeps one source of truth for the session
/// without putting the Cubit in the request path.
class AuthCubit extends Cubit<AuthState> {
  AuthCubit(this._repository) : super(const AuthState());

  final AuthRepository _repository;

  Future<void> restoreSession() async {
    final session = await _repository.restore();
    emit(
      session == null
          ? const AuthState(status: AuthStatus.unauthenticated)
          : AuthState(status: AuthStatus.authenticated, session: session),
    );
  }

  Future<void> logIn({required String email, required String password}) async {
    emit(state.copyWith(isSubmitting: true, clearFailure: true));
    try {
      final session = await _repository.logIn(email: email, password: password);
      emit(AuthState(status: AuthStatus.authenticated, session: session));
    } on AuthFailure catch (failure) {
      emit(
        AuthState(
          status: AuthStatus.unauthenticated,
          failure: failure,
        ),
      );
    }
  }

  Future<void> logOut() async {
    final session = state.session;
    emit(const AuthState(status: AuthStatus.unauthenticated));
    await _repository.logOut(session);
  }

  /// The interceptor obtained a new token pair. Kept in sync so the UI's notion of the session
  /// does not silently diverge from the one actually being sent on the wire.
  void onSessionRefreshed(AuthSession session) {
    if (state.status != AuthStatus.authenticated) return;
    emit(state.copyWith(session: session));
  }

  /// The refresh token was rejected. This is terminal — the backend revokes a whole token
  /// family on reuse, so there is nothing left to retry with.
  void onSessionLost() {
    emit(const AuthState(
      status: AuthStatus.unauthenticated,
      failure: AuthFailure.sessionExpired(),
    ));
  }
}
