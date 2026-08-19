//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'variant_exposure_dto.g.dart';

/// VariantExposureDto
///
/// Properties:
/// * [conversionRatePercent] 
/// * [conversions] 
/// * [exposures] 
/// * [sharePercent] 
/// * [variantKey] 
@BuiltValue()
abstract class VariantExposureDto implements Built<VariantExposureDto, VariantExposureDtoBuilder> {
  @BuiltValueField(wireName: r'conversionRatePercent')
  double get conversionRatePercent;

  @BuiltValueField(wireName: r'conversions')
  int get conversions;

  @BuiltValueField(wireName: r'exposures')
  int get exposures;

  @BuiltValueField(wireName: r'sharePercent')
  double get sharePercent;

  @BuiltValueField(wireName: r'variantKey')
  String? get variantKey;

  VariantExposureDto._();

  factory VariantExposureDto([void updates(VariantExposureDtoBuilder b)]) = _$VariantExposureDto;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(VariantExposureDtoBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<VariantExposureDto> get serializer => _$VariantExposureDtoSerializer();
}

class _$VariantExposureDtoSerializer implements PrimitiveSerializer<VariantExposureDto> {
  @override
  final Iterable<Type> types = const [VariantExposureDto, _$VariantExposureDto];

  @override
  final String wireName = r'VariantExposureDto';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    VariantExposureDto object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'conversionRatePercent';
    yield serializers.serialize(
      object.conversionRatePercent,
      specifiedType: const FullType(double),
    );
    yield r'conversions';
    yield serializers.serialize(
      object.conversions,
      specifiedType: const FullType(int),
    );
    yield r'exposures';
    yield serializers.serialize(
      object.exposures,
      specifiedType: const FullType(int),
    );
    yield r'sharePercent';
    yield serializers.serialize(
      object.sharePercent,
      specifiedType: const FullType(double),
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
    VariantExposureDto object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required VariantExposureDtoBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'conversionRatePercent':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(double),
          ) as double;
          result.conversionRatePercent = valueDes;
          break;
        case r'conversions':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(int),
          ) as int;
          result.conversions = valueDes;
          break;
        case r'exposures':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(int),
          ) as int;
          result.exposures = valueDes;
          break;
        case r'sharePercent':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(double),
          ) as double;
          result.sharePercent = valueDes;
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
  VariantExposureDto deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = VariantExposureDtoBuilder();
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

