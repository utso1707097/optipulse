// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'change_status_request.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$ChangeStatusRequest extends ChangeStatusRequest {
  @override
  final String status;

  factory _$ChangeStatusRequest([
    void Function(ChangeStatusRequestBuilder)? updates,
  ]) => (ChangeStatusRequestBuilder()..update(updates))._build();

  _$ChangeStatusRequest._({required this.status}) : super._();
  @override
  ChangeStatusRequest rebuild(
    void Function(ChangeStatusRequestBuilder) updates,
  ) => (toBuilder()..update(updates)).build();

  @override
  ChangeStatusRequestBuilder toBuilder() =>
      ChangeStatusRequestBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is ChangeStatusRequest && status == other.status;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, status.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(
      r'ChangeStatusRequest',
    )..add('status', status)).toString();
  }
}

class ChangeStatusRequestBuilder
    implements Builder<ChangeStatusRequest, ChangeStatusRequestBuilder> {
  _$ChangeStatusRequest? _$v;

  String? _status;
  String? get status => _$this._status;
  set status(String? status) => _$this._status = status;

  ChangeStatusRequestBuilder() {
    ChangeStatusRequest._defaults(this);
  }

  ChangeStatusRequestBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _status = $v.status;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(ChangeStatusRequest other) {
    _$v = other as _$ChangeStatusRequest;
  }

  @override
  void update(void Function(ChangeStatusRequestBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  ChangeStatusRequest build() => _build();

  _$ChangeStatusRequest _build() {
    final _$result =
        _$v ??
        _$ChangeStatusRequest._(
          status: BuiltValueNullFieldError.checkNotNull(
            status,
            r'ChangeStatusRequest',
            'status',
          ),
        );
    replace(_$result);
    return _$result;
  }
}

// ignore_for_file: deprecated_member_use_from_same_package,type=lint
