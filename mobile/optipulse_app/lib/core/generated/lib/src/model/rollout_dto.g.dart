// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'rollout_dto.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$RolloutDto extends RolloutDto {
  @override
  final int percentage;
  @override
  final String salt;

  factory _$RolloutDto([void Function(RolloutDtoBuilder)? updates]) =>
      (RolloutDtoBuilder()..update(updates))._build();

  _$RolloutDto._({required this.percentage, required this.salt}) : super._();
  @override
  RolloutDto rebuild(void Function(RolloutDtoBuilder) updates) =>
      (toBuilder()..update(updates)).build();

  @override
  RolloutDtoBuilder toBuilder() => RolloutDtoBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is RolloutDto &&
        percentage == other.percentage &&
        salt == other.salt;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, percentage.hashCode);
    _$hash = $jc(_$hash, salt.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'RolloutDto')
          ..add('percentage', percentage)
          ..add('salt', salt))
        .toString();
  }
}

class RolloutDtoBuilder implements Builder<RolloutDto, RolloutDtoBuilder> {
  _$RolloutDto? _$v;

  int? _percentage;
  int? get percentage => _$this._percentage;
  set percentage(int? percentage) => _$this._percentage = percentage;

  String? _salt;
  String? get salt => _$this._salt;
  set salt(String? salt) => _$this._salt = salt;

  RolloutDtoBuilder() {
    RolloutDto._defaults(this);
  }

  RolloutDtoBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _percentage = $v.percentage;
      _salt = $v.salt;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(RolloutDto other) {
    _$v = other as _$RolloutDto;
  }

  @override
  void update(void Function(RolloutDtoBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  RolloutDto build() => _build();

  _$RolloutDto _build() {
    final _$result =
        _$v ??
        _$RolloutDto._(
          percentage: BuiltValueNullFieldError.checkNotNull(
            percentage,
            r'RolloutDto',
            'percentage',
          ),
          salt: BuiltValueNullFieldError.checkNotNull(
            salt,
            r'RolloutDto',
            'salt',
          ),
        );
    replace(_$result);
    return _$result;
  }
}

// ignore_for_file: deprecated_member_use_from_same_package,type=lint
