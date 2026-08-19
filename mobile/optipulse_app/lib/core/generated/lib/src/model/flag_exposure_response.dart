//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_collection/built_collection.dart';
import 'package:openapi/src/model/variant_exposure_dto.dart';
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'flag_exposure_response.g.dart';

/// FlagExposureResponse
///
/// Properties:
/// * [byVariant] 
/// * [flagKey] 
/// * [totalConversions] 
/// * [totalExposures] 
@BuiltValue()
abstract class FlagExposureResponse implements Built<FlagExposureResponse, FlagExposureResponseBuilder> {
  @BuiltValueField(wireName: r'byVariant')
  BuiltList<VariantExposureDto> get byVariant;

  @BuiltValueField(wireName: r'flagKey')
  String get flagKey;

  @BuiltValueField(wireName: r'totalConversions')
  int get totalConversions;

  @BuiltValueField(wireName: r'totalExposures')
  int get totalExposures;

  FlagExposureResponse._();

  factory FlagExposureResponse([void updates(FlagExposureResponseBuilder b)]) = _$FlagExposureResponse;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(FlagExposureResponseBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<FlagExposureResponse> get serializer => _$FlagExposureResponseSerializer();
}

class _$FlagExposureResponseSerializer implements PrimitiveSerializer<FlagExposureResponse> {
  @override
  final Iterable<Type> types = const [FlagExposureResponse, _$FlagExposureResponse];

  @override
  final String wireName = r'FlagExposureResponse';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    FlagExposureResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'byVariant';
    yield serializers.serialize(
      object.byVariant,
      specifiedType: const FullType(BuiltList, [FullType(VariantExposureDto)]),
    );
    yield r'flagKey';
    yield serializers.serialize(
      object.flagKey,
      specifiedType: const FullType(String),
    );
    yield r'totalConversions';
    yield serializers.serialize(
      object.totalConversions,
      specifiedType: const FullType(int),
    );
    yield r'totalExposures';
    yield serializers.serialize(
      object.totalExposures,
      specifiedType: const FullType(int),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    FlagExposureResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required FlagExposureResponseBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'byVariant':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(BuiltList, [FullType(VariantExposureDto)]),
          ) as BuiltList<VariantExposureDto>;
          result.byVariant.replace(valueDes);
          break;
        case r'flagKey':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.flagKey = valueDes;
          break;
        case r'totalConversions':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(int),
          ) as int;
          result.totalConversions = valueDes;
          break;
        case r'totalExposures':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(int),
          ) as int;
          result.totalExposures = valueDes;
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  FlagExposureResponse deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = FlagExposureResponseBuilder();
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

