//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'rollout_dto.g.dart';

/// RolloutDto
///
/// Properties:
/// * [percentage] 
/// * [salt] 
@BuiltValue()
abstract class RolloutDto implements Built<RolloutDto, RolloutDtoBuilder> {
  @BuiltValueField(wireName: r'percentage')
  int get percentage;

  @BuiltValueField(wireName: r'salt')
  String get salt;

  RolloutDto._();

  factory RolloutDto([void updates(RolloutDtoBuilder b)]) = _$RolloutDto;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(RolloutDtoBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<RolloutDto> get serializer => _$RolloutDtoSerializer();
}

class _$RolloutDtoSerializer implements PrimitiveSerializer<RolloutDto> {
  @override
  final Iterable<Type> types = const [RolloutDto, _$RolloutDto];

  @override
  final String wireName = r'RolloutDto';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    RolloutDto object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'percentage';
    yield serializers.serialize(
      object.percentage,
      specifiedType: const FullType(int),
    );
    yield r'salt';
    yield serializers.serialize(
      object.salt,
      specifiedType: const FullType(String),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    RolloutDto object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required RolloutDtoBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'percentage':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(int),
          ) as int;
          result.percentage = valueDes;
          break;
        case r'salt':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.salt = valueDes;
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  RolloutDto deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = RolloutDtoBuilder();
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

