$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

$outputFile = 'project-structure.txt'

$ignoreDirs = @(
  '.git',
  '.vs',
  '.idea',
  '.vscode',
  '.husky',
  'node_modules',
  'bin',
  'obj',
  'dist',
  'build',
  'coverage',
  'TestResults',
  'artifacts'
)

$ignoreFiles = @(
  '.DS_Store',
  'Thumbs.db',
  'package-lock.json',
  'project-structure.txt',
  'project_structure.txt'
)

$ignoreFilePatterns = @(
  '*.user',
  '*.rsuser',
  '*.suo',
  '*.coverage',
  '*.coveragexml'
)

function Should-SkipItem {
  param(
    [Parameter(Mandatory = $true)]
    [System.IO.FileSystemInfo]$Item
  )

  $name = $Item.Name

  if ($Item.PSIsContainer) {
    return $ignoreDirs -contains $name
  }

  if ($ignoreFiles -contains $name) {
    return $true
  }

  foreach ($pattern in $ignoreFilePatterns) {
    if ($name -like $pattern) {
      return $true
    }
  }

  return $false
}

function Show-Tree {
  param(
    [string]$Path = '.',
    [string]$Prefix = ''
  )

  $items = Get-ChildItem -LiteralPath $Path -Force -ErrorAction Stop |
  Where-Object { -not (Should-SkipItem -Item $_) } |
  Sort-Object @{ Expression = { -not $_.PSIsContainer } }, Name

  for ($i = 0; $i -lt $items.Count; $i++) {
    $item = $items[$i]
    $isLast = $i -eq ($items.Count - 1)

    $branch = if ($isLast) { '\-- ' } else { '+-- ' }
    "$Prefix$branch$($item.Name)"

    if ($item.PSIsContainer) {
      $nextPrefix = if ($isLast) { "$Prefix    " } else { "$Prefix|   " }
      Show-Tree -Path $item.FullName -Prefix $nextPrefix
    }
  }
}

@(
  (Split-Path -Leaf (Get-Location))
  Show-Tree
) | Set-Content -LiteralPath (Join-Path $scriptDir $outputFile) -Encoding utf8

Write-Host "Created $outputFile in $scriptDir"