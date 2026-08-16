[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
Set-Location $repo

dotnet restore .\ToastifyReloaded.sln
dotnet build .\ToastifyReloaded.sln -c $Configuration --no-restore
