// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'conversion_response.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$ConversionResponse extends ConversionResponse {
  @override
  final bool duplicate;
  @override
  final bool recorded;

  factory _$ConversionResponse([
    void Function(ConversionResponseBuilder)? updates,
  ]) => (ConversionResponseBuilder()..update(updates))._build();

  _$ConversionResponse._({required this.duplicate, required this.recorded})
    : super._();
  @override
  ConversionResponse rebuild(
    void Function(ConversionResponseBuilder) updates,
  ) => (toBuilder()..update(updates)).build();

  @override
  ConversionResponseBuilder toBuilder() =>
      ConversionResponseBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is ConversionResponse &&
        duplicate == other.duplicate &&
        recorded == other.recorded;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, duplicate.hashCode);
    _$hash = $jc(_$hash, recorded.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'ConversionResponse')
          ..add('duplicate', duplicate)
          ..add('recorded', recorded))
        .toString();
  }
}

class ConversionResponseBuilder
    implements Builder<ConversionResponse, ConversionResponseBuilder> {
  _$ConversionResponse? _$v;

  bool? _duplicate;
  bool? get duplicate => _$this._duplicate;
  set duplicate(bool? duplicate) => _$this._duplicate = duplicate;

  bool? _recorded;
  bool? get recorded => _$this._recorded;
  set recorded(bool? recorded) => _$this._recorded = recorded;

  ConversionResponseBuilder() {
    ConversionResponse._defaults(this);
  }

  ConversionResponseBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _duplicate = $v.duplicate;
      _recorded = $v.recorded;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(ConversionResponse other) {
    _$v = other as _$ConversionResponse;
  }

  @override
  void update(void Function(ConversionResponseBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  ConversionResponse build() => _build();

  _$ConversionResponse _build() {
    final _$result =
        _$v ??
        _$ConversionResponse._(
          duplicate: BuiltValueNullFieldError.checkNotNull(
            duplicate,
            r'ConversionResponse',
            'duplicate',
          ),
          recorded: BuiltValueNullFieldError.checkNotNull(
            recorded,
            r'ConversionResponse',
            'recorded',
          ),
        );
    replace(_$result);
    return _$result;
  }
}

// ignore_for_file: deprecated_member_use_from_same_package,type=lint
