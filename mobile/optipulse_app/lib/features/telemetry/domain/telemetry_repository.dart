import 'live_telemetry.dart';

class TelemetryFailure implements Exception {
  const TelemetryFailure(this.message);
  const TelemetryFailure.network() : message = 'Could not reach OptiPulse.';
  final String message;

  @override
  String toString() => 'TelemetryFailure: $message';
}

abstract class TelemetryRepository {
  Future<LiveTelemetry> fetchLive();
}
