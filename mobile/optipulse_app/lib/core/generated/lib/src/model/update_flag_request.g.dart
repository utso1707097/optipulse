// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'update_flag_request.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$UpdateFlagRequest extends UpdateFlagRequest {
  @override
  final bool defaultOutcome;
  @override
  final String name;
  @override
  final RolloutDto? rollout;
  @override
  final BuiltList<TargetingRuleDto>? targetingRules;

  factory _$UpdateFlagRequest([
    void Function(UpdateFlagRequestBuilder)? updates,
  ]) => (UpdateFlagRequestBuilder()..update(updates))._build();

  _$UpdateFlagRequest._({
    required this.defaultOutcome,
    required this.name,
    this.rollout,
    this.targetingRules,
  }) : super._();
  @override
  UpdateFlagRequest rebuild(void Function(UpdateFlagRequestBuilder) updates) =>
      (toBuilder()..update(updates)).build();

  @override
  UpdateFlagRequestBuilder toBuilder() =>
      UpdateFlagRequestBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is UpdateFlagRequest &&
        defaultOutcome == other.defaultOutcome &&
        name == other.name &&
        rollout == other.rollout &&
        targetingRules == other.targetingRules;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, defaultOutcome.hashCode);
    _$hash = $jc(_$hash, name.hashCode);
    _$hash = $jc(_$hash, rollout.hashCode);
    _$hash = $jc(_$hash, targetingRules.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'UpdateFlagRequest')
          ..add('defaultOutcome', defaultOutcome)
          ..add('name', name)
          ..add('rollout', rollout)
          ..add('targetingRules', targetingRules))
        .toString();
  }
}

class UpdateFlagRequestBuilder
    implements Builder<UpdateFlagRequest, UpdateFlagRequestBuilder> {
  _$UpdateFlagRequest? _$v;

  bool? _defaultOutcome;
  bool? get defaultOutcome => _$this._defaultOutcome;
  set defaultOutcome(bool? defaultOutcome) =>
      _$this._defaultOutcome = defaultOutcome;

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

  UpdateFlagRequestBuilder() {
    UpdateFlagRequest._defaults(this);
  }

  UpdateFlagRequestBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _defaultOutcome = $v.defaultOutcome;
      _name = $v.name;
      _rollout = $v.rollout?.toBuilder();
      _targetingRules = $v.targetingRules?.toBuilder();
      _$v = null;
    }
    return this;
  }

  @override
  void replace(UpdateFlagRequest other) {
    _$v = other as _$UpdateFlagRequest;
  }

  @override
  void update(void Function(UpdateFlagRequestBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  UpdateFlagRequest build() => _build();

  _$UpdateFlagRequest _build() {
    _$UpdateFlagRequest _$result;
    try {
      _$result =
          _$v ??
          _$UpdateFlagRequest._(
            defaultOutcome: BuiltValueNullFieldError.checkNotNull(
              defaultOutcome,
              r'UpdateFlagRequest',
              'defaultOutcome',
            ),
            name: BuiltValueNullFieldError.checkNotNull(
              name,
              r'UpdateFlagRequest',
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
          r'UpdateFlagRequest',
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
