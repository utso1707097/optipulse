//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_collection/built_collection.dart';
import 'package:openapi/src/model/evaluate_response.dart';
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'batch_evaluate_response.g.dart';

/// BatchEvaluateResponse
///
/// Properties:
/// * [results] 
/// * [snapshotVersion] 
@BuiltValue()
abstract class BatchEvaluateResponse implements Built<BatchEvaluateResponse, BatchEvaluateResponseBuilder> {
  @BuiltValueField(wireName: r'results')
  BuiltList<EvaluateResponse> get results;

  @BuiltValueField(wireName: r'snapshotVersion')
  int get snapshotVersion;

  BatchEvaluateResponse._();

  factory BatchEvaluateResponse([void updates(BatchEvaluateResponseBuilder b)]) = _$BatchEvaluateResponse;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(BatchEvaluateResponseBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<BatchEvaluateResponse> get serializer => _$BatchEvaluateResponseSerializer();
}

class _$BatchEvaluateResponseSerializer implements PrimitiveSerializer<BatchEvaluateResponse> {
  @override
  final Iterable<Type> types = const [BatchEvaluateResponse, _$BatchEvaluateResponse];

  @override
  final String wireName = r'BatchEvaluateResponse';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    BatchEvaluateResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'results';
    yield serializers.serialize(
      object.results,
      specifiedType: const FullType(BuiltList, [FullType(EvaluateResponse)]),
    );
    yield r'snapshotVersion';
    yield serializers.serialize(
      object.snapshotVersion,
      specifiedType: const FullType(int),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    BatchEvaluateResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required BatchEvaluateResponseBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'results':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(BuiltList, [FullType(EvaluateResponse)]),
          ) as BuiltList<EvaluateResponse>;
          result.results.replace(valueDes);
          break;
        case r'snapshotVersion':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(int),
          ) as int;
          result.snapshotVersion = valueDes;
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  BatchEvaluateResponse deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = BatchEvaluateResponseBuilder();
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

