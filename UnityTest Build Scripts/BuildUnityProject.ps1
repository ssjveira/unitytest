# Default values for Unity 3D build parameters
$unityEditorPath = "C:\Program Files\Unity\Hub\Editor\2019.1.1f1\Editor\Unity.exe"
$buildDirectory = "C:\Github\unitytest\UnityTest"
$logPath = "$env:LOCALAPPDATA\Unity\Editor\Editor.log"
$buildMethod = "BuildUtils.CreateBuildWin64"
$buildPipelineName = "Unity Build";

# Get the unity editor path
if(Test-Path env:UNITY_EDITOR_PATH) {
    $unityEditorPath = $env:UNITY_EDITOR_PATH
}

# Get build directory
if(Test-Path env:BUILD_SOURCESDIRECTORY) {
    $buildDirectory = $env:BUILD_SOURCESDIRECTORY
}

# Get the build method
if(Test-Path env:UNITY_BUILD_METHOD) {
    $buildMethod = $env:UNITY_BUILD_METHOD
}

# Get the build pipeline name
if(Test-Path env:BUILD_DEFINITIONNAME) {
    $buildPipelineName = $env:BUILD_DEFINITIONNAME
}

# Launch the Unity Editor (HEADLESS MODE) in order to build the Unity project
Write-Host "Starting to build the Unity project for build pipeline '${buildPipelineName}'`r`n"

# Creating an array of build jobs
$buildJob = Start-Job -ScriptBlock { & $unityEditorPath -projectPath $buildDirectory -quit -batchMode -executeMethod $buildMethod }

# Output Unity Editor log
$complete = $false
$currentLine = 0
do {

    # Sleep for one second (To prevent spamming when reading out editor log)
    sleep -Seconds 1

    # Output new lines from log
    $lines = Get-Content -Path $logPath
    $newLineCount = $lines.Length - $currentLine

    if($newLineCount -gt 0) {
        $lines[$currentLine..($lines.Length-1)]
    }

    $currentLine = $lines.Length

    # Check if job is still running
    if ($buildJob.State -notmatch 'running') { 
        Write-Host "`r`n`r`nFinished building the Unity project for build pipeline '${buildPipelineName}'`r`n"
        $complete = $true 
    }
}
while(-not $complete)

# Don't wait for user input if we are running on a server
if(-Not (Test-Path env:SERVER)) {
    Write-Host -NoNewLine 'Press any key to continue...';
    $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');
}