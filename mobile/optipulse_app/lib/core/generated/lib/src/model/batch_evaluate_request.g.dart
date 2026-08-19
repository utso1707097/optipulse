// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'batch_evaluate_request.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$BatchEvaluateRequest extends BatchEvaluateRequest {
  @override
  final BuiltMap<String, String>? attributes;
  @override
  final String? contextKey;
  @override
  final BuiltList<String> flagKeys;

  factory _$BatchEvaluateRequest([
    void Function(BatchEvaluateRequestBuilder)? updates,
  ]) => (BatchEvaluateRequestBuilder()..update(updates))._build();

  _$BatchEvaluateRequest._({
    this.attributes,
    this.contextKey,
    required this.flagKeys,
  }) : super._();
  @override
  BatchEvaluateRequest rebuild(
    void Function(BatchEvaluateRequestBuilder) updates,
  ) => (toBuilder()..update(updates)).build();

  @override
  BatchEvaluateRequestBuilder toBuilder() =>
      BatchEvaluateRequestBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is BatchEvaluateRequest &&
        attributes == other.attributes &&
        contextKey == other.contextKey &&
        flagKeys == other.flagKeys;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, attributes.hashCode);
    _$hash = $jc(_$hash, contextKey.hashCode);
    _$hash = $jc(_$hash, flagKeys.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'BatchEvaluateRequest')
          ..add('attributes', attributes)
          ..add('contextKey', contextKey)
          ..add('flagKeys', flagKeys))
        .toString();
  }
}

class BatchEvaluateRequestBuilder
    implements Builder<BatchEvaluateRequest, BatchEvaluateRequestBuilder> {
  _$BatchEvaluateRequest? _$v;

  MapBuilder<String, String>? _attributes;
  MapBuilder<String, String> get attributes =>
      _$this._attributes ??= MapBuilder<String, String>();
  set attributes(MapBuilder<String, String>? attributes) =>
      _$this._attributes = attributes;

  String? _contextKey;
  String? get contextKey => _$this._contextKey;
  set contextKey(String? contextKey) => _$this._contextKey = contextKey;

  ListBuilder<String>? _flagKeys;
  ListBuilder<String> get flagKeys =>
      _$this._flagKeys ??= ListBuilder<String>();
  set flagKeys(ListBuilder<String>? flagKeys) => _$this._flagKeys = flagKeys;

  BatchEvaluateRequestBuilder() {
    BatchEvaluateRequest._defaults(this);
  }

  BatchEvaluateRequestBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _attributes = $v.attributes?.toBuilder();
      _contextKey = $v.contextKey;
      _flagKeys = $v.flagKeys.toBuilder();
      _$v = null;
    }
    return this;
  }

  @override
  void replace(BatchEvaluateRequest other) {
    _$v = other as _$BatchEvaluateRequest;
  }

  @override
  void update(void Function(BatchEvaluateRequestBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  BatchEvaluateRequest build() => _build();

  _$BatchEvaluateRequest _build() {
    _$BatchEvaluateRequest _$result;
    try {
      _$result =
          _$v ??
          _$BatchEvaluateRequest._(
            attributes: _attributes?.build(),
            contextKey: contextKey,
            flagKeys: flagKeys.build(),
          );
    } catch (_) {
      late String _$failedField;
      try {
        _$failedField = 'attributes';
        _attributes?.build();

        _$failedField = 'flagKeys';
        flagKeys.build();
      } catch (e) {
        throw BuiltValueNestedFieldError(
          r'BatchEvaluateRequest',
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
