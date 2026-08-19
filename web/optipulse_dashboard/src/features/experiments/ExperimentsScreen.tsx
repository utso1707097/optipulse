import { useEffect, useState, type FormEvent } from "react";
import { useAppDispatch, useAppSelector } from "../../hooks";
import { fetchFlags } from "../../store/flagsSlice";
import {
  changeExperimentStatus,
  createExperiment,
  fetchExperiments,
  mutationErrorDismissed,
  updateExperiment,
} from "../../store/experimentsSlice";
import type { ExperimentResponse, VariantDto } from "../../api/client";
import { num } from "../../api/client";
import {
  Badge,
  Button,
  Card,
  EmptyState,
  ErrorBanner,
  Field,
  Input,
  Select,
  Spinner,
} from "../../components/ui";

function statusTone(status: string): "ok" | "warn" | "muted" {
  if (status === "Running") return "ok";
  if (status === "Draft") return "warn";
  return "muted";
}

function totalWeight(variants: VariantDto[]): number {
  return variants.reduce((sum, v) => sum + num(v.weight), 0);
}

function ExperimentForm({
  experiment,
  flagKeys,
  busy,
  onCancel,
  onSubmit,
}: {
  experiment: ExperimentResponse | null;
  flagKeys: string[];
  busy: boolean;
  onCancel: () => void;
  onSubmit: (values: {
    flagKey: string;
    name: string;
    conversionGoal: string;
    variants: VariantDto[];
  }) => void;
}) {
  const editing = experiment !== null;
  const [flagKey, setFlagKey] = useState(
    experiment?.flagKey ?? flagKeys[0] ?? "",
  );
  const [name, setName] = useState(experiment?.name ?? "");
  const [conversionGoal, setConversionGoal] = useState(
    experiment?.conversionGoal ?? "",
  );
  const [variants, setVariants] = useState<VariantDto[]>(
    () =>
      experiment?.variants.map((v) => ({
        key: v.key,
        weight: num(v.weight),
      })) ?? [
        { key: "control", weight: 50 },
        { key: "treatment", weight: 50 },
      ],
  );

  const weight = totalWeight(variants);
  // Checked here as well as server-side, because catching it before the round trip lets the
  // user fix it while they still have the numbers in front of them.
  const weightsValid = weight === 100;

  function submit(event: FormEvent) {
    event.preventDefault();
    onSubmit({
      flagKey,
      name: name.trim(),
      conversionGoal: conversionGoal.trim(),
      variants,
    });
  }

  return (
    <Card className="p-5">
      <h2 className="mb-4 text-base font-semibold text-ink">
        {editing ? `Edit variants — ${experiment.name}` : "New experiment"}
      </h2>
      <form className="space-y-4" onSubmit={submit}>
        {!editing ? (
          <div className="grid gap-4 sm:grid-cols-2">
            <Field
              label="Flag"
              hint="The experiment splits traffic for this flag."
            >
              <Select
                required
                value={flagKey}
                onChange={(e) => setFlagKey(e.target.value)}
              >
                {flagKeys.map((key) => (
                  <option key={key} value={key}>
                    {key}
                  </option>
                ))}
              </Select>
            </Field>
            <Field label="Name">
              <Input
                required
                value={name}
                onChange={(e) => setName(e.target.value)}
              />
            </Field>
            <Field
              label="Conversion goal"
              hint="The event that counts as a success, e.g. checkout_completed. Conversions are reported against this name."
            >
              <Input
                value={conversionGoal}
                onChange={(e) => setConversionGoal(e.target.value)}
                placeholder="checkout_completed"
              />
            </Field>
          </div>
        ) : (
          <p className="text-sm text-muted">
            Only the variant split can be changed after an experiment exists.
            The flag and goal are fixed, because changing them mid-flight would
            make the exposures already recorded uninterpretable.
          </p>
        )}

        <fieldset className="rounded-md border border-line p-4">
          <legend className="px-1 text-sm font-medium text-ink">
            Variants
          </legend>
          {variants.map((variant, index) => (
            <div
              key={index}
              className="mb-2 grid gap-2 sm:grid-cols-[2fr_1fr_auto]"
            >
              <Input
                aria-label={`Variant key ${index + 1}`}
                required
                value={variant.key}
                onChange={(e) =>
                  setVariants((vs) =>
                    vs.map((v, i) =>
                      i === index ? { ...v, key: e.target.value } : v,
                    ),
                  )
                }
              />
              <Input
                aria-label={`Variant weight ${index + 1}`}
                type="number"
                min={0}
                max={100}
                value={num(variant.weight)}
                onChange={(e) =>
                  setVariants((vs) =>
                    vs.map((v, i) =>
                      i === index
                        ? { ...v, weight: Number(e.target.value) }
                        : v,
                    ),
                  )
                }
              />
              <Button
                type="button"
                tone="neutral"
                disabled={variants.length <= 2}
                onClick={() =>
                  setVariants((vs) => vs.filter((_, i) => i !== index))
                }
              >
                Remove
              </Button>
            </div>
          ))}
          <div className="mt-2 flex items-center gap-3">
            <Button
              type="button"
              tone="neutral"
              onClick={() =>
                setVariants((vs) => [...vs, { key: "", weight: 0 }])
              }
            >
              Add variant
            </Button>
            <span
              className={`text-sm ${weightsValid ? "text-muted" : "text-danger"}`}
            >
              Total weight {weight}%{" "}
              {weightsValid ? "" : "— must be exactly 100%"}
            </span>
          </div>
        </fieldset>

        <div className="flex gap-2">
          <Button type="submit" tone="brand" disabled={busy || !weightsValid}>
            {busy ? "Saving…" : editing ? "Save variants" : "Create experiment"}
          </Button>
          <Button
            type="button"
            tone="neutral"
            onClick={onCancel}
            disabled={busy}
          >
            Cancel
          </Button>
        </div>
      </form>
    </Card>
  );
}

