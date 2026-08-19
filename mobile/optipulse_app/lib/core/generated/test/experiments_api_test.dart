import 'package:test/test.dart';
import 'package:openapi/openapi.dart';


/// tests for ExperimentsApi
void main() {
  final instance = Openapi().getExperimentsApi();

  group(ExperimentsApi, () {
    //Future<ExperimentResponse> changeExperimentStatus(String id, ChangeStatusRequest changeStatusRequest) async
    test('test changeExperimentStatus', () async {
      // TODO
    });

    //Future<ExperimentResponse> createExperiment(CreateExperimentRequest createExperimentRequest) async
    test('test createExperiment', () async {
      // TODO
    });

    //Future<ExperimentResponse> getExperiment(String id) async
    test('test getExperiment', () async {
      // TODO
    });

    //Future<BuiltList<ExperimentResponse>> listExperiments({ String flagKey }) async
    test('test listExperiments', () async {
      // TODO
    });

    //Future<ExperimentResponse> updateExperiment(String id, UpdateExperimentRequest updateExperimentRequest) async
    test('test updateExperiment', () async {
      // TODO
    });

  });
}
