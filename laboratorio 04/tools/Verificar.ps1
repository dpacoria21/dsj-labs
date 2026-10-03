param(
    [string]$UnityExe = 'C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$labRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $labRoot 'LAB04_PursuitEvasion'
$evidencePath = Join-Path $labRoot 'evidencias'
$logPath = Join-Path $projectPath 'Logs'
if (-not (Test-Path -LiteralPath $UnityExe)) { throw "No se encontro Unity: $UnityExe" }
New-Item -ItemType Directory -Force -Path $logPath,$evidencePath | Out-Null
Write-Host 'Se regeneraran las ocho escenas. Guarda antes tus cambios en una escena propia.'
$buildLog = Join-Path $logPath 'verificar-build.log'
$arguments = '-batchmode -nographics -quit -projectPath "{0}" -executeMethod LabProjectBuilder.BuildAndVerify -logFile "{1}"' -f $projectPath,$buildLog
$build = Start-Process -FilePath $UnityExe -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
if ($build.ExitCode -ne 0) { throw "Unity fallo. Revisa $buildLog" }
$playerPath = Join-Path $projectPath 'Builds\Windows\LAB04.exe'
$runtimeLog = Join-Path $logPath 'verificar-runtime.log'
$arguments = '-force-d3d11 -screen-width 1440 -screen-height 900 -screen-fullscreen 0 --lab-verify --lab-output "{0}" -logFile "{1}"' -f $evidencePath,$runtimeLog
$player = Start-Process -FilePath $playerPath -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
if ($player.ExitCode -ne 0) { throw "La prueba fallo. Revisa $runtimeLog" }
Get-Content -LiteralPath (Join-Path $evidencePath 'resultado-runtime.txt')
Write-Host "Capturas y resultados: $evidencePath"
Write-Host 'Verifica visualmente las PNG antes de incorporarlas al informe.'
