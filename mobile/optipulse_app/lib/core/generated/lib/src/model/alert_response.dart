//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'alert_response.g.dart';

/// AlertResponse
///
/// Properties:
/// * [acknowledgedAt] 
/// * [acknowledgedBy] 
/// * [detail] 
/// * [flagKey] 
/// * [id] 
/// * [kind] 
/// * [raisedAt] 
/// * [severity] 
/// * [title] 
@BuiltValue()
abstract class AlertResponse implements Built<AlertResponse, AlertResponseBuilder> {
  @BuiltValueField(wireName: r'acknowledgedAt')
  DateTime? get acknowledgedAt;

  @BuiltValueField(wireName: r'acknowledgedBy')
  String? get acknowledgedBy;

  @BuiltValueField(wireName: r'detail')
  String get detail;

  @BuiltValueField(wireName: r'flagKey')
  String? get flagKey;

  @BuiltValueField(wireName: r'id')
  String get id;

  @BuiltValueField(wireName: r'kind')
  String get kind;

  @BuiltValueField(wireName: r'raisedAt')
  DateTime get raisedAt;

  @BuiltValueField(wireName: r'severity')
  String get severity;

  @BuiltValueField(wireName: r'title')
  String get title;

  AlertResponse._();

  factory AlertResponse([void updates(AlertResponseBuilder b)]) = _$AlertResponse;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(AlertResponseBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<AlertResponse> get serializer => _$AlertResponseSerializer();
}

class _$AlertResponseSerializer implements PrimitiveSerializer<AlertResponse> {
  @override
  final Iterable<Type> types = const [AlertResponse, _$AlertResponse];

  @override
  final String wireName = r'AlertResponse';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    AlertResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'acknowledgedAt';
    yield object.acknowledgedAt == null ? null : serializers.serialize(
      object.acknowledgedAt,
      specifiedType: const FullType.nullable(DateTime),
    );
    yield r'acknowledgedBy';
    yield object.acknowledgedBy == null ? null : serializers.serialize(
      object.acknowledgedBy,
      specifiedType: const FullType.nullable(String),
    );
    yield r'detail';
    yield serializers.serialize(
      object.detail,
      specifiedType: const FullType(String),
    );
    yield r'flagKey';
    yield object.flagKey == null ? null : serializers.serialize(
      object.flagKey,
      specifiedType: const FullType.nullable(String),
    );
    yield r'id';
    yield serializers.serialize(
      object.id,
      specifiedType: const FullType(String),
    );
    yield r'kind';
    yield serializers.serialize(
      object.kind,
      specifiedType: const FullType(String),
    );
    yield r'raisedAt';
    yield serializers.serialize(
      object.raisedAt,
      specifiedType: const FullType(DateTime),
    );
    yield r'severity';
    yield serializers.serialize(
      object.severity,
      specifiedType: const FullType(String),
    );
    yield r'title';
    yield serializers.serialize(
      object.title,
      specifiedType: const FullType(String),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    AlertResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required AlertResponseBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'acknowledgedAt':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType.nullable(DateTime),
          ) as DateTime?;
          if (valueDes == null) continue;
          result.acknowledgedAt = valueDes;
          break;
        case r'acknowledgedBy':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType.nullable(String),
          ) as String?;
          if (valueDes == null) continue;
          result.acknowledgedBy = valueDes;
          break;
        case r'detail':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.detail = valueDes;
          break;
        case r'flagKey':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType.nullable(String),
          ) as String?;
          if (valueDes == null) continue;
          result.flagKey = valueDes;
          break;
        case r'id':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.id = valueDes;
          break;
        case r'kind':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.kind = valueDes;
          break;
        case r'raisedAt':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(DateTime),
          ) as DateTime;
          result.raisedAt = valueDes;
          break;
        case r'severity':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.severity = valueDes;
          break;
        case r'title':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.title = valueDes;
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  AlertResponse deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = AlertResponseBuilder();
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

