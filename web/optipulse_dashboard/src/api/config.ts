/**
 * Resolves the API base URL.
 *
 * The dashboard is deployed as static assets on a different origin than the API (constitution
 * v2.3.0 deployment topology, FR-031), so the base URL cannot be a relative path in production.
 * It comes from VITE_API_URL, injected at build time.
 *
 * The fallback is an empty string, meaning same-origin relative requests. That keeps local
 * development working with a dev-server proxy and no configuration — while a deployed build
 * that forgets the variable fails loudly against its own origin rather than silently pointing
 * at a hardcoded host belonging to someone else's environment.
 */
const configured = import.meta.env.VITE_API_URL?.trim() ?? '';

// Trailing slashes are stripped so callers can always write `${API_BASE_URL}/api/v1/...`
// without producing a double slash, which some routers treat as a different path.
export const API_BASE_URL = configured.replace(/\/+$/, '');

export function apiUrl(path: string): string {
  const normalised = path.startsWith('/') ? path : `/${path}`;
  return `${API_BASE_URL}${normalised}`;
}
