import 'package:equatable/equatable.dart';
import 'package:hydrated_bloc/hydrated_bloc.dart';

import '../domain/alert.dart';
import '../domain/alert_repository.dart';

enum AlertsStatus { initial, loading, ready, failed }

class AlertsState extends Equatable {
  const AlertsState({
    this.status = AlertsStatus.initial,
    this.alerts = const [],
    this.acknowledging = const {},
    this.error,
  });

  final AlertsStatus status;
  final List<Alert> alerts;

  /// Ids with an acknowledgement in flight, so the control can be disabled per row rather than
  /// locking the whole list.
  final Set<String> acknowledging;

  final String? error;

  int get unacknowledgedCount => alerts.where((a) => !a.isAcknowledged).length;

  bool get hasUnacknowledgedCritical =>
      alerts.any((a) => !a.isAcknowledged && a.severity == AlertSeverity.critical);

  AlertsState copyWith({
    AlertsStatus? status,
    List<Alert>? alerts,
    Set<String>? acknowledging,
    String? error,
    bool clearError = false,
  }) =>
      AlertsState(
        status: status ?? this.status,
        alerts: alerts ?? this.alerts,
        acknowledging: acknowledging ?? this.acknowledging,
        error: clearError ? null : (error ?? this.error),
      );

  @override
  List<Object?> get props => [status, alerts, acknowledging, error];
}

/// Alert history and acknowledgement (T075).
///
/// <b>The cached history is KEPT when a refresh fails</b>, unlike the flag list. The distinction
/// is what the data is for: a flag list is operational state an admin acts on, so stale rows
/// invite a wrong action, whereas an alert is a record of something that already happened and
/// stays true. Discarding the history because the network dropped would hide the incident report
/// at the moment the network is worst — and the history existing when delivery fails is the
/// entire reason it is durable server-side.
class AlertsCubit extends HydratedCubit<AlertsState> {
  AlertsCubit(this._repository) : super(const AlertsState());

  final AlertRepository _repository;

  Future<void> load({bool unacknowledgedOnly = false}) async {
    emit(state.copyWith(status: AlertsStatus.loading, clearError: true));
    try {
      final alerts = await _repository.list(unacknowledgedOnly: unacknowledgedOnly, limit: 100);
      emit(state.copyWith(status: AlertsStatus.ready, alerts: alerts));
    } on AlertFailure catch (failure) {
      emit(state.copyWith(status: AlertsStatus.failed, error: failure.message));
    }
  }

  Future<void> acknowledge(String alertId) async {
    emit(state.copyWith(acknowledging: {...state.acknowledging, alertId}));

    try {
      final updated = await _repository.acknowledge(alertId);

      // The SERVER's version replaces the local row. Acknowledgement records who responded, and
      // the server keeps the FIRST responder — so optimistically writing this device's own name
      // could show the wrong person when two admins act at once.
      emit(state.copyWith(
        alerts: state.alerts.map((a) => a.id == alertId ? updated : a).toList(),
        acknowledging: {...state.acknowledging}..remove(alertId),
      ));
    } on AlertFailure catch (failure) {
      emit(state.copyWith(
        acknowledging: {...state.acknowledging}..remove(alertId),
        error: failure.message,
      ));
    }
  }

  @override
  AlertsState? fromJson(Map<String, dynamic> json) {
    final alerts = (json['alerts'] as List<dynamic>? ?? const [])
        .whereType<Map<String, dynamic>>()
        .map(Alert.fromJson)
        .whereType<Alert>()
        .toList();

    // `initial`, not `ready`: these are last-known alerts, and load() replaces them on open.
    return AlertsState(status: AlertsStatus.initial, alerts: alerts);
  }

  @override
  Map<String, dynamic>? toJson(AlertsState state) =>
      {'alerts': state.alerts.take(100).map((a) => a.toJson()).toList()};
}
