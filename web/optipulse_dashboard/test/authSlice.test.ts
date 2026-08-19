import { beforeEach, describe, expect, it, vi } from "vitest";
import { createAppStore } from "../src/store";
import {
  login,
  logout,
  restoreSession,
  __testing,
} from "../src/store/authSlice";

const { REFRESH_TOKEN_KEY } = __testing;

const LOGIN_BODY = {
  accessToken: "access-1",
  refreshToken: "refresh-1",
  tokenType: "Bearer",
  expiresInSeconds: 900,
  role: "Manager",
};

function mockFetch(
  handler: (url: string, init?: RequestInit) => Response | Promise<Response>,
) {
  const spy = vi.fn(handler);
  vi.stubGlobal("fetch", (input: RequestInfo | URL, init?: RequestInit) =>
    Promise.resolve(spy(String(input), init)),
  );
  return spy;
}

function json(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("authSlice", () => {
  beforeEach(() => {
    vi.unstubAllGlobals();
    window.localStorage.clear();
  });

  it("starts in 'restoring', not 'anonymous', so a signed-in user is not shown the login screen", () => {
    expect(createAppStore().getState().auth.status).toBe("restoring");
  });

  it("stores the refresh token but never persists the access token", async () => {
    mockFetch(() => json(LOGIN_BODY));
    const store = createAppStore();

    await store.dispatch(login({ email: "m@example.com", password: "pw" }));

    const state = store.getState().auth;
    expect(state.status).toBe("authenticated");
    expect(state.accessToken).toBe("access-1");
    expect(window.localStorage.getItem(REFRESH_TOKEN_KEY)).toBe("refresh-1");
    // The access token is short-lived and must not outlive the tab.
    expect(JSON.stringify(window.localStorage)).not.toContain("access-1");
  });

  it("reports an unreachable API differently from a rejected credential", async () => {
    vi.stubGlobal("fetch", () =>
      Promise.reject(new TypeError("Failed to fetch")),
    );
    const store = createAppStore();

    await store.dispatch(login({ email: "m@example.com", password: "pw" }));

    expect(store.getState().auth.loginError).toContain("Could not reach");
  });

  it("surfaces the server's message on bad credentials", async () => {
    mockFetch(() =>
      json(
        { title: "Invalid credentials", detail: "Invalid email or password." },
        401,
      ),
    );
    const store = createAppStore();

    await store.dispatch(login({ email: "m@example.com", password: "wrong" }));

    expect(store.getState().auth.loginError).toBe("Invalid email or password.");
    expect(store.getState().auth.status).toBe("anonymous");
  });

  it("restores a session from the stored refresh token", async () => {
    window.localStorage.setItem(REFRESH_TOKEN_KEY, "refresh-old");
    const fetchSpy = mockFetch(() =>
      json({
        ...LOGIN_BODY,
        accessToken: "access-2",
        refreshToken: "refresh-2",
      }),
    );
    const store = createAppStore();

    await store.dispatch(restoreSession());

    expect(fetchSpy.mock.calls[0][0]).toContain("/api/v1/auth/refresh");
    expect(store.getState().auth.status).toBe("authenticated");
    // Rotation: the server issued a new refresh token and the old one must not survive.
    expect(window.localStorage.getItem(REFRESH_TOKEN_KEY)).toBe("refresh-2");
  });

  it("falls back to anonymous when the stored refresh token is rejected", async () => {
    window.localStorage.setItem(REFRESH_TOKEN_KEY, "revoked");
    mockFetch(() => json({ title: "Refresh failed" }, 401));
    const store = createAppStore();

    await store.dispatch(restoreSession());

    expect(store.getState().auth.status).toBe("anonymous");
    expect(window.localStorage.getItem(REFRESH_TOKEN_KEY)).toBeNull();
  });

  it("clears the local session even when the logout call fails", async () => {
    mockFetch(() => json(LOGIN_BODY));
    const store = createAppStore();
    await store.dispatch(login({ email: "m@example.com", password: "pw" }));

    vi.stubGlobal("fetch", () => Promise.reject(new TypeError("offline")));
    await store.dispatch(logout());

    // Refusing to sign out because the network is down would strand the user signed in.
    expect(store.getState().auth.status).toBe("anonymous");
    expect(window.localStorage.getItem(REFRESH_TOKEN_KEY)).toBeNull();
  });
});
