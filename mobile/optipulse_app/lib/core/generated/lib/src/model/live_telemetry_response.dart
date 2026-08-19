//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'live_telemetry_response.g.dart';

/// LiveTelemetryResponse
///
/// Properties:
/// * [activeFlags] 
/// * [killSwitchesEngaged] 
/// * [serverTime] 
/// * [snapshotAgeSeconds] 
/// * [snapshotBuiltAt] 
/// * [snapshotVersion] 
@BuiltValue()
abstract class LiveTelemetryResponse implements Built<LiveTelemetryResponse, LiveTelemetryResponseBuilder> {
  @BuiltValueField(wireName: r'activeFlags')
  int get activeFlags;

  @BuiltValueField(wireName: r'killSwitchesEngaged')
  int get killSwitchesEngaged;

  @BuiltValueField(wireName: r'serverTime')
  DateTime get serverTime;

  @BuiltValueField(wireName: r'snapshotAgeSeconds')
  int? get snapshotAgeSeconds;

  @BuiltValueField(wireName: r'snapshotBuiltAt')
  DateTime get snapshotBuiltAt;

  @BuiltValueField(wireName: r'snapshotVersion')
  int get snapshotVersion;

  LiveTelemetryResponse._();

  factory LiveTelemetryResponse([void updates(LiveTelemetryResponseBuilder b)]) = _$LiveTelemetryResponse;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(LiveTelemetryResponseBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<LiveTelemetryResponse> get serializer => _$LiveTelemetryResponseSerializer();
}

class _$LiveTelemetryResponseSerializer implements PrimitiveSerializer<LiveTelemetryResponse> {
  @override
  final Iterable<Type> types = const [LiveTelemetryResponse, _$LiveTelemetryResponse];

  @override
  final String wireName = r'LiveTelemetryResponse';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    LiveTelemetryResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'activeFlags';
    yield serializers.serialize(
      object.activeFlags,
      specifiedType: const FullType(int),
    );
    yield r'killSwitchesEngaged';
    yield serializers.serialize(
      object.killSwitchesEngaged,
      specifiedType: const FullType(int),
    );
    yield r'serverTime';
    yield serializers.serialize(
      object.serverTime,
      specifiedType: const FullType(DateTime),
    );
    yield r'snapshotAgeSeconds';
    yield object.snapshotAgeSeconds == null ? null : serializers.serialize(
      object.snapshotAgeSeconds,
      specifiedType: const FullType.nullable(int),
    );
    yield r'snapshotBuiltAt';
    yield serializers.serialize(
      object.snapshotBuiltAt,
      specifiedType: const FullType(DateTime),
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
    LiveTelemetryResponse object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required LiveTelemetryResponseBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'activeFlags':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(int),
          ) as int;
          result.activeFlags = valueDes;
          break;
        case r'killSwitchesEngaged':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(int),
          ) as int;
          result.killSwitchesEngaged = valueDes;
          break;
        case r'serverTime':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(DateTime),
          ) as DateTime;
          result.serverTime = valueDes;
          break;
        case r'snapshotAgeSeconds':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType.nullable(int),
          ) as int?;
          if (valueDes == null) continue;
          result.snapshotAgeSeconds = valueDes;
          break;
        case r'snapshotBuiltAt':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(DateTime),
          ) as DateTime;
          result.snapshotBuiltAt = valueDes;
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
  LiveTelemetryResponse deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = LiveTelemetryResponseBuilder();
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

