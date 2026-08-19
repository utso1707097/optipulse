import { useState, type FormEvent } from "react";
import { useAppDispatch, useAppSelector } from "../../hooks";
import { login } from "../../store/authSlice";
import { Button, Card, ErrorBanner, Field, Input } from "../../components/ui";

export function LoginScreen() {
  const dispatch = useAppDispatch();
  const { loginPending, loginError } = useAppSelector((s) => s.auth);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  function onSubmit(event: FormEvent) {
    event.preventDefault();
    void dispatch(login({ email, password }));
  }

  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <Card className="w-full max-w-sm p-6">
        <h1 className="text-xl font-semibold text-ink">OptiPulse</h1>
        <p className="mt-1 mb-6 text-sm text-muted">
          Sign in to manage flags and experiments.
        </p>

        {loginError ? (
          <div className="mb-4">
            <ErrorBanner message={loginError} />
          </div>
        ) : null}

        <form className="space-y-4" onSubmit={onSubmit}>
          <Field label="Email">
            <Input
              type="email"
              name="email"
              autoComplete="username"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </Field>
          <Field label="Password">
            <Input
              type="password"
              name="password"
              autoComplete="current-password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </Field>
          <Button
            type="submit"
            tone="brand"
            className="w-full"
            disabled={loginPending}
          >
            {loginPending ? "Signing in…" : "Sign in"}
          </Button>
        </form>
      </Card>
    </div>
  );
}
