import '../domain/alert_repository.dart';

/// Obtains a push token from the platform.
///
/// <b>Deliberately empty by default, and this is the seam FCM plugs into.</b> The backend treats
/// push as a delivery optimisation over a durable history that is already the source of truth,
/// so an app with no push provider is fully functional: alerts arrive in the history and the
/// alerts screen reads them. Adding firebase_messaging means implementing this one interface and
/// registering it — no other file changes.
///
/// It is separated from [AlertRepository] because acquiring a token and registering it are
/// different concerns with different failure modes: acquisition depends on OS permission the
/// user can refuse, registration depends on the network.
abstract class PushRegistrar {
  /// Returns null when no push provider is configured, or when the user declined the OS prompt.
  /// A null token is an ordinary outcome, not an error — see the class comment.
  Future<String?> obtainToken();

  /// 'Ios' or 'Android', matching the API's DevicePlatform.
  String get platform;
}

/// The default: no push provider configured.
class NoPushRegistrar implements PushRegistrar {
  const NoPushRegistrar();

  @override
  Future<String?> obtainToken() async => null;

  @override
  String get platform => 'Ios';
}

/// Registers this device for push if a token is available.
///
/// Failure is swallowed on purpose. Push registration is an enhancement; an app that refused to
/// open because it could not register for notifications would be trading the feature that works
/// for the one that does not.
class PushRegistration {
  const PushRegistration(this._registrar, this._repository);

  final PushRegistrar _registrar;
  final AlertRepository _repository;

  Future<bool> registerIfAvailable() async {
    try {
      final token = await _registrar.obtainToken();
      if (token == null) return false;
      await _repository.registerDevice(platform: _registrar.platform, token: token);
      return true;
    } on Exception {
      return false;
    }
  }
}
