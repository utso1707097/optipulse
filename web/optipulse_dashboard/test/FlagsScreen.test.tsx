import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Provider } from "react-redux";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { createAppStore, type RootState } from "../src/store";
import { configureAuthBridge } from "../src/api/client";
import { FlagsScreen } from "../src/features/flags/FlagsScreen";

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

function renderWithRole(role: string) {
  const store = createAppStore({
    auth: {
      status: "authenticated",
      accessToken: "access",
      refreshToken: "refresh",
      expiresAt: Date.now() + 900_000,
      role,
      profile: null,
      loginError: null,
      loginPending: false,
    },
  } as Partial<RootState>);
  render(
    <Provider store={store}>
      <FlagsScreen />
    </Provider>,
  );
  return store;
}

describe("FlagsScreen", () => {
  beforeEach(() => {
    vi.unstubAllGlobals();
    configureAuthBridge({
      getAccessToken: () => "access",
      refresh: async () => null,
      onSessionLost: () => {},
    });
  });

  it("lists flags returned by the server", async () => {
    vi.stubGlobal("fetch", () => Promise.resolve(json([FLAG])));
    renderWithRole("Manager");

    expect(await screen.findByText("checkout.new")).toBeInTheDocument();
    expect(screen.getByText("New checkout")).toBeInTheDocument();
  });

  it("creates a flag and then shows the SERVER's list, not the submitted values", async () => {
    const bodies: string[] = [];
    vi.stubGlobal("fetch", (_: RequestInfo | URL, init?: RequestInit) => {
      if (init?.method === "POST") {
        bodies.push(String(init.body));
        return Promise.resolve(json({ ...FLAG, key: "search.rerank" }, 201));
      }
      // The server normalises the name; the screen must render THIS, not what was typed.
      return Promise.resolve(
        json(
          bodies.length
            ? [
                {
                  ...FLAG,
                  key: "search.rerank",
                  name: "Search rerank (normalised)",
                },
              ]
            : [],
        ),
      );
    });

    renderWithRole("Manager");
    const user = userEvent.setup();

    await user.click(await screen.findByRole("button", { name: "New flag" }));
    await user.type(screen.getByLabelText("Key"), "search.rerank");
    await user.type(screen.getByLabelText("Name"), "Search rerank");
    await user.click(screen.getByRole("button", { name: "Create flag" }));

    expect(
      await screen.findByText("Search rerank (normalised)"),
    ).toBeInTheDocument();
    expect(JSON.parse(bodies[0])).toMatchObject({
      key: "search.rerank",
      name: "Search rerank",
    });
    // The form closes only after the write succeeded.
    expect(
      screen.queryByRole("button", { name: "Create flag" }),
    ).not.toBeInTheDocument();
  });

  it("keeps the form open with the user's input when the save is rejected", async () => {
    vi.stubGlobal("fetch", (_: RequestInfo | URL, init?: RequestInit) =>
      Promise.resolve(
        init?.method === "POST"
          ? json(
              {
                title: "Duplicate key",
                detail: "A flag with that key already exists.",
              },
              409,
            )
          : json([]),
      ),
    );

    renderWithRole("Manager");
    const user = userEvent.setup();

    await user.click(await screen.findByRole("button", { name: "New flag" }));
    await user.type(screen.getByLabelText("Key"), "checkout.new");
    await user.type(screen.getByLabelText("Name"), "Duplicate");
    await user.click(screen.getByRole("button", { name: "Create flag" }));

    await waitFor(() => expect(screen.getByRole("alert")).toBeInTheDocument());
    // Closing the form here would silently discard everything the user typed.
    expect(screen.getByLabelText("Key")).toHaveValue("checkout.new");
  });

  it("offers the kill-switch to an Admin and hides it from a Manager", async () => {
    vi.stubGlobal("fetch", () => Promise.resolve(json([FLAG])));

    renderWithRole("Admin");
    expect(
      await screen.findByRole("button", { name: "Kill-switch" }),
    ).toBeInTheDocument();
  });

  it("does not render the kill-switch for a Manager", async () => {
    vi.stubGlobal("fetch", () => Promise.resolve(json([FLAG])));
    renderWithRole("Manager");

    await screen.findByText("checkout.new");
    // An affordance, not the enforcement — the API rejects the call either way.
    expect(
      screen.queryByRole("button", { name: "Kill-switch" }),
    ).not.toBeInTheDocument();
  });
});
