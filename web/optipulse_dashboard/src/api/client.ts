/**
 * Typed API client (T056).
 *
 * Every request and response type below is READ FROM `schema.d.ts`, which is generated from
 * `contracts-gen/openapi.json` by `contracts-gen/generate.sh`. Nothing here re-declares a
 * shape by hand: if the backend contract changes, this file stops compiling rather than
 * silently disagreeing with the server, and the CI drift gate catches the regeneration.
 */
import type { components } from "./schema";
import { apiUrl } from "./config";

type S = components["schemas"];

export type LoginRequest = S["LoginRequest"];
export type LoginResponse = S["LoginResponse"];
export type MeResponse = S["MeResponse"];
export type FlagResponse = S["FlagResponse"];
export type CreateFlagRequest = S["CreateFlagRequest"];
export type UpdateFlagRequest = S["UpdateFlagRequest"];
export type ExperimentResponse = S["ExperimentResponse"];
export type CreateExperimentRequest = S["CreateExperimentRequest"];
export type UpdateExperimentRequest = S["UpdateExperimentRequest"];
export type FlagExposureResponse = S["FlagExposureResponse"];
export type VariantExposureDto = S["VariantExposureDto"];
export type TargetingRuleDto = S["TargetingRuleDto"];
export type RolloutDto = S["RolloutDto"];
export type VariantDto = S["VariantDto"];
export type ProblemDetails = S["ProblemDetails"];

/**
 * int64/decimal fields are typed `number | string` because JSON cannot represent every
 * 64-bit integer exactly, so the serialiser is permitted to emit them as strings. Coercing at
 * the boundary keeps that detail out of every component that renders a version or a count.
 */
export function num(value: number | string | null | undefined): number {
  if (value === null || value === undefined) return 0;
  return typeof value === "number" ? value : Number(value);
}

export class ApiError extends Error {
  // Declared as fields rather than constructor parameter properties: tsconfig sets
  // `erasableSyntaxOnly`, so TypeScript-only syntax that emits runtime code is rejected.
  readonly status: number;
  readonly problem: ProblemDetails | null;

  constructor(status: number, problem: ProblemDetails | null, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.problem = problem;
  }

  /** True when the request never reached the server (offline, DNS, CORS). Drives T062. */
  get isNetworkFailure(): boolean {
    return this.status === 0;
  }

  /** A lost-update rejection: the caller edited a version the server has since moved past. */
  get isVersionConflict(): boolean {
    return this.status === 409 || this.status === 428;
  }
}

/**
 * The client does not own the session — the auth slice does (T057). It also must not import
 * the store, or store → client → store becomes a cycle that breaks module init order. So the
 * store injects these three callbacks once at startup.
 */
export interface AuthBridge {
  getAccessToken: () => string | null;
  /** Attempts a token refresh. Resolves to the new access token, or null if the session is over. */
  refresh: () => Promise<string | null>;
  /** Called when the session is definitively unusable, so the UI can return to the login screen. */
  onSessionLost: () => void;
}

let bridge: AuthBridge = {
  getAccessToken: () => null,
  refresh: async () => null,
  onSessionLost: () => {},
};

export function configureAuthBridge(next: AuthBridge): void {
  bridge = next;
}

interface RequestOptions {
  method?: string;
  body?: unknown;
  /** Sent as If-Match. Required by the server for PUT /flags and PUT /experiments (428 otherwise). */
  ifMatch?: number | string;
  /** Set for the auth endpoints themselves, which must not recurse into refresh-on-401. */
  anonymous?: boolean;
  signal?: AbortSignal;
}

async function readProblem(response: Response): Promise<ProblemDetails | null> {
  try {
    const text = await response.text();
    return text ? (JSON.parse(text) as ProblemDetails) : null;
  } catch {
    return null;
  }
}

function problemMessage(
  status: number,
  problem: ProblemDetails | null,
): string {
  const detail = problem?.detail?.trim();
  const title = problem?.title?.trim();
  return detail || title || `Request failed with status ${status}`;
}

async function send(
  path: string,
  options: RequestOptions,
  token: string | null,
): Promise<Response> {
  const headers: Record<string, string> = { Accept: "application/json" };
  if (options.body !== undefined) headers["Content-Type"] = "application/json";
  if (options.ifMatch !== undefined)
    headers["If-Match"] = String(options.ifMatch);
  if (token) headers.Authorization = `Bearer ${token}`;

  return fetch(apiUrl(path), {
    method: options.method ?? "GET",
    headers,
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
    signal: options.signal,
  });
}

