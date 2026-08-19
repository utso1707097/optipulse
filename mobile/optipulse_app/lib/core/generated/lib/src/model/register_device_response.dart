//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'register_device_response.g.dart';

/// RegisterDeviceResponse
///
/// Properties:
/// * [id] 
/// * [platform] 
/// * [registeredAt] 
@BuiltValue()
abstract class RegisterDeviceResponse implements Built<RegisterDeviceResponse, RegisterDeviceResponseBuilder> {
  @BuiltValueField(wireName: r'id')
  String get id;

  @BuiltValueField(wireName: r'platform')
  String get platform;

  @BuiltValueField(wireName: r'registeredAt')
  DateTime get registeredAt;

  RegisterDeviceResponse._();

  factory RegisterDeviceResponse([void updates(RegisterDeviceResponseBuilder b)]) = _$RegisterDeviceResponse;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(RegisterDeviceResponseBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<RegisterDeviceResponse> get serializer => _$RegisterDeviceResponseSerializer();
}

class _$RegisterDeviceResponseSerializer implements PrimitiveSerializer<RegisterDeviceResponse> {
  @override
  final Iterable<Type> types = const [RegisterDeviceResponse, _$RegisterDeviceResponse];

  @override
  final String wireName = r'RegisterDeviceResponse';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    RegisterDeviceResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'id';
    yield serializers.serialize(
      object.id,
      specifiedType: const FullType(String),
    );
    yield r'platform';
    yield serializers.serialize(
      object.platform,
      specifiedType: const FullType(String),
    );
    yield r'registeredAt';
    yield serializers.serialize(
      object.registeredAt,
      specifiedType: const FullType(DateTime),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    RegisterDeviceResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required RegisterDeviceResponseBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'id':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.id = valueDes;
          break;
        case r'platform':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.platform = valueDes;
          break;
        case r'registeredAt':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(DateTime),
          ) as DateTime;
          result.registeredAt = valueDes;
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  RegisterDeviceResponse deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = RegisterDeviceResponseBuilder();
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

