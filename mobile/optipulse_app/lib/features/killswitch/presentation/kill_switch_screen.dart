import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../auth/presentation/auth_cubit.dart';
import '../domain/flag.dart';
import 'kill_switch_cubit.dart';

class KillSwitchScreen extends StatelessWidget {
  const KillSwitchScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final isAdmin = context.select<AuthCubit, bool>((cubit) => cubit.state.isAdmin);

    return BlocBuilder<KillSwitchCubit, KillSwitchState>(
      builder: (context, state) {
        return Column(
          children: [
            if (!state.isOnline) const _OfflineBanner(),
            Expanded(child: _body(context, state, isAdmin)),
          ],
        );
      },
    );
  }

  Widget _body(BuildContext context, KillSwitchState state, bool isAdmin) {
    return RefreshIndicator(
          onRefresh: () => context.read<KillSwitchCubit>().reconcileWithServer(),
          child: switch (state.status) {
            FlagsStatus.initial || FlagsStatus.loading =>
              const Center(child: CircularProgressIndicator()),
            FlagsStatus.failed => _ErrorView(message: state.error ?? 'Something went wrong.'),
            FlagsStatus.ready when state.flags.isEmpty => const _EmptyView(),
            FlagsStatus.ready => ListView.separated(
                padding: const EdgeInsets.symmetric(vertical: 8),
                itemCount: state.flags.length,
                separatorBuilder: (_, _) => const Divider(height: 1),
                itemBuilder: (context, index) => _FlagTile(
                  flag: state.flags[index],
                  state: state,
                  isAdmin: isAdmin,
                ),
              ),
          },
        );
  }
}

/// Shown while the device reports no network path.
///
/// It says actions are QUEUED rather than blocked, because that is what happens: the controls
/// stay live, the attempt is still made, and anything that does not land is kept. Greying the
/// screen out would be both a lie and an obstruction — connectivity reports are a hint, and an
/// admin in a basement during an incident should still be able to press the button.
class _OfflineBanner extends StatelessWidget {
  const _OfflineBanner();

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Container(
      width: double.infinity,
      color: theme.colorScheme.tertiaryContainer,
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
      child: Row(
        children: [
          Icon(Icons.cloud_off, size: 18, color: theme.colorScheme.onTertiaryContainer),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              'Offline — actions are kept and sent when the connection returns.',
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onTertiaryContainer,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _FlagTile extends StatelessWidget {
  const _FlagTile({required this.flag, required this.state, required this.isAdmin});

  final Flag flag;
  final KillSwitchState state;
  final bool isAdmin;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final engaged = state.displayedEngagement(flag);
    final intent = state.intents[flag.key];
    final unconfirmed = state.isUnconfirmed(flag.key);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        ListTile(
          title: Text(flag.name, style: const TextStyle(fontWeight: FontWeight.w500)),
          subtitle: Text(
            flag.key,
            style: theme.textTheme.bodySmall?.copyWith(
              color: theme.colorScheme.onSurfaceVariant,
              fontFamily: 'monospace',
            ),
          ),
          trailing: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              if (engaged)
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: theme.colorScheme.errorContainer,
                    borderRadius: BorderRadius.circular(6),
                  ),
                  child: Text(
                    'KILLED',
                    style: theme.textTheme.labelSmall?.copyWith(
                      color: theme.colorScheme.onErrorContainer,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                ),
              const SizedBox(width: 8),
              // The switch is rendered for everyone but only OPERABLE by admins. Hiding it
              // entirely would leave a non-admin unable to see that a flag is killed, which is
              // information they need during an incident even when they cannot act on it.
              // This is an affordance, not the authorization: the API enforces the Admin
              // policy regardless of what this widget allows.
              Switch(
                value: engaged,
                onChanged: !isAdmin || (intent?.isPending ?? false)
                    ? null
                    : (value) => _confirm(context, flag, value),
              ),
            ],
          ),
        ),
        if (unconfirmed) _IntentBanner(flagKey: flag.key, state: state),
      ],
    );
  }

  Future<void> _confirm(BuildContext context, Flag flag, bool engage) async {
    final cubit = context.read<KillSwitchCubit>();

    // Engaging is destructive to live traffic and gets a confirmation. RELEASING does not:
    // during an incident the release is the recovery, and a dialog between an admin and
    // restoring service is friction in exactly the wrong place.
    if (engage) {
      final confirmed = await showDialog<bool>(
        context: context,
        builder: (dialogContext) => AlertDialog(
          title: const Text('Engage kill switch?'),
          content: Text(
            'Every evaluation of "${flag.key}" will immediately return its default outcome, '
            'for all users, on every node. Experiments on this flag stop collecting data.',
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(dialogContext).pop(false),
              child: const Text('Cancel'),
            ),
            FilledButton(
              onPressed: () => Navigator.of(dialogContext).pop(true),
              style: FilledButton.styleFrom(
                backgroundColor: Theme.of(dialogContext).colorScheme.error,
              ),
              child: const Text('Engage'),
            ),
          ],
        ),
      );
      if (confirmed != true) return;
    }

    await cubit.setKillSwitch(flagKey: flag.key, engage: engage);
  }
}

