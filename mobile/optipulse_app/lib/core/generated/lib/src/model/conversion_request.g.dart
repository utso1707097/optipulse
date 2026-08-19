// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'conversion_request.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$ConversionRequest extends ConversionRequest {
  @override
  final String? contextKey;
  @override
  final String? experimentId;
  @override
  final String flagKey;
  @override
  final String goal;
  @override
  final String idempotencyKey;
  @override
  final double? value;
  @override
  final String? variantKey;

  factory _$ConversionRequest([
    void Function(ConversionRequestBuilder)? updates,
  ]) => (ConversionRequestBuilder()..update(updates))._build();

  _$ConversionRequest._({
    this.contextKey,
    this.experimentId,
    required this.flagKey,
    required this.goal,
    required this.idempotencyKey,
    this.value,
    this.variantKey,
  }) : super._();
  @override
  ConversionRequest rebuild(void Function(ConversionRequestBuilder) updates) =>
      (toBuilder()..update(updates)).build();

  @override
  ConversionRequestBuilder toBuilder() =>
      ConversionRequestBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is ConversionRequest &&
        contextKey == other.contextKey &&
        experimentId == other.experimentId &&
        flagKey == other.flagKey &&
        goal == other.goal &&
        idempotencyKey == other.idempotencyKey &&
        value == other.value &&
        variantKey == other.variantKey;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, contextKey.hashCode);
    _$hash = $jc(_$hash, experimentId.hashCode);
    _$hash = $jc(_$hash, flagKey.hashCode);
    _$hash = $jc(_$hash, goal.hashCode);
    _$hash = $jc(_$hash, idempotencyKey.hashCode);
    _$hash = $jc(_$hash, value.hashCode);
    _$hash = $jc(_$hash, variantKey.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'ConversionRequest')
          ..add('contextKey', contextKey)
          ..add('experimentId', experimentId)
          ..add('flagKey', flagKey)
          ..add('goal', goal)
          ..add('idempotencyKey', idempotencyKey)
          ..add('value', value)
          ..add('variantKey', variantKey))
        .toString();
  }
}

class ConversionRequestBuilder
    implements Builder<ConversionRequest, ConversionRequestBuilder> {
  _$ConversionRequest? _$v;

  String? _contextKey;
  String? get contextKey => _$this._contextKey;
  set contextKey(String? contextKey) => _$this._contextKey = contextKey;

  String? _experimentId;
  String? get experimentId => _$this._experimentId;
  set experimentId(String? experimentId) => _$this._experimentId = experimentId;

  String? _flagKey;
  String? get flagKey => _$this._flagKey;
  set flagKey(String? flagKey) => _$this._flagKey = flagKey;

  String? _goal;
  String? get goal => _$this._goal;
  set goal(String? goal) => _$this._goal = goal;

  String? _idempotencyKey;
  String? get idempotencyKey => _$this._idempotencyKey;
  set idempotencyKey(String? idempotencyKey) =>
      _$this._idempotencyKey = idempotencyKey;

  double? _value;
  double? get value => _$this._value;
  set value(double? value) => _$this._value = value;

  String? _variantKey;
  String? get variantKey => _$this._variantKey;
  set variantKey(String? variantKey) => _$this._variantKey = variantKey;

  ConversionRequestBuilder() {
    ConversionRequest._defaults(this);
  }

  ConversionRequestBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _contextKey = $v.contextKey;
      _experimentId = $v.experimentId;
      _flagKey = $v.flagKey;
      _goal = $v.goal;
      _idempotencyKey = $v.idempotencyKey;
      _value = $v.value;
      _variantKey = $v.variantKey;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(ConversionRequest other) {
    _$v = other as _$ConversionRequest;
  }

  @override
  void update(void Function(ConversionRequestBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  ConversionRequest build() => _build();

  _$ConversionRequest _build() {
    final _$result =
        _$v ??
        _$ConversionRequest._(
          contextKey: contextKey,
          experimentId: experimentId,
          flagKey: BuiltValueNullFieldError.checkNotNull(
            flagKey,
            r'ConversionRequest',
            'flagKey',
          ),
          goal: BuiltValueNullFieldError.checkNotNull(
            goal,
            r'ConversionRequest',
            'goal',
          ),
          idempotencyKey: BuiltValueNullFieldError.checkNotNull(
            idempotencyKey,
            r'ConversionRequest',
            'idempotencyKey',
          ),
          value: value,
          variantKey: variantKey,
        );
    replace(_$result);
    return _$result;
  }
}

// ignore_for_file: deprecated_member_use_from_same_package,type=lint
