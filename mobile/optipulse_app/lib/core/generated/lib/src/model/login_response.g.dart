// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'login_response.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$LoginResponse extends LoginResponse {
  @override
  final String accessToken;
  @override
  final int expiresInSeconds;
  @override
  final String refreshToken;
  @override
  final String role;
  @override
  final String tokenType;

  factory _$LoginResponse([void Function(LoginResponseBuilder)? updates]) =>
      (LoginResponseBuilder()..update(updates))._build();

  _$LoginResponse._({
    required this.accessToken,
    required this.expiresInSeconds,
    required this.refreshToken,
    required this.role,
    required this.tokenType,
  }) : super._();
  @override
  LoginResponse rebuild(void Function(LoginResponseBuilder) updates) =>
      (toBuilder()..update(updates)).build();

  @override
  LoginResponseBuilder toBuilder() => LoginResponseBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is LoginResponse &&
        accessToken == other.accessToken &&
        expiresInSeconds == other.expiresInSeconds &&
        refreshToken == other.refreshToken &&
        role == other.role &&
        tokenType == other.tokenType;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, accessToken.hashCode);
    _$hash = $jc(_$hash, expiresInSeconds.hashCode);
    _$hash = $jc(_$hash, refreshToken.hashCode);
    _$hash = $jc(_$hash, role.hashCode);
    _$hash = $jc(_$hash, tokenType.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'LoginResponse')
          ..add('accessToken', accessToken)
          ..add('expiresInSeconds', expiresInSeconds)
          ..add('refreshToken', refreshToken)
          ..add('role', role)
          ..add('tokenType', tokenType))
        .toString();
  }
}

class LoginResponseBuilder
    implements Builder<LoginResponse, LoginResponseBuilder> {
  _$LoginResponse? _$v;

  String? _accessToken;
  String? get accessToken => _$this._accessToken;
  set accessToken(String? accessToken) => _$this._accessToken = accessToken;

  int? _expiresInSeconds;
  int? get expiresInSeconds => _$this._expiresInSeconds;
  set expiresInSeconds(int? expiresInSeconds) =>
      _$this._expiresInSeconds = expiresInSeconds;

  String? _refreshToken;
  String? get refreshToken => _$this._refreshToken;
  set refreshToken(String? refreshToken) => _$this._refreshToken = refreshToken;

  String? _role;
  String? get role => _$this._role;
  set role(String? role) => _$this._role = role;

  String? _tokenType;
  String? get tokenType => _$this._tokenType;
  set tokenType(String? tokenType) => _$this._tokenType = tokenType;

  LoginResponseBuilder() {
    LoginResponse._defaults(this);
  }

  LoginResponseBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _accessToken = $v.accessToken;
      _expiresInSeconds = $v.expiresInSeconds;
      _refreshToken = $v.refreshToken;
      _role = $v.role;
      _tokenType = $v.tokenType;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(LoginResponse other) {
    _$v = other as _$LoginResponse;
  }

  @override
  void update(void Function(LoginResponseBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  LoginResponse build() => _build();

  _$LoginResponse _build() {
    final _$result =
        _$v ??
        _$LoginResponse._(
          accessToken: BuiltValueNullFieldError.checkNotNull(
            accessToken,
            r'LoginResponse',
            'accessToken',
          ),
          expiresInSeconds: BuiltValueNullFieldError.checkNotNull(
            expiresInSeconds,
            r'LoginResponse',
            'expiresInSeconds',
          ),
          refreshToken: BuiltValueNullFieldError.checkNotNull(
            refreshToken,
            r'LoginResponse',
            'refreshToken',
          ),
          role: BuiltValueNullFieldError.checkNotNull(
            role,
            r'LoginResponse',
            'role',
          ),
          tokenType: BuiltValueNullFieldError.checkNotNull(
            tokenType,
            r'LoginResponse',
            'tokenType',
          ),
        );
    replace(_$result);
    return _$result;
  }
}

// ignore_for_file: deprecated_member_use_from_same_package,type=lint