export function ExperimentsScreen() {
  const dispatch = useAppDispatch();
  const { items, loading, error, mutating, mutationError, conflict } =
    useAppSelector((s) => s.experiments);
  const flags = useAppSelector((s) => s.flags.items);
  const role = useAppSelector((s) => s.auth.role);
  const canAuthor = role === "Manager";

  const [editing, setEditing] = useState<ExperimentResponse | null>(null);
  const [creating, setCreating] = useState(false);

  useEffect(() => {
    void dispatch(fetchExperiments(undefined));
    // The flag list feeds the create form's flag picker. Fetched here rather than assumed to be
    // in the store already, because this screen is reachable directly by URL.
    void dispatch(fetchFlags());
  }, [dispatch]);

  async function submit(values: {
    flagKey: string;
    name: string;
    conversionGoal: string;
    variants: VariantDto[];
  }) {
    // Dispatched per branch: see the matching note in FlagsScreen.
    const result = editing
      ? await dispatch(
          updateExperiment({
            id: editing.id,
            version: editing.version,
            body: { variants: values.variants },
          }),
        )
      : await dispatch(
          createExperiment({
            flagKey: values.flagKey,
            name: values.name,
            conversionGoal: values.conversionGoal || null,
            variants: values.variants,
          }),
        );

    if (!("error" in result)) {
      setEditing(null);
      setCreating(false);
    }
  }

  if (loading && items.length === 0)
    return <Spinner label="Loading experiments…" />;
  if (error) {
    return (
      <ErrorBanner
        message={error}
        onRetry={() => void dispatch(fetchExperiments(undefined))}
      />
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-lg font-semibold text-ink">Experiments</h1>
          <p className="text-sm text-muted">
            {items.length} experiment{items.length === 1 ? "" : "s"}
          </p>
        </div>
        {canAuthor && flags.length > 0 ? (
          <Button
            tone="brand"
            onClick={() => {
              setEditing(null);
              setCreating(true);
            }}
          >
            New experiment
          </Button>
        ) : null}
      </div>

      {mutationError ? (
        <ErrorBanner
          message={mutationError}
          onRetry={
            conflict
              ? () => void dispatch(fetchExperiments(undefined))
              : undefined
          }
          onDismiss={() => dispatch(mutationErrorDismissed())}
        />
      ) : null}

      {creating || editing ? (
        <ExperimentForm
          experiment={editing}
          flagKeys={flags.map((f) => f.key)}
          busy={mutating}
          onCancel={() => {
            setEditing(null);
            setCreating(false);
          }}
          onSubmit={submit}
        />
      ) : null}

      {items.length === 0 ? (
        <EmptyState
          title="No experiments yet"
          body="An experiment splits a flag's traffic across variants so you can compare their conversion rates."
        />
      ) : (
        <Card className="divide-y divide-line">
          {items.map((experiment) => (
            <div
              key={experiment.id}
              className="flex flex-wrap items-center gap-3 p-4"
            >
              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium text-ink">
                    {experiment.name}
                  </span>
                  <Badge tone={statusTone(experiment.status)}>
                    {experiment.status}
                  </Badge>
                </div>
                <p className="font-mono text-xs text-muted">
                  {experiment.flagKey}
                </p>
                <p className="text-xs text-muted">
                  {experiment.variants
                    .map((v) => `${v.key} ${num(v.weight)}%`)
                    .join(" · ")}
                  {experiment.conversionGoal
                    ? ` · goal: ${experiment.conversionGoal}`
                    : " · no goal set"}
                  {` · v${num(experiment.version)}`}
                </p>
              </div>

              {canAuthor ? (
                <div className="flex flex-wrap items-center gap-2">
                  <Select
                    aria-label={`Status for ${experiment.name}`}
                    value={experiment.status}
                    disabled={mutating}
                    onChange={(e) =>
                      void dispatch(
                        changeExperimentStatus({
                          id: experiment.id,
                          status: e.target.value,
                        }),
                      )
                    }
                  >
                    <option value="Draft">Draft</option>
                    <option value="Running">Running</option>
                    <option value="Concluded">Concluded</option>
                  </Select>
                  <Button
                    tone="neutral"
                    disabled={mutating}
                    onClick={() => {
                      setCreating(false);
                      setEditing(experiment);
                    }}
                  >
                    Edit variants
                  </Button>
                </div>
              ) : null}
            </div>
          ))}
        </Card>
      )}
    </div>
  );
}
