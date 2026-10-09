param([string]$UnityExe = 'C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe')
$ErrorActionPreference = 'Stop'
$labRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $labRoot 'LAB05_PrediccionTrayectoria'
$evidencePath = Join-Path $labRoot 'evidencias\2d'
$logPath = Join-Path $projectPath 'Logs'
if (-not (Test-Path -LiteralPath $UnityExe)) { throw "No se encontro Unity: $UnityExe" }
New-Item -ItemType Directory -Force -Path $logPath,$evidencePath | Out-Null
Write-Host 'Se regeneran las dos escenas del laboratorio. Conserva tus cambios en otra escena antes de ejecutar.'
$buildLog = Join-Path $logPath 'verificar-build.log'
$arguments = '-batchmode -nographics -quit -projectPath "{0}" -executeMethod Lab05ProjectBuilder.BuildAndVerify -logFile "{1}"' -f $projectPath,$buildLog
$build = Start-Process -FilePath $UnityExe -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
if ($build.ExitCode -ne 0) { throw "Unity fallo. Revisa $buildLog" }
Copy-Item -LiteralPath (Join-Path $logPath 'resultado-editor.txt') -Destination (Join-Path $evidencePath 'resultado-editor-2d.txt')
$playerPath = Join-Path $projectPath 'Builds\Windows\LAB05.exe'
$runtimeLog = Join-Path $logPath 'verificar-runtime.log'
$arguments = '-force-d3d11 -screen-width 1440 -screen-height 900 -screen-fullscreen 0 --lab05-verify --lab-output "{0}" -logFile "{1}"' -f $evidencePath,$runtimeLog
$player = Start-Process -FilePath $playerPath -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
if ($player.ExitCode -ne 0) { throw "La verificacion fallo. Revisa $runtimeLog" }
Get-Content -LiteralPath (Join-Path $evidencePath 'resultado-runtime-2d.txt')
Write-Host "Renders reales de la camara y resultados: $evidencePath"
