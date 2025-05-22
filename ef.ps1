<#
.SYNOPSIS
  Smart EF Core migration utility that auto-detects your project files.

.PARAMETER action
  The EF action: add, remove, update, drop, rollback, reset, status, help

.PARAMETER name
  Name of the migration (used with "add")

.PARAMETER context
  The DbContext to use (optional)
#>

param(
    [ValidateSet("add", "remove", "update", "drop", "rollback", "reset", "status", "help")]
    [string]$action = "help",

    [string]$name = "InitSchema",

    [string]$context = "ApplicationDbContext"
)

# Environment check
$psMajorVersion = $PSVersionTable.PSVersion.Major
$supportsEmoji = $psMajorVersion -ge 7

function Emoji {
    param (
        [string]$symbol,
        [string]$fallback
    )
    if ($supportsEmoji) { return $symbol } else { return $fallback }
}

# Show help
if ($action -eq "help") {
    $title = Emoji "🛠️" "[HELP]"
    Write-Host ""
    Write-Host "$title EF Core Migration Utility Help"
    Write-Host ""
    Write-Host "$(Emoji "📦" "[ADD]")     add        - Adds a new migration (uses -name)"
    Write-Host "$(Emoji "🗑️" "[REMOVE]")  remove     - Removes the last migration"
    Write-Host "$(Emoji "⬆️" "[UPDATE]")  update     - Applies the latest migration to the database"
    Write-Host "$(Emoji "🔥" "[DROP]")    drop       - Drops the entire database"
    Write-Host "$(Emoji "⏪" "[ROLLBACK]") rollback   - Rolls back the last migration"
    Write-Host "$(Emoji "🔁" "[RESET]")   reset      - Drops and reapplies all migrations"
    Write-Host "$(Emoji "📋" "[STATUS]")  status     - Lists applied and available migrations"
    Write-Host ""
    Write-Host "$(Emoji "🔧" " ") Usage:"
    Write-Host "   .\ef.ps1 -action add -name YourMigrationName"
    Write-Host "   .\ef.ps1 -action rollback"
    Write-Host ""
    Write-Host "$(Emoji "✨" " ") Run with '-action help' to see this again."
    Write-Host ""
    exit 0
}

# Check if dotnet-ef is available
$efVersion = & dotnet ef --version 2>$null
if (-not $efVersion) {
    $tag = Emoji "❌" "[ERROR]"
    Write-Host "$tag dotnet-ef is not installed or not found in PATH."
    Write-Host "$(Emoji "➡️" "->")  Install it with: dotnet tool install --global dotnet-ef"
    exit 1
}

# Auto-detect project paths
$infrastructureProj = Get-ChildItem -Recurse -Filter *.Infrastructure.csproj | Select-Object -First 1
$startupProj = Get-ChildItem -Recurse -Filter *.API.csproj | Select-Object -First 1

if (-not $infrastructureProj -or -not $startupProj) {
    $tag = Emoji "❌" "[ERROR]"
    Write-Host "$tag Could not auto-detect project files."
    if (-not $infrastructureProj) { Write-Host "$(Emoji "⚠️" "[WARN]") Missing: *.Infrastructure.csproj" }
    if (-not $startupProj) { Write-Host "$(Emoji "⚠️" "[WARN]") Missing: *.API.csproj" }
    exit 1
}

$project = $infrastructureProj.FullName
$startupProject = $startupProj.FullName

switch ($action) {
    "add" {
        $tag = Emoji "📦" "[ADD]"
        Write-Host "$tag Adding new migration: $name"
        dotnet ef migrations add $name --project $project --startup-project $startupProject --context $context
    }
    "remove" {
        $tag = Emoji "🗑️" "[REMOVE]"
        Write-Host "$tag Removing last migration"
        dotnet ef migrations remove --project $project --startup-project $startupProject --context $context
    }
    "update" {
        $tag = Emoji "⬆️" "[UPDATE]"
        Write-Host "$tag Updating database to latest migration"
        dotnet ef database update --project $project --startup-project $startupProject --context $context
    }
    "drop" {
        $tag = Emoji "🔥" "[DROP]"
        Write-Host "$tag Dropping database"
        dotnet ef database drop --project $project --startup-project $startupProject --context $context --force
    }
    "rollback" {
        $tag = Emoji "⏪" "[ROLLBACK]"
        Write-Host "$tag Rolling back last migration..."

        $migrations = dotnet ef migrations list --project $project --startup-project $startupProject --context $context
        $previousMigration = $migrations | Select-Object -SkipLast 1 -Last 1

        if (-not $previousMigration) {
            $tag = Emoji "❌" "[ERROR]"
            Write-Host "$tag No previous migration found."
        } else {
            $tag = Emoji "➡️" "[REVERT]"
            Write-Host "$tag Reverting to: $previousMigration"
            dotnet ef database update $previousMigration --project $project --startup-project $startupProject --context $context
            dotnet ef migrations remove --project $project --startup-project $startupProject --context $context
        }
    }
    "reset" {
        $tag = Emoji "🔁" "[RESET]"
        Write-Host "$tag Dropping and reapplying database migrations"
        dotnet ef database drop --project $project --startup-project $startupProject --context $context --force
        dotnet ef database update --project $project --startup-project $startupProject --context $context
    }
    "status" {
        $tag = Emoji "📋" "[STATUS]"
        Write-Host "$tag Listing applied and available migrations"
        dotnet ef migrations list --project $project --startup-project $startupProject --context $context
    }
    default {
        $tag = Emoji "❌" "[ERROR]"
        Write-Host "$tag Unknown action: $action"
    }
}
