#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot

$Fnpack = Join-Path $Root "tools/fnpack"

if ($IsLinux) {
    chmod +x $Fnpack
}

& $Fnpack build -d (Join-Path $Root "src/fn")
