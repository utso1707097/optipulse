import 'flag.dart';

class FlagRepositoryFailure implements Exception {
  const FlagRepositoryFailure(this.message, {this.isNetwork = false, this.isForbidden = false});

  const FlagRepositoryFailure.network()
      : message = 'Could not reach OptiPulse.',
        isNetwork = true,
        isForbidden = false;

  final String message;
  final bool isNetwork;
  final bool isForbidden;

  @override
  String toString() => 'FlagRepositoryFailure: $message';
}

abstract class FlagRepository {
  Future<List<Flag>> listFlags();

  /// Returns the flag as the SERVER now reports it, which is what makes an intent
  /// "confirmed" rather than merely "sent".
  Future<Flag> setKillSwitch({required String key, required bool engaged});
}
