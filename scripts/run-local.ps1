param(
    [string]$Url = "http://localhost:5000"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$env:DATA_DIR = Join-Path $root "data"
$env:PATH_BASE = "/hopital"
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = $Url

New-Item -ItemType Directory -Force -Path $env:DATA_DIR | Out-Null

dotnet restore .\GestionHopital.sln
dotnet build .\GestionHopital.sln

dotnet run --project .\Presentation\Presentation.csproj --no-build
