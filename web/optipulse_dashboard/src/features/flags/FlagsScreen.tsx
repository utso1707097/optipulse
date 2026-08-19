import { useEffect, useState } from "react";
import { useAppDispatch, useAppSelector } from "../../hooks";
import {
  changeFlagStatus,
  createFlag,
  fetchFlags,
  mutationErrorDismissed,
  setKillSwitch,
  updateFlag,
} from "../../store/flagsSlice";
import type { CreateFlagRequest, FlagResponse } from "../../api/client";
import { num } from "../../api/client";
import {
  Badge,
  Button,
  Card,
  EmptyState,
  ErrorBanner,
  Select,
  Spinner,
} from "../../components/ui";
import { FlagForm } from "./FlagForm";

function statusTone(status: string): "ok" | "warn" | "muted" {
  if (status === "Active") return "ok";
  if (status === "Draft") return "warn";
  return "muted";
}

export function FlagsScreen() {
  const dispatch = useAppDispatch();
  const { items, loading, error, mutating, mutationError, conflict } =
    useAppSelector((s) => s.flags);
  const role = useAppSelector((s) => s.auth.role);
  const [editing, setEditing] = useState<FlagResponse | null>(null);
  const [creating, setCreating] = useState(false);

  useEffect(() => {
    void dispatch(fetchFlags());
  }, [dispatch]);

  // The kill-switch is Admin-only server-side. Hiding it from a Manager is an AFFORDANCE, not
  // the enforcement — the API rejects the call regardless of what this dashboard renders.
  const canKillSwitch = role === "Admin";
  const canAuthor = role === "Manager";

  async function submitFlag(values: CreateFlagRequest) {
    // Dispatched per branch rather than building an `action` variable first: the two thunks
    // have different argument types, and their union is not a dispatchable action type.
    const result = editing
      ? await dispatch(
          updateFlag({
            key: editing.key,
            version: editing.version,
            body: {
              name: values.name,
              defaultOutcome: values.defaultOutcome,
              targetingRules: values.targetingRules,
              rollout: values.rollout,
            },
          }),
        )
      : await dispatch(createFlag(values));

    // Close only on success. A form that closes on a rejected save discards the user's work and
    // leaves them looking at a list that does not contain their change.
    if (!("error" in result)) {
      setEditing(null);
      setCreating(false);
    }
  }

  if (loading && items.length === 0) return <Spinner label="Loading flags…" />;

  if (error) {
    return (
      <ErrorBanner
        message={error}
        onRetry={() => void dispatch(fetchFlags())}
      />
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-lg font-semibold text-ink">Flags</h1>
          <p className="text-sm text-muted">
            {items.length} flag{items.length === 1 ? "" : "s"}
          </p>
        </div>
        {canAuthor ? (
          <Button
            tone="brand"
            onClick={() => {
              setEditing(null);
              setCreating(true);
            }}
          >
            New flag
          </Button>
        ) : null}
      </div>

      {mutationError ? (
        <ErrorBanner
          message={mutationError}
          onRetry={conflict ? () => void dispatch(fetchFlags()) : undefined}
          onDismiss={() => dispatch(mutationErrorDismissed())}
        />
      ) : null}

      {creating || editing ? (
        <FlagForm
          flag={editing}
          busy={mutating}
          onCancel={() => {
            setEditing(null);
            setCreating(false);
          }}
          onSubmit={submitFlag}
        />
      ) : null}

      {items.length === 0 ? (
        <EmptyState
          title="No flags yet"
          body="Create a flag to start controlling a feature without redeploying."
        />
      ) : (
        <Card className="divide-y divide-line">
          {items.map((flag) => (
            <div
              key={flag.key}
              className="flex flex-wrap items-center gap-3 p-4"
            >
              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-2">
                  <span className="font-mono text-sm text-ink">{flag.key}</span>
                  <Badge tone={statusTone(flag.status)}>{flag.status}</Badge>
                  {flag.killSwitchEngaged ? (
                    <Badge tone="danger">Killed</Badge>
                  ) : null}
                </div>
                <p className="truncate text-sm text-muted">{flag.name}</p>
                <p className="text-xs text-muted">
                  Default {flag.defaultOutcome ? "on" : "off"}
                  {flag.rollout
                    ? ` · ${num(flag.rollout.percentage)}% rollout`
                    : ""}
                  {flag.targetingRules.length
                    ? ` · ${flag.targetingRules.length} targeting rule${flag.targetingRules.length === 1 ? "" : "s"}`
                    : ""}
                  {` · v${num(flag.version)}`}
                </p>
              </div>

              <div className="flex flex-wrap items-center gap-2">
                {canAuthor ? (
                  <>
                    <Select
                      aria-label={`Status for ${flag.key}`}
                      value={flag.status}
                      disabled={mutating}
                      onChange={(e) =>
                        void dispatch(
                          changeFlagStatus({
                            key: flag.key,
                            status: e.target.value,
                          }),
                        )
                      }
                    >
                      <option value="Draft">Draft</option>
                      <option value="Active">Active</option>
                      <option value="Archived">Archived</option>
                    </Select>
                    <Button
                      tone="neutral"
                      disabled={mutating}
                      onClick={() => {
                        setCreating(false);
                        setEditing(flag);
                      }}
                    >
                      Edit
                    </Button>
                  </>
                ) : null}
                {canKillSwitch ? (
                  <Button
                    tone={flag.killSwitchEngaged ? "neutral" : "danger"}
                    disabled={mutating}
                    onClick={() =>
                      void dispatch(
                        setKillSwitch({
                          key: flag.key,
                          engaged: !flag.killSwitchEngaged,
                        }),
                      )
                    }
                  >
                    {flag.killSwitchEngaged
                      ? "Release kill-switch"
                      : "Kill-switch"}
                  </Button>
                ) : null}
              </div>
            </div>
          ))}
        </Card>
      )}
    </div>
  );
}
