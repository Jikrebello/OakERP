<#
.SYNOPSIS
  Smart EF Core migration utility that auto-detects your project files, with overrides.

.PARAMETER action
  The EF action: add, remove, update, drop, rollback, reset, status, help

.PARAMETER name
  Name of the migration (used with "add")

.PARAMETER context
  DbContext type name. Default: ApplicationDbContext

.PARAMETER project
  Path to the project that CONTAINS the DbContext (e.g., *.Infrastructure.csproj). Overrides autodetect.

.PARAMETER startup
  Path to the startup project (e.g., *.API.csproj or *.Web*.csproj). Overrides autodetect.
#>

[CmdletBinding()]
param(
  [ValidateSet('add','remove','update','drop','rollback','reset','status','help')]
  [string]$action = 'help',

  [string]$name = 'InitSchema',

  [string]$context = 'ApplicationDbContext',

  [string]$project,

  [string]$startup
)

$PSDefaultParameterValues['Out-File:Encoding']='utf8'

$psMajorVersion = $PSVersionTable.PSVersion.Major
$supportsEmoji = $psMajorVersion -ge 7
function Emoji([string]$ok, [string]$fallback){ if($supportsEmoji){$ok}else{$fallback} }

function Write-Section($t){ Write-Host "`n$(Emoji '📦' '[*]') $t`n" }

if ($action -eq 'help') {
  Write-Host ""
  Write-Host "$(Emoji '🛠️' '[HELP]') EF Core Migration Utility"
  Write-Host ""
  Write-Host "$(Emoji '📦' '[ADD]')     add        - Adds a new migration (uses -name)"
  Write-Host "$(Emoji '🗑️' '[REMOVE]')  remove     - Removes the last migration"
  Write-Host "$(Emoji '⬆️' '[UPDATE]')  update     - Applies the latest migration to the database"
  Write-Host "$(Emoji '🔥' '[DROP]')    drop       - Drops the entire database"
  Write-Host "$(Emoji '⏪' '[ROLLBACK]') rollback   - Rolls back the last migration"
  Write-Host "$(Emoji '🔁' '[RESET]')   reset      - Drops and reapplies all migrations"
  Write-Host "$(Emoji '📋' '[STATUS]')  status     - Lists applied and available migrations"
  Write-Host ""
  Write-Host "$(Emoji '🔧' ' ') Usage:"
  Write-Host "   .\ef.ps1 -action add -name YourMigrationName"
  Write-Host "   .\ef.ps1 -action update"
  Write-Host "   .\ef.ps1 -action rollback"
  Write-Host ""
  Write-Host "Overrides:"
  Write-Host "   -project <path to *.csproj with DbContext>"
  Write-Host "   -startup <path to startup *.csproj>"
  Write-Host "   -context <DbContextTypeName>"
  Write-Host ""
  exit 0
}

# Ensure dotnet-ef exists
$efVersion = & dotnet ef --version 2>$null
if (-not $efVersion) {
  Write-Host "$(Emoji '❌' '[ERROR]') dotnet-ef is not installed."
  Write-Host "Install: dotnet tool install --global dotnet-ef"
  exit 1
}

# Autodetect projects unless overridden
function Resolve-Project([string]$pattern){
  $matches = Get-ChildItem -Recurse -Filter $pattern -File | Sort-Object FullName
  if($matches.Count -ge 1){ return $matches[0].FullName }
  return $null
}

if (-not $project) {
  # Try common names for the project that contains the DbContext
  $project = Resolve-Project '*.Infrastructure.csproj'
  if (-not $project) { $project = Resolve-Project '*.Persistence.csproj' }
  if (-not $project) { $project = Resolve-Project '*.Data.csproj' }
}

if (-not $startup) {
  # Try common web/API startup names
  $startup = Resolve-Project '*.API.csproj'
  if (-not $startup) { $startup = Resolve-Project '*.Web*.csproj' }
  if (-not $startup) { $startup = Resolve-Project '*.Host*.csproj' }
  if (-not $startup) { $startup = $project } # fallback: same as project
}

if (-not $project -or -not $startup) {
  Write-Host "$(Emoji '❌' '[ERROR]') Could not auto-detect project files."
  if (-not $project) { Write-Host "$(Emoji '⚠️' '[WARN]') Missing: *.Infrastructure.csproj (or override with -project)" }
  if (-not $startup) { Write-Host "$(Emoji '⚠️' '[WARN]') Missing: *.API.csproj / *.Web*.csproj (or override with -startup)" }
  exit 1
}

Write-Host "$(Emoji '🧭' '[INFO]') Using:"
Write-Host "  Project:  $project"
Write-Host "  Startup:  $startup"
Write-Host "  Context:  $context"

# Helper to run dotnet-ef with common args
function Run-EF {
  param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]] $EfArgs
  )
  & dotnet ef @EfArgs --project $project --startup-project $startup --context $context
  if ($LASTEXITCODE -ne 0) { throw "dotnet ef failed. args: $EfArgs" }
}

switch ($action) {
  'add' {
    if (-not $name -or $name.Trim().Length -eq 0) { $name = 'InitSchema' }
    $name = ($name -replace '\s+','_')
    Write-Host "$(Emoji '📦' '[ADD]') Adding new migration: $name"
    Run-EF migrations add $name
  }
  'remove' {
    Write-Host "$(Emoji '🗑️' '[REMOVE]') Removing last migration (code only)"
    Run-EF migrations remove
  }
  'update' {
    Write-Host "$(Emoji '⬆️' '[UPDATE]') Updating database to latest migration"
    Run-EF database update
  }
  'drop' {
    Write-Host "$(Emoji '🔥' '[DROP]') Dropping database"
    Run-EF database drop --force
  }
  'rollback' {
    Write-Host "$(Emoji '⏪' '[ROLLBACK]') Rolling back last migration"
    $list = & dotnet ef migrations list --project $project --startup-project $startup --context $context 2>$null
    if ($LASTEXITCODE -ne 0) { throw "Failed to list migrations." }
    $lines = $list | Where-Object { $_ -and ($_ -notmatch 'Build started') -and ($_ -notmatch 'Build succeeded') }
    if ($lines.Count -lt 2) {
      Write-Host "$(Emoji '❌' '[ERROR]') No previous migration found."
    } else {
      $prev = $lines[$lines.Count-2] -replace '^\*?\s*',''
      Write-Host "$(Emoji '➡️' '[REVERT]') Reverting to: $prev"
      Run-EF database update $prev
      Run-EF migrations remove
    }
  }
  'reset' {
    Write-Host "$(Emoji '🔁' '[RESET]') Dropping and reapplying all migrations"
    Run-EF database drop --force
    Run-EF database update
  }
  'status' {
    Write-Host "$(Emoji '📋' '[STATUS]') Migrations:"
    Run-EF migrations list
  }
}

