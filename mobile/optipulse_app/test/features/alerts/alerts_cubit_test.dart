import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:optipulse_app/features/alerts/data/push_registrar.dart';
import 'package:optipulse_app/features/alerts/domain/alert.dart';
import 'package:optipulse_app/features/alerts/domain/alert_repository.dart';
import 'package:optipulse_app/features/alerts/presentation/alerts_cubit.dart';

import '../../support/memory_storage.dart';

class _MockRepo extends Mock implements AlertRepository {}

Alert _alert({
  String id = 'a1',
  AlertSeverity severity = AlertSeverity.critical,
  DateTime? acknowledgedAt,
  String? acknowledgedBy,
}) =>
    Alert(
      id: id,
      raisedAt: DateTime(2026, 8, 19, 12),
      kind: 'KillSwitchChanged',
      severity: severity,
      title: 'Kill switch ENGAGED on checkout',
      detail: 'ada engaged the kill switch.',
      flagKey: 'checkout',
      acknowledgedAt: acknowledgedAt,
      acknowledgedBy: acknowledgedBy,
    );

void main() {
  late _MockRepo repository;

  setUp(() {
    repository = _MockRepo();
    useMemoryStorage();
  });

  test('acknowledging takes the SERVER version, not an optimistic local one', () async {
    // The server keeps the FIRST responder. Writing this device's own name optimistically would
    // show the wrong person whenever two admins acknowledge at once.
    when(() => repository.list(
        unacknowledgedOnly: any(named: 'unacknowledgedOnly'),
        limit: any(named: 'limit'))).thenAnswer((_) async => [_alert()]);
    when(() => repository.acknowledge(any())).thenAnswer(
      (_) async => _alert(acknowledgedAt: DateTime(2026, 8, 19, 12, 5), acknowledgedBy: 'grace'),
    );

    final cubit = AlertsCubit(repository);
    await cubit.load();
    await cubit.acknowledge('a1');

    expect(cubit.state.alerts.single.acknowledgedBy, 'grace');
    expect(cubit.state.acknowledging, isEmpty);
    await cubit.close();
  });

  test('a failed acknowledgement clears the in-flight marker', () async {
    when(() => repository.list(
        unacknowledgedOnly: any(named: 'unacknowledgedOnly'),
        limit: any(named: 'limit'))).thenAnswer((_) async => [_alert()]);
    when(() => repository.acknowledge(any())).thenThrow(const AlertFailure.network());

    final cubit = AlertsCubit(repository);
    await cubit.load();
    await cubit.acknowledge('a1');

    expect(cubit.state.acknowledging, isEmpty, reason: 'the row must not stay stuck spinning');
    expect(cubit.state.error, isNotNull);
    expect(cubit.state.alerts.single.isAcknowledged, isFalse);
    await cubit.close();
  });

  test('the cached history is KEPT when a refresh fails', () async {
    // An alert is a record of something that already happened. Discarding it because the network
    // dropped would hide the incident report at the moment the network is worst — and the
    // history surviving delivery failure is the whole reason it is durable server-side.
    when(() => repository.list(
        unacknowledgedOnly: any(named: 'unacknowledgedOnly'),
        limit: any(named: 'limit'))).thenAnswer((_) async => [_alert()]);

    final cubit = AlertsCubit(repository);
    await cubit.load();

    when(() => repository.list(
        unacknowledgedOnly: any(named: 'unacknowledgedOnly'),
        limit: any(named: 'limit'))).thenThrow(const AlertFailure.network());
    await cubit.load();

    expect(cubit.state.alerts, hasLength(1));
    expect(cubit.state.status, AlertsStatus.failed);
    await cubit.close();
  });

  test('the badge counts unacknowledged alerts only', () {
    final state = AlertsState(alerts: [
      _alert(id: 'a1'),
      _alert(id: 'a2', acknowledgedAt: DateTime(2026, 8, 19, 12, 5)),
      _alert(id: 'a3', severity: AlertSeverity.warning),
    ]);

    expect(state.unacknowledgedCount, 2);
    expect(state.hasUnacknowledgedCritical, isTrue);
  });

  test('an unrecognised severity does not crash the app', () {
    // The server may add a severity this build has never heard of. An ops app that crashes on
    // an unknown alert fails exactly when there is something to report.
    expect(Alert.parseSeverity('apocalyptic'), AlertSeverity.unknown);
    expect(Alert.parseSeverity('CRITICAL'), AlertSeverity.critical);
  });

  test('history survives a restart', () async {
    when(() => repository.list(
        unacknowledgedOnly: any(named: 'unacknowledgedOnly'),
        limit: any(named: 'limit'))).thenAnswer((_) async => [_alert()]);

    final first = AlertsCubit(repository);
    await first.load();
    await first.close();

    final restored = AlertsCubit(repository);

    expect(restored.state.alerts, hasLength(1));
    expect(restored.state.status, AlertsStatus.initial, reason: 'cached, not freshly fetched');
    await restored.close();
  });

  group('push registration', () {
    test('reports false when no provider is configured, without throwing', () async {
      // Not a failure: the backend treats push as an optimisation over a durable history, so an
      // app with no provider is fully functional.
      final registration = PushRegistration(const NoPushRegistrar(), repository);

      await expectLater(registration.registerIfAvailable(), completion(isFalse));
      verifyNever(() => repository.registerDevice(
          platform: any(named: 'platform'), token: any(named: 'token')));
    });

    test('a registration failure is swallowed rather than blocking the app', () async {
      when(() => repository.registerDevice(
              platform: any(named: 'platform'), token: any(named: 'token')))
          .thenThrow(const AlertFailure.network());

      final registration = PushRegistration(_StubRegistrar('tok'), repository);

      await expectLater(registration.registerIfAvailable(), completion(isFalse));
    });

    test('a token is registered when one is available', () async {
      when(() => repository.registerDevice(
          platform: any(named: 'platform'), token: any(named: 'token'))).thenAnswer((_) async {});

      final registration = PushRegistration(_StubRegistrar('tok-123'), repository);

      await expectLater(registration.registerIfAvailable(), completion(isTrue));
      verify(() => repository.registerDevice(platform: 'Ios', token: 'tok-123')).called(1);
    });
  });
}

class _StubRegistrar implements PushRegistrar {
  const _StubRegistrar(this._token);
  final String _token;

  @override
  Future<String?> obtainToken() async => _token;

  @override
  String get platform => 'Ios';
}
