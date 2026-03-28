<#  OakERP - Migrate + Seed
    Usage (local):
      pwsh ./tools/migrate.ps1                          # Development, recommended operational path: migrator runs migrate+seed
      pwsh ./tools/migrate.ps1 -Env Testing             # Testing env
      pwsh ./tools/migrate.ps1 -Env Production          # Production-like (e.g., in Docker)
      pwsh ./tools/migrate.ps1 -UseEf                   # First run dotnet ef database update, then seed
      pwsh ./tools/migrate.ps1 -MigrateOnly -UseEf      # Only EF migration (no seeding)
      pwsh ./tools/migrate.ps1 -SeedOnly                # Only seed (migrator; note: migrator still calls MigrateAsync safely)
      pwsh ./tools/migrate.ps1 -Cs "Host=...;..."       # Override connection string via env var

    Ownership notes:
      - MigrationTool is the explicit operational schema migration path.
      - API startup may still seed when RunSeedOnStartup=true; that behavior is preserved and is not the primary migration path.
      - -UseEf remains available as a tooling option, but it does not replace MigrationTool ownership.
#>

param(
  [ValidateSet('Development','Testing','Production')]
  [string]$Env = 'Development',

  # If true, run `dotnet ef database update` before seeding
  [switch]$UseEf,

  # If true, _only_ run EF migration step (implies -UseEf)
  [switch]$MigrateOnly,

  # If true, run only seeding (via migrator) and skip EF
  # (Note: migrator will still call MigrateAsync idempotently)
  [switch]$SeedOnly,

  # Optional connection string override; sets ConnectionStrings__DefaultConnection
  [string]$Cs = $null,

  # Paths (adjust for your repo if different)
  [string]$InfraProj    = 'OakERP.Infrastructure/OakERP.Infrastructure.csproj',
  [string]$ApiProj      = 'OakERP.API/OakERP.API.csproj',
  [string]$MigratorProj = 'OakERP.MigrationTool/OakERP.MigrationTool.csproj',

  # Build controls
  [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
$PSStyle.OutputRendering = 'Host'  # nicer colors locally

function Info($m)  { Write-Host "[INFO ] $m" -ForegroundColor Cyan }
function Warn($m)  { Write-Host "[WARN ] $m" -ForegroundColor Yellow }
function ErrorMsg($m){ Write-Host "[ERROR] $m" -ForegroundColor Red }

try {
  # Ensure we run from repo root
  $repoRoot = (git rev-parse --show-toplevel 2>$null)
  if (-not $repoRoot) { $repoRoot = (Get-Location) }
  Push-Location $repoRoot

  # Set environment for both EF & Migrator
  $env:ASPNETCORE_ENVIRONMENT = $Env
  if ($Cs) {
    $env:ConnectionStrings__DefaultConnection = $Cs
    Info "Using connection string from -Cs override."
  } else {
    Remove-Item Env:\ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue
  }

  Info "Environment: $Env"
  if ($SeedOnly -and $MigrateOnly) { throw "Choose either -SeedOnly or -MigrateOnly, not both." }

  # Optionally run EF migrations (using API as startup so it loads the same appsettings)
  if ($UseEf -or $MigrateOnly) {
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
    Info "Skipping EF update (migrator will run MigrateAsync idempotently)."
  }

  # Run the migrator (migrate + seed)
  if ($SeedOnly) {
    Info "Running migrator (seed-only requested, note: migrator still calls MigrateAsync safely)."
  } else {
    Info "Running migrator (migrate + seed)..."
  }

  $noBuildRun = $NoBuild.IsPresent ? '--no-build' : ''
  dotnet run --project $MigratorProj --configuration Release $noBuildRun

  if ($LASTEXITCODE -ne 0) {
  throw "Migrator failed with exit code $LASTEXITCODE."
}


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
