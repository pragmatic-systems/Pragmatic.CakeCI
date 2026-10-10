---
name: dogfood-loop
description: Run the build, test, or benchmark pipeline for the Pragmatic.CakeCI repo via its dogfood loop. Before EVERY pipeline run, execute scripts/prepare-dogfood.ps1 to repack the addin as 0.1.0-dogfood into local-packages, then invoke dotnet cake. Use whenever building, testing, or benchmarking this repository.
---

# Dogfood Loop

This repo builds itself with its own `Pragmatic.CakeCI` addin. `build.cake` declares:

```csharp
#addin nuget:?package=Pragmatic.CakeCI&version=0.1.0-dogfood
```

`0.1.0-dogfood` does not exist on nuget.org — it only exists after a local pack into `local-packages/` (the local feed listed first in `nuget.config`). The pipeline therefore runs the code as it currently is on disk, but **only if you re-pack first**.

## Mandatory sequence (never reorder, never skip step 1)

1. **Prepare the dogfood package** — always, before any build/test/benchmark run:

   ```bash
   pwsh -ExecutionPolicy Bypass -File scripts/prepare-dogfood.ps1
   ```

   This clears `tools/Addins` (cached addin resolution), clears and re-creates `local-packages/`, packs `Pragmatic.CakeCI` as `0.1.0-dogfood` into `local-packages/`, and verifies the nupkg exists. Do not hand-roll a `dotnet pack` in its place — the script owns the full clean + pack + verify sequence.

2. **Run the pipeline through Cake** (this exercises the addin you just packed):

   ```bash
   dotnet cake build.cake
   ```

   Pick a target with `--target=<name>` (or the `target` env var):

   | Target | Does |
   |---|---|
   | `Default` (default) | lint + tests + benchmarks |
   | `BuildAndTest` | tests only |
   | `BuildAndBenchmark` | benchmarks only |
   | `BuildAndSonarScan` | lint + Sonar begin → tests + benchmarks → Sonar end (needs Sonar args) |
   | `NugetPackAndPush` | + pack & push NuGet (needs `NugetSource`, `NugetApiKey`) |
   | `DockerPackAndPush` | + docker build & push (needs `ContainerRegistry*` args) |
   | `FullPackAndPush` | both NuGet and Docker |

   Example:

   ```bash
   dotnet cake build.cake --target=BuildAndTest
   ```

   Extra arguments (`NugetSource`, `SonarOrg`, `VersionOverride`, …) are read by `CiArgument`, so pass them either as Cake args (`--NugetSource=...`) or as environment variables of the same name (or `INPUT_<NAME>` uppercased).

## Rules

- **Never** run `dotnet cake` against a stale or missing `local-packages/Pragmatic.CakeCI.0.1.0-dogfood.nupkg` — that would test yesterday's addin (or fail to resolve). When in doubt, re-run the prepare script.
- The dogfood pipeline (Cake) is the canonical way to build, test, and benchmark in this repo; prefer it over raw `dotnet test`/`dotnet build` for verification, since the raw commands bypass the addin under test.
- The prepare script requires PowerShell Core (`pwsh`).

## Troubleshooting

- **`0.1.0-dogfood` not found during `dotnet cake`**: the prepare script didn't complete. Check `local-packages/` for `Pragmatic.CakeCI.0.1.0-dogfood.nupkg` and that `nuget.config` still lists the `local-packages` feed before `nuget.org`.
