# HobScript Build Script
# This script builds the HobScript library and creates NuGet packages

param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0",
    [switch]$Clean,
    [switch]$Pack,
    [switch]$Test,
    [switch]$Help
)

if ($Help) {
    Write-Host "HobScript Build Script" -ForegroundColor Green
    Write-Host "=====================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage: .\build.ps1 [options]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  -Configuration <config>  Build configuration (Debug|Release) [Default: Release]"
    Write-Host "  -Version <version>       Package version [Default: 1.0.0]"
    Write-Host "  -Clean                   Clean before building"
    Write-Host "  -Pack                    Create NuGet packages"
    Write-Host "  -Test                    Run tests"
    Write-Host "  -Help                    Show this help message"
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  .\build.ps1                    # Build in Release mode"
    Write-Host "  .\build.ps1 -Clean -Pack       # Clean, build, and create packages"
    Write-Host "  .\build.ps1 -Configuration Debug -Test  # Build in Debug mode and run tests"
    exit 0
}

Write-Host "HobScript Build Script" -ForegroundColor Green
Write-Host "=====================" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Version: $Version" -ForegroundColor Yellow
Write-Host ""

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning..." -ForegroundColor Cyan
    dotnet clean HobScript.csproj --configuration $Configuration
    dotnet clean HobScript.Console.csproj --configuration $Configuration
    if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
    if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
    if (Test-Path "nupkgs") { Remove-Item -Recurse -Force "nupkgs" }
    Write-Host "Clean completed" -ForegroundColor Green
}

# Restore dependencies
Write-Host "Restoring dependencies..." -ForegroundColor Cyan
dotnet restore HobScript.csproj
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to restore dependencies" -ForegroundColor Red
    exit 1
}

# Update version if specified
if ($Version -ne "1.0.0") {
    Write-Host "Updating version to $Version..." -ForegroundColor Cyan
    $projectFile = "HobScript.csproj"
    $content = Get-Content $projectFile
    $content = $content -replace '<Version>.*</Version>', "<Version>$Version</Version>"
    $content = $content -replace '<PackageVersion>.*</PackageVersion>', "<PackageVersion>$Version</PackageVersion>"
    $content = $content -replace '<AssemblyVersion>.*</AssemblyVersion>', "<AssemblyVersion>$Version.0</AssemblyVersion>"
    $content = $content -replace '<FileVersion>.*</FileVersion>', "<FileVersion>$Version.0</FileVersion>"
    Set-Content $projectFile $content
    Write-Host "Version updated" -ForegroundColor Green
}

# Build library
Write-Host "Building HobScript library..." -ForegroundColor Cyan
dotnet build HobScript.csproj --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build HobScript library" -ForegroundColor Red
    exit 1
}

# Build console application
Write-Host "Building console application..." -ForegroundColor Cyan
dotnet build HobScript.Console.csproj --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build console application" -ForegroundColor Red
    exit 1
}

# Run tests if requested
if ($Test) {
    Write-Host "Running tests..." -ForegroundColor Cyan
    if (Test-Path "Tests") {
        dotnet test --configuration $Configuration --no-build
    } else {
        Write-Host "No tests found, running console application instead..." -ForegroundColor Yellow
        timeout 10 dotnet run --project HobScript.Console.csproj --configuration $Configuration --no-build
    }
}

# Create NuGet packages if requested
if ($Pack) {
    Write-Host "Creating NuGet packages..." -ForegroundColor Cyan
    if (!(Test-Path "nupkgs")) { New-Item -ItemType Directory -Path "nupkgs" }
    dotnet pack HobScript.csproj --configuration $Configuration --no-build --output nupkgs
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to create NuGet packages" -ForegroundColor Red
        exit 1
    }
    Write-Host "NuGet packages created in nupkgs/ directory" -ForegroundColor Green
    Get-ChildItem "nupkgs" -Filter "*.nupkg" | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Yellow }
}

Write-Host ""
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Green
