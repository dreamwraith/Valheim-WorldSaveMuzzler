[CmdletBinding()]
param(
    [string]$PackagePath,

    [string]$Token,

    [string]$Community,

    [string]$Namespace,

    [string[]]$Categories,

    [switch]$DryRun,

    [string]$ProjectDir
)

$ErrorActionPreference = "Stop"

if (-not $ProjectDir) {
    $ProjectDir = if ($PSScriptRoot) { Split-Path $PSScriptRoot -Parent } else { (Get-Location).Path }
}

# 1. Resolve Token
if (-not $Token) {
    $Token = $env:HEXIUM_TOKEN
}
if (-not $Token -and -not $DryRun) {
    if ([Environment]::UserInteractive -and -not $env:CI -and -not $env:GITHUB_ACTIONS) {
        $secureInput = Read-Host -Prompt "Enter Hexium Team API Token (from hexium.gg team settings)" -AsSecureString
        $Token = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureInput))
    }
}
if (-not $Token -and -not $DryRun) {
    Write-Error "No Hexium API token provided. Set the HEXIUM_TOKEN environment variable (or GitHub Secret) or pass -Token."
    exit 1
}

# 2. Resolve Community, Namespace, and Categories
if (-not $Community) {
    $Community = if ($env:HEXIUM_COMMUNITY) { $env:HEXIUM_COMMUNITY } elseif ($env:COMMUNITY) { $env:COMMUNITY } else { "valheim" }
}

if (-not $Namespace) {
    if ($env:HEXIUM_NAMESPACE) { $Namespace = $env:HEXIUM_NAMESPACE }
    elseif ($env:COMMUNITY_NAMESPACE) { $Namespace = $env:COMMUNITY_NAMESPACE }
    elseif ($env:MOD_NAMESPACE) { $Namespace = $env:MOD_NAMESPACE }
    elseif ($env:PUBLISH_NAMESPACE) { $Namespace = $env:PUBLISH_NAMESPACE }
    elseif ($env:NAMESPACE) { $Namespace = $env:NAMESPACE }
    else {
        $csproj = Get-ChildItem -Path $ProjectDir -Filter "*.csproj" | Select-Object -First 1
        if ($csproj) {
            [xml]$xml = Get-Content $csproj.FullName
            $authors = ($xml.Project.PropertyGroup | Where-Object { $_.Authors } | Select-Object -First 1).Authors
            if ($authors) { $Namespace = $authors }
        }
    }
}
if (-not $Namespace -and -not $DryRun) {
    if ([Environment]::UserInteractive -and -not $env:CI -and -not $env:GITHUB_ACTIONS) {
        $Namespace = Read-Host -Prompt "Enter Hexium Author / Team Namespace"
    }
}
if (-not $Namespace -and -not $DryRun) {
    Write-Error "No author/team namespace provided for Hexium. Set HEXIUM_NAMESPACE or COMMUNITY_NAMESPACE environment variable (or GitHub Secret), pass -Namespace, or set <Authors> in .csproj."
    exit 1
}

if (-not $Categories -or $Categories.Count -eq 0) {
    if ($env:HEXIUM_CATEGORIES) {
        $Categories = $env:HEXIUM_CATEGORIES -split ',' | ForEach-Object { $_.Trim() }
    } elseif ($env:MOD_CATEGORIES) {
        $Categories = $env:MOD_CATEGORIES -split ',' | ForEach-Object { $_.Trim() }
    } else {
        $Categories = @("mods")
    }
}

# 3. Locate Package
if (-not $PackagePath) {
    $publishDir = Join-Path $ProjectDir "bin\Publish"
    $latestZip = Get-ChildItem -Path $publishDir -Filter "*.zip" -ErrorAction SilentlyContinue |
                 Where-Object { $_.Name -notlike "*-Source.zip" } |
                 Sort-Object LastWriteTime -Descending |
                 Select-Object -First 1
    if ($latestZip) {
        $PackagePath = $latestZip.FullName
    } else {
        Write-Error "No distribution package found in '$publishDir'. Please run package.ps1 first."
        exit 1
    }
}

if (-not (Test-Path $PackagePath)) {
    Write-Error "Specified package path does not exist: $PackagePath"
    exit 1
}

Write-Host "Validating package for Hexium: $PackagePath" -ForegroundColor Cyan

# 4. Validate ZIP Contents (manifest.json, icon.png, README.md)
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zipArchive = [System.IO.Compression.ZipFile]::OpenRead($PackagePath)
$entryNames = $zipArchive.Entries | ForEach-Object { $_.FullName }
$zipArchive.Dispose()

$missingRequired = @()
foreach ($req in @("manifest.json", "icon.png", "README.md")) {
    if ($entryNames -notcontains $req) {
        $missingRequired += $req
    }
}

if ($missingRequired.Count -gt 0) {
    Write-Error "Package is missing required root files for Hexium: $($missingRequired -join ', ')"
    exit 1
}

# Read manifest from disk or archive
$manifestPath = Join-Path $ProjectDir "manifest.json"
$pkgName = "UnknownMod"
$pkgVersion = "1.0.0"
if (Test-Path $manifestPath) {
    $manifestData = Get-Content $manifestPath -Raw | ConvertFrom-Json
    $pkgName = $manifestData.name
    $pkgVersion = $manifestData.version_number
}

Write-Host "Package verified: $Namespace/$pkgName v$pkgVersion (Community: $Community)" -ForegroundColor Green

if ($DryRun) {
    Write-Host "[DryRun] Package structure valid. Submission payload would be sent to https://hexium.gg/api/experimental/submission/submit/" -ForegroundColor Yellow
    return
}

# 5. Perform Direct API Upload
Write-Host "Uploading $pkgName v$pkgVersion to Hexium..." -ForegroundColor Cyan

$endpoint = "https://hexium.gg/api/experimental/submission/submit/"

$httpClient = [System.Net.Http.HttpClient]::new()
try {
    $httpClient.DefaultRequestHeaders.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $Token)
    $httpClient.Timeout = [TimeSpan]::FromMinutes(5)

    $content = [System.Net.Http.MultipartFormDataContent]::new()

    # File stream
    $fileBytes = [System.IO.File]::ReadAllBytes($PackagePath)
    $fileContent = [System.Net.Http.ByteArrayContent]::new($fileBytes)
    $fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("application/zip")
    $fileName = [System.IO.Path]::GetFileName($PackagePath)
    $content.Add($fileContent, "file", $fileName)

    # Form parameters
    $content.Add([System.Net.Http.StringContent]::new($Namespace), "author")
    $content.Add([System.Net.Http.StringContent]::new($Community), "community")
    $content.Add([System.Net.Http.StringContent]::new("false"), "has_nsfw_content")

    $categoriesJson = ($Categories | ConvertTo-Json -Compress)
    $content.Add([System.Net.Http.StringContent]::new($categoriesJson), "categories")

    $response = $httpClient.PostAsync($endpoint, $content).GetAwaiter().GetResult()
    $responseBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()

    if ($response.IsSuccessStatusCode) {
        Write-Host "`nSuccessfully published to Hexium!" -ForegroundColor Green
        Write-Host "Mod URL: https://$Community.hexium.gg/" -ForegroundColor Cyan
    } else {
        Write-Error "Hexium upload failed with HTTP $([int]$response.StatusCode) ($($response.StatusCode)):`n$responseBody"
        exit 1
    }
} finally {
    $httpClient.Dispose()
}
