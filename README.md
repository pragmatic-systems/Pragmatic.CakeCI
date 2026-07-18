# CakeTools
Cake tools for build, test, benchmark, sonar scan and various pack/push operations.

## Sonar

## Why Cake?
Github Actions are closed source and proprietary - If they are unavailable and you rely on them for CI, you can't build, test, publish your application. 

Because of that I've opted for Cake which can be run both locally via cmd, via GHA, as well as in Circle CI, Jenkins or any other build pipeline.

## Overview
This build.cake file uses a `.cakemix` configuration file to determine what to pack, test, benchmark etc. It will create an initial version when first run in a project that will make a best guess default setup.

This project targets the Microsoft Testing Platform test runner, and assumes that the benchmark project is an executable that runs BenchmarkRunner.

## Build Status

| Measure | Level |
|:-|:-|
| [![Quality gate status](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) | [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=coverage)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) |
| [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) | [![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) |
| [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) | [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=bugs)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) |
| [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) | [![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.CakeCI&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.CakeCI) |

## Cakemix Sample Schema

```
{
  "NugetPackages": [
    "./src/Template.DbApi.Model/Template.DbApi.Model.csproj",
  ],
  "DockerPackages": [
    "./src/Template.DbApi.Api/Dockerfile",
    "./src/Template.DbApi.DbUp/Dockerfile"
  ],
  "Benchmarks": [
    "./test/Template.DbApi.Benchmark/Template.DbApi.Benchmark.csproj"
  ],
  "ApiSpecs": {
    "Template.DbApi.Api": "http://localhost:5080"
  }
}
```

> **Note:** Test projects (`*.Tests.csproj`) are auto-discovered — no need to list them in the manifest.

## Note
Use `/` for folder seperators as this works on both Windows and Linux.

## Targets
* `BuildAndTest` - Build and Test the projects. Writes results to `artifacts` folder.
* `BuildAndBenchmark` - Build and Benchmark the projects. Writes results to `artifacts` folder.
* `NugetPackAndPush` - Package and Push nuget packages. Writes results to `artifacts` folder.
* `DockerPackAndPush` - Package and push apps as docker images.
* `FullPackAndPush` - Package and Push nuget and docker images.

## Dogfood Build
This project dogfoods its own `Pragmatic.CakeCI` package. Before running the build script, prepare the local environment by rebuilding and repacking the project:

IMPORTANT - Always read and review un-trusted scripts before execution.

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

## Tools
* Cake Build - https://cakebuild.net/
* GitVersion - https://gitversion.net/
* Dotnet Pack - https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-pack
