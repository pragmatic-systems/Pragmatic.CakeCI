# Pragmatic.CakeCI

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)
[![NuGet](https://img.shields.io/nuget/v/Pragmatic.CakeCI.svg)](https://www.nuget.org/packages/Pragmatic.CakeCI)

Cake addin providing opinionated CI aliases for building, testing, benchmarking, linting, and publishing .NET projects via NuGet and Docker. Integrates with GitVersion for semantic versioning and SonarQube/SonarCloud for code quality analysis.

---

## Build Status

| Measure | Level |
|:-|:-|
| [![Quality gate status](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) | [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=coverage)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) |
| [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) | [![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) |
| [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) | [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=bugs)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) |
| [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) | [![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) |


---

## Why Cake?

GitHub Actions are closed-source and proprietary — if they go down and you rely exclusively on them for CI, you can't build, test, or publish.

Cake runs anywhere: locally from the command line, in GitHub Actions, CircleCI, Jenkins, or any CI that supports .NET.

---

## Requirements

| Dependency | Version |
|---|---|
| .NET SDK | 10.0+ |
| Cake | 6.x (via `dotnet-cake`) |
| GitVersion | 5.x (via `dotnet tool restore`) |
| Docker | (for Docker aliases) |
| SonarQube / SonarCloud | (for Sonar aliases) |

Install the tool dependencies:

```bash
dotnet tool restore
```

---

## Installation

Add the package as a Cake addin in your `build.cake`:

```csharp
#addin "nuget:?package=Pragmatic.CakeCI&version=1.0.0"
```

Pin the build tool dependencies in `dotnet-tools.json` (Cake, GitVersion, SonarScanner are required regardless):

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "cake.tool": { "version": "6.2.0", "commands": ["dotnet-cake"] },
    "GitVersion.Tool": { "version": "5.12.0", "commands": ["dotnet-gitversion"] },
    "dotnet-sonarscanner": { "version": "7.1.1", "commands": ["dotnet-sonarscanner"] }
  }
}
```

Then ensure your `nuget.config` includes the package source:

```xml
<packageSources>
  <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
</packageSources>
```

---

## Quick Start

The simplest `build.cake` to build, test, and lint a .NET solution:

```csharp
#addin "nuget:?package=Pragmatic.CakeCI&version=1.0.0"

Task("Default")
    .Does(() =>
    {
        CiLint();
        CiTest();
    });

RunTarget("Default");
```

Run it:

```bash
dotnet cake build.cake
```

That's it — test projects (`*.Tests.csproj`) and benchmark projects (`*.Benchmark.csproj`) are **auto-discovered**. No manifest needed for basic usage.

---

## Overview

Pragmatic.CakeCI provides a set of Cake aliases that encapsulate common CI operations. It uses a `.cakemix` configuration file to determine what to pack, test, benchmark, and publish. On first run in a new project, it auto-creates a default manifest with best-guess defaults.

This project targets the **Microsoft Testing Platform** test runner and assumes benchmark projects are executables running **BenchmarkRunner**.

### Auto-Discovery

| Pattern | Purpose |
|---|---|
| `*.Tests.csproj` | Test projects — auto-discovered by `CiTest()` and Sonar coverage |
| `*.Benchmark.csproj` | Benchmark projects — auto-discovered by `CiBenchmark()` |

### API Reference

| Alias | Category | Description |
|---|---|---|
| `CiTest()` | Test | Run all discovered test projects with coverage and CTRF reports |
| `CiLint()` | Lint | Verify code formatting via `dotnet format --verify-no-changes` |
| `CiVersion(override?)` | Version | Resolve semantic version from GitVersion (optional override) |
| `CiBenchmark()` | Benchmark | Run all discovered benchmark projects |
| `CiNugetPack(manifest, folder, version)` | NugetPack | Pack projects listed in the build manifest |
| `CiNugetPush(args, folder)` | NugetPush | Push packed `.nupkg` files to a NuGet source |
| `CiDockerBuild(manifest, args, version)` | Docker | Build Docker images from manifest-listed Dockerfiles |
| `CiDockerPush(manifest, args, version)` | Docker | Push built images to a container registry |
| `CiDockerLogin(args)` | Docker | Authenticate with a container registry (uses `--password-stdin`) |
| `CiDockerLogout(args)` | Docker | Disconnect from a container registry |
| `CiSonarScannerBegin(args, artifactsFolder)` | Sonar | Start a Sonar analysis session with auto-discovered coverage reports |
| `CiSonarScannerEnd(args)` | Sonar | End the Sonar analysis session (includes quality gate wait) |
| `LoadBuildManifest(file?)` | Manifest | Load or auto-create the `build.cakemix` manifest |
| `CiArgument(name)` | Arguments | Resolve CI argument from CLI → env var → GitHub `INPUT_*` fallback chain |
| `CiArgument(name, default)` | Arguments | Same as above with a default value |

### Argument Resolution

`CiArgument()` resolves values using a three-tier fallback:

1. **Command-line argument** — passed via `dotnet cake build.cake /MyArg=value`
2. **Environment variable** — e.g., `SONARTOKEN=abc dotnet cake build.cake`
3. **GitHub Actions input** — `INPUT_SONARTOKEN` (set automatically by `cake-action`)

This means the same `build.cake` works identically locally and in CI without conditional logic.

---

## Build Manifest (`build.cakemix`)

The manifest declares what to pack and publish. Test and benchmark projects are auto-discovered and don't need to be listed.

```json
{
  "NugetPackages": [
    "./src/MyApp.Model/MyApp.Model.csproj"
  ],
  "DockerPackages": [
    "./src/MyApp.Api/Dockerfile",
    "./src/MyApp.DbUp/Dockerfile"
  ],
  "Benchmarks": [
    "./test/MyApp.Benchmark/MyApp.Benchmark.csproj"
  ],
  "ApiSpecs": {
    "MyApp.Api": "http://localhost:5080"
  }
}
```

> **Note:** Use `/` for folder separators — this works on both Windows and Linux.

If no `build.cakemix` exists, `LoadBuildManifest()` auto-creates one by scanning for Dockerfiles and `*.Benchmark.csproj` files.

---

## Build Targets

| Target | Description | Required Arguments |
|---|---|---|
| `BuildAndTest` | Lint, build, and test all projects. Writes results to `artifacts/`. | None |
| `BuildAndBenchmark` | Lint, build, and benchmark all projects. Writes results to `artifacts/`. | None |
| `BuildAndSonarScan` | Lint, Sonar begin, test, benchmark, Sonar end. | `SonarOrg`, `SonarToken`, `SonarProjectKey`, `SonarProjectName`, `SonarBranch` |
| `NugetPackAndPush` | Lint, Sonar begin, test, benchmark, pack, push NuGet packages, Sonar end. | All Sonar args + `NugetSource`, `NugetApiKey` |
| `DockerPackAndPush` | Lint, Sonar begin, test, benchmark, build/push Docker images, Sonar end. | All Sonar args + `ContainerRegistry`, `ContainerRegistryToken`, `ContainerRegistryUserName` |
| `FullPackAndPush` | Lint, Sonar begin, test, benchmark, pack NuGet + build/push Docker, Sonar end. | All Sonar args + all NuGet args + all Docker args |

Example with arguments:

```bash
dotnet cake build.cake \
  /target=NugetPackAndPush \
  /NugetSource=https://api.nuget.org/v3/index.json \
  /NugetApiKey=my-api-key \
  /SonarOrg=my-org \
  /SonarToken=my-token \
  /SonarProjectKey=my-key \
  /SonarProjectName="My Project" \
  /SonarBranch=main
```

---

## Versioning

Pragmatic.CakeCI uses [GitVersion](https://gitversion.net/) in **mainline mode**. The version is determined from your git history — no manual bumps needed. Configure it via `GitVersion.yml` at the repo root:

```yaml
mode: mainline
branches:
  main:
    increment: Patch
```

Override the version at runtime:

```csharp
var version = CiVersion("1.2.3-custom");
```

---

## Dogfood Build

This project dogfoods its own `Pragmatic.CakeCI` package. Before running the build script, prepare the local environment by rebuilding and repacking the project:

> **IMPORTANT** — Always read and review untrusted scripts before execution.

```bash
pwsh -ExecutionPolicy Bypass -File scripts/prepare-dogfood.ps1
```

This script:
1. Clears the `tools/Addins` folder (removes cached addins)
2. Clears and re-creates the `local-packages` folder
3. Builds and packs `Pragmatic.CakeCI` with version `0.1.0-dogfood` into `local-packages`

Once prepared, run the build as usual:

```bash
dotnet cake build.cake
```

---

## Tools

| Tool | Purpose |
|---|---|
| [Cake Build](https://cakebuild.net/) | Build automation orchestration |
| [GitVersion](https://gitversion.net/) | Semantic versioning from git history |
| [dotnet-pack](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-pack) | NuGet package creation |
| [dotnet-sonarscanner](https://docs.sonarsource.com/sonarqube/latest/analyzing-source-code/scanners/sonarscanner-msbuild/) | SonarQube/SonarCloud code analysis |
| [Microsoft Testing Platform](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-platform-overview) | Test runner and coverage |

---

## License

This project is licensed under the [MIT License](LICENSE.txt).
