// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'evaluate_request.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$EvaluateRequest extends EvaluateRequest {
  @override
  final BuiltMap<String, String>? attributes;
  @override
  final String? contextKey;
  @override
  final String flagKey;

  factory _$EvaluateRequest([void Function(EvaluateRequestBuilder)? updates]) =>
      (EvaluateRequestBuilder()..update(updates))._build();

  _$EvaluateRequest._({this.attributes, this.contextKey, required this.flagKey})
    : super._();
  @override
  EvaluateRequest rebuild(void Function(EvaluateRequestBuilder) updates) =>
      (toBuilder()..update(updates)).build();

  @override
  EvaluateRequestBuilder toBuilder() => EvaluateRequestBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is EvaluateRequest &&
        attributes == other.attributes &&
        contextKey == other.contextKey &&
        flagKey == other.flagKey;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, attributes.hashCode);
    _$hash = $jc(_$hash, contextKey.hashCode);
    _$hash = $jc(_$hash, flagKey.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'EvaluateRequest')
          ..add('attributes', attributes)
          ..add('contextKey', contextKey)
          ..add('flagKey', flagKey))
        .toString();
  }
}

class EvaluateRequestBuilder
    implements Builder<EvaluateRequest, EvaluateRequestBuilder> {
  _$EvaluateRequest? _$v;

  MapBuilder<String, String>? _attributes;
  MapBuilder<String, String> get attributes =>
      _$this._attributes ??= MapBuilder<String, String>();
  set attributes(MapBuilder<String, String>? attributes) =>
      _$this._attributes = attributes;

  String? _contextKey;
  String? get contextKey => _$this._contextKey;
  set contextKey(String? contextKey) => _$this._contextKey = contextKey;

  String? _flagKey;
  String? get flagKey => _$this._flagKey;
  set flagKey(String? flagKey) => _$this._flagKey = flagKey;

  EvaluateRequestBuilder() {
    EvaluateRequest._defaults(this);
  }

  EvaluateRequestBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _attributes = $v.attributes?.toBuilder();
      _contextKey = $v.contextKey;
      _flagKey = $v.flagKey;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(EvaluateRequest other) {
    _$v = other as _$EvaluateRequest;
  }

  @override
  void update(void Function(EvaluateRequestBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  EvaluateRequest build() => _build();

  _$EvaluateRequest _build() {
    _$EvaluateRequest _$result;
    try {
      _$result =
          _$v ??
          _$EvaluateRequest._(
            attributes: _attributes?.build(),
            contextKey: contextKey,
            flagKey: BuiltValueNullFieldError.checkNotNull(
              flagKey,
              r'EvaluateRequest',
              'flagKey',
            ),
          );
    } catch (_) {
      late String _$failedField;
      try {
        _$failedField = 'attributes';
        _attributes?.build();
      } catch (e) {
        throw BuiltValueNestedFieldError(
          r'EvaluateRequest',
          _$failedField,
          e.toString(),
        );
      }
      rethrow;
    }
    replace(_$result);
    return _$result;
  }
}

// ignore_for_file: deprecated_member_use_from_same_package,type=lint
