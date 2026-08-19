# API layer

Two kinds of file live here, and the difference matters:

| File          | Origin                                                                     | Editable?                                                                                                                            |
| ------------- | -------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| `schema.d.ts` | **Generated** by `contracts-gen/generate.sh` (pinned `openapi-typescript`) | No — and Prettier is barred from it in `.prettierignore`, because a reformat is indistinguishable from contract drift to the CI gate |
| `client.ts`   | Hand-written                                                               | Yes                                                                                                                                  |
| `config.ts`   | Hand-written                                                               | Yes                                                                                                                                  |

`client.ts` derives every request and response type from `schema.d.ts` rather than declaring its
own, so a backend contract change surfaces as a TypeScript error here instead of a runtime
surprise in a component.

The CI drift gate re-runs the generator and fails on any diff across this directory. Hand-written
files never change during regeneration, so the gate only fires on real drift — but it does mean
an uncommitted edit to `client.ts` or `config.ts` will also show up. Commit before running it.

See [contracts/openapi-pipeline.md](../../../../specs/001-optipulse-platform/contracts/openapi-pipeline.md).
