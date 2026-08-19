import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../domain/alert.dart';
import 'alerts_cubit.dart';

class AlertsScreen extends StatelessWidget {
  const AlertsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<AlertsCubit, AlertsState>(
      builder: (context, state) {
        return RefreshIndicator(
          onRefresh: () => context.read<AlertsCubit>().load(),
          child: switch (state.status) {
            AlertsStatus.initial || AlertsStatus.loading when state.alerts.isEmpty =>
              const Center(child: CircularProgressIndicator()),
            AlertsStatus.failed when state.alerts.isEmpty =>
              _Message(icon: Icons.cloud_off, text: state.error ?? 'Could not load alerts.'),
            _ when state.alerts.isEmpty =>
              const _Message(icon: Icons.notifications_none, text: 'No alerts. Nothing has needed attention.'),
            _ => ListView.separated(
                padding: const EdgeInsets.symmetric(vertical: 8),
                itemCount: state.alerts.length + (state.error == null ? 0 : 1),
                separatorBuilder: (_, _) => const Divider(height: 1),
                itemBuilder: (context, index) {
                  // The cached history is shown even when the refresh failed — an alert is a
                  // record of something that already happened and stays true, unlike a flag list
                  // where stale rows invite a wrong action.
                  if (state.error != null && index == 0) {
                    return _StaleBanner(message: state.error!);
                  }
                  final alert = state.alerts[index - (state.error == null ? 0 : 1)];
                  return _AlertTile(alert: alert, state: state);
                },
              ),
          },
        );
      },
    );
  }
}

class _AlertTile extends StatelessWidget {
  const _AlertTile({required this.alert, required this.state});

  final Alert alert;
  final AlertsState state;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final busy = state.acknowledging.contains(alert.id);

    final (color, icon) = switch (alert.severity) {
      AlertSeverity.critical => (theme.colorScheme.error, Icons.error),
      AlertSeverity.warning => (Colors.orange, Icons.warning_amber_rounded),
      AlertSeverity.info => (theme.colorScheme.primary, Icons.info_outline),
      AlertSeverity.unknown => (theme.colorScheme.outline, Icons.help_outline),
    };

    return Opacity(
      // Acknowledged alerts are dimmed, never hidden. The history is the record; removing rows
      // once seen would make it impossible to review what happened during an incident.
      opacity: alert.isAcknowledged ? 0.55 : 1,
      child: ListTile(
        isThreeLine: true,
        leading: Icon(icon, color: color),
        title: Text(alert.title, style: const TextStyle(fontWeight: FontWeight.w500)),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SizedBox(height: 2),
            Text(alert.detail),
            const SizedBox(height: 4),
            Text(
              alert.isAcknowledged
                  ? '${_when(alert.raisedAt)} · acknowledged by ${alert.acknowledgedBy ?? 'someone'}'
                  : _when(alert.raisedAt),
              style: theme.textTheme.bodySmall?.copyWith(color: theme.colorScheme.onSurfaceVariant),
            ),
          ],
        ),
        trailing: alert.isAcknowledged
            ? Icon(Icons.check_circle, color: theme.colorScheme.outline, size: 20)
            : busy
                ? const SizedBox(
                    height: 18, width: 18, child: CircularProgressIndicator(strokeWidth: 2))
                : TextButton(
                    onPressed: () => context.read<AlertsCubit>().acknowledge(alert.id),
                    child: const Text('Ack'),
                  ),
      ),
    );
  }

  static String _when(DateTime raisedAt) {
    final diff = DateTime.now().difference(raisedAt);
    if (diff.inDays > 0) return '${diff.inDays}d ago';
    if (diff.inHours > 0) return '${diff.inHours}h ago';
    if (diff.inMinutes > 0) return '${diff.inMinutes}m ago';
    return 'just now';
  }
}

class _StaleBanner extends StatelessWidget {
  const _StaleBanner({required this.message});

  final String message;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Container(
      width: double.infinity,
      color: theme.colorScheme.tertiaryContainer,
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
      child: Text(
        '$message Showing the last alerts this device received.',
        style: theme.textTheme.bodySmall?.copyWith(color: theme.colorScheme.onTertiaryContainer),
      ),
    );
  }
}

class _Message extends StatelessWidget {
  const _Message({required this.icon, required this.text});

  final IconData icon;
  final String text;

  @override
  Widget build(BuildContext context) => ListView(
        children: [
          const SizedBox(height: 120),
          Icon(icon, size: 48, color: Theme.of(context).colorScheme.outline),
          const SizedBox(height: 12),
          Center(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 32),
              child: Text(text, textAlign: TextAlign.center),
            ),
          ),
        ],
      );
}
