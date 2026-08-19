//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:openapi/src/model/rollout_dto.dart';
import 'package:built_collection/built_collection.dart';
import 'package:openapi/src/model/targeting_rule_dto.dart';
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'update_flag_request.g.dart';

/// UpdateFlagRequest
///
/// Properties:
/// * [defaultOutcome] 
/// * [name] 
/// * [rollout] 
/// * [targetingRules] 
@BuiltValue()
abstract class UpdateFlagRequest implements Built<UpdateFlagRequest, UpdateFlagRequestBuilder> {
  @BuiltValueField(wireName: r'defaultOutcome')
  bool get defaultOutcome;

  @BuiltValueField(wireName: r'name')
  String get name;

  @BuiltValueField(wireName: r'rollout')
  RolloutDto? get rollout;

  @BuiltValueField(wireName: r'targetingRules')
  BuiltList<TargetingRuleDto>? get targetingRules;

  UpdateFlagRequest._();

  factory UpdateFlagRequest([void updates(UpdateFlagRequestBuilder b)]) = _$UpdateFlagRequest;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(UpdateFlagRequestBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<UpdateFlagRequest> get serializer => _$UpdateFlagRequestSerializer();
}

class _$UpdateFlagRequestSerializer implements PrimitiveSerializer<UpdateFlagRequest> {
  @override
  final Iterable<Type> types = const [UpdateFlagRequest, _$UpdateFlagRequest];

  @override
  final String wireName = r'UpdateFlagRequest';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    UpdateFlagRequest object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'defaultOutcome';
    yield serializers.serialize(
      object.defaultOutcome,
      specifiedType: const FullType(bool),
    );
    yield r'name';
    yield serializers.serialize(
      object.name,
      specifiedType: const FullType(String),
    );
    yield r'rollout';
    yield object.rollout == null ? null : serializers.serialize(
      object.rollout,
      specifiedType: const FullType.nullable(RolloutDto),
    );
    yield r'targetingRules';
    yield object.targetingRules == null ? null : serializers.serialize(
      object.targetingRules,
      specifiedType: const FullType.nullable(BuiltList, [FullType(TargetingRuleDto)]),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    UpdateFlagRequest object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required UpdateFlagRequestBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'defaultOutcome':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(bool),
          ) as bool;
          result.defaultOutcome = valueDes;
          break;
        case r'name':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.name = valueDes;
          break;
        case r'rollout':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType.nullable(RolloutDto),
          ) as RolloutDto?;
          if (valueDes == null) continue;
          result.rollout.replace(valueDes);
          break;
        case r'targetingRules':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType.nullable(BuiltList, [FullType(TargetingRuleDto)]),
          ) as BuiltList<TargetingRuleDto>?;
          if (valueDes == null) continue;
          result.targetingRules.replace(valueDes);
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  UpdateFlagRequest deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = UpdateFlagRequestBuilder();
    final serializedList = (serialized as Iterable<Object?>).toList();
    final unhandled = <Object?>[];
    _deserializeProperties(
      serializers,
      serialized,
      specifiedType: specifiedType,
      serializedList: serializedList,
      unhandled: unhandled,
      result: result,
    );
    return result.build();
  }
}

