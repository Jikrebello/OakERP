<#  OakERP - Migrate + Seed
    Usage (local):
      pwsh ./tools/migrate.ps1                          # Development, migrator only (migrate+seed)
      pwsh ./tools/migrate.ps1 -Env Production          # Production, migrator only
      pwsh ./tools/migrate.ps1 -UseEf                   # First run dotnet ef database update, then seed
      pwsh ./tools/migrate.ps1 -MigrateOnly -UseEf      # Only EF migration (no seeding)
      pwsh ./tools/migrate.ps1 -SeedOnly                # Only seed (migrator; skips EF)
      pwsh ./tools/migrate.ps1 -Cs "Host=...;..."       # Override connection string via env var
#>

param(
  [ValidateSet('Development','Staging','Production')]
  [string]$Env = 'Development',

  # If true, run `dotnet ef database update` before seeding
  [switch]$UseEf,

  # If true, _only_ run EF migration step (implies -UseEf)
  [switch]$MigrateOnly,

  # If true, run only seeding (via migrator) and skip EF
  [switch]$SeedOnly,

  # Optional connection string override; sets ConnectionStrings__DefaultConnection
  [string]$Cs = $null,

  # Paths (adjust for your repo)
  [string]$InfraProj = 'src/OakERP.Infrastructure/OakERP.Infrastructure.csproj',
  [string]$ApiProj = 'src/OakERP.Api/OakERP.Api.csproj',
  [string]$MigratorProj = 'src/OakERP.MigrationTool/OakERP.MigrationTool.csproj',

  # Build controls
  [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
$PSStyle.OutputRendering = 'Host'  # nicer colors locally

function Info($m)  { Write-Host "[INFO ] $m" -ForegroundColor Cyan }
function Warn($m)  { Write-Host "[WARN ] $m" -ForegroundColor Yellow }
function ErrorMsg($m){ Write-Host "[ERROR] $m" -ForegroundColor Red }

try {
  Push-Location (git rev-parse --show-toplevel 2>$null | % { if ($_){$_} else {Get-Location} })

  # Set environment for both EF & Migrator
  $env:ASPNETCORE_ENVIRONMENT = $Env
  if ($Cs) { $env:ConnectionStrings__DefaultConnection = $Cs }

  # Decide plan
  if ($MigrateOnly) { $UseEf = $true }
  if ($SeedOnly -and $MigrateOnly) { throw "Choose either -SeedOnly or -MigrateOnly, not both." }

  Info "Environment: $Env"
  if ($Cs) { Info "Using connection string from -Cs." }

  if ($UseEf) {
    Info "Running EF database update..."
    $noBuildFlag = $NoBuild.IsPresent ? '--no-build' : ''
    dotnet ef database update `
      --project $InfraProj `
      --startup-project $ApiProj `
      $noBuildFlag

    Info "EF database update completed."
    if ($MigrateOnly) {
      Info "Migrate-only mode complete."
      exit 0
    }
  } else {
    Info "Skipping EF update (migrator will run MigrateAsync())."
  }

  if (-not $SeedOnly -and -not $UseEf) {
    # We’ll still run migrator (it migrates + seeds)
    Info "Running migrator (migrate + seed)..."
  } elseif ($SeedOnly) {
    Info "Running migrator (seed only path requested, but migrator always calls MigrateAsync() idempotently)."
  } else {
    Info "Running migrator (seeding after EF update)..."
  }

  $noBuildRun = $NoBuild.IsPresent ? '--no-build' : ''
  dotnet run --project $MigratorProj --configuration Release $noBuildRun
  Info "Migrator finished successfully."

  exit 0
}
catch {
  ErrorMsg $_.Exception.Message
  exit 1
}
finally {
  Pop-Location | Out-Null
}
