import 'package:test/test.dart';
import 'package:openapi/openapi.dart';


/// tests for AlertsApi
void main() {
  final instance = Openapi().getAlertsApi();

  group(AlertsApi, () {
    //Future<AlertResponse> acknowledgeAlert(String id) async
    test('test acknowledgeAlert', () async {
      // TODO
    });

    //Future<BuiltList<AlertResponse>> listAlerts({ bool unacknowledgedOnly, int limit }) async
    test('test listAlerts', () async {
      // TODO
    });

    //Future<RegisterDeviceResponse> registerPushDevice(RegisterDeviceRequest registerDeviceRequest) async
    test('test registerPushDevice', () async {
      // TODO
    });

    //Future revokePushDevice(RegisterDeviceRequest registerDeviceRequest) async
    test('test revokePushDevice', () async {
      // TODO
    });

  });
}
