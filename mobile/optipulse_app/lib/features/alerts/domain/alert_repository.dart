import 'alert.dart';

class AlertFailure implements Exception {
  const AlertFailure(this.message);
  const AlertFailure.network() : message = 'Could not reach OptiPulse.';
  final String message;

  @override
  String toString() => 'AlertFailure: $message';
}

abstract class AlertRepository {
  Future<List<Alert>> list({bool unacknowledgedOnly = false, int limit = 50});

  Future<Alert> acknowledge(String alertId);

  /// Registers this device for push. Called with a token obtained from the platform's push
  /// service; see PushRegistrar for why it is separated from token acquisition.
  Future<void> registerDevice({required String platform, required String token});
}
