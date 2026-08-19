import { useEffect } from "react";
import { Navigate, Route, Routes } from "react-router-dom";
import { useAppDispatch, useAppSelector } from "./hooks";
import { loadProfile, restoreSession } from "./store/authSlice";
import { AppShell } from "./components/AppShell";
import { Spinner } from "./components/ui";
import { LoginScreen } from "./features/auth/LoginScreen";
import { FlagsScreen } from "./features/flags/FlagsScreen";
import { ExperimentsScreen } from "./features/experiments/ExperimentsScreen";
import { AnalyticsScreen } from "./features/analytics/AnalyticsScreen";

export default function App() {
  const dispatch = useAppDispatch();
  const status = useAppSelector((s) => s.auth.status);
  const hasProfile = useAppSelector((s) => s.auth.profile !== null);

  useEffect(() => {
    void dispatch(restoreSession());
  }, [dispatch]);

  useEffect(() => {
    if (status === "authenticated" && !hasProfile) void dispatch(loadProfile());
  }, [dispatch, status, hasProfile]);

  if (status === "restoring")
    return <Spinner label="Restoring your session…" />;
  if (status !== "authenticated") return <LoginScreen />;

  return (
    <Routes>
      <Route element={<AppShell />}>
        <Route path="/flags" element={<FlagsScreen />} />
        <Route path="/experiments" element={<ExperimentsScreen />} />
        <Route path="/analytics" element={<AnalyticsScreen />} />
        <Route path="*" element={<Navigate to="/flags" replace />} />
      </Route>
    </Routes>
  );
}
