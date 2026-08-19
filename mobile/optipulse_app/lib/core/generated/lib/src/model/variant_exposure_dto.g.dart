// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'variant_exposure_dto.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$VariantExposureDto extends VariantExposureDto {
  @override
  final double conversionRatePercent;
  @override
  final int conversions;
  @override
  final int exposures;
  @override
  final double sharePercent;
  @override
  final String? variantKey;

  factory _$VariantExposureDto([
    void Function(VariantExposureDtoBuilder)? updates,
  ]) => (VariantExposureDtoBuilder()..update(updates))._build();

  _$VariantExposureDto._({
    required this.conversionRatePercent,
    required this.conversions,
    required this.exposures,
    required this.sharePercent,
    this.variantKey,
  }) : super._();
  @override
  VariantExposureDto rebuild(
    void Function(VariantExposureDtoBuilder) updates,
  ) => (toBuilder()..update(updates)).build();

  @override
  VariantExposureDtoBuilder toBuilder() =>
      VariantExposureDtoBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is VariantExposureDto &&
        conversionRatePercent == other.conversionRatePercent &&
        conversions == other.conversions &&
        exposures == other.exposures &&
        sharePercent == other.sharePercent &&
        variantKey == other.variantKey;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, conversionRatePercent.hashCode);
    _$hash = $jc(_$hash, conversions.hashCode);
    _$hash = $jc(_$hash, exposures.hashCode);
    _$hash = $jc(_$hash, sharePercent.hashCode);
    _$hash = $jc(_$hash, variantKey.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'VariantExposureDto')
          ..add('conversionRatePercent', conversionRatePercent)
          ..add('conversions', conversions)
          ..add('exposures', exposures)
          ..add('sharePercent', sharePercent)
          ..add('variantKey', variantKey))
        .toString();
  }
}

class VariantExposureDtoBuilder
    implements Builder<VariantExposureDto, VariantExposureDtoBuilder> {
  _$VariantExposureDto? _$v;

  double? _conversionRatePercent;
  double? get conversionRatePercent => _$this._conversionRatePercent;
  set conversionRatePercent(double? conversionRatePercent) =>
      _$this._conversionRatePercent = conversionRatePercent;

  int? _conversions;
  int? get conversions => _$this._conversions;
  set conversions(int? conversions) => _$this._conversions = conversions;

  int? _exposures;
  int? get exposures => _$this._exposures;
  set exposures(int? exposures) => _$this._exposures = exposures;

  double? _sharePercent;
  double? get sharePercent => _$this._sharePercent;
  set sharePercent(double? sharePercent) => _$this._sharePercent = sharePercent;

  String? _variantKey;
  String? get variantKey => _$this._variantKey;
  set variantKey(String? variantKey) => _$this._variantKey = variantKey;

  VariantExposureDtoBuilder() {
    VariantExposureDto._defaults(this);
  }

  VariantExposureDtoBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _conversionRatePercent = $v.conversionRatePercent;
      _conversions = $v.conversions;
      _exposures = $v.exposures;
      _sharePercent = $v.sharePercent;
      _variantKey = $v.variantKey;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(VariantExposureDto other) {
    _$v = other as _$VariantExposureDto;
  }

  @override
  void update(void Function(VariantExposureDtoBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  VariantExposureDto build() => _build();

  _$VariantExposureDto _build() {
    final _$result =
        _$v ??
        _$VariantExposureDto._(
          conversionRatePercent: BuiltValueNullFieldError.checkNotNull(
            conversionRatePercent,
            r'VariantExposureDto',
            'conversionRatePercent',
          ),
          conversions: BuiltValueNullFieldError.checkNotNull(
            conversions,
            r'VariantExposureDto',
            'conversions',
          ),
          exposures: BuiltValueNullFieldError.checkNotNull(
            exposures,
            r'VariantExposureDto',
            'exposures',
          ),
          sharePercent: BuiltValueNullFieldError.checkNotNull(
            sharePercent,
            r'VariantExposureDto',
            'sharePercent',
          ),
          variantKey: variantKey,
        );
    replace(_$result);
    return _$result;
  }
}

// ignore_for_file: deprecated_member_use_from_same_package,type=lint
