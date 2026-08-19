//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'conversion_request.g.dart';

/// ConversionRequest
///
/// Properties:
/// * [contextKey] 
/// * [experimentId] 
/// * [flagKey] 
/// * [goal] 
/// * [idempotencyKey] 
/// * [value] 
/// * [variantKey] 
@BuiltValue()
abstract class ConversionRequest implements Built<ConversionRequest, ConversionRequestBuilder> {
  @BuiltValueField(wireName: r'contextKey')
  String? get contextKey;

  @BuiltValueField(wireName: r'experimentId')
  String? get experimentId;

  @BuiltValueField(wireName: r'flagKey')
  String get flagKey;

  @BuiltValueField(wireName: r'goal')
  String get goal;

  @BuiltValueField(wireName: r'idempotencyKey')
  String get idempotencyKey;

  @BuiltValueField(wireName: r'value')
  double? get value;

  @BuiltValueField(wireName: r'variantKey')
  String? get variantKey;

  ConversionRequest._();

  factory ConversionRequest([void updates(ConversionRequestBuilder b)]) = _$ConversionRequest;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(ConversionRequestBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<ConversionRequest> get serializer => _$ConversionRequestSerializer();
}

class _$ConversionRequestSerializer implements PrimitiveSerializer<ConversionRequest> {
  @override
  final Iterable<Type> types = const [ConversionRequest, _$ConversionRequest];

  @override
  final String wireName = r'ConversionRequest';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    ConversionRequest object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'contextKey';
    yield object.contextKey == null ? null : serializers.serialize(
      object.contextKey,
      specifiedType: const FullType.nullable(String),
    );
    yield r'experimentId';
    yield object.experimentId == null ? null : serializers.serialize(
      object.experimentId,
      specifiedType: const FullType.nullable(String),
    );
    yield r'flagKey';
    yield serializers.serialize(
      object.flagKey,
      specifiedType: const FullType(String),
    );
    yield r'goal';
    yield serializers.serialize(
      object.goal,
      specifiedType: const FullType(String),
    );
    yield r'idempotencyKey';
    yield serializers.serialize(
      object.idempotencyKey,
      specifiedType: const FullType(String),
    );
    yield r'value';
    yield object.value == null ? null : serializers.serialize(
      object.value,
      specifiedType: const FullType.nullable(double),
    );
    yield r'variantKey';
    yield object.variantKey == null ? null : serializers.serialize(
      object.variantKey,
      specifiedType: const FullType.nullable(String),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    ConversionRequest object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required ConversionRequestBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'contextKey':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType.nullable(String),
          ) as String?;
          if (valueDes == null) continue;
          result.contextKey = valueDes;
          break;
        case r'experimentId':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType.nullable(String),
          ) as String?;
          if (valueDes == null) continue;
          result.experimentId = valueDes;
          break;
        case r'flagKey':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.flagKey = valueDes;
          break;
        case r'goal':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.goal = valueDes;
          break;
        case r'idempotencyKey':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.idempotencyKey = valueDes;
          break;
        case r'value':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType.nullable(double),
          ) as double?;
          if (valueDes == null) continue;
          result.value = valueDes;
          break;
        case r'variantKey':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType.nullable(String),
          ) as String?;
          if (valueDes == null) continue;
          result.variantKey = valueDes;
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  ConversionRequest deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = ConversionRequestBuilder();
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

