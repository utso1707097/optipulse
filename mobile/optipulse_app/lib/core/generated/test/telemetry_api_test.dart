import 'package:test/test.dart';
import 'package:openapi/openapi.dart';


/// tests for TelemetryApi
void main() {
  final instance = Openapi().getTelemetryApi();

  group(TelemetryApi, () {
    //Future<FlagExposureResponse> getFlagExposures(String key) async
    test('test getFlagExposures', () async {
      // TODO
    });

    //Future<ConversionResponse> recordConversion(ConversionRequest conversionRequest) async
    test('test recordConversion', () async {
      // TODO
    });

  });
}
