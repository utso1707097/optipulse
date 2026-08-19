import 'package:test/test.dart';
import 'package:openapi/openapi.dart';


/// tests for FlagsApi
void main() {
  final instance = Openapi().getFlagsApi();

  group(FlagsApi, () {
    //Future<FlagResponse> changeFlagStatus(String key, ChangeStatusRequest changeStatusRequest) async
    test('test changeFlagStatus', () async {
      // TODO
    });

    //Future<FlagResponse> createFlag(CreateFlagRequest createFlagRequest) async
    test('test createFlag', () async {
      // TODO
    });

    //Future<FlagResponse> getFlag(String key) async
    test('test getFlag', () async {
      // TODO
    });

    //Future<BuiltList<FlagResponse>> listFlags() async
    test('test listFlags', () async {
      // TODO
    });

    //Future<FlagResponse> setKillSwitch(String key, KillSwitchRequest killSwitchRequest) async
    test('test setKillSwitch', () async {
      // TODO
    });

    //Future<FlagResponse> updateFlag(String key, UpdateFlagRequest updateFlagRequest) async
    test('test updateFlag', () async {
      // TODO
    });

  });
}
