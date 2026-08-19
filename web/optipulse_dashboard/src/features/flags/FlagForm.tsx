import { useState, type FormEvent } from "react";
import type {
  CreateFlagRequest,
  FlagResponse,
  TargetingRuleDto,
} from "../../api/client";
import { num } from "../../api/client";
import { Button, Card, Field, Input, Select } from "../../components/ui";

const OPERATORS = [
  "Equals",
  "NotEquals",
  "In",
  "Contains",
  "GreaterThan",
  "LessThan",
];

export interface FlagFormValues {
  key: string;
  name: string;
  defaultOutcome: boolean;
  targetingRules: TargetingRuleDto[];
  rollout: { percentage: number; salt: string } | null;
}

function toValues(flag: FlagResponse | null): FlagFormValues {
  if (!flag) {
    return {
      key: "",
      name: "",
      defaultOutcome: false,
      targetingRules: [],
      rollout: null,
    };
  }
  return {
    key: flag.key,
    name: flag.name,
    defaultOutcome: flag.defaultOutcome,
    targetingRules: flag.targetingRules.map((r) => ({
      ...r,
      values: [...r.values],
    })),
    rollout: flag.rollout
      ? { percentage: num(flag.rollout.percentage), salt: flag.rollout.salt }
      : null,
  };
}

export function FlagForm({
  flag,
  busy,
  onCancel,
  onSubmit,
}: {
  /** null = create. Non-null = edit; the key becomes immutable because it is the flag's identity
   *  and the SDKs in production evaluate against it. */
  flag: FlagResponse | null;
  busy: boolean;
  onCancel: () => void;
  onSubmit: (values: CreateFlagRequest) => void;
}) {
  const [values, setValues] = useState<FlagFormValues>(() => toValues(flag));
  const editing = flag !== null;

  function submit(event: FormEvent) {
    event.preventDefault();
    onSubmit({
      key: values.key.trim(),
      name: values.name.trim(),
      defaultOutcome: values.defaultOutcome,
      targetingRules: values.targetingRules.length
        ? values.targetingRules
        : null,
      rollout: values.rollout
        ? {
            percentage: values.rollout.percentage,
            salt: values.rollout.salt.trim(),
          }
        : null,
    });
  }

  function updateRule(index: number, patch: Partial<TargetingRuleDto>) {
    setValues((v) => ({
      ...v,
      targetingRules: v.targetingRules.map((r, i) =>
        i === index ? { ...r, ...patch } : r,
      ),
    }));
  }

  return (
    <Card className="p-5">
      <h2 className="mb-4 text-base font-semibold text-ink">
        {editing ? `Edit ${flag.key}` : "New flag"}
      </h2>
      <form className="space-y-4" onSubmit={submit}>
        <div className="grid gap-4 sm:grid-cols-2">
          <Field
            label="Key"
            hint={
              editing
                ? "The key identifies the flag to your SDKs and cannot be changed."
                : "Lowercase, dot- or dash-separated."
            }
          >
            <Input
              required
              value={values.key}
              disabled={editing}
              onChange={(e) =>
                setValues((v) => ({ ...v, key: e.target.value }))
              }
            />
          </Field>
          <Field label="Name">
            <Input
              required
              value={values.name}
              onChange={(e) =>
                setValues((v) => ({ ...v, name: e.target.value }))
              }
            />
          </Field>
        </div>

        <Field
          label="Default outcome"
          hint="Returned when no targeting rule matches and no rollout applies."
        >
          <Select
            value={values.defaultOutcome ? "on" : "off"}
            onChange={(e) =>
              setValues((v) => ({
                ...v,
                defaultOutcome: e.target.value === "on",
              }))
            }
          >
            <option value="off">Off</option>
            <option value="on">On</option>
          </Select>
        </Field>

        <fieldset className="rounded-md border border-line p-4">
          <legend className="px-1 text-sm font-medium text-ink">
            Targeting rules
          </legend>
          <p className="mb-3 text-xs text-muted">
            Evaluated in order; the first match decides the outcome and the
            rollout is skipped.
          </p>
          {values.targetingRules.map((rule, index) => (
            <div
              key={index}
              className="mb-3 grid gap-2 sm:grid-cols-[1fr_1fr_2fr_auto_auto]"
            >
              <Input
                aria-label={`Attribute ${index + 1}`}
                placeholder="attribute"
                value={rule.attribute}
                onChange={(e) =>
                  updateRule(index, { attribute: e.target.value })
                }
              />
              <Select
                aria-label={`Operator ${index + 1}`}
                value={rule.operator}
                onChange={(e) =>
                  updateRule(index, { operator: e.target.value })
                }
              >
                {OPERATORS.map((op) => (
                  <option key={op} value={op}>
                    {op}
                  </option>
                ))}
              </Select>
              <Input
                aria-label={`Values ${index + 1}`}
                placeholder="comma,separated,values"
                value={rule.values.join(",")}
                onChange={(e) =>
                  updateRule(index, {
                    values: e.target.value
                      .split(",")
                      .map((s) => s.trim())
                      .filter(Boolean),
                  })
                }
              />
              <Select
                aria-label={`Outcome ${index + 1}`}
                value={rule.outcome ? "on" : "off"}
                onChange={(e) =>
                  updateRule(index, { outcome: e.target.value === "on" })
                }
              >
                <option value="on">On</option>
                <option value="off">Off</option>
              </Select>
              <Button
                type="button"
                tone="neutral"
                onClick={() =>
                  setValues((v) => ({
                    ...v,
                    targetingRules: v.targetingRules.filter(
                      (_, i) => i !== index,
                    ),
                  }))
                }
              >
                Remove
              </Button>
            </div>
          ))}
          <Button
            type="button"
            tone="neutral"
            onClick={() =>
              setValues((v) => ({
                ...v,
                targetingRules: [
                  ...v.targetingRules,
                  {
                    attribute: "",
                    operator: "Equals",
                    values: [],
                    outcome: true,
                  },
                ],
              }))
            }
          >
            Add rule
          </Button>
        </fieldset>

        <fieldset className="rounded-md border border-line p-4">
          <legend className="px-1 text-sm font-medium text-ink">
            Percentage rollout
          </legend>
          <label className="mb-3 flex items-center gap-2 text-sm text-ink">
            <input
              type="checkbox"
              checked={values.rollout !== null}
              onChange={(e) =>
                setValues((v) => ({
                  ...v,
                  rollout: e.target.checked
                    ? { percentage: 50, salt: v.key || "rollout" }
                    : null,
                }))
              }
            />
            Roll out to a percentage of users
          </label>
          {values.rollout ? (
            <div className="grid gap-4 sm:grid-cols-2">
              <Field label="Percentage">
                <Input
                  type="number"
                  min={0}
                  max={100}
                  value={values.rollout.percentage}
                  onChange={(e) =>
                    setValues((v) => ({
                      ...v,
                      rollout: {
                        ...v.rollout!,
                        percentage: Number(e.target.value),
                      },
                    }))
                  }
                />
              </Field>
              <Field
                label="Salt"
                hint="Fixes which users fall inside the rollout. Changing it reshuffles everyone, so keep it stable once traffic is flowing."
              >
                <Input
                  required
                  value={values.rollout.salt}
                  onChange={(e) =>
                    setValues((v) => ({
                      ...v,
                      rollout: { ...v.rollout!, salt: e.target.value },
                    }))
                  }
                />
              </Field>
            </div>
          ) : null}
        </fieldset>

        <div className="flex gap-2">
          <Button type="submit" tone="brand" disabled={busy}>
            {busy ? "Saving…" : editing ? "Save changes" : "Create flag"}
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
