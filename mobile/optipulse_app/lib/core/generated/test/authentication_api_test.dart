import 'package:test/test.dart';
import 'package:openapi/openapi.dart';


/// tests for AuthenticationApi
void main() {
  final instance = Openapi().getAuthenticationApi();

  group(AuthenticationApi, () {
    //Future<LoginResponse> login(LoginRequest loginRequest) async
    test('test login', () async {
      // TODO
    });

    //Future logout(RefreshRequest refreshRequest) async
    test('test logout', () async {
      // TODO
    });

    //Future<MeResponse> me() async
    test('test me', () async {
      // TODO
    });

    //Future<LoginResponse> refresh(RefreshRequest refreshRequest) async
    test('test refresh', () async {
      // TODO
    });

  });
}
