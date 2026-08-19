// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'update_experiment_request.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$UpdateExperimentRequest extends UpdateExperimentRequest {
  @override
  final BuiltList<VariantDto> variants;

  factory _$UpdateExperimentRequest([
    void Function(UpdateExperimentRequestBuilder)? updates,
  ]) => (UpdateExperimentRequestBuilder()..update(updates))._build();

  _$UpdateExperimentRequest._({required this.variants}) : super._();
  @override
  UpdateExperimentRequest rebuild(
    void Function(UpdateExperimentRequestBuilder) updates,
  ) => (toBuilder()..update(updates)).build();

  @override
  UpdateExperimentRequestBuilder toBuilder() =>
      UpdateExperimentRequestBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is UpdateExperimentRequest && variants == other.variants;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, variants.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(
      r'UpdateExperimentRequest',
    )..add('variants', variants)).toString();
  }
}

class UpdateExperimentRequestBuilder
    implements
        Builder<UpdateExperimentRequest, UpdateExperimentRequestBuilder> {
  _$UpdateExperimentRequest? _$v;

  ListBuilder<VariantDto>? _variants;
  ListBuilder<VariantDto> get variants =>
      _$this._variants ??= ListBuilder<VariantDto>();
  set variants(ListBuilder<VariantDto>? variants) =>
      _$this._variants = variants;

  UpdateExperimentRequestBuilder() {
    UpdateExperimentRequest._defaults(this);
  }

  UpdateExperimentRequestBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _variants = $v.variants.toBuilder();
      _$v = null;
    }
    return this;
  }

  @override
  void replace(UpdateExperimentRequest other) {
    _$v = other as _$UpdateExperimentRequest;
  }

  @override
  void update(void Function(UpdateExperimentRequestBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  UpdateExperimentRequest build() => _build();

  _$UpdateExperimentRequest _build() {
    _$UpdateExperimentRequest _$result;
    try {
      _$result = _$v ?? _$UpdateExperimentRequest._(variants: variants.build());
    } catch (_) {
      late String _$failedField;
      try {
        _$failedField = 'variants';
        variants.build();
      } catch (e) {
        throw BuiltValueNestedFieldError(
          r'UpdateExperimentRequest',
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
