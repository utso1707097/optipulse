import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../domain/live_telemetry.dart';
import 'telemetry_cubit.dart';

class TelemetryScreen extends StatefulWidget {
  const TelemetryScreen({super.key});

  @override
  State<TelemetryScreen> createState() => _TelemetryScreenState();
}

class _TelemetryScreenState extends State<TelemetryScreen> {
  @override
  void initState() {
    super.initState();
    context.read<TelemetryCubit>().startPolling();
  }

  @override
  void dispose() {
    context.read<TelemetryCubit>().stopPolling();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<TelemetryCubit, TelemetryState>(
      builder: (context, state) {
        final reading = state.reading;

        return RefreshIndicator(
          onRefresh: () => context.read<TelemetryCubit>().refresh(),
          child: ListView(
            padding: const EdgeInsets.all(16),
            children: [
              if (state.error != null) _ErrorNotice(message: state.error!),
              if (reading == null && state.error == null)
                const Padding(
                  padding: EdgeInsets.symmetric(vertical: 64),
                  child: Center(child: CircularProgressIndicator()),
                ),
              if (reading != null) ...[
                _StalenessNotice(reading: reading, state: state),
                const SizedBox(height: 8),
                _Metric(
                  label: 'Active flags',
                  value: '${reading.activeFlags}',
                  icon: Icons.flag_outlined,
                ),
                _Metric(
                  label: 'Kill switches engaged',
                  value: '${reading.killSwitchesEngaged}',
                  icon: Icons.block,
                  emphasis: reading.hasEngagedKillSwitches,
                ),
                _Metric(
                  label: 'Snapshot version',
                  value: '${reading.snapshotVersion}',
                  icon: Icons.tag,
                ),
                _Metric(
                  label: 'Snapshot age',
                  value: reading.snapshotAgeSeconds == null
                      ? 'nothing published yet'
                      : '${reading.snapshotAgeSeconds}s',
                  icon: Icons.schedule,
                  emphasis: reading.isSnapshotStale,
                ),
                if (reading.isSnapshotStale) const _StaleSnapshotExplainer(),
              ],
            ],
          ),
        );
      },
    );
  }
}

/// A cached reading must be labelled with when it was taken. Without this an ops screen showing
/// Tuesday's figures is indistinguishable from one showing the last fifteen seconds.
class _StalenessNotice extends StatelessWidget {
  const _StalenessNotice({required this.reading, required this.state});

  final LiveTelemetry reading;
  final TelemetryState state;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final age = DateTime.now().difference(reading.observedAt);
    final isStale = state.isStale(DateTime.now());

    return Row(
      children: [
        if (state.isRefreshing)
          const SizedBox(height: 12, width: 12, child: CircularProgressIndicator(strokeWidth: 2))
        else
          Icon(
            isStale ? Icons.history : Icons.circle,
            size: 10,
            color: isStale ? theme.colorScheme.outline : Colors.green,
          ),
        const SizedBox(width: 8),
        Text(
          isStale ? 'As of ${_ago(age)} ago (cached)' : 'Live',
          style: theme.textTheme.bodySmall?.copyWith(color: theme.colorScheme.onSurfaceVariant),
        ),
      ],
    );
  }

  static String _ago(Duration d) {
    if (d.inDays > 0) return '${d.inDays}d';
    if (d.inHours > 0) return '${d.inHours}h';
    if (d.inMinutes > 0) return '${d.inMinutes}m';
    return '${d.inSeconds}s';
  }
}

class _StaleSnapshotExplainer extends StatelessWidget {
  const _StaleSnapshotExplainer();

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Container(
      margin: const EdgeInsets.only(top: 12),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: theme.colorScheme.errorContainer,
        borderRadius: BorderRadius.circular(8),
      ),
      child: Text(
        'The served flag set has not been refreshed recently. Evaluation is still answering, '
        'but from rules that may be out of date — check that invalidation is reaching the API.',
        style: theme.textTheme.bodySmall?.copyWith(color: theme.colorScheme.onErrorContainer),
      ),
    );
  }
}

class _Metric extends StatelessWidget {
  const _Metric({
    required this.label,
    required this.value,
    required this.icon,
    this.emphasis = false,
  });

  final String label;
  final String value;
  final IconData icon;
  final bool emphasis;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Card(
      margin: const EdgeInsets.only(top: 12),
      child: ListTile(
        leading: Icon(icon, color: emphasis ? theme.colorScheme.error : null),
        title: Text(label),
        trailing: Text(
          value,
          style: theme.textTheme.titleMedium?.copyWith(
            fontWeight: FontWeight.w600,
            color: emphasis ? theme.colorScheme.error : null,
          ),
        ),
      ),
    );
  }
}

class _ErrorNotice extends StatelessWidget {
  const _ErrorNotice({required this.message});

  final String message;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Container(
      padding: const EdgeInsets.all(12),
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: theme.colorScheme.errorContainer,
        borderRadius: BorderRadius.circular(8),
      ),
      child: Text(message, style: TextStyle(color: theme.colorScheme.onErrorContainer)),
    );
  }
}
