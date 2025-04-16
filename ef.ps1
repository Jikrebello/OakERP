<#
.SYNOPSIS
  Utility script for managing EF Core migrations in OakERP.
.DESCRIPTION
  Targets OakERP.Infrastructure as the DbContext project
  and OakERP.WebAPI as the startup project.
#>

param(
    [ValidateSet("add", "remove", "update", "drop")]
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
    default {
        Write-Host "❌ Unknown action"
    }
}
