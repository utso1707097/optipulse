//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_import

import 'package:one_of_serializer/any_of_serializer.dart';
import 'package:one_of_serializer/one_of_serializer.dart';
import 'package:built_collection/built_collection.dart';
import 'package:built_value/json_object.dart';
import 'package:built_value/serializer.dart';
import 'package:built_value/standard_json_plugin.dart';
import 'package:built_value/iso_8601_date_time_serializer.dart';
import 'package:openapi/src/date_serializer.dart';
import 'package:openapi/src/model/date.dart';

import 'package:openapi/src/model/alert_response.dart';
import 'package:openapi/src/model/batch_evaluate_request.dart';
import 'package:openapi/src/model/batch_evaluate_response.dart';
import 'package:openapi/src/model/change_status_request.dart';
import 'package:openapi/src/model/conversion_request.dart';
import 'package:openapi/src/model/conversion_response.dart';
import 'package:openapi/src/model/create_experiment_request.dart';
import 'package:openapi/src/model/create_flag_request.dart';
import 'package:openapi/src/model/evaluate_request.dart';
import 'package:openapi/src/model/evaluate_response.dart';
import 'package:openapi/src/model/experiment_response.dart';
import 'package:openapi/src/model/flag_exposure_response.dart';
import 'package:openapi/src/model/flag_response.dart';
import 'package:openapi/src/model/kill_switch_request.dart';
import 'package:openapi/src/model/live_telemetry_response.dart';
import 'package:openapi/src/model/login_request.dart';
import 'package:openapi/src/model/login_response.dart';
import 'package:openapi/src/model/me_response.dart';
import 'package:openapi/src/model/problem_details.dart';
import 'package:openapi/src/model/refresh_request.dart';
import 'package:openapi/src/model/register_device_request.dart';
import 'package:openapi/src/model/register_device_response.dart';
import 'package:openapi/src/model/rollout_dto.dart';
import 'package:openapi/src/model/snapshot_version_response.dart';
import 'package:openapi/src/model/targeting_rule_dto.dart';
import 'package:openapi/src/model/update_experiment_request.dart';
import 'package:openapi/src/model/update_flag_request.dart';
import 'package:openapi/src/model/variant_dto.dart';
import 'package:openapi/src/model/variant_exposure_dto.dart';

part 'serializers.g.dart';

@SerializersFor([
  AlertResponse,
  BatchEvaluateRequest,
  BatchEvaluateResponse,
  ChangeStatusRequest,
  ConversionRequest,
  ConversionResponse,
  CreateExperimentRequest,
  CreateFlagRequest,
  EvaluateRequest,
  EvaluateResponse,
  ExperimentResponse,
  FlagExposureResponse,
  FlagResponse,
  KillSwitchRequest,
  LiveTelemetryResponse,
  LoginRequest,
  LoginResponse,
  MeResponse,
  ProblemDetails,
  RefreshRequest,
  RegisterDeviceRequest,
  RegisterDeviceResponse,
  RolloutDto,
  SnapshotVersionResponse,
  TargetingRuleDto,
  UpdateExperimentRequest,
  UpdateFlagRequest,
  VariantDto,
  VariantExposureDto,
])
Serializers serializers = (_$serializers.toBuilder()
      ..addBuilderFactory(
        const FullType(BuiltMap, [FullType(String), FullType(String)]),
        () => MapBuilder<String, String>(),
      )
      ..addBuilderFactory(
        const FullType(BuiltList, [FullType(EvaluateResponse)]),
        () => ListBuilder<EvaluateResponse>(),
      )
      ..addBuilderFactory(
        const FullType(BuiltList, [FullType(AlertResponse)]),
        () => ListBuilder<AlertResponse>(),
      )
      ..addBuilderFactory(
        const FullType(BuiltList, [FullType(ExperimentResponse)]),
        () => ListBuilder<ExperimentResponse>(),
      )
      ..addBuilderFactory(
        const FullType(BuiltList, [FullType(VariantDto)]),
        () => ListBuilder<VariantDto>(),
      )
      ..addBuilderFactory(
        const FullType(BuiltList, [FullType(String)]),
        () => ListBuilder<String>(),
      )
      ..addBuilderFactory(
        const FullType(BuiltList, [FullType(TargetingRuleDto)]),
        () => ListBuilder<TargetingRuleDto>(),
      )
      ..addBuilderFactory(
        const FullType(BuiltList, [FullType(VariantExposureDto)]),
        () => ListBuilder<VariantExposureDto>(),
      )
      ..addBuilderFactory(
        const FullType(BuiltList, [FullType(FlagResponse)]),
        () => ListBuilder<FlagResponse>(),
      )
      ..add(const OneOfSerializer())
      ..add(const AnyOfSerializer())
      ..add(const DateSerializer())
      ..add(Iso8601DateTimeSerializer())
    ).build();

Serializers standardSerializers =
    (serializers.toBuilder()..addPlugin(StandardJsonPlugin())).build();
