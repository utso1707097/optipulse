// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'create_flag_request.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$CreateFlagRequest extends CreateFlagRequest {
  @override
  final bool defaultOutcome;
  @override
  final String key;
  @override
  final String name;
  @override
  final RolloutDto? rollout;
  @override
  final BuiltList<TargetingRuleDto>? targetingRules;

  factory _$CreateFlagRequest([
    void Function(CreateFlagRequestBuilder)? updates,
  ]) => (CreateFlagRequestBuilder()..update(updates))._build();

  _$CreateFlagRequest._({
    required this.defaultOutcome,
    required this.key,
    required this.name,
    this.rollout,
    this.targetingRules,
  }) : super._();
  @override
  CreateFlagRequest rebuild(void Function(CreateFlagRequestBuilder) updates) =>
      (toBuilder()..update(updates)).build();

  @override
  CreateFlagRequestBuilder toBuilder() =>
      CreateFlagRequestBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is CreateFlagRequest &&
        defaultOutcome == other.defaultOutcome &&
        key == other.key &&
        name == other.name &&
        rollout == other.rollout &&
        targetingRules == other.targetingRules;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, defaultOutcome.hashCode);
    _$hash = $jc(_$hash, key.hashCode);
    _$hash = $jc(_$hash, name.hashCode);
    _$hash = $jc(_$hash, rollout.hashCode);
    _$hash = $jc(_$hash, targetingRules.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'CreateFlagRequest')
          ..add('defaultOutcome', defaultOutcome)
          ..add('key', key)
          ..add('name', name)
          ..add('rollout', rollout)
          ..add('targetingRules', targetingRules))
        .toString();
  }
}

class CreateFlagRequestBuilder
    implements Builder<CreateFlagRequest, CreateFlagRequestBuilder> {
  _$CreateFlagRequest? _$v;

  bool? _defaultOutcome;
  bool? get defaultOutcome => _$this._defaultOutcome;
  set defaultOutcome(bool? defaultOutcome) =>
      _$this._defaultOutcome = defaultOutcome;

  String? _key;
  String? get key => _$this._key;
  set key(String? key) => _$this._key = key;

  String? _name;
  String? get name => _$this._name;
  set name(String? name) => _$this._name = name;

  RolloutDtoBuilder? _rollout;
  RolloutDtoBuilder get rollout => _$this._rollout ??= RolloutDtoBuilder();
  set rollout(RolloutDtoBuilder? rollout) => _$this._rollout = rollout;

  ListBuilder<TargetingRuleDto>? _targetingRules;
  ListBuilder<TargetingRuleDto> get targetingRules =>
      _$this._targetingRules ??= ListBuilder<TargetingRuleDto>();
  set targetingRules(ListBuilder<TargetingRuleDto>? targetingRules) =>
      _$this._targetingRules = targetingRules;

  CreateFlagRequestBuilder() {
    CreateFlagRequest._defaults(this);
  }

  CreateFlagRequestBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _defaultOutcome = $v.defaultOutcome;
      _key = $v.key;
      _name = $v.name;
      _rollout = $v.rollout?.toBuilder();
      _targetingRules = $v.targetingRules?.toBuilder();
      _$v = null;
    }
    return this;
  }

  @override
  void replace(CreateFlagRequest other) {
    _$v = other as _$CreateFlagRequest;
  }

  @override
  void update(void Function(CreateFlagRequestBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  CreateFlagRequest build() => _build();

  _$CreateFlagRequest _build() {
    _$CreateFlagRequest _$result;
    try {
      _$result =
          _$v ??
          _$CreateFlagRequest._(
            defaultOutcome: BuiltValueNullFieldError.checkNotNull(
              defaultOutcome,
              r'CreateFlagRequest',
              'defaultOutcome',
            ),
            key: BuiltValueNullFieldError.checkNotNull(
              key,
              r'CreateFlagRequest',
              'key',
            ),
            name: BuiltValueNullFieldError.checkNotNull(
              name,
              r'CreateFlagRequest',
              'name',
            ),
            rollout: _rollout?.build(),
            targetingRules: _targetingRules?.build(),
          );
    } catch (_) {
      late String _$failedField;
      try {
        _$failedField = 'rollout';
        _rollout?.build();
        _$failedField = 'targetingRules';
        _targetingRules?.build();
      } catch (e) {
        throw BuiltValueNestedFieldError(
          r'CreateFlagRequest',
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
