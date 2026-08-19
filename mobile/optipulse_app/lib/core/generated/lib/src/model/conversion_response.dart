//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'conversion_response.g.dart';

/// ConversionResponse
///
/// Properties:
/// * [duplicate] 
/// * [recorded] 
@BuiltValue()
abstract class ConversionResponse implements Built<ConversionResponse, ConversionResponseBuilder> {
  @BuiltValueField(wireName: r'duplicate')
  bool get duplicate;

  @BuiltValueField(wireName: r'recorded')
  bool get recorded;

  ConversionResponse._();

  factory ConversionResponse([void updates(ConversionResponseBuilder b)]) = _$ConversionResponse;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(ConversionResponseBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<ConversionResponse> get serializer => _$ConversionResponseSerializer();
}

class _$ConversionResponseSerializer implements PrimitiveSerializer<ConversionResponse> {
  @override
  final Iterable<Type> types = const [ConversionResponse, _$ConversionResponse];

  @override
  final String wireName = r'ConversionResponse';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    ConversionResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'duplicate';
    yield serializers.serialize(
      object.duplicate,
      specifiedType: const FullType(bool),
    );
    yield r'recorded';
    yield serializers.serialize(
      object.recorded,
      specifiedType: const FullType(bool),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    ConversionResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required ConversionResponseBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'duplicate':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(bool),
          ) as bool;
          result.duplicate = valueDes;
          break;
        case r'recorded':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(bool),
          ) as bool;
          result.recorded = valueDes;
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  ConversionResponse deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = ConversionResponseBuilder();
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

