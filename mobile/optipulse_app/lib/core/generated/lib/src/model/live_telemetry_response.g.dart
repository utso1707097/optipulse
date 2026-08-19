// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'live_telemetry_response.dart';

// **************************************************************************
// BuiltValueGenerator
// **************************************************************************

class _$LiveTelemetryResponse extends LiveTelemetryResponse {
  @override
  final int activeFlags;
  @override
  final int killSwitchesEngaged;
  @override
  final DateTime serverTime;
  @override
  final int? snapshotAgeSeconds;
  @override
  final DateTime snapshotBuiltAt;
  @override
  final int snapshotVersion;

  factory _$LiveTelemetryResponse([
    void Function(LiveTelemetryResponseBuilder)? updates,
  ]) => (LiveTelemetryResponseBuilder()..update(updates))._build();

  _$LiveTelemetryResponse._({
    required this.activeFlags,
    required this.killSwitchesEngaged,
    required this.serverTime,
    this.snapshotAgeSeconds,
    required this.snapshotBuiltAt,
    required this.snapshotVersion,
  }) : super._();
  @override
  LiveTelemetryResponse rebuild(
    void Function(LiveTelemetryResponseBuilder) updates,
  ) => (toBuilder()..update(updates)).build();

  @override
  LiveTelemetryResponseBuilder toBuilder() =>
      LiveTelemetryResponseBuilder()..replace(this);

  @override
  bool operator ==(Object other) {
    if (identical(other, this)) return true;
    return other is LiveTelemetryResponse &&
        activeFlags == other.activeFlags &&
        killSwitchesEngaged == other.killSwitchesEngaged &&
        serverTime == other.serverTime &&
        snapshotAgeSeconds == other.snapshotAgeSeconds &&
        snapshotBuiltAt == other.snapshotBuiltAt &&
        snapshotVersion == other.snapshotVersion;
  }

  @override
  int get hashCode {
    var _$hash = 0;
    _$hash = $jc(_$hash, activeFlags.hashCode);
    _$hash = $jc(_$hash, killSwitchesEngaged.hashCode);
    _$hash = $jc(_$hash, serverTime.hashCode);
    _$hash = $jc(_$hash, snapshotAgeSeconds.hashCode);
    _$hash = $jc(_$hash, snapshotBuiltAt.hashCode);
    _$hash = $jc(_$hash, snapshotVersion.hashCode);
    _$hash = $jf(_$hash);
    return _$hash;
  }

  @override
  String toString() {
    return (newBuiltValueToStringHelper(r'LiveTelemetryResponse')
          ..add('activeFlags', activeFlags)
          ..add('killSwitchesEngaged', killSwitchesEngaged)
          ..add('serverTime', serverTime)
          ..add('snapshotAgeSeconds', snapshotAgeSeconds)
          ..add('snapshotBuiltAt', snapshotBuiltAt)
          ..add('snapshotVersion', snapshotVersion))
        .toString();
  }
}

class LiveTelemetryResponseBuilder
    implements Builder<LiveTelemetryResponse, LiveTelemetryResponseBuilder> {
  _$LiveTelemetryResponse? _$v;

  int? _activeFlags;
  int? get activeFlags => _$this._activeFlags;
  set activeFlags(int? activeFlags) => _$this._activeFlags = activeFlags;

  int? _killSwitchesEngaged;
  int? get killSwitchesEngaged => _$this._killSwitchesEngaged;
  set killSwitchesEngaged(int? killSwitchesEngaged) =>
      _$this._killSwitchesEngaged = killSwitchesEngaged;

  DateTime? _serverTime;
  DateTime? get serverTime => _$this._serverTime;
  set serverTime(DateTime? serverTime) => _$this._serverTime = serverTime;

  int? _snapshotAgeSeconds;
  int? get snapshotAgeSeconds => _$this._snapshotAgeSeconds;
  set snapshotAgeSeconds(int? snapshotAgeSeconds) =>
      _$this._snapshotAgeSeconds = snapshotAgeSeconds;

  DateTime? _snapshotBuiltAt;
  DateTime? get snapshotBuiltAt => _$this._snapshotBuiltAt;
  set snapshotBuiltAt(DateTime? snapshotBuiltAt) =>
      _$this._snapshotBuiltAt = snapshotBuiltAt;

  int? _snapshotVersion;
  int? get snapshotVersion => _$this._snapshotVersion;
  set snapshotVersion(int? snapshotVersion) =>
      _$this._snapshotVersion = snapshotVersion;

  LiveTelemetryResponseBuilder() {
    LiveTelemetryResponse._defaults(this);
  }

  LiveTelemetryResponseBuilder get _$this {
    final $v = _$v;
    if ($v != null) {
      _activeFlags = $v.activeFlags;
      _killSwitchesEngaged = $v.killSwitchesEngaged;
      _serverTime = $v.serverTime;
      _snapshotAgeSeconds = $v.snapshotAgeSeconds;
      _snapshotBuiltAt = $v.snapshotBuiltAt;
      _snapshotVersion = $v.snapshotVersion;
      _$v = null;
    }
    return this;
  }

  @override
  void replace(LiveTelemetryResponse other) {
    _$v = other as _$LiveTelemetryResponse;
  }

  @override
  void update(void Function(LiveTelemetryResponseBuilder)? updates) {
    if (updates != null) updates(this);
  }

  @override
  LiveTelemetryResponse build() => _build();

  _$LiveTelemetryResponse _build() {
    final _$result =
        _$v ??
        _$LiveTelemetryResponse._(
          activeFlags: BuiltValueNullFieldError.checkNotNull(
            activeFlags,
            r'LiveTelemetryResponse',
            'activeFlags',
          ),
          killSwitchesEngaged: BuiltValueNullFieldError.checkNotNull(
            killSwitchesEngaged,
            r'LiveTelemetryResponse',
            'killSwitchesEngaged',
          ),
          serverTime: BuiltValueNullFieldError.checkNotNull(
            serverTime,
            r'LiveTelemetryResponse',
            'serverTime',
          ),
          snapshotAgeSeconds: snapshotAgeSeconds,
          snapshotBuiltAt: BuiltValueNullFieldError.checkNotNull(
            snapshotBuiltAt,
            r'LiveTelemetryResponse',
            'snapshotBuiltAt',
          ),
          snapshotVersion: BuiltValueNullFieldError.checkNotNull(
            snapshotVersion,
            r'LiveTelemetryResponse',
            'snapshotVersion',
          ),
        );
    replace(_$result);
    return _$result;
  }
}

// ignore_for_file: deprecated_member_use_from_same_package,type=lint
