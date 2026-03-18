# Run script for SOEEApp
# Usage: .\run.ps1

Write-Host "SOEEApp run helper"
if (Test-Path ".\SOEEApp.sln") {
    Write-Host "Solution found: SOEEApp.sln"
} else {
    Write-Host "Error: Could not find SOEEApp.sln in current folder." -ForegroundColor Red
    exit 1
}

# Check for VS IDE
$devenvPaths = @(
    "C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\IDE\devenv.exe",
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\devenv.exe"
)

$devenv = $devenvPaths | Where-Object { Test-Path $_ } | Select-Object -First 1

if ($devenv) {
    Write-Host "Launching Visual Studio with solution..."
    Start-Process -FilePath $devenv -ArgumentList '"' + (Resolve-Path .\SOEEApp.sln).Path + '"'
    exit 0
}

Write-Host "Visual Studio not detected in common paths." -ForegroundColor Yellow
Write-Host "If you have Visual Studio installed, open the .sln file and run the project using IIS Express."

# Try to run with msbuild from MSBuild tools
$msbuild = Get-Command msbuild -ErrorAction SilentlyContinue
if ($msbuild) {
    Write-Host "msbuild found: $($msbuild.Path)"
    Write-Host "Building solution..."
    & $msbuild.Path ".\SOEEApp.sln" /t:Restore,Build
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Build succeeded. Run with Visual Studio or directly from IIS Express." -ForegroundColor Green
    } else {
        Write-Host "Build failed. Please open the solution in Visual Studio and fix errors." -ForegroundColor Red
    }
    exit $LASTEXITCODE
}

Write-Host "Neither devenv nor msbuild command was found." -ForegroundColor Red
Write-Host "Please install Visual Studio with .NET desktop development and reopen this folder." -ForegroundColor Red
exit 1
