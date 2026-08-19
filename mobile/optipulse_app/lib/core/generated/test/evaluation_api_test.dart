import 'package:test/test.dart';
import 'package:openapi/openapi.dart';


/// tests for EvaluationApi
void main() {
  final instance = Openapi().getEvaluationApi();

  group(EvaluationApi, () {
    //Future<EvaluateResponse> evaluate(EvaluateRequest evaluateRequest) async
    test('test evaluate', () async {
      // TODO
    });

    //Future<BatchEvaluateResponse> evaluateBatch(BatchEvaluateRequest batchEvaluateRequest) async
    test('test evaluateBatch', () async {
      // TODO
    });

    //Future<SnapshotVersionResponse> getSnapshotVersion() async
    test('test getSnapshotVersion', () async {
      // TODO
    });

  });
}
