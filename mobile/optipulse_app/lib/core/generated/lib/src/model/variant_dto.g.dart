// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'variant_dto.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$VariantDto extends VariantDto {
  @override
  final String key;
  @override
  final int weight;

  factory _$VariantDto([void Function(VariantDtoBuilder)? updates]) =>
      (VariantDtoBuilder()..update(updates))._build();

  _$VariantDto._({required this.key, required this.weight}) : super._();
  @override
  VariantDto rebuild(void Function(VariantDtoBuilder) updates) =>
      (toBuilder()..update(updates)).build();

  @override
  VariantDtoBuilder toBuilder() => VariantDtoBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is VariantDto && key == other.key && weight == other.weight;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, key.hashCode);
    _$hash = $jc(_$hash, weight.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'VariantDto')
          ..add('key', key)
          ..add('weight', weight))
        .toString();
  }
}

class VariantDtoBuilder implements Builder<VariantDto, VariantDtoBuilder> {
  _$VariantDto? _$v;

  String? _key;
  String? get key => _$this._key;
  set key(String? key) => _$this._key = key;

  int? _weight;
  int? get weight => _$this._weight;
  set weight(int? weight) => _$this._weight = weight;

  VariantDtoBuilder() {
    VariantDto._defaults(this);
  }

  VariantDtoBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _key = $v.key;
      _weight = $v.weight;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(VariantDto other) {
    _$v = other as _$VariantDto;
  }

  @override
  void update(void Function(VariantDtoBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  VariantDto build() => _build();

  _$VariantDto _build() {
    final _$result =
        _$v ??
        _$VariantDto._(
          key: BuiltValueNullFieldError.checkNotNull(key, r'VariantDto', 'key'),
          weight: BuiltValueNullFieldError.checkNotNull(
            weight,
            r'VariantDto',
            'weight',
          ),
        );
    replace(_$result);
    return _$result;
  }
}

// ignore_for_file: deprecated_member_use_from_same_package,type=lint
