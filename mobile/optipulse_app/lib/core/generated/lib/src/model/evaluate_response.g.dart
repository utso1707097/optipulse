// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'evaluate_response.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$EvaluateResponse extends EvaluateResponse {
  @override
  final String flagKey;
  @override
  final bool outcome;
  @override
  final String reason;
  @override
  final int snapshotVersion;
  @override
  final String? variantKey;

  factory _$EvaluateResponse([
    void Function(EvaluateResponseBuilder)? updates,
  ]) => (EvaluateResponseBuilder()..update(updates))._build();

  _$EvaluateResponse._({
    required this.flagKey,
    required this.outcome,
    required this.reason,
    required this.snapshotVersion,
    this.variantKey,
  }) : super._();
  @override
  EvaluateResponse rebuild(void Function(EvaluateResponseBuilder) updates) =>
      (toBuilder()..update(updates)).build();

  @override
  EvaluateResponseBuilder toBuilder() =>
      EvaluateResponseBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is EvaluateResponse &&
        flagKey == other.flagKey &&
        outcome == other.outcome &&
        reason == other.reason &&
        snapshotVersion == other.snapshotVersion &&
        variantKey == other.variantKey;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, flagKey.hashCode);
    _$hash = $jc(_$hash, outcome.hashCode);
    _$hash = $jc(_$hash, reason.hashCode);
    _$hash = $jc(_$hash, snapshotVersion.hashCode);
    _$hash = $jc(_$hash, variantKey.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'EvaluateResponse')
          ..add('flagKey', flagKey)
          ..add('outcome', outcome)
          ..add('reason', reason)
          ..add('snapshotVersion', snapshotVersion)
          ..add('variantKey', variantKey))
        .toString();
  }
}

class EvaluateResponseBuilder
    implements Builder<EvaluateResponse, EvaluateResponseBuilder> {
  _$EvaluateResponse? _$v;

  String? _flagKey;
  String? get flagKey => _$this._flagKey;
  set flagKey(String? flagKey) => _$this._flagKey = flagKey;

  bool? _outcome;
  bool? get outcome => _$this._outcome;
  set outcome(bool? outcome) => _$this._outcome = outcome;

  String? _reason;
  String? get reason => _$this._reason;
  set reason(String? reason) => _$this._reason = reason;

  int? _snapshotVersion;
  int? get snapshotVersion => _$this._snapshotVersion;
  set snapshotVersion(int? snapshotVersion) =>
      _$this._snapshotVersion = snapshotVersion;

  String? _variantKey;
  String? get variantKey => _$this._variantKey;
  set variantKey(String? variantKey) => _$this._variantKey = variantKey;

  EvaluateResponseBuilder() {
    EvaluateResponse._defaults(this);
  }

  EvaluateResponseBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _flagKey = $v.flagKey;
      _outcome = $v.outcome;
      _reason = $v.reason;
      _snapshotVersion = $v.snapshotVersion;
      _variantKey = $v.variantKey;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(EvaluateResponse other) {
    _$v = other as _$EvaluateResponse;
  }

  @override
  void update(void Function(EvaluateResponseBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  EvaluateResponse build() => _build();

  _$EvaluateResponse _build() {
    final _$result =
        _$v ??
        _$EvaluateResponse._(
          flagKey: BuiltValueNullFieldError.checkNotNull(
            flagKey,
            r'EvaluateResponse',
            'flagKey',
          ),
          outcome: BuiltValueNullFieldError.checkNotNull(
            outcome,
            r'EvaluateResponse',
            'outcome',
          ),
          reason: BuiltValueNullFieldError.checkNotNull(
            reason,
            r'EvaluateResponse',
            'reason',
          ),
          snapshotVersion: BuiltValueNullFieldError.checkNotNull(
            snapshotVersion,
            r'EvaluateResponse',
            'snapshotVersion',
          ),
          variantKey: variantKey,
        );
    replace(_$result);
    return _$result;
  }
}

// ignore_for_file: deprecated_member_use_from_same_package,type=lint
