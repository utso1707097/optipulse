import 'package:equatable/equatable.dart';

enum AlertSeverity { info, warning, critical, unknown }

class Alert extends Equatable {
  const Alert({
    required this.id,
    required this.raisedAt,
    required this.kind,
    required this.severity,
    required this.title,
    required this.detail,
    this.flagKey,
    this.acknowledgedAt,
    this.acknowledgedBy,
  });

  final String id;
  final DateTime raisedAt;
  final String kind;
  final AlertSeverity severity;
  final String title;
  final String detail;
  final String? flagKey;
  final DateTime? acknowledgedAt;
  final String? acknowledgedBy;

  bool get isAcknowledged => acknowledgedAt != null;

  /// Parsed leniently, and unknown values become [AlertSeverity.unknown] rather than throwing.
  /// The server may add a severity this build has never heard of, and an ops app that crashes
  /// on an unrecognised alert fails precisely when there is something to report.
  static AlertSeverity parseSeverity(String raw) => switch (raw.toLowerCase()) {
        'info' => AlertSeverity.info,
        'warning' => AlertSeverity.warning,
        'critical' => AlertSeverity.critical,
        _ => AlertSeverity.unknown,
      };

  Alert copyWith({DateTime? acknowledgedAt, String? acknowledgedBy}) => Alert(
        id: id,
        raisedAt: raisedAt,
        kind: kind,
        severity: severity,
        title: title,
        detail: detail,
        flagKey: flagKey,
        acknowledgedAt: acknowledgedAt ?? this.acknowledgedAt,
        acknowledgedBy: acknowledgedBy ?? this.acknowledgedBy,
      );

  Map<String, dynamic> toJson() => {
        'id': id,
        'raisedAt': raisedAt.toIso8601String(),
        'kind': kind,
        'severity': severity.name,
        'title': title,
        'detail': detail,
        'flagKey': flagKey,
        'acknowledgedAt': acknowledgedAt?.toIso8601String(),
        'acknowledgedBy': acknowledgedBy,
      };

  static Alert? fromJson(Map<String, dynamic> json) {
    final id = json['id'];
    final raisedAt = DateTime.tryParse(json['raisedAt'] as String? ?? '');
    final title = json['title'];
    if (id is! String || raisedAt == null || title is! String) return null;

    return Alert(
      id: id,
      raisedAt: raisedAt,
      kind: json['kind'] as String? ?? 'Unknown',
      severity: parseSeverity(json['severity'] as String? ?? ''),
      title: title,
      detail: json['detail'] as String? ?? '',
      flagKey: json['flagKey'] as String?,
      acknowledgedAt: DateTime.tryParse(json['acknowledgedAt'] as String? ?? ''),
      acknowledgedBy: json['acknowledgedBy'] as String?,
    );
  }

  @override
  List<Object?> get props =>
      [id, raisedAt, kind, severity, title, detail, flagKey, acknowledgedAt, acknowledgedBy];
}
