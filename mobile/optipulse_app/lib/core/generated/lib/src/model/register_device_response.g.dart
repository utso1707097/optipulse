// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'register_device_response.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$RegisterDeviceResponse extends RegisterDeviceResponse {
  @override
  final String id;
  @override
  final String platform;
  @override
  final DateTime registeredAt;

  factory _$RegisterDeviceResponse([
    void Function(RegisterDeviceResponseBuilder)? updates,
  ]) => (RegisterDeviceResponseBuilder()..update(updates))._build();

  _$RegisterDeviceResponse._({
    required this.id,
    required this.platform,
    required this.registeredAt,
  }) : super._();
  @override
  RegisterDeviceResponse rebuild(
    void Function(RegisterDeviceResponseBuilder) updates,
  ) => (toBuilder()..update(updates)).build();

  @override
  RegisterDeviceResponseBuilder toBuilder() =>
      RegisterDeviceResponseBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is RegisterDeviceResponse &&
        id == other.id &&
        platform == other.platform &&
        registeredAt == other.registeredAt;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, id.hashCode);
    _$hash = $jc(_$hash, platform.hashCode);
    _$hash = $jc(_$hash, registeredAt.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'RegisterDeviceResponse')
          ..add('id', id)
          ..add('platform', platform)
          ..add('registeredAt', registeredAt))
        .toString();
  }
}

class RegisterDeviceResponseBuilder
    implements Builder<RegisterDeviceResponse, RegisterDeviceResponseBuilder> {
  _$RegisterDeviceResponse? _$v;

  String? _id;
  String? get id => _$this._id;
  set id(String? id) => _$this._id = id;

  String? _platform;
  String? get platform => _$this._platform;
  set platform(String? platform) => _$this._platform = platform;

  DateTime? _registeredAt;
  DateTime? get registeredAt => _$this._registeredAt;
  set registeredAt(DateTime? registeredAt) =>
      _$this._registeredAt = registeredAt;

  RegisterDeviceResponseBuilder() {
    RegisterDeviceResponse._defaults(this);
  }

  RegisterDeviceResponseBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _id = $v.id;
      _platform = $v.platform;
      _registeredAt = $v.registeredAt;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(RegisterDeviceResponse other) {
    _$v = other as _$RegisterDeviceResponse;
  }

  @override
  void update(void Function(RegisterDeviceResponseBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  RegisterDeviceResponse build() => _build();

  _$RegisterDeviceResponse _build() {
    final _$result =
        _$v ??
        _$RegisterDeviceResponse._(
          id: BuiltValueNullFieldError.checkNotNull(
            id,
            r'RegisterDeviceResponse',
            'id',
          ),
          platform: BuiltValueNullFieldError.checkNotNull(
            platform,
            r'RegisterDeviceResponse',
            'platform',
          ),
          registeredAt: BuiltValueNullFieldError.checkNotNull(
            registeredAt,
            r'RegisterDeviceResponse',
            'registeredAt',
          ),
        );
    replace(_$result);
    return _$result;
  }
}

// ignore_for_file: deprecated_member_use_from_same_package,type=lint
