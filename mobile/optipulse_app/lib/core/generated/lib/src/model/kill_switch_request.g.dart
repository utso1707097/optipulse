// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'kill_switch_request.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$KillSwitchRequest extends KillSwitchRequest {
  @override
  final bool engaged;

  factory _$KillSwitchRequest([
    void Function(KillSwitchRequestBuilder)? updates,
  ]) => (KillSwitchRequestBuilder()..update(updates))._build();

  _$KillSwitchRequest._({required this.engaged}) : super._();
  @override
  KillSwitchRequest rebuild(void Function(KillSwitchRequestBuilder) updates) =>
      (toBuilder()..update(updates)).build();

  @override
  KillSwitchRequestBuilder toBuilder() =>
      KillSwitchRequestBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is KillSwitchRequest && engaged == other.engaged;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, engaged.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(
      r'KillSwitchRequest',
    )..add('engaged', engaged)).toString();
  }
}

class KillSwitchRequestBuilder
    implements Builder<KillSwitchRequest, KillSwitchRequestBuilder> {
  _$KillSwitchRequest? _$v;

  bool? _engaged;
  bool? get engaged => _$this._engaged;
  set engaged(bool? engaged) => _$this._engaged = engaged;

  KillSwitchRequestBuilder() {
    KillSwitchRequest._defaults(this);
  }

  KillSwitchRequestBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _engaged = $v.engaged;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(KillSwitchRequest other) {
    _$v = other as _$KillSwitchRequest;
  }

  @override
  void update(void Function(KillSwitchRequestBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  KillSwitchRequest build() => _build();

  _$KillSwitchRequest _build() {
    final _$result =
        _$v ??
        _$KillSwitchRequest._(
          engaged: BuiltValueNullFieldError.checkNotNull(
            engaged,
            r'KillSwitchRequest',
            'engaged',
          ),
        );
    replace(_$result);
    return _$result;
  }
}

// ignore_for_file: deprecated_member_use_from_same_package,type=lint
