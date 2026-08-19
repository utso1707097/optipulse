// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'snapshot_version_response.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$SnapshotVersionResponse extends SnapshotVersionResponse {
  @override
  final DateTime builtAt;
  @override
  final int version;

  factory _$SnapshotVersionResponse([
    void Function(SnapshotVersionResponseBuilder)? updates,
  ]) => (SnapshotVersionResponseBuilder()..update(updates))._build();

  _$SnapshotVersionResponse._({required this.builtAt, required this.version})
    : super._();
  @override
  SnapshotVersionResponse rebuild(
    void Function(SnapshotVersionResponseBuilder) updates,
  ) => (toBuilder()..update(updates)).build();

  @override
  SnapshotVersionResponseBuilder toBuilder() =>
      SnapshotVersionResponseBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is SnapshotVersionResponse &&
        builtAt == other.builtAt &&
        version == other.version;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, builtAt.hashCode);
    _$hash = $jc(_$hash, version.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'SnapshotVersionResponse')
          ..add('builtAt', builtAt)
          ..add('version', version))
        .toString();
  }
}

class SnapshotVersionResponseBuilder
    implements
        Builder<SnapshotVersionResponse, SnapshotVersionResponseBuilder> {
  _$SnapshotVersionResponse? _$v;

  DateTime? _builtAt;
  DateTime? get builtAt => _$this._builtAt;
  set builtAt(DateTime? builtAt) => _$this._builtAt = builtAt;

  int? _version;
  int? get version => _$this._version;
  set version(int? version) => _$this._version = version;

  SnapshotVersionResponseBuilder() {
    SnapshotVersionResponse._defaults(this);
  }

  SnapshotVersionResponseBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _builtAt = $v.builtAt;
      _version = $v.version;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(SnapshotVersionResponse other) {
    _$v = other as _$SnapshotVersionResponse;
  }

  @override
  void update(void Function(SnapshotVersionResponseBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  SnapshotVersionResponse build() => _build();

  _$SnapshotVersionResponse _build() {
    final _$result =
        _$v ??
        _$SnapshotVersionResponse._(
          builtAt: BuiltValueNullFieldError.checkNotNull(
            builtAt,
            r'SnapshotVersionResponse',
            'builtAt',
          ),
          version: BuiltValueNullFieldError.checkNotNull(
            version,
            r'SnapshotVersionResponse',
            'version',
          ),
        );
    replace(_$result);
    return _$result;
  }
}

// ignore_for_file: deprecated_member_use_from_same_package,type=lint
