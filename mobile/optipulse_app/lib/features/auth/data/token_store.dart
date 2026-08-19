import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../domain/auth_session.dart';

/// Persists the session in the platform keystore — Keychain on iOS, EncryptedSharedPreferences
/// on Android.
///
/// This is the one place the mobile app can do better than the dashboard. The web client has to
/// keep its refresh token in localStorage, readable by any script on the origin, because the API
/// returns tokens in the response body and an HttpOnly cookie would need SameSite=None plus CSRF
/// defences the backend does not implement. On mobile there is no such constraint: the OS gives
/// us storage that other apps cannot read and that survives reinstall policy correctly.
///
/// The ACCESS token is stored too, unlike on web where it is memory-only. The reasoning differs
/// because the threat differs: a phone is backgrounded and killed constantly, and re-entering a
/// password every time the OS reclaims memory is the kind of friction that gets an ops app
/// uninstalled. The token is short-lived and the store is hardware-backed.
class TokenStore {
  TokenStore(this._storage);

  final FlutterSecureStorage _storage;

  static const _accessKey = 'optipulse.accessToken';
  static const _refreshKey = 'optipulse.refreshToken';
  static const _roleKey = 'optipulse.role';
  static const _expiresKey = 'optipulse.expiresAt';

  Future<void> write(AuthSession session) async {
    await Future.wait([
      _storage.write(key: _accessKey, value: session.accessToken),
      _storage.write(key: _refreshKey, value: session.refreshToken),
      _storage.write(key: _roleKey, value: session.role),
      _storage.write(
        key: _expiresKey,
        value: session.expiresAt.toIso8601String(),
      ),
    ]);
  }

  Future<AuthSession?> read() async {
    final accessToken = await _storage.read(key: _accessKey);
    final refreshToken = await _storage.read(key: _refreshKey);
    final role = await _storage.read(key: _roleKey);
    final expiresAt = await _storage.read(key: _expiresKey);

    // Any missing part means the stored session is not usable. Partial state is treated as no
    // state rather than reconstructed with defaults — guessing a role would be inventing an
    // authorization claim the server never made.
    if (accessToken == null || refreshToken == null || role == null || expiresAt == null) {
      return null;
    }

    final parsedExpiry = DateTime.tryParse(expiresAt);
    if (parsedExpiry == null) return null;

    return AuthSession(
      accessToken: accessToken,
      refreshToken: refreshToken,
      role: role,
      expiresAt: parsedExpiry,
    );
  }

  Future<void> clear() async {
    await Future.wait([
      _storage.delete(key: _accessKey),
      _storage.delete(key: _refreshKey),
      _storage.delete(key: _roleKey),
      _storage.delete(key: _expiresKey),
    ]);
  }
}
