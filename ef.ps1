<#
.SYNOPSIS
  Utility script for managing EF Core migrations in OakERP.
.DESCRIPTION
  Targets OakERP.Infrastructure as the DbContext project
  and OakERP.WebAPI as the startup project.

.PARAMETER action
  The action to perform: add, remove, update, drop, rollback, reset, status

.PARAMETER name
  The name of the migration (used with "add")
#>

param(
    [ValidateSet("add", "remove", "update", "drop", "rollback", "reset", "status")]
    [string]$action = "add",

    [string]$name = "InitSchema"
)

$contextProject = "OakERP.Infrastructure"
$startupProject = "OakERP.WebAPI"

switch ($action) {
    "add" {
        Write-Host "📦 Adding new migration: $name"
        dotnet ef migrations add $name --project $contextProject --startup-project $startupProject
    }
    "remove" {
        Write-Host "🗑️ Removing last migration"
        dotnet ef migrations remove --project $contextProject --startup-project $startupProject
    }
    "update" {
        Write-Host "⬆️ Updating database to latest migration"
        dotnet ef database update --project $contextProject --startup-project $startupProject
    }
    "drop" {
        Write-Host "🔥 Dropping database"
        dotnet ef database drop --project $contextProject --startup-project $startupProject --force
    }
    "rollback" {
        Write-Host "⏪ Rolling back last migration..."

        $migrations = dotnet ef migrations list --project $contextProject --startup-project $startupProject
        $previousMigration = $migrations | Select-Object -SkipLast 1 -Last 1

        if (-not $previousMigration) {
            Write-Host "❌ No previous migration found. You might only have 1 migration, or none at all."
        } else {
            Write-Host "➡️ Reverting database to previous migration: $previousMigration"
            dotnet ef database update $previousMigration --project $contextProject --startup-project $startupProject
            dotnet ef migrations remove --project $contextProject --startup-project $startupProject
        }
    }
    "reset" {
        Write-Host "🔁 Dropping and reapplying database migrations"
        dotnet ef database drop --project $contextProject --startup-project $startupProject --force
        dotnet ef database update --project $contextProject --startup-project $startupProject
    }
    "status" {
        Write-Host "📋 Listing applied and available migrations"
        dotnet ef migrations list --project $contextProject --startup-project $startupProject
    }
    default {
        Write-Host "❌ Unknown action: $action"
    }
}
