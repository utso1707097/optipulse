import 'package:equatable/equatable.dart';

/// A flag as the mobile app cares about it.
///
/// Deliberately narrower than the API's FlagResponse. This app engages and releases kill
/// switches; it does not author targeting rules or rollout percentages, so it does not model
/// them. Carrying fields no screen renders would invite someone to start editing them here,
/// which is the dashboard's job.
class Flag extends Equatable {
  const Flag({
    required this.key,
    required this.name,
    required this.status,
    required this.killSwitchEngaged,
    required this.version,
  });

  final String key;
  final String name;
  final String status;
  final bool killSwitchEngaged;

  /// Optimistic-concurrency token from the server. Kept so a later write can detect that
  /// someone else changed the flag in between.
  final int version;

  bool get isActive => status.toLowerCase() == 'active';

  Flag copyWith({bool? killSwitchEngaged, int? version, String? status}) => Flag(
        key: key,
        name: name,
        status: status ?? this.status,
        killSwitchEngaged: killSwitchEngaged ?? this.killSwitchEngaged,
        version: version ?? this.version,
      );

  @override
  List<Object?> get props => [key, name, status, killSwitchEngaged, version];
}
