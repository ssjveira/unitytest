$UnityPath = "C:\Program Files\Unity\Hub\Editor\2019.1.1f1\Editor\Unity.exe"
$BuildOutputPath = "C:\Github\unitytest\UnityTest"
$WinLogPath = "$env:LOCALAPPDATA\Unity\Editor\Editor.log"

# Invoke the Unity Editor in order to build the Unity project
Start-Process -FilePath $UnityPath -NoNewWindow -Wait -ArgumentList "-projectPath $BuildOutputPath -quit -batchMode -executeMethod BuildUtils.CreateBuildWin64"

# Dump Unity log to console (TODO: Get a little bit of the log at a time while polling the state of the process to see if it's still running or not)
Get-Content $WinLogPath

# COMMENT THIS OUT WHEN RUNNING ON BUILD SERVER
Write-Host -NoNewLine 'Press any key to continue...';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');