# OptiPulse Mobile App — Claude Code Guidance

Flutter, the offline-first Admin/DevOps app (iOS + Android only). Read
[`.specify/memory/constitution.md`](../../.specify/memory/constitution.md) first (Principle V) —
this file only adds mobile-specific operating detail on top of it.

## State management — pinned, no exceptions

- **BLoC/Cubit + HydratedBloc only.** `.kits/flutter-ai-rules` (git-ignored, local reference only)
  ships `riverpod`, `provider`, and `flutter-change-notifier` skills as equal peers to `bloc` — those
  are **not used** here. If you find yourself reaching for `ChangeNotifier`, `Provider`-as-state, or
  Riverpod providers for app state, stop — use a Cubit/Bloc instead.
- `provider` the *package* may appear only as DI plumbing (e.g., under `BlocProvider`), never as the
  app-state mechanism itself.
- Always include the domain layer per feature (`domain/data/presentation`) — the kit treats domain as
  optional for "simple CRUD"; this project does not.

## Networking

- **Dio only**, behind a repository abstraction in each feature's `data/` layer. Kit examples use
  `http.Client` — treat every such example as "Dio" when adapting it.

## API contract

- `lib/core/generated/` is **generated** from the backend's native OpenAPI spec by
  `../../contracts-gen/generate.sh` — never hand-edit files there. Do not follow the kit's
  anti-OpenAPI-codegen opinion (`enterprise-scale.md`); OpenAPI→Dart generation is mandatory here
  (constitution Principle VII).

## Scaffold status

⚠️ Flutter SDK was not available when this project was scaffolded — see the root `README.md` in this
directory for the exact `flutter create` command to run before writing platform code.

## Tooling recommendations (from `.kits/flutter-ai-rules`, adopt as-is)

- `bloc`, `flutter-best-practices`, `architecture-feature-first`, `testing`, `mocktail` (not
  `mockito`), `patrol-e2e-testing`, `flutter-errors`, `effective-dart`.
- `firebase-messaging` for push notifications (FR-026), `firebase-crashlytics` for crash telemetry.
- Skip: `riverpod`, `provider` (as state mgmt), `flutter-change-notifier`, `mockito`, any
  Flutter-Web-specific guidance, `revenuecat-testing`.
