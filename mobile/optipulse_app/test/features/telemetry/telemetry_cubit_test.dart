import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:optipulse_app/features/telemetry/domain/live_telemetry.dart';
import 'package:optipulse_app/features/telemetry/domain/telemetry_repository.dart';
import 'package:optipulse_app/features/telemetry/presentation/telemetry_cubit.dart';

import '../../support/memory_storage.dart';

class _MockRepo extends Mock implements TelemetryRepository {}

LiveTelemetry _reading({int killed = 0, int? ageSeconds = 5, DateTime? observedAt}) => LiveTelemetry(
      snapshotVersion: 7,
      snapshotBuiltAt: DateTime(2026, 8, 19, 12),
      snapshotAgeSeconds: ageSeconds,
      activeFlags: 4,
      killSwitchesEngaged: killed,
      serverTime: DateTime(2026, 8, 19, 12, 0, 5),
      observedAt: observedAt ?? DateTime(2026, 8, 19, 12, 0, 5),
    );

void main() {
  late _MockRepo repository;

  setUp(() {
    repository = _MockRepo();
    useMemoryStorage();
  });

  test('a failed refresh KEEPS the last reading', () async {
    // Opposite of the flag list's rule, and deliberately. A stale flag list invites a wrong
    // action; a telemetry reading is a historical fact that stays true, so discarding it would
    // replace real information with none exactly when the network is worst.
    when(repository.fetchLive).thenAnswer((_) async => _reading());
    final cubit = TelemetryCubit(repository);
    await cubit.refresh();

    when(repository.fetchLive).thenThrow(const TelemetryFailure.network());
    await cubit.refresh();

    expect(cubit.state.reading, isNotNull, reason: 'the last good reading must survive');
    expect(cubit.state.error, isNotNull);
    await cubit.close();
  });

  test('a cached reading is reported as stale', () {
    // Without this an ops screen showing Tuesday's figures looks identical to one showing the
    // last fifteen seconds.
    final now = DateTime(2026, 8, 19, 12, 10);
    final state = TelemetryState(reading: _reading(observedAt: DateTime(2026, 8, 19, 12)));

    expect(state.isStale(now), isTrue);
    expect(state.isStale(DateTime(2026, 8, 19, 12, 0, 10)), isFalse);
  });

  test('a snapshot that stopped updating is flagged', () {
    // A stale snapshot still serves perfectly valid-looking answers from out-of-date rules,
    // which is exactly why it needs surfacing rather than inferring from a healthy-looking API.
    expect(_reading(ageSeconds: 30).isSnapshotStale, isFalse);
    expect(_reading(ageSeconds: 900).isSnapshotStale, isTrue);
  });

  test('nothing published yet is not treated as stale', () {
    expect(_reading(ageSeconds: null).isSnapshotStale, isFalse);
  });

  test('the reading survives a restart', () async {
    when(repository.fetchLive).thenAnswer((_) async => _reading(killed: 2));
    final first = TelemetryCubit(repository);
    await first.refresh();
    await first.close();

    final restored = TelemetryCubit(repository);

    expect(restored.state.reading?.killSwitchesEngaged, 2);
    await restored.close();
  });

  test('polling stops when the screen goes away', () async {
    when(repository.fetchLive).thenAnswer((_) async => _reading());
    final cubit = TelemetryCubit(repository, pollInterval: const Duration(milliseconds: 20));

    cubit.startPolling();
    await Future<void>.delayed(const Duration(milliseconds: 70));
    cubit.stopPolling();

    final callsAtStop = verify(repository.fetchLive).callCount;
    await Future<void>.delayed(const Duration(milliseconds: 80));

    verifyNever(repository.fetchLive);
    expect(callsAtStop, greaterThan(1), reason: 'it really was polling');
    await cubit.close();
  });
}
