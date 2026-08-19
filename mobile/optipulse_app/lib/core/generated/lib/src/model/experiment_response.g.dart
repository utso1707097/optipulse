// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'experiment_response.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$ExperimentResponse extends ExperimentResponse {
  @override
  final String? conversionGoal;
  @override
  final DateTime createdAt;
  @override
  final String flagKey;
  @override
  final String id;
  @override
  final String name;
  @override
  final String status;
  @override
  final DateTime updatedAt;
  @override
  final BuiltList<VariantDto> variants;
  @override
  final int version;

  factory _$ExperimentResponse([
    void Function(ExperimentResponseBuilder)? updates,
  ]) => (ExperimentResponseBuilder()..update(updates))._build();

  _$ExperimentResponse._({
    this.conversionGoal,
    required this.createdAt,
    required this.flagKey,
    required this.id,
    required this.name,
    required this.status,
    required this.updatedAt,
    required this.variants,
    required this.version,
  }) : super._();
  @override
  ExperimentResponse rebuild(
    void Function(ExperimentResponseBuilder) updates,
  ) => (toBuilder()..update(updates)).build();

  @override
  ExperimentResponseBuilder toBuilder() =>
      ExperimentResponseBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is ExperimentResponse &&
        conversionGoal == other.conversionGoal &&
        createdAt == other.createdAt &&
        flagKey == other.flagKey &&
        id == other.id &&
        name == other.name &&
        status == other.status &&
        updatedAt == other.updatedAt &&
        variants == other.variants &&
        version == other.version;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, conversionGoal.hashCode);
    _$hash = $jc(_$hash, createdAt.hashCode);
    _$hash = $jc(_$hash, flagKey.hashCode);
    _$hash = $jc(_$hash, id.hashCode);
    _$hash = $jc(_$hash, name.hashCode);
    _$hash = $jc(_$hash, status.hashCode);
    _$hash = $jc(_$hash, updatedAt.hashCode);
    _$hash = $jc(_$hash, variants.hashCode);
    _$hash = $jc(_$hash, version.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'ExperimentResponse')
          ..add('conversionGoal', conversionGoal)
          ..add('createdAt', createdAt)
          ..add('flagKey', flagKey)
          ..add('id', id)
          ..add('name', name)
          ..add('status', status)
          ..add('updatedAt', updatedAt)
          ..add('variants', variants)
          ..add('version', version))
        .toString();
  }
}

class ExperimentResponseBuilder
    implements Builder<ExperimentResponse, ExperimentResponseBuilder> {
  _$ExperimentResponse? _$v;

  String? _conversionGoal;
  String? get conversionGoal => _$this._conversionGoal;
  set conversionGoal(String? conversionGoal) =>
      _$this._conversionGoal = conversionGoal;

  DateTime? _createdAt;
  DateTime? get createdAt => _$this._createdAt;
  set createdAt(DateTime? createdAt) => _$this._createdAt = createdAt;

  String? _flagKey;
  String? get flagKey => _$this._flagKey;
  set flagKey(String? flagKey) => _$this._flagKey = flagKey;

  String? _id;
  String? get id => _$this._id;
  set id(String? id) => _$this._id = id;

  String? _name;
  String? get name => _$this._name;
  set name(String? name) => _$this._name = name;

  String? _status;
  String? get status => _$this._status;
  set status(String? status) => _$this._status = status;

  DateTime? _updatedAt;
  DateTime? get updatedAt => _$this._updatedAt;
  set updatedAt(DateTime? updatedAt) => _$this._updatedAt = updatedAt;

  ListBuilder<VariantDto>? _variants;
  ListBuilder<VariantDto> get variants =>
      _$this._variants ??= ListBuilder<VariantDto>();
  set variants(ListBuilder<VariantDto>? variants) =>
      _$this._variants = variants;

  int? _version;
  int? get version => _$this._version;
  set version(int? version) => _$this._version = version;

  ExperimentResponseBuilder() {
    ExperimentResponse._defaults(this);
  }

  ExperimentResponseBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _conversionGoal = $v.conversionGoal;
      _createdAt = $v.createdAt;
      _flagKey = $v.flagKey;
      _id = $v.id;
      _name = $v.name;
      _status = $v.status;
      _updatedAt = $v.updatedAt;
      _variants = $v.variants.toBuilder();
      _version = $v.version;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(ExperimentResponse other) {
    _$v = other as _$ExperimentResponse;
  }

  @override
  void update(void Function(ExperimentResponseBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  ExperimentResponse build() => _build();

  _$ExperimentResponse _build() {
    _$ExperimentResponse _$result;
    try {
      _$result =
          _$v ??
          _$ExperimentResponse._(
            conversionGoal: conversionGoal,
            createdAt: BuiltValueNullFieldError.checkNotNull(
              createdAt,
              r'ExperimentResponse',
              'createdAt',
            ),
            flagKey: BuiltValueNullFieldError.checkNotNull(
              flagKey,
              r'ExperimentResponse',
              'flagKey',
            ),
            id: BuiltValueNullFieldError.checkNotNull(
              id,
              r'ExperimentResponse',
              'id',
            ),
            name: BuiltValueNullFieldError.checkNotNull(
              name,
              r'ExperimentResponse',
              'name',
            ),
            status: BuiltValueNullFieldError.checkNotNull(
              status,
              r'ExperimentResponse',
              'status',
            ),
            updatedAt: BuiltValueNullFieldError.checkNotNull(
              updatedAt,
              r'ExperimentResponse',
              'updatedAt',
            ),
            variants: variants.build(),
            version: BuiltValueNullFieldError.checkNotNull(
              version,
              r'ExperimentResponse',
              'version',
            ),
          );
    } catch (_) {
      late String _$failedField;
      try {
        _$failedField = 'variants';
        variants.build();
      } catch (e) {
        throw BuiltValueNestedFieldError(
          r'ExperimentResponse',
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
