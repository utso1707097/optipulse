//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_collection/built_collection.dart';
import 'package:openapi/src/model/variant_dto.dart';
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'update_experiment_request.g.dart';

/// UpdateExperimentRequest
///
/// Properties:
/// * [variants] 
@BuiltValue()
abstract class UpdateExperimentRequest implements Built<UpdateExperimentRequest, UpdateExperimentRequestBuilder> {
  @BuiltValueField(wireName: r'variants')
  BuiltList<VariantDto> get variants;

  UpdateExperimentRequest._();

  factory UpdateExperimentRequest([void updates(UpdateExperimentRequestBuilder b)]) = _$UpdateExperimentRequest;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(UpdateExperimentRequestBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<UpdateExperimentRequest> get serializer => _$UpdateExperimentRequestSerializer();
}

class _$UpdateExperimentRequestSerializer implements PrimitiveSerializer<UpdateExperimentRequest> {
  @override
  final Iterable<Type> types = const [UpdateExperimentRequest, _$UpdateExperimentRequest];

  @override
  final String wireName = r'UpdateExperimentRequest';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    UpdateExperimentRequest object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'variants';
    yield serializers.serialize(
      object.variants,
      specifiedType: const FullType(BuiltList, [FullType(VariantDto)]),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    UpdateExperimentRequest object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required UpdateExperimentRequestBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'variants':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(BuiltList, [FullType(VariantDto)]),
          ) as BuiltList<VariantDto>;
          result.variants.replace(valueDes);
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  UpdateExperimentRequest deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = UpdateExperimentRequestBuilder();
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

