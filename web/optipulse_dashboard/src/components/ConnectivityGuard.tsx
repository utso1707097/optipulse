/**
 * Always-online guard (T062).
 *
 * OptiPulse's web dashboard is deliberately NOT offline-capable (constitution Principle V,
 * reaffirmed in v2.4.0 when Redux was permitted). Offline support belongs to the Flutter app,
 * which has a reconciliation model for it; the web client has none.
 *
 * The failure this guards against is specific. Every screen here operates live production
 * behaviour — engaging a kill-switch, changing a rollout percentage, reading the conversion
 * numbers an experiment will be decided on. If the network drops and the last-rendered page
 * stays interactive, the user is editing a picture of the system rather than the system, and
 * their "save" will either fail or, worse, apply to state they never actually saw.
 *
 * So the guard REPLACES its children rather than dimming them or overlaying a banner. There is
 * no stale editable state on screen because there is no stale state on screen at all.
 */
import type { ReactNode } from "react";
import { useOnlineStatus } from "../hooks/useOnlineStatus";
import { Button } from "./ui";

export function ConnectivityGuard({ children }: { children: ReactNode }) {
  const online = useOnlineStatus();

  if (online) return <>{children}</>;

  return (
    <div
      role="alert"
      aria-live="assertive"
      className="mx-auto flex max-w-lg flex-col items-center gap-4 rounded-lg border border-warn/30 bg-warn-soft px-6 py-12 text-center"
    >
      <h2 className="text-lg font-semibold text-warn">
        OptiPulse requires a connection
      </h2>
      <p className="text-sm text-warn">
        This dashboard controls live feature behaviour, so it always reads and
        writes against the server — it has no offline mode. Your work area is
        hidden rather than frozen, because acting on out-of-date flag state is
        how the wrong thing gets shipped.
      </p>
      <p className="text-sm text-warn">
        Reconnect and this screen will restore itself automatically.
      </p>
      <Button tone="neutral" onClick={() => window.location.reload()}>
        Reload now
      </Button>
    </div>
  );
}
