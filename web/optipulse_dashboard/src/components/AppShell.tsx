import { NavLink, Outlet } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "../hooks";
import { logout } from "../store/authSlice";
import { Button } from "./ui";
import { ConnectivityGuard } from "./ConnectivityGuard";

const NAV = [
  { to: "/flags", label: "Flags" },
  { to: "/experiments", label: "Experiments" },
  { to: "/analytics", label: "Analytics" },
];

export function AppShell() {
  const dispatch = useAppDispatch();
  const { profile, role } = useAppSelector((s) => s.auth);

  return (
    <div className="min-h-screen">
      <header className="border-b border-line bg-surface">
        <div className="mx-auto flex max-w-5xl flex-wrap items-center gap-4 px-4 py-3">
          <span className="font-semibold text-ink">OptiPulse</span>
          <nav className="flex gap-1">
            {NAV.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  `rounded-md px-3 py-1.5 text-sm ${
                    isActive
                      ? "bg-brand-soft font-medium text-brand"
                      : "text-muted hover:text-ink"
                  }`
                }
              >
                {item.label}
              </NavLink>
            ))}
          </nav>
          <div className="ml-auto flex items-center gap-3">
            <span className="text-sm text-muted">
              {profile?.name ?? profile?.email ?? "Signed in"}
              {role ? ` · ${role}` : ""}
            </span>
            <Button tone="neutral" onClick={() => void dispatch(logout())}>
              Sign out
            </Button>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-5xl px-4 py-6">
        {/* Wraps the routed content, not the chrome: signing out must stay available even when
            the connection drops, so the header is outside the guard. */}
        <ConnectivityGuard>
          <Outlet />
        </ConnectivityGuard>
      </main>
    </div>
  );
}
