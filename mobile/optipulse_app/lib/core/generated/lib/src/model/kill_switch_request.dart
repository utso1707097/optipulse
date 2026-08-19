//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'kill_switch_request.g.dart';

/// KillSwitchRequest
///
/// Properties:
/// * [engaged] 
@BuiltValue()
abstract class KillSwitchRequest implements Built<KillSwitchRequest, KillSwitchRequestBuilder> {
  @BuiltValueField(wireName: r'engaged')
  bool get engaged;

  KillSwitchRequest._();

  factory KillSwitchRequest([void updates(KillSwitchRequestBuilder b)]) = _$KillSwitchRequest;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(KillSwitchRequestBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<KillSwitchRequest> get serializer => _$KillSwitchRequestSerializer();
}

class _$KillSwitchRequestSerializer implements PrimitiveSerializer<KillSwitchRequest> {
  @override
  final Iterable<Type> types = const [KillSwitchRequest, _$KillSwitchRequest];

  @override
  final String wireName = r'KillSwitchRequest';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    KillSwitchRequest object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'engaged';
    yield serializers.serialize(
      object.engaged,
      specifiedType: const FullType(bool),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    KillSwitchRequest object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required KillSwitchRequestBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'engaged':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(bool),
          ) as bool;
          result.engaged = valueDes;
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  KillSwitchRequest deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = KillSwitchRequestBuilder();
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

