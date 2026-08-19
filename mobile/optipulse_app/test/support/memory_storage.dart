import 'package:hydrated_bloc/hydrated_bloc.dart';

/// In-memory [Storage] for HydratedCubit tests.
///
/// HydratedBloc.storage is global and has no default, so any test touching a HydratedCubit
/// fails at construction without one. Installing this in setUp also gives each test a FRESH
/// store, which matters: persistence is the thing under test, and a store leaking between
/// tests would let one test's intents satisfy another's assertions.
class MemoryStorage implements Storage {
  final Map<String, dynamic> _store = {};

  @override
  dynamic read(String key) => _store[key];

  @override
  Future<void> write(String key, dynamic value) async => _store[key] = value;

  @override
  Future<void> delete(String key) async => _store.remove(key);

  @override
  Future<void> clear() async => _store.clear();

  @override
  Future<void> close() async {}
}

/// Installs a fresh store. Call from `setUp`.
MemoryStorage useMemoryStorage() {
  final storage = MemoryStorage();
  HydratedBloc.storage = storage;
  return storage;
}
