import 'dart:async';

import 'package:connectivity_plus/connectivity_plus.dart';

/// Reports whether the device believes it has a network path.
///
/// An interface rather than direct use of connectivity_plus, because reconnection is the
/// hardest thing in this feature to test and the easiest to get wrong. A fake implementation
/// lets a test drive offline -> online transitions deterministically; a plugin cannot be driven
/// from a unit test at all.
abstract class ConnectivityMonitor {
  /// Emits `true` when a network path appears, `false` when it goes away. Emits the current
  /// value on subscription so a listener does not have to wait for a change to learn the state.
  Stream<bool> get onConnectivityChanged;

  Future<bool> get isOnline;
}

class ConnectivityPlusMonitor implements ConnectivityMonitor {
  ConnectivityPlusMonitor([Connectivity? connectivity])
      : _connectivity = connectivity ?? Connectivity();

  final Connectivity _connectivity;

  static bool _hasPath(List<ConnectivityResult> results) =>
      results.any((result) => result != ConnectivityResult.none);

  @override
  Stream<bool> get onConnectivityChanged async* {
    yield await isOnline;
    yield* _connectivity.onConnectivityChanged.map(_hasPath).distinct();
  }

  @override
  Future<bool> get isOnline async => _hasPath(await _connectivity.checkConnectivity());
}
