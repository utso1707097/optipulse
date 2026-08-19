import { act, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { ConnectivityGuard } from "../src/components/ConnectivityGuard";

function setOnline(value: boolean) {
  vi.spyOn(navigator, "onLine", "get").mockReturnValue(value);
  act(() => {
    window.dispatchEvent(new Event(value ? "online" : "offline"));
  });
}

describe("ConnectivityGuard (T062)", () => {
  afterEach(() => vi.restoreAllMocks());

  it("renders its children while online", () => {
    setOnline(true);
    render(
      <ConnectivityGuard>
        <button>Engage kill-switch</button>
      </ConnectivityGuard>,
    );
    expect(
      screen.getByRole("button", { name: "Engage kill-switch" }),
    ).toBeInTheDocument();
  });

  it("REMOVES the work area when offline rather than leaving it interactive", () => {
    vi.spyOn(navigator, "onLine", "get").mockReturnValue(false);
    render(
      <ConnectivityGuard>
        <button>Engage kill-switch</button>
      </ConnectivityGuard>,
    );

    // The point of the guard: not a banner over stale controls, but no stale controls at all.
    expect(
      screen.queryByRole("button", { name: "Engage kill-switch" }),
    ).not.toBeInTheDocument();
    expect(screen.getByRole("alert")).toHaveTextContent(
      "requires a connection",
    );
  });

  it("restores the work area when the connection returns", () => {
    vi.spyOn(navigator, "onLine", "get").mockReturnValue(false);
    render(
      <ConnectivityGuard>
        <button>Engage kill-switch</button>
      </ConnectivityGuard>,
    );
    expect(
      screen.queryByRole("button", { name: "Engage kill-switch" }),
    ).not.toBeInTheDocument();

    setOnline(true);

    expect(
      screen.getByRole("button", { name: "Engage kill-switch" }),
    ).toBeInTheDocument();
  });
});
