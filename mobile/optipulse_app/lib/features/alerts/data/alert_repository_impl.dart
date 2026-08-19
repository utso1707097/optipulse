import 'package:dio/dio.dart';
import 'package:openapi/openapi.dart' as api;

import '../domain/alert.dart';
import '../domain/alert_repository.dart';

class AlertRepositoryImpl implements AlertRepository {
  AlertRepositoryImpl(this._api);

  final api.AlertsApi _api;

  @override
  Future<List<Alert>> list({bool unacknowledgedOnly = false, int limit = 50}) async {
    try {
      final response = await _api.listAlerts(unacknowledgedOnly: unacknowledgedOnly, limit: limit);
      return (response.data ?? const <api.AlertResponse>[]).map(_toAlert).toList();
    } on DioException catch (error) {
      throw _translate(error);
    }
  }

  @override
  Future<Alert> acknowledge(String alertId) async {
    try {
      final response = await _api.acknowledgeAlert(id: alertId);
      return _toAlert(response.data!);
    } on DioException catch (error) {
      throw _translate(error);
    }
  }

  @override
  Future<void> registerDevice({required String platform, required String token}) async {
    try {
      await _api.registerPushDevice(
        registerDeviceRequest: api.RegisterDeviceRequest(
          (b) => b
            ..platform = platform
            ..token = token,
        ),
      );
    } on DioException catch (error) {
      throw _translate(error);
    }
  }

  Alert _toAlert(api.AlertResponse response) => Alert(
        id: response.id,
        raisedAt: response.raisedAt,
        kind: response.kind,
        severity: Alert.parseSeverity(response.severity),
        title: response.title,
        detail: response.detail,
        flagKey: response.flagKey,
        acknowledgedAt: response.acknowledgedAt,
        acknowledgedBy: response.acknowledgedBy,
      );

  AlertFailure _translate(DioException error) => switch (error.response?.statusCode) {
        403 => const AlertFailure('Alerts are Admin-only.'),
        404 => const AlertFailure('That alert no longer exists.'),
        _ => const AlertFailure.network(),
      };
}
