// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'flag_exposure_response.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$FlagExposureResponse extends FlagExposureResponse {
  @override
  final BuiltList<VariantExposureDto> byVariant;
  @override
  final String flagKey;
  @override
  final int totalConversions;
  @override
  final int totalExposures;

  factory _$FlagExposureResponse([
    void Function(FlagExposureResponseBuilder)? updates,
  ]) => (FlagExposureResponseBuilder()..update(updates))._build();

  _$FlagExposureResponse._({
    required this.byVariant,
    required this.flagKey,
    required this.totalConversions,
    required this.totalExposures,
  }) : super._();
  @override
  FlagExposureResponse rebuild(
    void Function(FlagExposureResponseBuilder) updates,
  ) => (toBuilder()..update(updates)).build();

  @override
  FlagExposureResponseBuilder toBuilder() =>
      FlagExposureResponseBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is FlagExposureResponse &&
        byVariant == other.byVariant &&
        flagKey == other.flagKey &&
        totalConversions == other.totalConversions &&
        totalExposures == other.totalExposures;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, byVariant.hashCode);
    _$hash = $jc(_$hash, flagKey.hashCode);
    _$hash = $jc(_$hash, totalConversions.hashCode);
    _$hash = $jc(_$hash, totalExposures.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'FlagExposureResponse')
          ..add('byVariant', byVariant)
          ..add('flagKey', flagKey)
          ..add('totalConversions', totalConversions)
          ..add('totalExposures', totalExposures))
        .toString();
  }
}

class FlagExposureResponseBuilder
    implements Builder<FlagExposureResponse, FlagExposureResponseBuilder> {
  _$FlagExposureResponse? _$v;

  ListBuilder<VariantExposureDto>? _byVariant;
  ListBuilder<VariantExposureDto> get byVariant =>
      _$this._byVariant ??= ListBuilder<VariantExposureDto>();
  set byVariant(ListBuilder<VariantExposureDto>? byVariant) =>
      _$this._byVariant = byVariant;

  String? _flagKey;
  String? get flagKey => _$this._flagKey;
  set flagKey(String? flagKey) => _$this._flagKey = flagKey;

  int? _totalConversions;
  int? get totalConversions => _$this._totalConversions;
  set totalConversions(int? totalConversions) =>
      _$this._totalConversions = totalConversions;

  int? _totalExposures;
  int? get totalExposures => _$this._totalExposures;
  set totalExposures(int? totalExposures) =>
      _$this._totalExposures = totalExposures;

  FlagExposureResponseBuilder() {
    FlagExposureResponse._defaults(this);
  }

  FlagExposureResponseBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _byVariant = $v.byVariant.toBuilder();
      _flagKey = $v.flagKey;
      _totalConversions = $v.totalConversions;
      _totalExposures = $v.totalExposures;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(FlagExposureResponse other) {
    _$v = other as _$FlagExposureResponse;
  }

  @override
  void update(void Function(FlagExposureResponseBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  FlagExposureResponse build() => _build();

  _$FlagExposureResponse _build() {
    _$FlagExposureResponse _$result;
    try {
      _$result =
          _$v ??
          _$FlagExposureResponse._(
            byVariant: byVariant.build(),
            flagKey: BuiltValueNullFieldError.checkNotNull(
              flagKey,
              r'FlagExposureResponse',
              'flagKey',
            ),
            totalConversions: BuiltValueNullFieldError.checkNotNull(
              totalConversions,
              r'FlagExposureResponse',
              'totalConversions',
            ),
            totalExposures: BuiltValueNullFieldError.checkNotNull(
              totalExposures,
              r'FlagExposureResponse',
              'totalExposures',
            ),
          );
    } catch (_) {
      late String _$failedField;
      try {
        _$failedField = 'byVariant';
        byVariant.build();
      } catch (e) {
        throw BuiltValueNestedFieldError(
          r'FlagExposureResponse',
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
