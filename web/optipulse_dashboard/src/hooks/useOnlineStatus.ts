import { useEffect, useState } from "react";

/**
 * Tracks browser connectivity for the always-online guard (T062).
 *
 * Lives in its own module rather than beside ConnectivityGuard so that file exports only
 * components — a mixed module breaks React Fast Refresh, which oxlint flags.
 */
export function useOnlineStatus(): boolean {
  const [online, setOnline] = useState(() =>
    typeof navigator === "undefined" ? true : navigator.onLine,
  );

  useEffect(() => {
    const goOnline = () => setOnline(true);
    const goOffline = () => setOnline(false);
    window.addEventListener("online", goOnline);
    window.addEventListener("offline", goOffline);
    // navigator.onLine can have changed between the initial render and this effect running.
    setOnline(navigator.onLine);
    return () => {
      window.removeEventListener("online", goOnline);
      window.removeEventListener("offline", goOffline);
    };
  }, []);

  return online;
}
