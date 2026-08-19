/**
 * Presentational primitives. Deliberately small and unabstracted: the dashboard has five
 * screens, and a component library would be more code than the screens it serves.
 */
import { cloneElement, useId } from "react";
import type {
  ButtonHTMLAttributes,
  InputHTMLAttributes,
  ReactElement,
  ReactNode,
  SelectHTMLAttributes,
} from "react";

type Tone = "brand" | "neutral" | "danger";

const BUTTON_TONES: Record<Tone, string> = {
  brand: "bg-brand text-brand-ink hover:bg-violet-700",
  neutral: "bg-surface text-ink border border-line hover:bg-canvas",
  danger: "bg-danger text-white hover:bg-red-800",
};

export function Button({
  tone = "neutral",
  className = "",
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & { tone?: Tone }) {
  return (
    <button
      {...props}
      className={`inline-flex items-center justify-center gap-2 rounded-md px-3 py-1.5 text-sm font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-50 ${BUTTON_TONES[tone]} ${className}`}
    />
  );
}

export function Card({
  children,
  className = "",
}: {
  children: ReactNode;
  className?: string;
}) {
  return (
    <div
      className={`rounded-lg border border-line bg-surface shadow-sm ${className}`}
    >
      {children}
    </div>
  );
}

/**
 * The hint sits OUTSIDE the <label> and is linked with aria-describedby.
 *
 * Nesting it inside would fold the hint into the control's accessible NAME, so a field labelled
 * "Salt" would announce as "Salt Fixes which users fall inside the rollout…" — and any test or
 * assistive technology looking for the control called "Salt" would not find it. Name and
 * description are different things and screen readers treat them differently.
 */
export function Field({
  label,
  hint,
  children,
}: {
  label: string;
  hint?: string;
  children: ReactElement<{ "aria-describedby"?: string }>;
}) {
  const hintId = useId();
  return (
    <div>
      <label className="block">
        <span className="mb-1 block text-sm font-medium text-ink">{label}</span>
        {hint
          ? cloneElement(children, { "aria-describedby": hintId })
          : children}
      </label>
      {hint ? (
        <p id={hintId} className="mt-1 text-xs text-muted">
          {hint}
        </p>
      ) : null}
    </div>
  );
}

const CONTROL =
  "w-full rounded-md border border-line bg-surface px-3 py-1.5 text-sm text-ink outline-none focus:border-brand focus:ring-2 focus:ring-brand/20";

export function Input({
  className = "",
  ...props
}: InputHTMLAttributes<HTMLInputElement>) {
  return <input {...props} className={`${CONTROL} ${className}`} />;
}

export function Select({
  className = "",
  ...props
}: SelectHTMLAttributes<HTMLSelectElement>) {
  return <select {...props} className={`${CONTROL} ${className}`} />;
}

const BADGE_TONES: Record<string, string> = {
  ok: "bg-ok-soft text-ok",
  warn: "bg-warn-soft text-warn",
  danger: "bg-danger-soft text-danger",
  muted: "bg-canvas text-muted border border-line",
};

export function Badge({
  tone = "muted",
  children,
}: {
  tone?: keyof typeof BADGE_TONES;
  children: ReactNode;
}) {
  return (
    <span
      className={`inline-block rounded-full px-2 py-0.5 text-xs font-medium ${BADGE_TONES[tone]}`}
    >
      {children}
    </span>
  );
}

/**
 * An error the user can act on. `onRetry` is offered only when retrying is meaningful — a 403
 * will not resolve by pressing a button, and offering one would waste the user's time.
 */
export function ErrorBanner({
  message,
  onRetry,
  onDismiss,
}: {
  message: string;
  onRetry?: () => void;
  onDismiss?: () => void;
}) {
  return (
    <div
      role="alert"
      className="flex items-start gap-3 rounded-md border border-danger/30 bg-danger-soft px-4 py-3"
    >
      <p className="flex-1 text-sm text-danger">{message}</p>
      {onRetry ? (
        <Button tone="neutral" onClick={onRetry}>
          Retry
        </Button>
      ) : null}
      {onDismiss ? (
        <Button tone="neutral" onClick={onDismiss} aria-label="Dismiss">
          Dismiss
        </Button>
      ) : null}
    </div>
  );
}

export function Spinner({ label }: { label: string }) {
  return (
    <p role="status" className="py-8 text-center text-sm text-muted">
      {label}
    </p>
  );
}

export function EmptyState({ title, body }: { title: string; body: string }) {
  return (
    <div className="py-12 text-center">
      <p className="text-sm font-medium text-ink">{title}</p>
      <p className="mt-1 text-sm text-muted">{body}</p>
    </div>
  );
}