async function request<T>(
  path: string,
  options: RequestOptions = {},
): Promise<T> {
  let response: Response;
  try {
    response = await send(
      path,
      options,
      options.anonymous ? null : bridge.getAccessToken(),
    );
  } catch (cause) {
    // fetch rejects only for transport-level failures. Status 0 is the signal ConnectivityGuard
    // reads; a 500 is the server answering, which is a different problem with a different fix.
    throw new ApiError(
      0,
      null,
      cause instanceof Error ? cause.message : "Network request failed",
    );
  }

  // A single refresh attempt, never a loop: if the refreshed token is also rejected the session
  // is genuinely over, and retrying would only produce a slower failure.
  if (response.status === 401 && !options.anonymous) {
    const refreshed = await bridge.refresh();
    if (!refreshed) {
      bridge.onSessionLost();
      throw new ApiError(
        401,
        await readProblem(response),
        "Your session has expired. Sign in again.",
      );
    }
    try {
      response = await send(path, options, refreshed);
    } catch (cause) {
      throw new ApiError(
        0,
        null,
        cause instanceof Error ? cause.message : "Network request failed",
      );
    }
    if (response.status === 401) {
      bridge.onSessionLost();
      throw new ApiError(
        401,
        await readProblem(response),
        "Your session has expired. Sign in again.",
      );
    }
  }

  if (!response.ok) {
    const problem = await readProblem(response);
    throw new ApiError(
      response.status,
      problem,
      problemMessage(response.status, problem),
    );
  }

  if (response.status === 204) return undefined as T;
  const text = await response.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

const V1 = "/api/v1";

export const authApi = {
  login: (body: LoginRequest) =>
    request<LoginResponse>(`${V1}/auth/login`, {
      method: "POST",
      body,
      anonymous: true,
    }),
  /** Anonymous by design: the refresh token IS the credential, and sending a dead access
   *  token alongside it would re-enter the 401 path this call exists to resolve. */
  refresh: (refreshToken: string) =>
    request<LoginResponse>(`${V1}/auth/refresh`, {
      method: "POST",
      body: { refreshToken },
      anonymous: true,
    }),
  logout: (refreshToken: string) =>
    request<void>(`${V1}/auth/logout`, {
      method: "POST",
      body: { refreshToken },
      anonymous: true,
    }),
  me: () => request<MeResponse>(`${V1}/auth/me`),
};

export const flagsApi = {
  list: (signal?: AbortSignal) =>
    request<FlagResponse[]>(`${V1}/flags`, { signal }),
  get: (key: string) =>
    request<FlagResponse>(`${V1}/flags/${encodeURIComponent(key)}`),
  create: (body: CreateFlagRequest) =>
    request<FlagResponse>(`${V1}/flags`, { method: "POST", body }),
  update: (key: string, version: number | string, body: UpdateFlagRequest) =>
    request<FlagResponse>(`${V1}/flags/${encodeURIComponent(key)}`, {
      method: "PUT",
      body,
      ifMatch: version,
    }),
  changeStatus: (key: string, status: string) =>
    request<FlagResponse>(`${V1}/flags/${encodeURIComponent(key)}/status`, {
      method: "POST",
      body: { status },
    }),
  setKillSwitch: (key: string, engaged: boolean) =>
    request<FlagResponse>(
      `${V1}/flags/${encodeURIComponent(key)}/kill-switch`,
      {
        method: "POST",
        body: { engaged },
      },
    ),
};

export const experimentsApi = {
  list: (flagKey?: string, signal?: AbortSignal) =>
    request<ExperimentResponse[]>(
      flagKey
        ? `${V1}/experiments?flagKey=${encodeURIComponent(flagKey)}`
        : `${V1}/experiments`,
      { signal },
    ),
  create: (body: CreateExperimentRequest) =>
    request<ExperimentResponse>(`${V1}/experiments`, { method: "POST", body }),
  update: (
    id: string,
    version: number | string,
    body: UpdateExperimentRequest,
  ) =>
    request<ExperimentResponse>(`${V1}/experiments/${id}`, {
      method: "PUT",
      body,
      ifMatch: version,
    }),
  changeStatus: (id: string, status: string) =>
    request<ExperimentResponse>(`${V1}/experiments/${id}/status`, {
      method: "POST",
      body: { status },
    }),
};

export const analyticsApi = {
  exposures: (flagKey: string, signal?: AbortSignal) =>
    request<FlagExposureResponse>(
      `${V1}/telemetry/flags/${encodeURIComponent(flagKey)}/exposures`,
      { signal },
    ),
};
