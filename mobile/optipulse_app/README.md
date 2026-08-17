# OptiPulse Mobile App (Flutter)

The offline-first ops app for Admin & DevOps: real-time telemetry monitoring, push notifications on
critical events, and instant kill-switch activation. **iOS + Android only — not Flutter Web.**
Clean Architecture with BLoC/Cubit, HydratedBloc (offline persistence), and Dio behind repositories.
API access via Dart models generated from the backend OpenAPI spec.

- Design & constraints: [../../specs/001-optipulse-platform/plan.md](../../specs/001-optipulse-platform/plan.md) (Principle V)
- Contract generation: [../../specs/001-optipulse-platform/contracts/openapi-pipeline.md](../../specs/001-optipulse-platform/contracts/openapi-pipeline.md)
- Tasks: Phase 6 (US4) in [../../specs/001-optipulse-platform/tasks.md](../../specs/001-optipulse-platform/tasks.md)
- Best practices: `.kits/flutter-ai-rules` — adopt `bloc`, `flutter-best-practices`,
  `architecture-feature-first`, `testing`, `mocktail`, `firebase-messaging`,
  `firebase-crashlytics`; **skip** `riverpod`/`provider`/`change-notifier` (state mgmt).

## ⚠️ Scaffold status: blocked on Flutter SDK

The Flutter CLI is **not installed** in the environment this was authored in, so the standard
`flutter create` bootstrap (platform folders, Gradle/Xcode projects, `.metadata`, generated main.dart)
could not be run. What exists today is **hand-authored, unverified**:

- `pubspec.yaml` — dependencies pinned to real pub.dev versions at authoring time.
- `lib/` and `test/` folder structure below, matching plan.md.

**To unblock**, install Flutter, then from this directory run:

```bash
flutter create . --platforms=ios,android --org com.optipulse
flutter pub get
flutter analyze
```

This will generate `android/`, `ios/`, `lib/main.dart`, and platform config without touching the
folders below — resolve any warnings the toolchain raises before writing feature code.

## Structure

```text
lib/
├── core/
│   ├── di/          # get_it service locator (DI only, not app state)
│   ├── network/      # Dio client configuration
│   ├── reconcile/    # deterministic offline reconcile on reconnect
│   └── generated/    # Dart models generated from backend OpenAPI spec
└── features/
    ├── auth/{domain,data,presentation}/
    ├── telemetry/{domain,data,presentation}/    # real-time monitoring
    ├── alerts/{domain,data,presentation}/       # push notifications + history
    └── killswitch/{domain,data,presentation}/   # instant kill-switch
test/                                             # bloc_test + widget tests
```
