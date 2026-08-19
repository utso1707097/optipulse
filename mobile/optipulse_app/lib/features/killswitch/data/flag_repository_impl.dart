import 'package:dio/dio.dart';
import 'package:openapi/openapi.dart' as api;

import '../domain/flag.dart';
import '../domain/flag_repository.dart';

class FlagRepositoryImpl implements FlagRepository {
  FlagRepositoryImpl(this._api);

  final api.FlagsApi _api;

  @override
  Future<List<Flag>> listFlags() async {
    try {
      final response = await _api.listFlags();
      return (response.data ?? const <api.FlagResponse>[]).map(_toFlag).toList();
    } on DioException catch (error) {
      throw _translate(error);
    }
  }

  @override
  Future<Flag> setKillSwitch({required String key, required bool engaged}) async {
    try {
      final response = await _api.setKillSwitch(
        key: key,
        killSwitchRequest: api.KillSwitchRequest((b) => b..engaged = engaged),
      );
      return _toFlag(response.data!);
    } on DioException catch (error) {
      throw _translate(error);
    }
  }

  Flag _toFlag(api.FlagResponse response) => Flag(
        key: response.key,
        name: response.name,
        status: response.status,
        killSwitchEngaged: response.killSwitchEngaged,
        version: response.version,
      );

  FlagRepositoryFailure _translate(DioException error) {
    switch (error.type) {
      case DioExceptionType.connectionError:
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.sendTimeout:
      case DioExceptionType.receiveTimeout:
        return const FlagRepositoryFailure.network();
      case DioExceptionType.badResponse:
        final status = error.response?.statusCode;
        if (status == 403) {
          // The server refused on authorization grounds. Surfaced distinctly because it is the
          // one failure that retrying cannot fix, and the UI should stop offering a retry.
          return const FlagRepositoryFailure(
            'Only Admins can operate the kill switch.',
            isForbidden: true,
          );
        }
        if (status == 404) {
          return const FlagRepositoryFailure('That flag no longer exists.');
        }
        return FlagRepositoryFailure(
          'OptiPulse returned an unexpected error (${status ?? 'no status'}).',
        );
      default:
        return const FlagRepositoryFailure.network();
    }
  }
}
