// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'batch_evaluate_response.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$BatchEvaluateResponse extends BatchEvaluateResponse {
  @override
  final BuiltList<EvaluateResponse> results;
  @override
  final int snapshotVersion;

  factory _$BatchEvaluateResponse([
    void Function(BatchEvaluateResponseBuilder)? updates,
  ]) => (BatchEvaluateResponseBuilder()..update(updates))._build();

  _$BatchEvaluateResponse._({
    required this.results,
    required this.snapshotVersion,
  }) : super._();
  @override
  BatchEvaluateResponse rebuild(
    void Function(BatchEvaluateResponseBuilder) updates,
  ) => (toBuilder()..update(updates)).build();

  @override
  BatchEvaluateResponseBuilder toBuilder() =>
      BatchEvaluateResponseBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is BatchEvaluateResponse &&
        results == other.results &&
        snapshotVersion == other.snapshotVersion;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, results.hashCode);
    _$hash = $jc(_$hash, snapshotVersion.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'BatchEvaluateResponse')
          ..add('results', results)
          ..add('snapshotVersion', snapshotVersion))
        .toString();
  }
}

class BatchEvaluateResponseBuilder
    implements Builder<BatchEvaluateResponse, BatchEvaluateResponseBuilder> {
  _$BatchEvaluateResponse? _$v;

  ListBuilder<EvaluateResponse>? _results;
  ListBuilder<EvaluateResponse> get results =>
      _$this._results ??= ListBuilder<EvaluateResponse>();
  set results(ListBuilder<EvaluateResponse>? results) =>
      _$this._results = results;

  int? _snapshotVersion;
  int? get snapshotVersion => _$this._snapshotVersion;
  set snapshotVersion(int? snapshotVersion) =>
      _$this._snapshotVersion = snapshotVersion;

  BatchEvaluateResponseBuilder() {
    BatchEvaluateResponse._defaults(this);
  }

  BatchEvaluateResponseBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _results = $v.results.toBuilder();
      _snapshotVersion = $v.snapshotVersion;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(BatchEvaluateResponse other) {
    _$v = other as _$BatchEvaluateResponse;
  }

  @override
  void update(void Function(BatchEvaluateResponseBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  BatchEvaluateResponse build() => _build();

  _$BatchEvaluateResponse _build() {
    _$BatchEvaluateResponse _$result;
    try {
      _$result =
          _$v ??
          _$BatchEvaluateResponse._(
            results: results.build(),
            snapshotVersion: BuiltValueNullFieldError.checkNotNull(
              snapshotVersion,
              r'BatchEvaluateResponse',
              'snapshotVersion',
            ),
          );
    } catch (_) {
      late String _$failedField;
      try {
        _$failedField = 'results';
        results.build();
      } catch (e) {
        throw BuiltValueNestedFieldError(
          r'BatchEvaluateResponse',
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
