[CmdletBinding()]
param(
    [string] $SourceSvg = (Join-Path $PSScriptRoot 'logo.svg'),
    [int[]] $Sizes = @(16, 32, 48, 64, 128, 180, 192, 256, 400, 512, 1024),
    [string] $Inkscape = 'inkscape'
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $SourceSvg)) {
    throw "Source SVG not found: $SourceSvg"
}

if (-not (Get-Command $Inkscape -ErrorAction SilentlyContinue)) {
    throw "Inkscape is required to generate the logo PNG assets, but the command '$Inkscape' was not found. Install Inkscape and make sure it is available on PATH as 'inkscape', or run this script with -Inkscape set to the full path of the Inkscape executable."
}

$families = @(
    @{
        OutputPath      = Join-Path $PSScriptRoot 'generated\logo'
        UseWhite        = $false
        UseNoMarginCrop = $false
    },
    @{
        OutputPath      = Join-Path $PSScriptRoot 'generated-white-background\logo'
        UseWhite        = $true
        UseNoMarginCrop = $false
    },
    @{
        OutputPath      = Join-Path $PSScriptRoot 'generated-no-margin\logo'
        UseWhite        = $false
        UseNoMarginCrop = $true
    },
    @{
        OutputPath      = Join-Path $PSScriptRoot 'generated-white-background-no-margin\logo'
        UseWhite        = $true
        UseNoMarginCrop = $true
    }
)

foreach ($family in $families) {
    New-Item -ItemType Directory -Force -Path $family.OutputPath | Out-Null

    foreach ($size in $Sizes) {
        $outputFile = Join-Path $family.OutputPath "logo-$size.png"
        $inkscapeArgs = @(
            $SourceSvg,
            '--export-type=png',
            "--export-width=$size",
            "--export-height=$size",
            "--export-filename=$outputFile"
        )

        if ($family.UseNoMarginCrop) {
            $inkscapeArgs += '--export-area=36:36:476:476'
        }

        if ($family.UseWhite) {
            $inkscapeArgs += '--export-background=#ffffff'
            $inkscapeArgs += '--export-background-opacity=1'
        }

        & $Inkscape @inkscapeArgs
        if ($LASTEXITCODE -ne 0) {
            throw "Inkscape failed while generating $outputFile"
        }
    }
}
