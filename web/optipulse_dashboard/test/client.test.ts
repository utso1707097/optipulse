import { beforeEach, describe, expect, it, vi } from "vitest";
import {
  ApiError,
  configureAuthBridge,
  flagsApi,
  num,
} from "../src/api/client";

function json(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

const FLAG = {
  id: "00000000-0000-0000-0000-000000000001",
  key: "checkout.new",
  name: "New checkout",
  defaultOutcome: false,
  status: "Active",
  killSwitchEngaged: false,
  version: 3,
  targetingRules: [],
  rollout: null,
  createdAt: "2026-01-01T00:00:00Z",
  updatedAt: "2026-01-01T00:00:00Z",
};

describe("api client", () => {
  beforeEach(() => {
    vi.unstubAllGlobals();
    configureAuthBridge({
      getAccessToken: () => "access-1",
      refresh: async () => null,
      onSessionLost: () => {},
    });
  });

  it("coerces int64 fields that the server may serialise as strings", () => {
    expect(num("9007199254740993")).toBe(Number("9007199254740993"));
    expect(num(42)).toBe(42);
    expect(num(null)).toBe(0);
  });

  it("sends the bearer token and the If-Match version on an update", async () => {
    const spy = vi.fn(() => json(FLAG));
    vi.stubGlobal("fetch", (_: unknown, init?: RequestInit) =>
      Promise.resolve(spy(init as never)),
    );

    await flagsApi.update("checkout.new", 3, {
      name: "New checkout",
      defaultOutcome: true,
      targetingRules: null,
      rollout: null,
    });

    const headers = (spy.mock.calls[0][0] as unknown as RequestInit)
      .headers as Record<string, string>;
    expect(headers.Authorization).toBe("Bearer access-1");
    // Without If-Match the server answers 428 — the header is the lost-update defence.
    expect(headers["If-Match"]).toBe("3");
  });

  it("classifies a transport failure as status 0, distinct from any server response", async () => {
    vi.stubGlobal("fetch", () =>
      Promise.reject(new TypeError("Failed to fetch")),
    );

    const error = await flagsApi.list().catch((e) => e as ApiError);

    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).isNetworkFailure).toBe(true);
  });

  it("treats 409 and 428 as version conflicts", async () => {
    for (const status of [409, 428]) {
      vi.stubGlobal("fetch", () =>
        Promise.resolve(json({ title: "Conflict" }, status)),
      );
      const error = (await flagsApi
        .update("k", 1, {
          name: "n",
          defaultOutcome: false,
          targetingRules: null,
          rollout: null,
        })
        .catch((e) => e)) as ApiError;
      expect(error.isVersionConflict).toBe(true);
    }
  });

  it("refreshes ONCE on a 401 and retries the original request", async () => {
    let refreshCount = 0;
    configureAuthBridge({
      getAccessToken: () => "expired",
      refresh: async () => {
        refreshCount += 1;
        return "fresh";
      },
      onSessionLost: () => {},
    });

    const seen: string[] = [];
    vi.stubGlobal("fetch", (_: unknown, init?: RequestInit) => {
      const auth = ((init?.headers ?? {}) as Record<string, string>)
        .Authorization;
      seen.push(auth);
      return Promise.resolve(
        auth === "Bearer fresh" ? json([FLAG]) : json({}, 401),
      );
    });

    const flags = await flagsApi.list();

    expect(refreshCount).toBe(1);
    expect(seen).toEqual(["Bearer expired", "Bearer fresh"]);
    expect(flags).toHaveLength(1);
  });

  it("gives up and reports the session lost when the refreshed token is also rejected", async () => {
    const onSessionLost = vi.fn();
    configureAuthBridge({
      getAccessToken: () => "expired",
      refresh: async () => "also-bad",
      onSessionLost,
    });
    const fetchSpy = vi.fn(() => json({}, 401));
    vi.stubGlobal("fetch", () => Promise.resolve(fetchSpy()));

    await expect(flagsApi.list()).rejects.toBeInstanceOf(ApiError);

    // Exactly two attempts: the original and one retry. Looping here would turn an expired
    // session into an infinite request storm.
    expect(fetchSpy).toHaveBeenCalledTimes(2);
    expect(onSessionLost).toHaveBeenCalledOnce();
  });
});
