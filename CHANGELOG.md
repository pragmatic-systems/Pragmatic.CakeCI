# Changelog

All notable changes to **Pragmatic.CakeCI** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

---

## [1.0.0] — 2026-07-19

> **First stable release.**

### Summary
Pragmatic.CakeCI is a Cake addin providing opinionated CI aliases for building, testing, benchmarking, linting, and publishing .NET projects via NuGet and Docker. It integrates with GitVersion for semantic versioning and SonarQube/SonarCloud for code quality analysis.

### Key Features
- **Auto-discovery** of test and benchmark projects — no manual configuration needed.
- **Build manifest** (`build.cakemix`) for declaring NuGet packages, Docker images, and API specs.
- **Cross-platform** — runs locally, in GitHub Actions, CircleCI, Jenkins, or any CI that supports Cake.
- **Dogfooding** — the project builds itself using its own aliases.
- **Quality gates** — lint checks, Sonar quality gate wait, and test coverage reporting built into every pipeline.

### API Surface

| Alias | Category | Description |
|---|---|---|
| `CiTest()` | Test | Run all discovered test projects with coverage |
| `CiLint()` | Lint | Verify code formatting |
| `CiVersion()` | Version | Resolve version from GitVersion |
| `CiBenchmark()` | Benchmark | Run discovered benchmark projects |
| `CiNugetPack()` | NugetPack | Pack projects from manifest |
| `CiNugetPush()` | NugetPush | Push packages to a NuGet source |
| `CiDockerBuild()` | Docker | Build Docker images from manifest |
| `CiDockerPush()` | Docker | Push Docker images to a registry |
| `CiDockerLogin()` | Docker | Authenticate with a container registry |
| `CiDockerLogout()` | Docker | Disconnect from a container registry |
| `CiSonarScannerBegin()` | Sonar | Start Sonar analysis session |
| `CiSonarScannerEnd()` | Sonar | End Sonar analysis session |
| `LoadBuildManifest()` | Manifest | Load or create the build manifest |
| `CiArgument()` | Arguments | Resolve CI arguments with fallback chain |

### Requirements
- **.NET 10.0** SDK
- **Cake** 6.x (installed via `dotnet-cake` tool)
- **GitVersion** (installed via `dotnet tool restore`)
- **Docker** (for Docker aliases)
- **SonarQube/SonarCloud** (for Sonar aliases)

### Installation

Add to `build.cake`:
```csharp
#addin "nuget:?package=Pragmatic.CakeCI&version=1.0.0"
```
