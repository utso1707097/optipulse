/**
 * Auth slice (T057) — replaces the previous AuthContext pin, which constitution v2.4.0 lifted.
 *
 * TWO RULES THIS FILE EXISTS TO KEEP (Principle VI):
 *
 * 1. The access token is OPAQUE. Nothing here decodes it. The role stored below is the one the
 *    SERVER reported (in LoginResponse / GET /auth/me) — reading it out of the JWT payload
 *    would mean the client deciding what the client is allowed to do.
 * 2. The role is used for AFFORDANCES ONLY — whether the kill-switch control is rendered, not
 *    whether it is permitted. Every protected call is still authorised server-side, so a user
 *    who forges a role in devtools gets a 403 from the API and nothing else.
 */
import {
  createAsyncThunk,
  createSlice,
  type PayloadAction,
} from "@reduxjs/toolkit";
import {
  ApiError,
  authApi,
  configureAuthBridge,
  num,
  type MeResponse,
} from "../api/client";

/**
 * WHERE THE TOKENS LIVE, and the trade-off, stated rather than assumed:
 *
 * - The ACCESS token is kept in memory only. It is short-lived and never written to storage.
 * - The REFRESH token is written to localStorage so a page reload does not sign the user out.
 *
 * localStorage is readable by any script running on this origin, so a successful XSS can steal
 * the refresh token. The alternative — an HttpOnly cookie — is not available to us: the API
 * issues refresh tokens in the response body and the dashboard is on a different origin, so a
 * cookie would need SameSite=None plus CSRF defences the backend does not implement today.
 * Rotation with family reuse detection is the mitigation that IS in place: a stolen token stops
 * working the moment the real client refreshes with it, and the whole family is revoked.
 */
const REFRESH_TOKEN_KEY = "optipulse.refreshToken";

function readStoredRefreshToken(): string | null {
  try {
    return window.localStorage.getItem(REFRESH_TOKEN_KEY);
  } catch {
    return null; // Private-mode / blocked storage: degrade to a session that ends on reload.
  }
}

function writeStoredRefreshToken(token: string | null): void {
  try {
    if (token) window.localStorage.setItem(REFRESH_TOKEN_KEY, token);
    else window.localStorage.removeItem(REFRESH_TOKEN_KEY);
  } catch {
    /* ignore — see above */
  }
}

export type AuthStatus = "restoring" | "anonymous" | "authenticated";

export interface AuthState {
  status: AuthStatus;
  accessToken: string | null;
  refreshToken: string | null;
  /** Epoch ms at which the access token stops being accepted. Drives the pre-emptive refresh. */
  expiresAt: number | null;
  role: string | null;
  profile: MeResponse | null;
  loginError: string | null;
  loginPending: boolean;
}

const initialState: AuthState = {
  // "restoring", not "anonymous": on first paint we do not yet know whether the stored refresh
  // token is still good. Starting at "anonymous" would flash the login screen at a signed-in user.
  status: "restoring",
  accessToken: null,
  refreshToken: null,
  expiresAt: null,
  role: null,
  profile: null,
  loginError: null,
  loginPending: false,
};

interface SessionPayload {
  accessToken: string;
  refreshToken: string;
  expiresAt: number;
  role: string;
}

function toSession(response: {
  accessToken: string;
  refreshToken: string;
  expiresInSeconds: number | string;
  role: string;
}): SessionPayload {
  return {
    accessToken: response.accessToken,
    refreshToken: response.refreshToken,
    expiresAt: Date.now() + num(response.expiresInSeconds) * 1000,
    role: response.role,
  };
}

export const login = createAsyncThunk<
  SessionPayload,
  { email: string; password: string },
  { rejectValue: string }
>("auth/login", async (credentials, { rejectWithValue }) => {
  try {
    return toSession(await authApi.login(credentials));
  } catch (error) {
    if (error instanceof ApiError) {
      // A wrong password and an unreachable API are different problems for the person at the
      // keyboard; collapsing both into "login failed" sends them looking in the wrong place.
      return rejectWithValue(
        error.isNetworkFailure
          ? "Could not reach the OptiPulse API. Check your connection and try again."
          : error.message,
      );
    }
    return rejectWithValue("Sign-in failed.");
  }
});

/** Re-establishes a session from the stored refresh token on app start. */
export const restoreSession = createAsyncThunk<SessionPayload | null, void>(
  "auth/restore",
  async () => {
    const stored = readStoredRefreshToken();
    if (!stored) return null;
    try {
      return toSession(await authApi.refresh(stored));
    } catch {
      // Includes the offline case. Treating it as "no session" is deliberate: this app is
      // online-only (T062), and a half-restored session with no server to check it against is
      // exactly the stale local authority the constitution forbids.
      return null;
    }
  },
);

export const loadProfile = createAsyncThunk<MeResponse, void>("auth/me", () =>
  authApi.me(),
);

