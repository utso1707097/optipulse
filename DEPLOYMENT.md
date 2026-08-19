# Deploying OptiPulse

**Start here: the one-click path.** About five minutes, and you type five values. The manual
walkthrough further down is for when you want control over each piece.

Every environment variable in this document was read out of the source, not from memory.

**You perform these steps.** Each involves a credential or an authorization against your own
accounts — none of it should be delegated to an assistant or pasted into a chat window.

---

## Quick path — Render Blueprint (~5 minutes)

`render.yaml` declares the API, PostgreSQL and Redis together. Render creates all three and
**injects the connection strings itself**, so there is nothing to copy between dashboards. The
API accepts the `postgres://` and `redis://` URLs Render injects natively, which is what makes
that possible.

1. Sign in at **render.com** with GitHub.
2. **New → Blueprint** → pick this repository. Render reads `render.yaml`.
3. It prompts for exactly five values:

   | Prompt | What to enter |
   |---|---|
   | `Bootstrap__Manager__Email` | an email you control |
   | `Bootstrap__Manager__Password` | a strong password |
   | `Bootstrap__Admin__Email` | an email you control |
   | `Bootstrap__Admin__Password` | a different strong password |
   | `Cors__AllowedOrigins__0` | your dashboard URL — put a placeholder now, correct it after step 6 |

4. **Apply.** The first build takes about five minutes.

5. Check it:

   ```bash
   curl https://YOUR-API.onrender.com/health/ready
   # {"status":"ready","database":"up","snapshotVersion":0}
   ```

6. **Copy the SDK key from the logs.** Printed exactly once:

   ```
   Bootstrap service-account key (shown once, not recoverable): opk_...
   ```

   Only its hash is stored, so it cannot be recovered — the same property that makes a database
   leak useless to an attacker. Lose it and you issue a new service account.

7. Dashboard on Vercel: import the repo, root directory `web/optipulse_dashboard`, framework
   Vite, `VITE_API_URL` = your Render URL. Then correct `Cors__AllowedOrigins__0` in Render to
   the URL Vercel gives you.

**What you did not have to do:** create a Neon account, create an Upstash account, copy a
connection string, or generate a signing key. Render provisions Postgres and Redis, injects their
URLs, and generates the JWT signing key itself.

### The trade-off you are accepting

Render's **free PostgreSQL has historically been removed after a trial window**, which would take
the audit trail and every flag with it. Fine for a portfolio demo; not fine for anything you want
to keep.

To avoid it: delete the `databases:` block from `render.yaml` and point the three
`ConnectionStrings__*` variables at a Neon database. The API accepts Neon's `postgresql://` URL
as-is.

---

## Manual path — full control

Use this if you want the data somewhere durable from the start, or you are deploying somewhere
other than Render.

## What you are deploying, and why it constrains the choice

The API runs **three long-running background services** — the exposure drain, the Redis
invalidation subscriber, and the service-account refresh. That single fact rules out serverless
hosting entirely: Vercel Functions, Cloudflare Workers and Lambda freeze between requests, so the
exposure drain would never flush and invalidation would never arrive.

**You need a long-running container.** Hence Render for the API, and Vercel only for the
dashboard, which is static assets.

| Component | Host | Why |
|---|---|---|
| API | Render (Docker web service) | Long-running process, free tier |
| PostgreSQL | **Neon** | Render's free database has historically been deleted after a trial window — that would take the audit trail and every flag with it |
| Redis | **Upstash** | Free tier that persists. **Verify Pub/Sub is supported** — invalidation depends on it |
| Dashboard | Vercel | Static hosting, no cold start |
| Flutter app | *not hostable* | iOS/Android only by constitution. Attach an APK to a GitHub release |

---

## 1. PostgreSQL (Neon)

1. Create a project at neon.tech and copy the connection string.
2. Append `;SSL Mode=Require;Trust Server Certificate=true` if it is not already TLS-enabled —
   Npgsql will not connect to a managed provider without TLS.

All three contexts share one physical database and each owns its own tables, so **the same
connection string goes in all three variables** below. That is deliberate (per-context
`DbContext`, one database) and not a copy-paste error.

## 2. Redis (Upstash)

Create a database and copy the connection string in `host:port,password=...,ssl=True` form —
StackExchange.Redis expects that shape, not a `redis://` URL.

If Pub/Sub turns out to be unavailable on the free tier, the API still runs: evaluation serves
from its in-memory snapshot and fails safe. What degrades is cross-instance invalidation, which
on a single free instance is not doing much anyway.

## 3. Generate a JWT signing key

```bash
openssl rand -base64 48
```

Keep it out of git, out of chat, and out of screenshots. The API **refuses to start** outside
Development without it — that refusal is deliberate (Principle VI).

## 4. Choose bootstrap credentials

Pick a real email and a strong password for each of the two accounts. The API creates them **only
into an empty database** and never modifies them afterwards, so a redeploy will not reset a
password you later change.

