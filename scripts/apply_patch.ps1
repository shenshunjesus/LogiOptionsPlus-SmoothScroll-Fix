param(
    [ValidateSet("global", "restore", "status")]
    [string]$Action = "global"
)

$ErrorActionPreference = "Stop"

# Resolve directories dynamically based on script location
$ScriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

# 1. Determine ExePath (Prefer existing pre-compiled binary in script dir, or bin folder)
if (Test-Path (Join-Path $ScriptDir "LogiSmoothScroll.exe")) {
    $ExePath = Join-Path $ScriptDir "LogiSmoothScroll.exe"
} elseif (Test-Path (Join-Path $ProjectRoot "bin\LogiSmoothScroll.exe")) {
    $ExePath = Join-Path $ProjectRoot "bin\LogiSmoothScroll.exe"
} else {
    $BinDir = Join-Path $ProjectRoot "bin"
    if (-not (Test-Path $BinDir)) { New-Item -ItemType Directory -Path $BinDir -Force | Out-Null }
    $ExePath = Join-Path $BinDir "LogiSmoothScroll.exe"
}

# 2. Determine CsPath if compilation is ever needed
if (Test-Path (Join-Path $ScriptDir "LogiSmoothScroll.cs")) {
    $CsPath = Join-Path $ScriptDir "LogiSmoothScroll.cs"
} elseif (Test-Path (Join-Path $ProjectRoot "src\LogiSmoothScroll.cs")) {
    $CsPath = Join-Path $ProjectRoot "src\LogiSmoothScroll.cs"
} else {
    $CsPath = Join-Path $ProjectRoot "LogiSmoothScroll.cs"
}

$RunKey  = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
$RunName = "LogiSmoothScrollEngine"

function Ensure-Daemon-Ready {
    if (Test-Path $ExePath) {
        Write-Host "[1/3] Using pre-compiled LogiSmoothScroll.exe..." -ForegroundColor Green
        return
    }

    if (-not (Test-Path $CsPath)) {
        throw "Neither pre-compiled LogiSmoothScroll.exe nor source LogiSmoothScroll.cs was found in $ScriptDir or $ProjectRoot"
    }

    Write-Host "[1/3] Compiling LogiSmoothScroll.cs from source..." -ForegroundColor Cyan
    $csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
    if (-not (Test-Path $csc)) {
        throw "Cannot find .NET Framework 64-bit compiler at $csc"
    }

    Get-Process -Name "LogiSmoothScroll" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 300
    & $csc /nologo /target:winexe /optimize+ /out:$ExePath $CsPath
    if ($LASTEXITCODE -ne 0) {
        throw "Compilation of LogiSmoothScroll.cs failed."
    }
}

if ($Action -eq "global") {
    Ensure-Daemon-Ready

    Write-Host "[2/3] Patching running logioptionsplus_agent.exe in RAM..." -ForegroundColor Cyan
    & $ExePath --once
    Invoke-WmiMethod -Class Win32_Process -Name Create -ArgumentList "`"$ExePath`"" | Out-Null

    Write-Host "[3/3] Registering auto-start in HKCU Run..." -ForegroundColor Cyan
    Set-ItemProperty -Path $RunKey -Name $RunName -Value "`"$ExePath`""
    Write-Host "[OK] Global Smooth Scrolling & SmartShift Ratchet RAM Patch is active and persistent!" -ForegroundColor Green
}
elseif ($Action -eq "restore") {
    Write-Host "[1/2] Stopping RAM Patcher Daemon and removing auto-start..." -ForegroundColor Yellow
    Get-Process -Name "LogiSmoothScroll" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Remove-ItemProperty -Path $RunKey -Name $RunName -ErrorAction SilentlyContinue

    if (Test-Path $ExePath) {
        Write-Host "[2/2] Restoring original behavior in logioptionsplus_agent.exe RAM..." -ForegroundColor Yellow
        & $ExePath --restore
    }
    Write-Host "[OK] Restored default Logi Options+ behavior." -ForegroundColor Green
}
elseif ($Action -eq "status") {
    $agent  = Get-Process -Name "logioptionsplus_agent" -ErrorAction SilentlyContinue
    $daemon = Get-Process -Name "LogiSmoothScroll" -ErrorAction SilentlyContinue
    Write-Host "logioptionsplus_agent PID(s): $(if ($agent) { ($agent.Id -join ', ') } else { 'Not running' })"
    Write-Host "LogiSmoothScroll Daemon PID : $(if ($daemon) { ($daemon.Id -join ', ') } else { 'Not running' })"
}
