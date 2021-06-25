.\build.ps1 -ScriptArgs '-sonarurl="https://sonarqube.bnymellon.net"'
$result = $LastExitCode
Write-Host "ExitCode " $result
Exit $result