/// <reference types="vitest/config" />
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

/**
 * Where `npm run dev` forwards /api calls. Defaults to a locally-running API host; set
 * OPTIPULSE_API_PROXY to point the local dashboard at the deployed one instead:
 *
 *   OPTIPULSE_API_PROXY=https://optipulse-api.onrender.com npm run dev
 */
const apiProxyTarget =
  process.env.OPTIPULSE_API_PROXY ?? "http://localhost:5289";

// https://vite.dev/config/
export default defineConfig({
  // Tailwind v4 runs as a Vite plugin rather than a PostCSS pass: there is no
  // tailwind.config.js, and the design tokens live in `@theme` inside src/index.css.
  plugins: [react(), tailwindcss()],

  server: {
    /**
     * A DEV PROXY, not a CORS entry — and the difference is the point.
     *
     * With VITE_API_URL unset locally, the client issues same-origin requests to
     * http://localhost:5173/api/... which Vite forwards to the API. The browser never sees a
     * cross-origin request, so no preflight happens and no CORS configuration is involved.
     *
     * The alternative — adding http://localhost:5173 to the API's production allowlist — would
     * mean the deployed API permanently trusts an origin that any developer's machine can
     * serve, including one running something other than this dashboard. A local convenience
     * should not widen what production accepts.
     *
     * changeOrigin rewrites the Host header, which a TLS host like Render requires to route the
     * request to the right service at all.
     */
    proxy: {
      "/api": { target: apiProxyTarget, changeOrigin: true },
      "/health": { target: apiProxyTarget, changeOrigin: true },
    },
  },

  test: {
    environment: "jsdom",
    globals: true,
    setupFiles: ["./test/setup.ts"],
    include: ["test/**/*.test.{ts,tsx}", "src/**/*.test.{ts,tsx}"],
  },
});
