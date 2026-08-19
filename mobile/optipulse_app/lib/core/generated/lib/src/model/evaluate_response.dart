//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'evaluate_response.g.dart';

/// EvaluateResponse
///
/// Properties:
/// * [flagKey] 
/// * [outcome] 
/// * [reason] 
/// * [snapshotVersion] 
/// * [variantKey] 
@BuiltValue()
abstract class EvaluateResponse implements Built<EvaluateResponse, EvaluateResponseBuilder> {
  @BuiltValueField(wireName: r'flagKey')
  String get flagKey;

  @BuiltValueField(wireName: r'outcome')
  bool get outcome;

  @BuiltValueField(wireName: r'reason')
  String get reason;

  @BuiltValueField(wireName: r'snapshotVersion')
  int get snapshotVersion;

  @BuiltValueField(wireName: r'variantKey')
  String? get variantKey;

  EvaluateResponse._();

  factory EvaluateResponse([void updates(EvaluateResponseBuilder b)]) = _$EvaluateResponse;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(EvaluateResponseBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<EvaluateResponse> get serializer => _$EvaluateResponseSerializer();
}

class _$EvaluateResponseSerializer implements PrimitiveSerializer<EvaluateResponse> {
  @override
  final Iterable<Type> types = const [EvaluateResponse, _$EvaluateResponse];

  @override
  final String wireName = r'EvaluateResponse';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    EvaluateResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'flagKey';
    yield serializers.serialize(
      object.flagKey,
      specifiedType: const FullType(String),
    );
    yield r'outcome';
    yield serializers.serialize(
      object.outcome,
      specifiedType: const FullType(bool),
    );
    yield r'reason';
    yield serializers.serialize(
      object.reason,
      specifiedType: const FullType(String),
    );
    yield r'snapshotVersion';
    yield serializers.serialize(
      object.snapshotVersion,
      specifiedType: const FullType(int),
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
    EvaluateResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required EvaluateResponseBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'flagKey':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.flagKey = valueDes;
          break;
        case r'outcome':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(bool),
          ) as bool;
          result.outcome = valueDes;
          break;
        case r'reason':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.reason = valueDes;
          break;
        case r'snapshotVersion':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(int),
          ) as int;
          result.snapshotVersion = valueDes;
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
  EvaluateResponse deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = EvaluateResponseBuilder();
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

