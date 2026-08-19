import 'package:equatable/equatable.dart';

/// What evaluation is serving right now.
///
/// Read from the API's in-memory snapshot rather than its database, which is the reason this is
/// worth showing at all: during a database outage the platform keeps serving last-known-good
/// flags, so "what is actually being served" and "what the database says" can legitimately
/// differ — and the first one is what an operator needs during an incident.
class LiveTelemetry extends Equatable {
  const LiveTelemetry({
    required this.snapshotVersion,
    required this.snapshotBuiltAt,
    required this.snapshotAgeSeconds,
    required this.activeFlags,
    required this.killSwitchesEngaged,
    required this.serverTime,
    required this.observedAt,
  });

  final int snapshotVersion;
  final DateTime snapshotBuiltAt;

  /// Null before anything has been published.
  final int? snapshotAgeSeconds;

  final int activeFlags;
  final int killSwitchesEngaged;
  final DateTime serverTime;

  /// When THIS DEVICE read the figures. Distinct from [serverTime] on purpose: a cached reading
  /// shown after the app was reopened must be labelled with when it was taken, or a stale
  /// number reads as a current one.
  final DateTime observedAt;

  bool get hasEngagedKillSwitches => killSwitchesEngaged > 0;

  /// A snapshot that stopped updating still serves perfectly valid-looking answers from stale
  /// rules, which is the failure this number exists to make visible. Five minutes is well beyond
  /// the sub-second propagation the platform targets, so anything past it means invalidation is
  /// not arriving.
  bool get isSnapshotStale => (snapshotAgeSeconds ?? 0) > 300;

  Map<String, dynamic> toJson() => {
        'snapshotVersion': snapshotVersion,
        'snapshotBuiltAt': snapshotBuiltAt.toIso8601String(),
        'snapshotAgeSeconds': snapshotAgeSeconds,
        'activeFlags': activeFlags,
        'killSwitchesEngaged': killSwitchesEngaged,
        'serverTime': serverTime.toIso8601String(),
        'observedAt': observedAt.toIso8601String(),
      };

  static LiveTelemetry? fromJson(Map<String, dynamic> json) {
    final builtAt = DateTime.tryParse(json['snapshotBuiltAt'] as String? ?? '');
    final serverTime = DateTime.tryParse(json['serverTime'] as String? ?? '');
    final observedAt = DateTime.tryParse(json['observedAt'] as String? ?? '');
    final version = json['snapshotVersion'];
    final active = json['activeFlags'];
    final killed = json['killSwitchesEngaged'];

    if (builtAt == null || serverTime == null || observedAt == null ||
        version is! int || active is! int || killed is! int) {
      return null;
    }

    return LiveTelemetry(
      snapshotVersion: version,
      snapshotBuiltAt: builtAt,
      snapshotAgeSeconds: json['snapshotAgeSeconds'] as int?,
      activeFlags: active,
      killSwitchesEngaged: killed,
      serverTime: serverTime,
      observedAt: observedAt,
    );
  }

  @override
  List<Object?> get props => [
        snapshotVersion,
        snapshotBuiltAt,
        snapshotAgeSeconds,
        activeFlags,
        killSwitchesEngaged,
        serverTime,
        observedAt,
      ];
}
