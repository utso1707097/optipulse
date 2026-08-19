import { beforeEach, describe, expect, it, vi } from "vitest";
import { createAppStore } from "../src/store";
import { createFlag, fetchFlags, setKillSwitch } from "../src/store/flagsSlice";
import { configureAuthBridge } from "../src/api/client";

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
  version: 1,
  targetingRules: [],
  rollout: null,
  createdAt: "2026-01-01T00:00:00Z",
  updatedAt: "2026-01-01T00:00:00Z",
};

describe("flagsSlice", () => {
  beforeEach(() => {
    vi.unstubAllGlobals();
    configureAuthBridge({
      getAccessToken: () => "access",
      refresh: async () => null,
      onSessionLost: () => {},
    });
  });

  it("discards the previously loaded list when a read fails", async () => {
    vi.stubGlobal("fetch", () => Promise.resolve(json([FLAG])));
    const store = createAppStore();
    await store.dispatch(fetchFlags());
    expect(store.getState().flags.items).toHaveLength(1);

    vi.stubGlobal("fetch", () => Promise.reject(new TypeError("offline")));
    await store.dispatch(fetchFlags());

    // Showing the old list next to an error banner would let someone operate a kill-switch
    // against a picture of the system rather than the system.
    expect(store.getState().flags.items).toEqual([]);
    expect(store.getState().flags.error).toContain("Could not reach");
  });

  it("re-reads the list from the server after a create instead of patching locally", async () => {
    const calls: Array<{ url: string; method: string }> = [];
    vi.stubGlobal("fetch", (input: RequestInfo | URL, init?: RequestInit) => {
      const url = String(input);
      const method = init?.method ?? "GET";
      calls.push({ url, method });
      return Promise.resolve(
        method === "POST" ? json(FLAG, 201) : json([FLAG]),
      );
    });

    const store = createAppStore();
    await store.dispatch(
      createFlag({
        key: "checkout.new",
        name: "New checkout",
        defaultOutcome: false,
        targetingRules: null,
        rollout: null,
      }),
    );

    expect(calls.map((c) => c.method)).toEqual(["POST", "GET"]);
    expect(store.getState().flags.items).toHaveLength(1);
    expect(store.getState().flags.mutationError).toBeNull();
  });

  it("keeps a failed write out of the read error, so the visible list is not blamed", async () => {
    vi.stubGlobal("fetch", () => Promise.resolve(json([FLAG])));
    const store = createAppStore();
    await store.dispatch(fetchFlags());

    vi.stubGlobal("fetch", () =>
      Promise.resolve(json({ title: "Forbidden" }, 403)),
    );
    await store.dispatch(setKillSwitch({ key: "checkout.new", engaged: true }));

    const state = store.getState().flags;
    expect(state.mutationError).toContain("role does not permit");
    expect(state.error).toBeNull();
    expect(state.items).toHaveLength(1);
  });

  it("flags a version conflict so the UI can tell the user to reload rather than retry blindly", async () => {
    vi.stubGlobal("fetch", () =>
      Promise.resolve(json({ title: "Conflict" }, 409)),
    );
    const store = createAppStore();

    await store.dispatch(setKillSwitch({ key: "checkout.new", engaged: true }));

    expect(store.getState().flags.conflict).toBe(true);
    expect(store.getState().flags.mutationError).toContain(
      "Someone else changed",
    );
  });
});
