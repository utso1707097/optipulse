import 'dart:async';

import 'package:equatable/equatable.dart';
import 'package:hydrated_bloc/hydrated_bloc.dart';

import '../domain/live_telemetry.dart';
import '../domain/telemetry_repository.dart';

class TelemetryState extends Equatable {
  const TelemetryState({this.reading, this.isRefreshing = false, this.error});

  final LiveTelemetry? reading;
  final bool isRefreshing;
  final String? error;

  /// True when what is on screen came from the cache rather than this session. Drives the
  /// "as of" label — a cached reading presented without one is indistinguishable from a live
  /// one, and on an ops screen that is the difference between "the platform is fine" and "I
  /// have not heard from the platform since Tuesday".
  bool isStale(DateTime now) =>
      reading != null && now.difference(reading!.observedAt) > const Duration(seconds: 30);

  TelemetryState copyWith({
    LiveTelemetry? reading,
    bool? isRefreshing,
    String? error,
    bool clearError = false,
  }) =>
      TelemetryState(
        reading: reading ?? this.reading,
        isRefreshing: isRefreshing ?? this.isRefreshing,
        error: clearError ? null : (error ?? this.error),
      );

  @override
  List<Object?> get props => [reading, isRefreshing, error];
}

/// Live platform signals, polled while the screen is open (T074).
///
/// <b>The last good reading is kept when a refresh fails.</b> That is the opposite of the rule
/// the flag list follows, and deliberately so: a stale FLAG list invites an operator to act on
/// wrong state, whereas a stale telemetry reading is a historical fact that stays true — the
/// platform really did report those figures at that time. Clearing it would replace real
/// information with none at the moment the network is worst. It is labelled with when it was
/// taken so nobody mistakes it for current.
class TelemetryCubit extends HydratedCubit<TelemetryState> {
  TelemetryCubit(this._repository, {Duration? pollInterval, DateTime Function()? now})
      : _pollInterval = pollInterval ?? const Duration(seconds: 15),
        _now = now ?? DateTime.now,
        super(const TelemetryState());

  final TelemetryRepository _repository;
  final Duration _pollInterval;
  final DateTime Function() _now;
  Timer? _timer;

  /// Polling starts when the screen appears and stops when it leaves. A background timer on an
  /// ops app is a battery cost paid for data nobody is looking at.
  void startPolling() {
    unawaited(refresh());
    _timer?.cancel();
    _timer = Timer.periodic(_pollInterval, (_) => unawaited(refresh()));
  }

  void stopPolling() {
    _timer?.cancel();
    _timer = null;
  }

  Future<void> refresh() async {
    emit(state.copyWith(isRefreshing: true, clearError: true));
    try {
      final reading = await _repository.fetchLive();
      emit(TelemetryState(reading: reading));
    } on TelemetryFailure catch (failure) {
      emit(state.copyWith(isRefreshing: false, error: failure.message));
    }
  }

  bool get isStaleNow => state.isStale(_now());

  @override
  Future<void> close() {
    _timer?.cancel();
    return super.close();
  }

  @override
  TelemetryState? fromJson(Map<String, dynamic> json) {
    final raw = json['reading'];
    if (raw is! Map<String, dynamic>) return null;
    final reading = LiveTelemetry.fromJson(raw);
    return reading == null ? null : TelemetryState(reading: reading);
  }

  @override
  Map<String, dynamic>? toJson(TelemetryState state) =>
      state.reading == null ? null : {'reading': state.reading!.toJson()};
}
