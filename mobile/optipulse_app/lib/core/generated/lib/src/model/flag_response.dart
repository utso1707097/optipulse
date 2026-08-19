//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:openapi/src/model/rollout_dto.dart';
import 'package:built_collection/built_collection.dart';
import 'package:openapi/src/model/targeting_rule_dto.dart';
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'flag_response.g.dart';

/// FlagResponse
///
/// Properties:
/// * [createdAt] 
/// * [defaultOutcome] 
/// * [id] 
/// * [key] 
/// * [killSwitchEngaged] 
/// * [name] 
/// * [rollout] 
/// * [status] 
/// * [targetingRules] 
/// * [updatedAt] 
/// * [version] 
@BuiltValue()
abstract class FlagResponse implements Built<FlagResponse, FlagResponseBuilder> {
  @BuiltValueField(wireName: r'createdAt')
  DateTime get createdAt;

  @BuiltValueField(wireName: r'defaultOutcome')
  bool get defaultOutcome;

  @BuiltValueField(wireName: r'id')
  String get id;

  @BuiltValueField(wireName: r'key')
  String get key;

  @BuiltValueField(wireName: r'killSwitchEngaged')
  bool get killSwitchEngaged;

  @BuiltValueField(wireName: r'name')
  String get name;

  @BuiltValueField(wireName: r'rollout')
  RolloutDto? get rollout;

  @BuiltValueField(wireName: r'status')
  String get status;

  @BuiltValueField(wireName: r'targetingRules')
  BuiltList<TargetingRuleDto> get targetingRules;

  @BuiltValueField(wireName: r'updatedAt')
  DateTime get updatedAt;

  @BuiltValueField(wireName: r'version')
  int get version;

  FlagResponse._();

  factory FlagResponse([void updates(FlagResponseBuilder b)]) = _$FlagResponse;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(FlagResponseBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<FlagResponse> get serializer => _$FlagResponseSerializer();
}

class _$FlagResponseSerializer implements PrimitiveSerializer<FlagResponse> {
  @override
  final Iterable<Type> types = const [FlagResponse, _$FlagResponse];

  @override
  final String wireName = r'FlagResponse';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    FlagResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'createdAt';
    yield serializers.serialize(
      object.createdAt,
      specifiedType: const FullType(DateTime),
    );
    yield r'defaultOutcome';
    yield serializers.serialize(
      object.defaultOutcome,
      specifiedType: const FullType(bool),
    );
    yield r'id';
    yield serializers.serialize(
      object.id,
      specifiedType: const FullType(String),
    );
    yield r'key';
    yield serializers.serialize(
      object.key,
      specifiedType: const FullType(String),
    );
    yield r'killSwitchEngaged';
    yield serializers.serialize(
      object.killSwitchEngaged,
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
    yield r'status';
    yield serializers.serialize(
      object.status,
      specifiedType: const FullType(String),
    );
    yield r'targetingRules';
    yield serializers.serialize(
      object.targetingRules,
      specifiedType: const FullType(BuiltList, [FullType(TargetingRuleDto)]),
    );
    yield r'updatedAt';
    yield serializers.serialize(
      object.updatedAt,
      specifiedType: const FullType(DateTime),
    );
    yield r'version';
    yield serializers.serialize(
      object.version,
      specifiedType: const FullType(int),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    FlagResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required FlagResponseBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'createdAt':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(DateTime),
          ) as DateTime;
          result.createdAt = valueDes;
          break;
        case r'defaultOutcome':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(bool),
          ) as bool;
          result.defaultOutcome = valueDes;
          break;
        case r'id':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.id = valueDes;
          break;
        case r'key':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.key = valueDes;
          break;
        case r'killSwitchEngaged':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(bool),
          ) as bool;
          result.killSwitchEngaged = valueDes;
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
        case r'status':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.status = valueDes;
          break;
        case r'targetingRules':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(BuiltList, [FullType(TargetingRuleDto)]),
          ) as BuiltList<TargetingRuleDto>;
          result.targetingRules.replace(valueDes);
          break;
        case r'updatedAt':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(DateTime),
          ) as DateTime;
          result.updatedAt = valueDes;
          break;
        case r'version':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(int),
          ) as int;
          result.version = valueDes;
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  FlagResponse deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = FlagResponseBuilder();
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

