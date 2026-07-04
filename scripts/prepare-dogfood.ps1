#!/usr/bin/env pwsh
#
# prepare-dogfood.ps1
#
# Prepares the local dogfood environment by:
#   1. Clearing the tools/Addins folder
#   2. Clearing and re-creating the local-packages folder
#   3. Building and packing Pragsys.CakeCI with version 0.1.0-dogfood
#

$ErrorActionPreference = "Stop"

$rootDir = Split-Path -Parent $PSScriptRoot
$addinsDir = Join-Path $rootDir "tools/Addins"
$localPackagesDir = Join-Path $rootDir "local-packages"
$projectPath = Join-Path $rootDir "Pragsys.CakeCI/Pragsys.CakeCI.csproj"
$packageVersion = "0.1.0-dogfood"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Pragsys.CakeCI Dogfood Preparation" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Clear tools/Addins
Write-Host "[1/3] Clearing tools/Addins folder..." -ForegroundColor Yellow
if (Test-Path $addinsDir) {
    $items = Get-ChildItem -Path $addinsDir -Force
    if ($items) {
        Remove-Item -Path $addinsDir -Recurse -Force
        Write-Host "  Removed $($items.Count) item(s) from tools/Addins" -ForegroundColor Green
    } else {
        Write-Host "  tools/Addins is already empty" -ForegroundColor Green
    }
} else {
    Write-Host "  tools/Addins does not exist, skipping" -ForegroundColor Green
}
Write-Host ""

# Step 2: Clear and re-create local-packages
Write-Host "[2/3] Clearing and re-creating local-packages folder..." -ForegroundColor Yellow
if (Test-Path $localPackagesDir) {
    $items = Get-ChildItem -Path $localPackagesDir -Force
    if ($items) {
        Remove-Item -Path $localPackagesDir -Recurse -Force
        Write-Host "  Removed $($items.Count) item(s) from local-packages" -ForegroundColor Green
    }
}
New-Item -ItemType Directory -Path $localPackagesDir -Force | Out-Null
Write-Host "  local-packages folder ready" -ForegroundColor Green
Write-Host ""

# Step 3: Build and pack Pragsys.CakeCI
Write-Host "[3/3] Building and packing Pragsys.CakeCI v$packageVersion..." -ForegroundColor Yellow

$packArgs = @(
    "pack",
    $projectPath,
    "-c", "Release",
    "-p:PackageVersion=$packageVersion",
    "-o", $localPackagesDir,
    "/nowarn:NU5128",
    "--verbosity", "minimal"
)

Write-Host "  Running: dotnet $($packArgs -join ' ')" -ForegroundColor Gray
$dotnetResult = dotnet $packArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: dotnet pack failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Verify the package was created
$expectedPackage = Join-Path $localPackagesDir "Pragsys.CakeCI.${packageVersion}.nupkg"
if (Test-Path $expectedPackage) {
    $size = (Get-Item $expectedPackage).Length
    Write-Host "  Package created: Pragsys.CakeCI.${packageVersion}.nupkg ($size bytes)" -ForegroundColor Green
} else {
    Write-Host "ERROR: Expected package not found at $expectedPackage" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Dogfood preparation complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "You can now run the build with:" -ForegroundColor Gray
Write-Host "  dotnet cake build.cake" -ForegroundColor Gray
