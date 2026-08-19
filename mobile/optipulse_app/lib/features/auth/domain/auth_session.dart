import 'package:equatable/equatable.dart';

/// Who the server says you are, and what it gave you to prove it.
///
/// [role] is the role the SERVER reported in the login response — nothing in this app decodes
/// the access token to find out. That distinction matters: a client that reads its own
/// permissions out of a JWT payload is a client deciding what it is allowed to do. Every
/// protected call is still authorised server-side, so the role here only decides which controls
/// are *rendered*, never which are *permitted*.
class AuthSession extends Equatable {
  const AuthSession({
    required this.accessToken,
    required this.refreshToken,
    required this.role,
    required this.expiresAt,
  });

  /// Opaque to this app. Passed as a bearer credential, never parsed.
  final String accessToken;

  final String refreshToken;

  /// Server-reported. Affordances only — see the class comment.
  final String role;

  final DateTime expiresAt;

  bool get isAdmin => role.toLowerCase() == 'admin';

  /// Treated as expired slightly early so a request cannot be sent with a token that dies in
  /// flight. Without the margin, a token with 200ms left passes this check and still comes back
  /// 401 — which works, but spends a wasted round trip to discover something already knowable.
  bool expiresWithin(Duration margin, {required DateTime now}) =>
      !expiresAt.isAfter(now.add(margin));

  AuthSession copyWith({String? accessToken, String? refreshToken, DateTime? expiresAt}) =>
      AuthSession(
        accessToken: accessToken ?? this.accessToken,
        refreshToken: refreshToken ?? this.refreshToken,
        role: role,
        expiresAt: expiresAt ?? this.expiresAt,
      );

  @override
  List<Object?> get props => [accessToken, refreshToken, role, expiresAt];
}
