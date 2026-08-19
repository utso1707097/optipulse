// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'targeting_rule_dto.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$TargetingRuleDto extends TargetingRuleDto {
  @override
  final String attribute;
  @override
  final String operator_;
  @override
  final bool outcome;
  @override
  final BuiltList<String> values;

  factory _$TargetingRuleDto([
    void Function(TargetingRuleDtoBuilder)? updates,
  ]) => (TargetingRuleDtoBuilder()..update(updates))._build();

  _$TargetingRuleDto._({
    required this.attribute,
    required this.operator_,
    required this.outcome,
    required this.values,
  }) : super._();
  @override
  TargetingRuleDto rebuild(void Function(TargetingRuleDtoBuilder) updates) =>
      (toBuilder()..update(updates)).build();

  @override
  TargetingRuleDtoBuilder toBuilder() =>
      TargetingRuleDtoBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is TargetingRuleDto &&
        attribute == other.attribute &&
        operator_ == other.operator_ &&
        outcome == other.outcome &&
        values == other.values;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, attribute.hashCode);
    _$hash = $jc(_$hash, operator_.hashCode);
    _$hash = $jc(_$hash, outcome.hashCode);
    _$hash = $jc(_$hash, values.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'TargetingRuleDto')
          ..add('attribute', attribute)
          ..add('operator_', operator_)
          ..add('outcome', outcome)
          ..add('values', values))
        .toString();
  }
}

class TargetingRuleDtoBuilder
    implements Builder<TargetingRuleDto, TargetingRuleDtoBuilder> {
  _$TargetingRuleDto? _$v;

  String? _attribute;
  String? get attribute => _$this._attribute;
  set attribute(String? attribute) => _$this._attribute = attribute;

  String? _operator_;
  String? get operator_ => _$this._operator_;
  set operator_(String? operator_) => _$this._operator_ = operator_;

  bool? _outcome;
  bool? get outcome => _$this._outcome;
  set outcome(bool? outcome) => _$this._outcome = outcome;

  ListBuilder<String>? _values;
  ListBuilder<String> get values => _$this._values ??= ListBuilder<String>();
  set values(ListBuilder<String>? values) => _$this._values = values;

  TargetingRuleDtoBuilder() {
    TargetingRuleDto._defaults(this);
  }

  TargetingRuleDtoBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _attribute = $v.attribute;
      _operator_ = $v.operator_;
      _outcome = $v.outcome;
      _values = $v.values.toBuilder();
      _$v = null;
    }
    return this;
  }

  @override
  void replace(TargetingRuleDto other) {
    _$v = other as _$TargetingRuleDto;
  }

  @override
  void update(void Function(TargetingRuleDtoBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  TargetingRuleDto build() => _build();

  _$TargetingRuleDto _build() {
    _$TargetingRuleDto _$result;
    try {
      _$result =
          _$v ??
          _$TargetingRuleDto._(
            attribute: BuiltValueNullFieldError.checkNotNull(
              attribute,
              r'TargetingRuleDto',
              'attribute',
            ),
            operator_: BuiltValueNullFieldError.checkNotNull(
              operator_,
              r'TargetingRuleDto',
              'operator_',
            ),
            outcome: BuiltValueNullFieldError.checkNotNull(
              outcome,
              r'TargetingRuleDto',
              'outcome',
            ),
            values: values.build(),
          );
    } catch (_) {
      late String _$failedField;
      try {
        _$failedField = 'values';
        values.build();
      } catch (e) {
        throw BuiltValueNestedFieldError(
          r'TargetingRuleDto',
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
