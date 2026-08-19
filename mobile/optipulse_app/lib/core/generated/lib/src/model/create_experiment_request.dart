//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_collection/built_collection.dart';
import 'package:openapi/src/model/variant_dto.dart';
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'create_experiment_request.g.dart';

/// CreateExperimentRequest
///
/// Properties:
/// * [conversionGoal] 
/// * [flagKey] 
/// * [name] 
/// * [variants] 
@BuiltValue()
abstract class CreateExperimentRequest implements Built<CreateExperimentRequest, CreateExperimentRequestBuilder> {
  @BuiltValueField(wireName: r'conversionGoal')
  String? get conversionGoal;

  @BuiltValueField(wireName: r'flagKey')
  String get flagKey;

  @BuiltValueField(wireName: r'name')
  String get name;

  @BuiltValueField(wireName: r'variants')
  BuiltList<VariantDto> get variants;

  CreateExperimentRequest._();

  factory CreateExperimentRequest([void updates(CreateExperimentRequestBuilder b)]) = _$CreateExperimentRequest;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(CreateExperimentRequestBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<CreateExperimentRequest> get serializer => _$CreateExperimentRequestSerializer();
}

class _$CreateExperimentRequestSerializer implements PrimitiveSerializer<CreateExperimentRequest> {
  @override
  final Iterable<Type> types = const [CreateExperimentRequest, _$CreateExperimentRequest];

  @override
  final String wireName = r'CreateExperimentRequest';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    CreateExperimentRequest object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'conversionGoal';
    yield object.conversionGoal == null ? null : serializers.serialize(
      object.conversionGoal,
      specifiedType: const FullType.nullable(String),
    );
    yield r'flagKey';
    yield serializers.serialize(
      object.flagKey,
      specifiedType: const FullType(String),
    );
    yield r'name';
    yield serializers.serialize(
      object.name,
      specifiedType: const FullType(String),
    );
    yield r'variants';
    yield serializers.serialize(
      object.variants,
      specifiedType: const FullType(BuiltList, [FullType(VariantDto)]),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    CreateExperimentRequest object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required CreateExperimentRequestBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'conversionGoal':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType.nullable(String),
          ) as String?;
          if (valueDes == null) continue;
          result.conversionGoal = valueDes;
          break;
        case r'flagKey':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.flagKey = valueDes;
          break;
        case r'name':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.name = valueDes;
          break;
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
  CreateExperimentRequest deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = CreateExperimentRequestBuilder();
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

