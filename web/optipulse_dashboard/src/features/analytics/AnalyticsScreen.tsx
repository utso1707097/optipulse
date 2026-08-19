import { useEffect } from "react";
import { useAppDispatch, useAppSelector } from "../../hooks";
import { fetchFlags } from "../../store/flagsSlice";
import { fetchExposures, flagSelected } from "../../store/analyticsSlice";
import { num } from "../../api/client";
import {
  Card,
  EmptyState,
  ErrorBanner,
  Field,
  Select,
  Spinner,
} from "../../components/ui";

/**
 * A conversion rate computed from a handful of exposures is noise, and presenting it in the same
 * type as a rate computed from thousands invites someone to act on it. This is not a
 * significance test — it is a floor below which the screen says so out loud.
 */
const LOW_SAMPLE_THRESHOLD = 100;

export function AnalyticsScreen() {
  const dispatch = useAppDispatch();
  const flags = useAppSelector((s) => s.flags.items);
  const { selectedFlagKey, byFlagKey, loading, error } = useAppSelector(
    (s) => s.analytics,
  );

  useEffect(() => {
    void dispatch(fetchFlags());
  }, [dispatch]);

  useEffect(() => {
    // Default to the first flag once the list arrives, so the screen is useful without a click.
    if (!selectedFlagKey && flags.length > 0) {
      dispatch(flagSelected(flags[0].key));
    }
  }, [dispatch, flags, selectedFlagKey]);

  useEffect(() => {
    if (selectedFlagKey) void dispatch(fetchExposures(selectedFlagKey));
  }, [dispatch, selectedFlagKey]);

  const report = selectedFlagKey ? byFlagKey[selectedFlagKey] : undefined;
  const best = report?.byVariant.reduce<
    (typeof report.byVariant)[number] | null
  >(
    (leader, v) =>
      leader === null || v.conversionRatePercent > leader.conversionRatePercent
        ? v
        : leader,
    null,
  );

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-lg font-semibold text-ink">Analytics</h1>
        <p className="text-sm text-muted">
          Exposures and conversions, by variant.
        </p>
      </div>

      {flags.length === 0 ? (
        <EmptyState
          title="No flags to report on"
          body="Create a flag first, then return here."
        />
      ) : (
        <div className="max-w-xs">
          <Field label="Flag">
            <Select
              value={selectedFlagKey ?? ""}
              onChange={(e) => dispatch(flagSelected(e.target.value))}
            >
              {flags.map((flag) => (
                <option key={flag.key} value={flag.key}>
                  {flag.key}
                </option>
              ))}
            </Select>
          </Field>
        </div>
      )}

      {error ? (
        <ErrorBanner
          message={error}
          onRetry={
            selectedFlagKey
              ? () => void dispatch(fetchExposures(selectedFlagKey))
              : undefined
          }
        />
      ) : null}

      {loading && !report ? <Spinner label="Loading analytics…" /> : null}

      {report ? (
        <>
          <div className="grid gap-4 sm:grid-cols-2">
            <Card className="p-4">
              <p className="text-sm text-muted">Total exposures</p>
              <p className="text-2xl font-semibold text-ink">
                {num(report.totalExposures).toLocaleString()}
              </p>
            </Card>
            <Card className="p-4">
              <p className="text-sm text-muted">Total conversions</p>
              <p className="text-2xl font-semibold text-ink">
                {num(report.totalConversions).toLocaleString()}
              </p>
            </Card>
          </div>

          {report.byVariant.length === 0 ? (
            <EmptyState
              title="No exposures recorded yet"
              body="Exposures appear once your application starts evaluating this flag."
            />
          ) : (
            <Card className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="border-b border-line text-left text-muted">
                  <tr>
                    <th className="px-4 py-2 font-medium">Variant</th>
                    <th className="px-4 py-2 font-medium">Exposures</th>
                    <th className="px-4 py-2 font-medium">Traffic share</th>
                    <th className="px-4 py-2 font-medium">Conversions</th>
                    <th className="px-4 py-2 font-medium">Conversion rate</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-line">
                  {report.byVariant.map((variant) => {
                    const exposures = num(variant.exposures);
                    const lowSample = exposures < LOW_SAMPLE_THRESHOLD;
                    return (
                      <tr key={variant.variantKey ?? "(none)"}>
                        <td className="px-4 py-2 font-mono text-ink">
                          {variant.variantKey ?? "(no variant)"}
                          {best &&
                          best.variantKey === variant.variantKey &&
                          !lowSample ? (
                            <span className="ml-2 text-xs text-ok">
                              highest rate
                            </span>
                          ) : null}
                        </td>
                        <td className="px-4 py-2 text-ink">
                          {exposures.toLocaleString()}
                        </td>
                        <td className="px-4 py-2 text-muted">
                          {num(variant.sharePercent)}%
                        </td>
                        <td className="px-4 py-2 text-ink">
                          {num(variant.conversions).toLocaleString()}
                        </td>
                        <td className="px-4 py-2 text-ink">
                          {num(variant.conversionRatePercent)}%
                          {lowSample ? (
                            <span className="ml-2 text-xs text-warn">
                              too few exposures to read
                            </span>
                          ) : null}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </Card>
          )}

          <p className="text-xs text-muted">
            These are raw counts, not a statistical verdict. A higher rate is a
            reason to look closer, not on its own a reason to declare a winner.
          </p>
        </>
      ) : null}
    </div>
  );
}
