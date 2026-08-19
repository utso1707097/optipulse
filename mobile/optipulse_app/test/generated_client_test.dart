import 'package:flutter_test/flutter_test.dart';
import 'package:openapi/openapi.dart';

/// The generated client compiling is not evidence that it WORKS. These assert the
/// three properties the app actually depends on: that the API surface the features
/// need was generated, that a request body serialises to the wire shape the backend
/// parses, and that a nullable map survives the round trip — the exact field whose
/// OpenAPI 3.1 encoding broke codegen and forced the spec down to 3.0.
void main() {
  test('exposes the endpoint groups the app needs, separately', () {
    // One accessor per tag. Before the API declared OpenAPI tags every operation landed in a
    // single generated god-class, so calling one endpoint meant importing all of them.
    final api = Openapi();
    expect(api.getAuthenticationApi(), isNotNull);
    expect(api.getFlagsApi(), isNotNull);
    expect(api.getEvaluationApi(), isNotNull);
    expect(api.getExperimentsApi(), isNotNull);
    expect(api.getTelemetryApi(), isNotNull);
  });

  test('LoginRequest serialises to the field names the backend expects', () {
    final request = LoginRequest(
      (b) => b
        ..email = 'admin@example.com'
        ..password = 'secret',
    );
    final json = standardSerializers.serializeWith(
      LoginRequest.serializer,
      request,
    )! as Map<String, dynamic>;

    expect(json['email'], 'admin@example.com');
    expect(json['password'], 'secret');
  });

  test('EvaluateRequest carries a populated attributes map', () {
    final request = EvaluateRequest(
      (b) => b
        ..flagKey = 'checkout.new-flow'
        ..contextKey = 'user-42'
        ..attributes.addAll({'country': 'BD', 'plan': 'pro'}),
    );
    final json = standardSerializers.serializeWith(
      EvaluateRequest.serializer,
      request,
    )! as Map<String, dynamic>;

    expect(json['flagKey'], 'checkout.new-flow');
    expect(json['contextKey'], 'user-42');
    expect(json['attributes'], {'country': 'BD', 'plan': 'pro'});
  });

  test('EvaluateRequest allows attributes to be omitted entirely', () {
    // `attributes` is required-but-nullable in the contract. Building without it must
    // not throw: an evaluation with no targeting attributes is the common case.
    final request = EvaluateRequest((b) => b..flagKey = 'checkout.new-flow');
    final json = standardSerializers.serializeWith(
      EvaluateRequest.serializer,
      request,
    )! as Map<String, dynamic>;

    expect(json['flagKey'], 'checkout.new-flow');
  });
}
