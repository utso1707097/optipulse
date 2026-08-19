//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_collection/built_collection.dart';
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'targeting_rule_dto.g.dart';

/// TargetingRuleDto
///
/// Properties:
/// * [attribute] 
/// * [operator_] 
/// * [outcome] 
/// * [values] 
@BuiltValue()
abstract class TargetingRuleDto implements Built<TargetingRuleDto, TargetingRuleDtoBuilder> {
  @BuiltValueField(wireName: r'attribute')
  String get attribute;

  @BuiltValueField(wireName: r'operator')
  String get operator_;

  @BuiltValueField(wireName: r'outcome')
  bool get outcome;

  @BuiltValueField(wireName: r'values')
  BuiltList<String> get values;

  TargetingRuleDto._();

  factory TargetingRuleDto([void updates(TargetingRuleDtoBuilder b)]) = _$TargetingRuleDto;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(TargetingRuleDtoBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<TargetingRuleDto> get serializer => _$TargetingRuleDtoSerializer();
}

class _$TargetingRuleDtoSerializer implements PrimitiveSerializer<TargetingRuleDto> {
  @override
  final Iterable<Type> types = const [TargetingRuleDto, _$TargetingRuleDto];

  @override
  final String wireName = r'TargetingRuleDto';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    TargetingRuleDto object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'attribute';
    yield serializers.serialize(
      object.attribute,
      specifiedType: const FullType(String),
    );
    yield r'operator';
    yield serializers.serialize(
      object.operator_,
      specifiedType: const FullType(String),
    );
    yield r'outcome';
    yield serializers.serialize(
      object.outcome,
      specifiedType: const FullType(bool),
    );
    yield r'values';
    yield serializers.serialize(
      object.values,
      specifiedType: const FullType(BuiltList, [FullType(String)]),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    TargetingRuleDto object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required TargetingRuleDtoBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'attribute':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.attribute = valueDes;
          break;
        case r'operator':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.operator_ = valueDes;
          break;
        case r'outcome':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(bool),
          ) as bool;
          result.outcome = valueDes;
          break;
        case r'values':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(BuiltList, [FullType(String)]),
          ) as BuiltList<String>;
          result.values.replace(valueDes);
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  TargetingRuleDto deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = TargetingRuleDtoBuilder();
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

