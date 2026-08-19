//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_collection/built_collection.dart';
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'evaluate_request.g.dart';

/// EvaluateRequest
///
/// Properties:
/// * [attributes] 
/// * [contextKey] 
/// * [flagKey] 
@BuiltValue()
abstract class EvaluateRequest implements Built<EvaluateRequest, EvaluateRequestBuilder> {
  @BuiltValueField(wireName: r'attributes')
  BuiltMap<String, String>? get attributes;

  @BuiltValueField(wireName: r'contextKey')
  String? get contextKey;

  @BuiltValueField(wireName: r'flagKey')
  String get flagKey;

  EvaluateRequest._();

  factory EvaluateRequest([void updates(EvaluateRequestBuilder b)]) = _$EvaluateRequest;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(EvaluateRequestBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<EvaluateRequest> get serializer => _$EvaluateRequestSerializer();
}

class _$EvaluateRequestSerializer implements PrimitiveSerializer<EvaluateRequest> {
  @override
  final Iterable<Type> types = const [EvaluateRequest, _$EvaluateRequest];

  @override
  final String wireName = r'EvaluateRequest';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    EvaluateRequest object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'attributes';
    yield object.attributes == null ? null : serializers.serialize(
      object.attributes,
      specifiedType: const FullType.nullable(BuiltMap, [FullType(String), FullType(String)]),
    );
    yield r'contextKey';
    yield object.contextKey == null ? null : serializers.serialize(
      object.contextKey,
      specifiedType: const FullType.nullable(String),
    );
    yield r'flagKey';
    yield serializers.serialize(
      object.flagKey,
      specifiedType: const FullType(String),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    EvaluateRequest object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required EvaluateRequestBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'attributes':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType.nullable(BuiltMap, [FullType(String), FullType(String)]),
          ) as BuiltMap<String, String>?;
          if (valueDes == null) continue;
          result.attributes.replace(valueDes);
          break;
        case r'contextKey':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType.nullable(String),
          ) as String?;
          if (valueDes == null) continue;
          result.contextKey = valueDes;
          break;
        case r'flagKey':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.flagKey = valueDes;
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  EvaluateRequest deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = EvaluateRequestBuilder();
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

