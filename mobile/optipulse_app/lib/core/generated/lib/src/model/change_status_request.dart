//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:built_value/built_value.dart';
import 'package:built_value/serializer.dart';

part 'change_status_request.g.dart';

/// ChangeStatusRequest
///
/// Properties:
/// * [status] 
@BuiltValue()
abstract class ChangeStatusRequest implements Built<ChangeStatusRequest, ChangeStatusRequestBuilder> {
  @BuiltValueField(wireName: r'status')
  String get status;

  ChangeStatusRequest._();

  factory ChangeStatusRequest([void updates(ChangeStatusRequestBuilder b)]) = _$ChangeStatusRequest;

  @BuiltValueHook(initializeBuilder: true)
  static void _defaults(ChangeStatusRequestBuilder b) => b;

  @BuiltValueSerializer(custom: true)
  static Serializer<ChangeStatusRequest> get serializer => _$ChangeStatusRequestSerializer();
}

class _$ChangeStatusRequestSerializer implements PrimitiveSerializer<ChangeStatusRequest> {
  @override
  final Iterable<Type> types = const [ChangeStatusRequest, _$ChangeStatusRequest];

  @override
  final String wireName = r'ChangeStatusRequest';

  Iterable<Object?> _serializeProperties(
    Serializers serializers,
    ChangeStatusRequest object, {
    FullType specifiedType = FullType.unspecified,
  }) sync* {
    yield r'status';
    yield serializers.serialize(
      object.status,
      specifiedType: const FullType(String),
    );
  }

  @override
  Object serialize(
    Serializers serializers,
    ChangeStatusRequest object, {
    FullType specifiedType = FullType.unspecified,
  }) {
    return _serializeProperties(serializers, object, specifiedType: specifiedType).toList();
  }

  void _deserializeProperties(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
    required List<Object?> serializedList,
    required ChangeStatusRequestBuilder result,
    required List<Object?> unhandled,
  }) {
    for (var i = 0; i < serializedList.length; i += 2) {
      final key = serializedList[i] as String;
      final value = serializedList[i + 1];
      switch (key) {
        case r'status':
          final valueDes = serializers.deserialize(
            value,
            specifiedType: const FullType(String),
          ) as String;
          result.status = valueDes;
          break;
        default:
          unhandled.add(key);
          unhandled.add(value);
          break;
      }
    }
  }

  @override
  ChangeStatusRequest deserialize(
    Serializers serializers,
    Object serialized, {
    FullType specifiedType = FullType.unspecified,
  }) {
    final result = ChangeStatusRequestBuilder();
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

