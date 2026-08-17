import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { AuthProvider, useAuth } from "../src/context/AuthContext";

function Probe() {
  const { session } = useAuth();
  return <span>{session ? session.role : "anonymous"}</span>;
}

describe("AuthContext", () => {
  it("provides a default null session", () => {
    render(
      <AuthProvider>
        <Probe />
      </AuthProvider>,
    );
    expect(screen.getByText("anonymous")).toBeInTheDocument();
  });
});
