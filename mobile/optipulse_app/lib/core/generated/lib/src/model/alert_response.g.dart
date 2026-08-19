// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'alert_response.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$AlertResponse extends AlertResponse {
  @override
  final DateTime? acknowledgedAt;
  @override
  final String? acknowledgedBy;
  @override
  final String detail;
  @override
  final String? flagKey;
  @override
  final String id;
  @override
  final String kind;
  @override
  final DateTime raisedAt;
  @override
  final String severity;
  @override
  final String title;

  factory _$AlertResponse([void Function(AlertResponseBuilder)? updates]) =>
      (AlertResponseBuilder()..update(updates))._build();

  _$AlertResponse._({
    this.acknowledgedAt,
    this.acknowledgedBy,
    required this.detail,
    this.flagKey,
    required this.id,
    required this.kind,
    required this.raisedAt,
    required this.severity,
    required this.title,
  }) : super._();
  @override
  AlertResponse rebuild(void Function(AlertResponseBuilder) updates) =>
      (toBuilder()..update(updates)).build();

  @override
  AlertResponseBuilder toBuilder() => AlertResponseBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is AlertResponse &&
        acknowledgedAt == other.acknowledgedAt &&
        acknowledgedBy == other.acknowledgedBy &&
        detail == other.detail &&
        flagKey == other.flagKey &&
        id == other.id &&
        kind == other.kind &&
        raisedAt == other.raisedAt &&
        severity == other.severity &&
        title == other.title;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, acknowledgedAt.hashCode);
    _$hash = $jc(_$hash, acknowledgedBy.hashCode);
    _$hash = $jc(_$hash, detail.hashCode);
    _$hash = $jc(_$hash, flagKey.hashCode);
    _$hash = $jc(_$hash, id.hashCode);
    _$hash = $jc(_$hash, kind.hashCode);
    _$hash = $jc(_$hash, raisedAt.hashCode);
    _$hash = $jc(_$hash, severity.hashCode);
    _$hash = $jc(_$hash, title.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'AlertResponse')
          ..add('acknowledgedAt', acknowledgedAt)
          ..add('acknowledgedBy', acknowledgedBy)
          ..add('detail', detail)
          ..add('flagKey', flagKey)
          ..add('id', id)
          ..add('kind', kind)
          ..add('raisedAt', raisedAt)
          ..add('severity', severity)
          ..add('title', title))
        .toString();
  }
}

class AlertResponseBuilder
    implements Builder<AlertResponse, AlertResponseBuilder> {
  _$AlertResponse? _$v;

  DateTime? _acknowledgedAt;
  DateTime? get acknowledgedAt => _$this._acknowledgedAt;
  set acknowledgedAt(DateTime? acknowledgedAt) =>
      _$this._acknowledgedAt = acknowledgedAt;

  String? _acknowledgedBy;
  String? get acknowledgedBy => _$this._acknowledgedBy;
  set acknowledgedBy(String? acknowledgedBy) =>
      _$this._acknowledgedBy = acknowledgedBy;

  String? _detail;
  String? get detail => _$this._detail;
  set detail(String? detail) => _$this._detail = detail;

  String? _flagKey;
  String? get flagKey => _$this._flagKey;
  set flagKey(String? flagKey) => _$this._flagKey = flagKey;

  String? _id;
  String? get id => _$this._id;
  set id(String? id) => _$this._id = id;

  String? _kind;
  String? get kind => _$this._kind;
  set kind(String? kind) => _$this._kind = kind;

  DateTime? _raisedAt;
  DateTime? get raisedAt => _$this._raisedAt;
  set raisedAt(DateTime? raisedAt) => _$this._raisedAt = raisedAt;

  String? _severity;
  String? get severity => _$this._severity;
  set severity(String? severity) => _$this._severity = severity;

  String? _title;
  String? get title => _$this._title;
  set title(String? title) => _$this._title = title;

  AlertResponseBuilder() {
    AlertResponse._defaults(this);
  }

  AlertResponseBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _acknowledgedAt = $v.acknowledgedAt;
      _acknowledgedBy = $v.acknowledgedBy;
      _detail = $v.detail;
      _flagKey = $v.flagKey;
      _id = $v.id;
      _kind = $v.kind;
      _raisedAt = $v.raisedAt;
      _severity = $v.severity;
      _title = $v.title;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(AlertResponse other) {
    _$v = other as _$AlertResponse;
  }

  @override
  void update(void Function(AlertResponseBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  AlertResponse build() => _build();

  _$AlertResponse _build() {
    final _$result =
        _$v ??
        _$AlertResponse._(
          acknowledgedAt: acknowledgedAt,
          acknowledgedBy: acknowledgedBy,
          detail: BuiltValueNullFieldError.checkNotNull(
            detail,
            r'AlertResponse',
            'detail',
          ),
          flagKey: flagKey,
          id: BuiltValueNullFieldError.checkNotNull(id, r'AlertResponse', 'id'),
          kind: BuiltValueNullFieldError.checkNotNull(
            kind,
            r'AlertResponse',
            'kind',
          ),
          raisedAt: BuiltValueNullFieldError.checkNotNull(
            raisedAt,
            r'AlertResponse',
            'raisedAt',
          ),
          severity: BuiltValueNullFieldError.checkNotNull(
            severity,
            r'AlertResponse',
            'severity',
          ),
          title: BuiltValueNullFieldError.checkNotNull(
            title,
            r'AlertResponse',
            'title',
          ),
        );
    replace(_$result);
    return _$result;
  }
}

// ignore_for_file: deprecated_member_use_from_same_package,type=lint