export const logout = createAsyncThunk<void, void>("auth/logout", async () => {
  const stored = readStoredRefreshToken();
  if (stored) {
    // Best-effort: the local session is cleared either way by the fulfilled/rejected reducers.
    // Refusing to sign out because the network is down would be a worse failure than an
    // orphaned server-side token, which expires on its own.
    try {
      await authApi.logout(stored);
    } catch {
      /* ignore */
    }
  }
});

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    sessionEstablished(state, action: PayloadAction<SessionPayload>) {
      state.status = "authenticated";
      state.accessToken = action.payload.accessToken;
      state.refreshToken = action.payload.refreshToken;
      state.expiresAt = action.payload.expiresAt;
      state.role = action.payload.role;
      state.loginError = null;
      writeStoredRefreshToken(action.payload.refreshToken);
    },
    sessionCleared(state) {
      state.status = "anonymous";
      state.accessToken = null;
      state.refreshToken = null;
      state.expiresAt = null;
      state.role = null;
      state.profile = null;
      writeStoredRefreshToken(null);
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(login.pending, (state) => {
        state.loginPending = true;
        state.loginError = null;
      })
      .addCase(login.fulfilled, (state, action) => {
        state.loginPending = false;
        authSlice.caseReducers.sessionEstablished(state, action);
      })
      .addCase(login.rejected, (state, action) => {
        state.loginPending = false;
        state.loginError = action.payload ?? "Sign-in failed.";
        // Force "anonymous". A failed sign-in that arrives while the initial restore is still
        // in flight would otherwise leave status at "restoring", and App renders a spinner in
        // that state — the user would watch "Restoring your session…" forever and never see
        // why their password was rejected.
        state.status = "anonymous";
      })
      .addCase(restoreSession.fulfilled, (state, action) => {
        if (action.payload)
          authSlice.caseReducers.sessionEstablished(
            state,
            action as PayloadAction<SessionPayload>,
          );
        else authSlice.caseReducers.sessionCleared(state);
      })
      .addCase(restoreSession.rejected, (state) => {
        authSlice.caseReducers.sessionCleared(state);
      })
      .addCase(loadProfile.fulfilled, (state, action) => {
        state.profile = action.payload;
        // The server's answer wins over the login response if they ever disagree.
        state.role = action.payload.role;
      })
      .addCase(logout.fulfilled, (state) =>
        authSlice.caseReducers.sessionCleared(state),
      )
      .addCase(logout.rejected, (state) =>
        authSlice.caseReducers.sessionCleared(state),
      );
  },
});

export const { sessionEstablished, sessionCleared } = authSlice.actions;
export default authSlice.reducer;

/* ------------------------------------------------------------------------------------------ */
/* Bridge + silent refresh                                                                     */
/* ------------------------------------------------------------------------------------------ */

/** Refresh this long before the token actually expires, so an in-flight request is not caught mid-call. */
const REFRESH_LEAD_MS = 60_000;

interface MinimalStore {
  getState: () => { auth: AuthState };
  dispatch: (action: unknown) => unknown;
}

/**
 * Wires the API client to this slice and starts the silent-refresh timer.
 *
 * `inFlight` deduplicates: a page that fires five requests at once and gets five 401s must
 * perform ONE refresh, not five. Five concurrent refreshes with a rotating, reuse-detecting
 * token store would look exactly like token theft and revoke the whole family — the user would
 * be signed out by their own dashboard loading normally.
 */
export function installAuthBridge(store: MinimalStore): () => void {
  let inFlight: Promise<string | null> | null = null;

  const doRefresh = async (): Promise<string | null> => {
    const token =
      store.getState().auth.refreshToken ?? readStoredRefreshToken();
    if (!token) return null;
    try {
      const session = toSession(await authApi.refresh(token));
      store.dispatch(sessionEstablished(session));
      return session.accessToken;
    } catch {
      store.dispatch(sessionCleared());
      return null;
    }
  };

  const refresh = (): Promise<string | null> => {
    if (!inFlight) {
      inFlight = doRefresh().finally(() => {
        inFlight = null;
      });
    }
    return inFlight;
  };

  configureAuthBridge({
    getAccessToken: () => store.getState().auth.accessToken,
    refresh,
    onSessionLost: () => store.dispatch(sessionCleared()),
  });

  // Poll rather than setTimeout-to-expiry: a laptop that sleeps through the timeout wakes with
  // a token that expired hours ago, and a 15s tick notices that on the next wake.
  const timer = window.setInterval(() => {
    const { status, expiresAt } = store.getState().auth;
    if (status !== "authenticated" || expiresAt === null) return;
    if (expiresAt - Date.now() <= REFRESH_LEAD_MS) void refresh();
  }, 15_000);

  return () => window.clearInterval(timer);
}

export const __testing = {
  REFRESH_TOKEN_KEY,
  readStoredRefreshToken,
  writeStoredRefreshToken,
};
