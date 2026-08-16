[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
Set-Location $repo

dotnet restore .\ToastifyModern.sln
dotnet build .\ToastifyModern.sln -c $Configuration --no-restore