Outside Development, if these are absent the API **refuses to seed** rather than inventing
credentials — you will get a running API with no accounts and a warning in the log. That is the
intended behaviour, not a bug.

## 5. Render web service

Create a **Web Service** → connect this repository → runtime **Docker**.

- Dockerfile path: `backend/Dockerfile`
- Docker context: `backend`
- Health check path: `/health/live`
- Auto-deploy: **off** (CI triggers deploys only after all gates pass)

### Environment variables

| Variable | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Database__Provider` | `Postgres` |
| `Database__SchemaStrategy` | `Migrate` |
| `ConnectionStrings__Flags` | *(Neon connection string)* |
| `ConnectionStrings__Audit` | *(same Neon string)* |
| `ConnectionStrings__Identity` | *(same Neon string)* |
| `Redis__ConnectionString` | *(Upstash connection string)* |
| `Jwt__SigningKey` | *(from step 3)* |
| `Bootstrap__Manager__Email` | your manager email |
| `Bootstrap__Manager__Password` | strong password |
| `Bootstrap__Admin__Email` | your admin email |
| `Bootstrap__Admin__Password` | strong password |
| `Cors__AllowedOrigins__0` | your Vercel URL, e.g. `https://optipulse.vercel.app` |

**Double underscores, not colons.** `Jwt__SigningKey` maps to `Jwt:SigningKey`; a single
underscore or a colon will be silently ignored and the API will refuse to start.

`Cors__AllowedOrigins__0` is an array index — add `__1`, `__2` for more origins. A `*` is
**rejected at startup** on purpose: this API carries bearer tokens and an Admin kill-switch.

### Capture the SDK key

On first boot the log prints, exactly once:

```
Bootstrap service-account key (shown once, not recoverable): opk_...
```

Copy it. Only its hash is stored, so it cannot be recovered — losing it means issuing a new
service account, which is the same property that makes a database leak useless to an attacker.

## 6. Vercel dashboard

Import the repo, root directory `web/optipulse_dashboard`, framework **Vite**.

Set `VITE_API_URL` to your Render URL (e.g. `https://optipulse-api.onrender.com`).

This must be a **build-time** variable. Vite inlines it into the bundle; a static build has no
runtime environment to read, so changing it later requires a rebuild, not a restart.

## 7. GitHub secrets for automatic deploys

`Settings → Secrets and variables → Actions`:

| Secret | Where to find it |
|---|---|
| `RENDER_DEPLOY_HOOK_URL` | Render service → Settings → Deploy Hook |
| `RENDER_API_BASE_URL` | your Render URL, no trailing slash |
| `VERCEL_TOKEN` | Vercel → Account Settings → Tokens |
| `VERCEL_ORG_ID`, `VERCEL_PROJECT_ID` | `.vercel/project.json` after running `vercel link` locally |
| `VITE_API_URL` | same as step 6 |

Push to `main` now deploys **only after** backend tests, the contract drift gate, web and mobile
all pass — a red build cannot deploy. The pipeline then polls `/health/ready` and fails if the new
instance never becomes healthy, so a bad connection string or a failed migration surfaces as a red
pipeline instead of a silently dead service.

Missing secrets cause the deploy jobs to **skip**, not fail, so a fork without these accounts
still builds green.

---

## Verifying it works

```bash
curl https://YOUR-API.onrender.com/health/ready
# {"status":"ready","database":"up","snapshotVersion":0}
```

Then log in with the Manager credentials from step 4, create a flag, and activate it.

## Things that will look broken but are not

**The first request after ~15 minutes idle takes about a minute.** Render's free tier suspends
idle instances, and this app then runs migrations and loads its flag snapshot on wake. Say so in
your README so a visitor does not conclude the service is down.

**The live demo will show 200–500 ms per evaluation.** That is the internet, not the engine. The
in-process evaluation is 6–40 ns with zero allocations, proven by the benchmark gate in CI — those
two numbers measure different things, and the benchmark is the one that speaks to the design.
Publish the benchmark table as your performance evidence; treat the deployment as a convenience
for clicking through the workflow.

**`/evaluate` returns 401 without a key.** By design — it is the machine surface and takes a
service-account credential, never a human login.

## Known limits of a free-tier deployment

- **One instance.** Cross-node invalidation is untested in practice, and startup migrations would
  race if you ever scale beyond one.
- **The exposure table grows unbounded.** There is no retention policy yet; on a free database it
  will eventually consume the storage quota.
- **No rate limiting** on `/auth/login`. PBKDF2 at 210k iterations makes each guess expensive,
  which on a 0.5 vCPU instance makes repeated login attempts a CPU-exhaustion vector rather than a
  password-cracking one.
- **The audit trail cannot be read through the API yet** (T080). It is written and — since T081 —
  genuinely immutable at the database level, but there is no query endpoint.
