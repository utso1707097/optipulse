import 'package:dio/dio.dart';
import 'package:openapi/openapi.dart' as api;

import '../domain/live_telemetry.dart';
import '../domain/telemetry_repository.dart';

class TelemetryRepositoryImpl implements TelemetryRepository {
  TelemetryRepositoryImpl(this._api, {DateTime Function()? now})
      : _now = now ?? DateTime.now;

  final api.TelemetryApi _api;
  final DateTime Function() _now;

  @override
  Future<LiveTelemetry> fetchLive() async {
    try {
      final response = await _api.getLiveTelemetry();
      final data = response.data!;

      return LiveTelemetry(
        snapshotVersion: data.snapshotVersion,
        snapshotBuiltAt: data.snapshotBuiltAt,
        snapshotAgeSeconds: data.snapshotAgeSeconds,
        activeFlags: data.activeFlags,
        killSwitchesEngaged: data.killSwitchesEngaged,
        serverTime: data.serverTime,
        // Stamped on the DEVICE, so a cached reading can be labelled with when it was taken
        // rather than silently presented as current.
        observedAt: _now(),
      );
    } on DioException catch (error) {
      throw TelemetryFailure(
        error.response?.statusCode == 403
            ? 'Live telemetry is Admin-only.'
            : 'Could not reach OptiPulse.',
      );
    }
  }
}
