<#
.SYNOPSIS
  Smart EF Core migration utility that auto-detects your project files.

.PARAMETER action
  The EF action: add, remove, update, drop, rollback, reset, status

.PARAMETER name
  Name of the migration (used with "add")

.PARAMETER context
  The DbContext to use (optional)
#>

param(
    [ValidateSet("add", "remove", "update", "drop", "rollback", "reset", "status")]
    [string]$action = "add",

    [string]$name = "InitSchema",

    [string]$context = "AppDbContext"
)

# Auto-detect project paths
$infrastructureProj = Get-ChildItem -Recurse -Filter *.Infrastructure.csproj | Select-Object -First 1
$startupProj = Get-ChildItem -Recurse -Filter *.API.csproj | Select-Object -First 1

if (-not $infrastructureProj -or -not $startupProj) {
    Write-Host "❌ Could not auto-detect project files."
    if (-not $infrastructureProj) { Write-Host "⚠️ Missing: *.Infrastructure.csproj" }
    if (-not $startupProj) { Write-Host "⚠️ Missing: *.API.csproj" }
    exit 1
}

$project = $infrastructureProj.FullName
$startupProject = $startupProj.FullName

switch ($action) {
    "add" {
        Write-Host "📦 Adding new migration: $name"
        dotnet ef migrations add $name --project $project --startup-project $startupProject --context $context
    }
    "remove" {
        Write-Host "🗑️ Removing last migration"
        dotnet ef migrations remove --project $project --startup-project $startupProject --context $context
    }
    "update" {
        Write-Host "⬆️ Updating database to latest migration"
        dotnet ef database update --project $project --startup-project $startupProject --context $context
    }
    "drop" {
        Write-Host "🔥 Dropping database"
        dotnet ef database drop --project $project --startup-project $startupProject --context $context --force
    }
    "rollback" {
        Write-Host "⏪ Rolling back last migration..."

        $migrations = dotnet ef migrations list --project $project --startup-project $startupProject --context $context
        $previousMigration = $migrations | Select-Object -SkipLast 1 -Last 1

        if (-not $previousMigration) {
            Write-Host "❌ No previous migration found."
        } else {
            Write-Host "➡️ Reverting to: $previousMigration"
            dotnet ef database update $previousMigration --project $project --startup-project $startupProject --context $context
            dotnet ef migrations remove --project $project --startup-project $startupProject --context $context
        }
    }
    "reset" {
        Write-Host "🔁 Dropping and reapplying database migrations"
        dotnet ef database drop --project $project --startup-project $startupProject --context $context --force
        dotnet ef database update --project $project --startup-project $startupProject --context $context
    }
    "status" {
        Write-Host "📋 Listing applied and available migrations"
        dotnet ef migrations list --project $project --startup-project $startupProject --context $context
    }
    default {
        Write-Host "❌ Unknown action: $action"
    }
}
