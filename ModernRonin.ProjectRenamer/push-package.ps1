param ([parameter(mandatory)]$version)

if (-not $nugetApiKey) {Write-Host "You need to set the variable nugetApiKey"}

dotnet nuget push .\ModernRonin.ProjectRenamer\nupkg\ModernRonin.ProjectRenamer.$version.nupkg -s https://api.nuget.org/v3/index.json --api-key $nugetApiKey