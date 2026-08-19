//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_collection/built_collection.dart';
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'batch_evaluate_request.g.dart';

/// BatchEvaluateRequest
///
/// Properties:
/// * [attributes] 
/// * [contextKey] 
/// * [flagKeys] 
@BuiltValue()
abstract class BatchEvaluateRequest implements Built<BatchEvaluateRequest, BatchEvaluateRequestBuilder> {
  @BuiltValueField(wireName: r'attributes')
  BuiltMap<String, String>? get attributes;

  @BuiltValueField(wireName: r'contextKey')
  String? get contextKey;

  @BuiltValueField(wireName: r'flagKeys')
  BuiltList<String> get flagKeys;

  BatchEvaluateRequest._();

  factory BatchEvaluateRequest([void updates(BatchEvaluateRequestBuilder b)]) = _$BatchEvaluateRequest;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(BatchEvaluateRequestBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<BatchEvaluateRequest> get serializer => _$BatchEvaluateRequestSerializer();
}

class _$BatchEvaluateRequestSerializer implements PrimitiveSerializer<BatchEvaluateRequest> {
  @override
  final Iterable<Type> types = const [BatchEvaluateRequest, _$BatchEvaluateRequest];

  @override
  final String wireName = r'BatchEvaluateRequest';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    BatchEvaluateRequest object, {
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
    yield r'flagKeys';
    yield serializers.serialize(
      object.flagKeys,
      specifiedType: const FullType(BuiltList, [FullType(String)]),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    BatchEvaluateRequest object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required BatchEvaluateRequestBuilder result,
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
        case r'flagKeys':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(BuiltList, [FullType(String)]),
          ) as BuiltList<String>;
          result.flagKeys.replace(valueDes);
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  BatchEvaluateRequest deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = BatchEvaluateRequestBuilder();
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

