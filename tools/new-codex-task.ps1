param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$TaskName,

    [switch]$Force
)

$ErrorActionPreference = 'Stop'

function Convert-ToSlug {
    param([Parameter(Mandatory = $true)][string]$Value)

    $slug = $Value.ToLowerInvariant()
    $slug = $slug -replace '[^a-z0-9\-_ ]', ''
    $slug = $slug -replace '\s+', '-'
    $slug = $slug -replace '-{2,}', '-'
    $slug = $slug.Trim('-')

    if ([string]::IsNullOrWhiteSpace($slug)) {
        throw "Task name '$Value' could not be converted into a valid folder name."
    }

    return $slug
}

function Ensure-Directory {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
        Write-Host "Created directory: $Path"
    }
}

function Write-Utf8NoBomFile {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Content,
        [switch]$Force
    )

    if ((Test-Path -LiteralPath $Path) -and (-not $Force)) {
        Write-Host "Skipped existing file: $Path"
        return
    }

    $parent = Split-Path -Parent $Path
    if ($parent) {
        Ensure-Directory -Path $parent
    }

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $Content, $utf8NoBom)
    Write-Host "Wrote file: $Path"
}

$toolRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $toolRoot

$templatesRoot = Join-Path $repoRoot 'docs\ai\tasks\templates'
$activeRoot = Join-Path $repoRoot 'docs\ai\tasks\active'

if (-not (Test-Path -LiteralPath $templatesRoot)) {
    throw "Task templates folder not found: $templatesRoot"
}

Ensure-Directory -Path $activeRoot

$slug = Convert-ToSlug -Value $TaskName
$taskRoot = Join-Path $activeRoot $slug
Ensure-Directory -Path $taskRoot

$templateFiles = @(
    'task_plan.md',
    'findings.md',
    'progress.md'
)

$now = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
$taskTitle = $TaskName.Trim()

foreach ($fileName in $templateFiles) {
    $templatePath = Join-Path $templatesRoot $fileName
    $targetPath = Join-Path $taskRoot $fileName

    if (-not (Test-Path -LiteralPath $templatePath)) {
        throw "Template file not found: $templatePath"
    }

    $content = Get-Content -LiteralPath $templatePath -Raw
    $content = $content.Replace('<replace-me>', $taskTitle)
    $content = $content.Replace('<date/time>', $now)

    Write-Utf8NoBomFile -Path $targetPath -Content $content -Force:$Force
}

Write-Host ''
Write-Host "Task scaffold ready:"
Write-Host "  $taskRoot"
Write-Host ''
Write-Host 'Files:'
Write-Host '  - task_plan.md'
Write-Host '  - findings.md'
Write-Host '  - progress.md'
Write-Host ''
Write-Host 'Usage examples:'
Write-Host '  .\tools\new-codex-task.ps1 auth-gateway-cleanup'
Write-Host '  .\tools\new-codex-task.ps1 config-cleanup -Force'