param()

$ErrorActionPreference = 'Stop'

Write-Host 'Restoring local tools...'
dotnet tool restore

Write-Host 'Checking formatting with CSharpier...'
dotnet csharpier check .

Write-Host 'Restoring API project...'
dotnet restore OakERP.API/OakERP.API.csproj

Write-Host 'Restoring unit test project...'
dotnet restore OakERP.Tests.Unit/OakERP.Tests.Unit.csproj

Write-Host 'Restoring integration test project...'
dotnet restore OakERP.Tests.Integration/OakERP.Tests.Integration.csproj

Write-Host 'Building API project...'
dotnet build OakERP.API/OakERP.API.csproj --no-restore

Write-Host 'Running unit tests...'
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore --verbosity normal

Write-Host 'Running integration tests...'
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore --verbosity normal

Write-Host 'PR validation completed successfully.'