/// Shown while an intent is unconfirmed, and kept on screen if it failed.
class _IntentBanner extends StatelessWidget {
  const _IntentBanner({required this.flagKey, required this.state});

  final String flagKey;
  final KillSwitchState state;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final intent = state.intents[flagKey]!;
    final conflicted = state.conflicts.contains(flagKey);
    final failed = intent.isFailed || conflicted;

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.fromLTRB(16, 8, 8, 12),
      color: failed
          ? theme.colorScheme.errorContainer
          : theme.colorScheme.secondaryContainer,
      child: Row(
        children: [
          if (!failed)
            const SizedBox(
              height: 14,
              width: 14,
              child: CircularProgressIndicator(strokeWidth: 2),
            )
          else
            Icon(Icons.warning_amber_rounded, size: 18, color: theme.colorScheme.onErrorContainer),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              conflicted
                  ? 'Someone engaged this kill switch after you asked to release it.'
                  : failed
                      ? '${intent.engage ? 'Engage' : 'Release'} failed — ${intent.error}'
                      : '${intent.engage ? 'Engaging' : 'Releasing'}…',
              style: theme.textTheme.bodySmall?.copyWith(
                color: failed
                    ? theme.colorScheme.onErrorContainer
                    : theme.colorScheme.onSecondaryContainer,
              ),
            ),
          ),
          if (conflicted) ...[
            // A conflict is not a failed request, so it does not offer Retry. The server has
            // engaged this kill switch since the release was asked for; re-sending the release
            // is a decision to override that, and it should be made deliberately.
            TextButton(
              onPressed: () => context.read<KillSwitchCubit>().retry(flagKey),
              child: const Text('Release anyway'),
            ),
            TextButton(
              onPressed: () => context.read<KillSwitchCubit>().acceptServerState(flagKey),
              child: const Text('Keep killed'),
            ),
          ] else if (failed) ...[
            TextButton(
              onPressed: () => context.read<KillSwitchCubit>().retry(flagKey),
              child: const Text('Retry'),
            ),
            TextButton(
              onPressed: () => context.read<KillSwitchCubit>().dismiss(flagKey),
              child: const Text('Dismiss'),
            ),
          ],
        ],
      ),
    );
  }
}

class _EmptyView extends StatelessWidget {
  const _EmptyView();

  @override
  Widget build(BuildContext context) => ListView(
        children: const [
          SizedBox(height: 120),
          Icon(Icons.flag_outlined, size: 48),
          SizedBox(height: 12),
          Center(child: Text('No flags yet. Create one in the web dashboard.')),
        ],
      );
}

class _ErrorView extends StatelessWidget {
  const _ErrorView({required this.message});

  final String message;

  @override
  Widget build(BuildContext context) => ListView(
        children: [
          const SizedBox(height: 100),
          Icon(Icons.cloud_off, size: 48, color: Theme.of(context).colorScheme.error),
          const SizedBox(height: 12),
          Center(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 32),
              child: Text(message, textAlign: TextAlign.center),
            ),
          ),
          const SizedBox(height: 8),
          const Center(child: Text('Pull down to retry.')),
        ],
      );
}
