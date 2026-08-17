import { createContext, useContext, useState, type ReactNode } from "react";

// Constitution Principle V/VI: the ONLY cross-cutting global state in this app.
// Holds the opaque session; never parses JWT claims for authorization decisions.

interface Session {
  accessToken: string;
  role: "Manager" | "Admin";
}

interface AuthContextValue {
  session: Session | null;
  setSession: (session: Session | null) => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(null);
  return (
    <AuthContext.Provider value={{ session, setSession }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
  return ctx;
}
