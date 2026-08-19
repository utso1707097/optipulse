// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'flag_response.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$FlagResponse extends FlagResponse {
  @override
  final DateTime createdAt;
  @override
  final bool defaultOutcome;
  @override
  final String id;
  @override
  final String key;
  @override
  final bool killSwitchEngaged;
  @override
  final String name;
  @override
  final RolloutDto? rollout;
  @override
  final String status;
  @override
  final BuiltList<TargetingRuleDto> targetingRules;
  @override
  final DateTime updatedAt;
  @override
  final int version;

  factory _$FlagResponse([void Function(FlagResponseBuilder)? updates]) =>
      (FlagResponseBuilder()..update(updates))._build();

  _$FlagResponse._({
    required this.createdAt,
    required this.defaultOutcome,
    required this.id,
    required this.key,
    required this.killSwitchEngaged,
    required this.name,
    this.rollout,
    required this.status,
    required this.targetingRules,
    required this.updatedAt,
    required this.version,
  }) : super._();
  @override
  FlagResponse rebuild(void Function(FlagResponseBuilder) updates) =>
      (toBuilder()..update(updates)).build();

  @override
  FlagResponseBuilder toBuilder() => FlagResponseBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is FlagResponse &&
        createdAt == other.createdAt &&
        defaultOutcome == other.defaultOutcome &&
        id == other.id &&
        key == other.key &&
        killSwitchEngaged == other.killSwitchEngaged &&
        name == other.name &&
        rollout == other.rollout &&
        status == other.status &&
        targetingRules == other.targetingRules &&
        updatedAt == other.updatedAt &&
        version == other.version;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, createdAt.hashCode);
    _$hash = $jc(_$hash, defaultOutcome.hashCode);
    _$hash = $jc(_$hash, id.hashCode);
    _$hash = $jc(_$hash, key.hashCode);
    _$hash = $jc(_$hash, killSwitchEngaged.hashCode);
    _$hash = $jc(_$hash, name.hashCode);
    _$hash = $jc(_$hash, rollout.hashCode);
    _$hash = $jc(_$hash, status.hashCode);
    _$hash = $jc(_$hash, targetingRules.hashCode);
    _$hash = $jc(_$hash, updatedAt.hashCode);
    _$hash = $jc(_$hash, version.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'FlagResponse')
          ..add('createdAt', createdAt)
          ..add('defaultOutcome', defaultOutcome)
          ..add('id', id)
          ..add('key', key)
          ..add('killSwitchEngaged', killSwitchEngaged)
          ..add('name', name)
          ..add('rollout', rollout)
          ..add('status', status)
          ..add('targetingRules', targetingRules)
          ..add('updatedAt', updatedAt)
          ..add('version', version))
        .toString();
  }
}

class FlagResponseBuilder
    implements Builder<FlagResponse, FlagResponseBuilder> {
  _$FlagResponse? _$v;

  DateTime? _createdAt;
  DateTime? get createdAt => _$this._createdAt;
  set createdAt(DateTime? createdAt) => _$this._createdAt = createdAt;

  bool? _defaultOutcome;
  bool? get defaultOutcome => _$this._defaultOutcome;
  set defaultOutcome(bool? defaultOutcome) =>
      _$this._defaultOutcome = defaultOutcome;

  String? _id;
  String? get id => _$this._id;
  set id(String? id) => _$this._id = id;

  String? _key;
  String? get key => _$this._key;
  set key(String? key) => _$this._key = key;

  bool? _killSwitchEngaged;
  bool? get killSwitchEngaged => _$this._killSwitchEngaged;
  set killSwitchEngaged(bool? killSwitchEngaged) =>
      _$this._killSwitchEngaged = killSwitchEngaged;

  String? _name;
  String? get name => _$this._name;
  set name(String? name) => _$this._name = name;

  RolloutDtoBuilder? _rollout;
  RolloutDtoBuilder get rollout => _$this._rollout ??= RolloutDtoBuilder();
  set rollout(RolloutDtoBuilder? rollout) => _$this._rollout = rollout;

  String? _status;
  String? get status => _$this._status;
  set status(String? status) => _$this._status = status;

  ListBuilder<TargetingRuleDto>? _targetingRules;
  ListBuilder<TargetingRuleDto> get targetingRules =>
      _$this._targetingRules ??= ListBuilder<TargetingRuleDto>();
  set targetingRules(ListBuilder<TargetingRuleDto>? targetingRules) =>
      _$this._targetingRules = targetingRules;

  DateTime? _updatedAt;
  DateTime? get updatedAt => _$this._updatedAt;
  set updatedAt(DateTime? updatedAt) => _$this._updatedAt = updatedAt;

  int? _version;
  int? get version => _$this._version;
  set version(int? version) => _$this._version = version;

  FlagResponseBuilder() {
    FlagResponse._defaults(this);
  }

  FlagResponseBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _createdAt = $v.createdAt;
      _defaultOutcome = $v.defaultOutcome;
      _id = $v.id;
      _key = $v.key;
      _killSwitchEngaged = $v.killSwitchEngaged;
      _name = $v.name;
      _rollout = $v.rollout?.toBuilder();
      _status = $v.status;
      _targetingRules = $v.targetingRules.toBuilder();
      _updatedAt = $v.updatedAt;
      _version = $v.version;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(FlagResponse other) {
    _$v = other as _$FlagResponse;
  }

  @override
  void update(void Function(FlagResponseBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  FlagResponse build() => _build();

  _$FlagResponse _build() {
    _$FlagResponse _$result;
    try {
      _$result =
          _$v ??
          _$FlagResponse._(
            createdAt: BuiltValueNullFieldError.checkNotNull(
              createdAt,
              r'FlagResponse',
              'createdAt',
            ),
            defaultOutcome: BuiltValueNullFieldError.checkNotNull(
              defaultOutcome,
              r'FlagResponse',
              'defaultOutcome',
            ),
            id: BuiltValueNullFieldError.checkNotNull(
              id,
              r'FlagResponse',
              'id',
            ),
            key: BuiltValueNullFieldError.checkNotNull(
              key,
              r'FlagResponse',
              'key',
            ),
            killSwitchEngaged: BuiltValueNullFieldError.checkNotNull(
              killSwitchEngaged,
              r'FlagResponse',
              'killSwitchEngaged',
            ),
            name: BuiltValueNullFieldError.checkNotNull(
              name,
              r'FlagResponse',
              'name',
            ),
            rollout: _rollout?.build(),
            status: BuiltValueNullFieldError.checkNotNull(
              status,
              r'FlagResponse',
              'status',
            ),
            targetingRules: targetingRules.build(),
            updatedAt: BuiltValueNullFieldError.checkNotNull(
              updatedAt,
              r'FlagResponse',
              'updatedAt',
            ),
            version: BuiltValueNullFieldError.checkNotNull(
              version,
              r'FlagResponse',
              'version',
            ),
          );
    } catch (_) {
      late String _$failedField;
      try {
        _$failedField = 'rollout';
        _rollout?.build();

        _$failedField = 'targetingRules';
        targetingRules.build();
      } catch (e) {
        throw BuiltValueNestedFieldError(
          r'FlagResponse',
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
