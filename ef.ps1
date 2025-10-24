<#
.SYNOPSIS
  Smart EF Core migration utility (autodetect + overrides).

.PARAMETER action
  add | remove | update | drop | rollback | reset | status | script | help

.PARAMETER name
  Migration name (for 'add').

.PARAMETER context
  DbContext type name. Default: ApplicationDbContext

.PARAMETER project
  Path to the project that CONTAINS the DbContext (e.g. *.Infrastructure.csproj).

.PARAMETER startup
  Path to the startup project (e.g. *.API.csproj or *.Web*.csproj).

.PARAMETER connection
  Optional connection string override (passed to EF CLI --connection).

.PARAMETER to
  Target migration for 'update' (e.g. 20250101010101_Initial or 0).

.PARAMETER from
  Start migration for 'script' (optional).

.PARAMETER idempotent
  Generate idempotent script (for 'script').

.PARAMETER ignoreChanges
  Adds migration without diffs (for 'add').

.PARAMETER noBuild
  Pass --no-build to dotnet ef.

.PARAMETER verbose
  Pass -v to dotnet ef for verbose logs.
#>

[CmdletBinding()]
param(
  [ValidateSet('add','remove','update','drop','rollback','reset','status','script','help')]
  [string]$action = 'help',

  [string]$name = 'InitSchema',
  [string]$context = 'ApplicationDbContext',

  [string]$project,
  [string]$startup,
  [string]$connection,

  [string]$to,
  [string]$from,
  [switch]$idempotent,

  [switch]$ignoreChanges,
  [switch]$noBuild,
  [switch]$verbose
)

$ErrorActionPreference = 'Stop'
$PSDefaultParameterValues['Out-File:Encoding'] = 'utf8'

$psMajorVersion = $PSVersionTable.PSVersion.Major
$supportsEmoji = $psMajorVersion -ge 7
function Emoji([string]$ok, [string]$fallback){ if($supportsEmoji){$ok}else{$fallback} }
function Section($t){ Write-Host "`n$(Emoji '📦' '[*]') $t`n" -ForegroundColor Cyan }
function Info($t){ Write-Host "$(Emoji '🧭' '[INFO]') $t" }
function Warn($t){ Write-Host "$(Emoji '⚠️' '[WARN]') $t" -ForegroundColor Yellow }
function Err ($t){ Write-Host "$(Emoji '❌' '[ERROR]') $t" -ForegroundColor Red }

