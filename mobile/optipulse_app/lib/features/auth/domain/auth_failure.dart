import 'package:equatable/equatable.dart';

/// Why a sign-in or refresh did not succeed.
///
/// Deliberately coarse. [invalidCredentials] does not distinguish "no such user" from "wrong
/// password", because the API does not either — telling them apart would turn the login form
/// into an account-enumeration oracle.
enum AuthFailureKind {
  invalidCredentials,
  network,
  sessionExpired,
  server,
}

class AuthFailure extends Equatable implements Exception {
  const AuthFailure(this.kind, this.message);

  const AuthFailure.invalidCredentials()
      : kind = AuthFailureKind.invalidCredentials,
        message = 'Email or password is incorrect.';

  const AuthFailure.network()
      : kind = AuthFailureKind.network,
        message = 'Could not reach OptiPulse. Check your connection and try again.';

  const AuthFailure.sessionExpired()
      : kind = AuthFailureKind.sessionExpired,
        message = 'Your session expired. Please sign in again.';

  final AuthFailureKind kind;
  final String message;

  @override
  List<Object?> get props => [kind, message];

  @override
  String toString() => 'AuthFailure($kind): $message';
}
