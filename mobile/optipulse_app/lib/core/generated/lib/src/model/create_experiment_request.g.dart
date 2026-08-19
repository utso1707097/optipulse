// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'create_experiment_request.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$CreateExperimentRequest extends CreateExperimentRequest {
  @override
  final String? conversionGoal;
  @override
  final String flagKey;
  @override
  final String name;
  @override
  final BuiltList<VariantDto> variants;

  factory _$CreateExperimentRequest([
    void Function(CreateExperimentRequestBuilder)? updates,
  ]) => (CreateExperimentRequestBuilder()..update(updates))._build();

  _$CreateExperimentRequest._({
    this.conversionGoal,
    required this.flagKey,
    required this.name,
    required this.variants,
  }) : super._();
  @override
  CreateExperimentRequest rebuild(
    void Function(CreateExperimentRequestBuilder) updates,
  ) => (toBuilder()..update(updates)).build();

  @override
  CreateExperimentRequestBuilder toBuilder() =>
      CreateExperimentRequestBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is CreateExperimentRequest &&
        conversionGoal == other.conversionGoal &&
        flagKey == other.flagKey &&
        name == other.name &&
        variants == other.variants;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, conversionGoal.hashCode);
    _$hash = $jc(_$hash, flagKey.hashCode);
    _$hash = $jc(_$hash, name.hashCode);
    _$hash = $jc(_$hash, variants.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'CreateExperimentRequest')
          ..add('conversionGoal', conversionGoal)
          ..add('flagKey', flagKey)
          ..add('name', name)
          ..add('variants', variants))
        .toString();
  }
}

class CreateExperimentRequestBuilder
    implements
        Builder<CreateExperimentRequest, CreateExperimentRequestBuilder> {
  _$CreateExperimentRequest? _$v;

  String? _conversionGoal;
  String? get conversionGoal => _$this._conversionGoal;
  set conversionGoal(String? conversionGoal) =>
      _$this._conversionGoal = conversionGoal;

  String? _flagKey;
  String? get flagKey => _$this._flagKey;
  set flagKey(String? flagKey) => _$this._flagKey = flagKey;

  String? _name;
  String? get name => _$this._name;
  set name(String? name) => _$this._name = name;

  ListBuilder<VariantDto>? _variants;
  ListBuilder<VariantDto> get variants =>
      _$this._variants ??= ListBuilder<VariantDto>();
  set variants(ListBuilder<VariantDto>? variants) =>
      _$this._variants = variants;

  CreateExperimentRequestBuilder() {
    CreateExperimentRequest._defaults(this);
  }

  CreateExperimentRequestBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _conversionGoal = $v.conversionGoal;
      _flagKey = $v.flagKey;
      _name = $v.name;
      _variants = $v.variants.toBuilder();
      _$v = null;
    }
    return this;
  }

  @override
  void replace(CreateExperimentRequest other) {
    _$v = other as _$CreateExperimentRequest;
  }

  @override
  void update(void Function(CreateExperimentRequestBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  CreateExperimentRequest build() => _build();

  _$CreateExperimentRequest _build() {
    _$CreateExperimentRequest _$result;
    try {
      _$result =
          _$v ??
          _$CreateExperimentRequest._(
            conversionGoal: conversionGoal,
            flagKey: BuiltValueNullFieldError.checkNotNull(
              flagKey,
              r'CreateExperimentRequest',
              'flagKey',
            ),
            name: BuiltValueNullFieldError.checkNotNull(
              name,
              r'CreateExperimentRequest',
              'name',
            ),
            variants: variants.build(),
          );
    } catch (_) {
      late String _$failedField;
      try {
        _$failedField = 'variants';
        variants.build();
      } catch (e) {
        throw BuiltValueNestedFieldError(
          r'CreateExperimentRequest',
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