if ($action -eq 'help') {
  Write-Host ""
  Write-Host "$(Emoji '🛠️' '[HELP]') EF Core Migration Utility"
  Write-Host ""
  Write-Host "$(Emoji '📦' '[ADD]')     add        - Add migration (-name, [-ignoreChanges])"
  Write-Host "$(Emoji '🗑️' '[REMOVE]')  remove     - Remove last migration (code only)"
  Write-Host "$(Emoji '⬆️' '[UPDATE]')  update     - Update DB ([-to <migration>] [-connection])"
  Write-Host "$(Emoji '🔥' '[DROP]')    drop       - Drop database"
  Write-Host "$(Emoji '⏪' '[ROLLBACK]') rollback   - Roll back last migration (DB + remove)"
  Write-Host "$(Emoji '🔁' '[RESET]')   reset      - Drop & update"
  Write-Host "$(Emoji '📋' '[STATUS]')  status     - List migrations"
  Write-Host "$(Emoji '📜' '[SCRIPT]')  script     - Generate SQL ([-from] [-to] [-idempotent])"
  Write-Host ""
  Write-Host "Common overrides: -project, -startup, -context, -connection, -noBuild, -verbose"
  Write-Host ""
  Write-Host "Examples:"
  Write-Host "  .\ef.ps1 -action add -name Init_GL_AP_AR"
  Write-Host "  .\ef.ps1 -action update -connection `"Host=localhost;Port=5433;...`""
  Write-Host "  .\ef.ps1 -action script -from 0 -to Latest -idempotent > deploy.sql"
  exit 0
}

# Ensure dotnet-ef available
$efVersion = & dotnet ef --version 2>$null
if (-not $efVersion) {
  Err "dotnet-ef is not installed. Install: dotnet tool install --global dotnet-ef"
  exit 1
}

# Autodetect projects unless overridden
function Resolve-Project([string]$pattern){
  Get-ChildItem -Recurse -Filter $pattern -File | Sort-Object FullName | Select-Object -First 1 -ExpandProperty FullName
}

if (-not $project) {
  $project = Resolve-Project '*.Infrastructure.csproj'
  if (-not $project) { $project = Resolve-Project '*.Persistence.csproj' }
  if (-not $project) { $project = Resolve-Project '*.Data.csproj' }
}
if (-not $startup) {
  $startup = Resolve-Project '*.API.csproj'
  if (-not $startup) { $startup = Resolve-Project '*.Web*.csproj' }
  if (-not $startup) { $startup = Resolve-Project '*.Host*.csproj' }
  if (-not $startup) { $startup = $project }
}

if (-not $project -or -not $startup) {
  Err "Could not auto-detect project files."
  if (-not $project) { Warn "Missing: *.Infrastructure.csproj (or override with -project)" }
  if (-not $startup) { Warn "Missing: *.API.csproj / *.Web*.csproj (or override with -startup)" }
  exit 1
}

# Mask password in connection string for display only
function Mask-Conn([string]$cs){
  if (-not $cs) { return $null }
  ($cs -replace '(?i)(Password\s*=\s*)([^;]+)', '${1}****')
}

# Build base args
$common = @('--project', $project, '--startup-project', $startup, '--context', $context)
if ($noBuild) { $common += '--no-build' }
if ($verbose) { $common += '-v' }

# Connection handling (EF Core 9 supports --connection for many commands)
if ($connection) {
  $masked = Mask-Conn $connection
  Info "Connection override: $masked"
}

function Run-EF {
  param([Parameter(Mandatory=$true)][string[]]$Args)
  $full = @('ef') + $Args + $common
  if ($connection) { $full += @('--connection', $connection) }
  & dotnet @full
  if ($LASTEXITCODE -ne 0) { throw "dotnet ef failed. args: $($Args -join ' ')" }
}

Info "Using:"
Write-Host "  Project:  $project"
Write-Host "  Startup:  $startup"
Write-Host "  Context:  $context"

switch ($action) {
  'add' {
    if (-not $name -or $name.Trim().Length -eq 0) { $name = 'InitSchema' }
    $name = ($name -replace '\s+','_')
    Section "Adding migration: $name"
    $args = @('migrations','add',$name)
    if ($ignoreChanges) { $args += '--ignore-changes' }
    Run-EF $args
  }
  'remove' {
    Section "Removing last migration (code only)"
    Run-EF @('migrations','remove')
  }
  'update' {
    Section "Updating database"
    $args = @('database','update')
    if ($to) { $args += $to }
    Run-EF $args
  }
  'drop' {
    Section "Dropping database"
    Run-EF @('database','drop','--force')
  }
  'rollback' {
    Section "Rolling back last migration"
    $list = (& dotnet ef migrations list @common 2>$null) | Where-Object { $_ -and ($_ -notmatch 'Build started') -and ($_ -notmatch 'Build succeeded') }
    if (-not $list -or $list.Count -lt 2) {
      Err "No previous migration found."
      break
    }
    $prev = ($list[$list.Count-2] -replace '^\*?\s*','')
    Info "Reverting to: $prev"
    Run-EF @('database','update', $prev)
    Run-EF @('migrations','remove')
  }
  'reset' {
    Section "Dropping and reapplying all migrations"
    Run-EF @('database','drop','--force')
    Run-EF @('database','update')
  }
  'status' {
    Section "Migrations"
    Run-EF @('migrations','list')
  }
  'script' {
    Section "Generating SQL script"
    $args = @('migrations','script')
    if ($from) { $args += $from }
    if ($to)   { $args += $to }
    if ($idempotent) { $args += '--idempotent' }
    Run-EF $args
  }
}
