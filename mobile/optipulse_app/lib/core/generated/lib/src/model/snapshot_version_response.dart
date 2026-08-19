//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'snapshot_version_response.g.dart';

/// SnapshotVersionResponse
///
/// Properties:
/// * [builtAt] 
/// * [version] 
@BuiltValue()
abstract class SnapshotVersionResponse implements Built<SnapshotVersionResponse, SnapshotVersionResponseBuilder> {
  @BuiltValueField(wireName: r'builtAt')
  DateTime get builtAt;

  @BuiltValueField(wireName: r'version')
  int get version;

  SnapshotVersionResponse._();

  factory SnapshotVersionResponse([void updates(SnapshotVersionResponseBuilder b)]) = _$SnapshotVersionResponse;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(SnapshotVersionResponseBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<SnapshotVersionResponse> get serializer => _$SnapshotVersionResponseSerializer();
}

class _$SnapshotVersionResponseSerializer implements PrimitiveSerializer<SnapshotVersionResponse> {
  @override
  final Iterable<Type> types = const [SnapshotVersionResponse, _$SnapshotVersionResponse];

  @override
  final String wireName = r'SnapshotVersionResponse';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    SnapshotVersionResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'builtAt';
    yield serializers.serialize(
      object.builtAt,
      specifiedType: const FullType(DateTime),
    );
    yield r'version';
    yield serializers.serialize(
      object.version,
      specifiedType: const FullType(int),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    SnapshotVersionResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required SnapshotVersionResponseBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'builtAt':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(DateTime),
          ) as DateTime;
          result.builtAt = valueDes;
          break;
        case r'version':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(int),
          ) as int;
          result.version = valueDes;
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  SnapshotVersionResponse deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = SnapshotVersionResponseBuilder();
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

