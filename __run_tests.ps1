$ErrorActionPreference = "Continue"
$projectPath = "C:\Users\franc\Desktop\TileStories\TileStories"
$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.19f1\Editor\Unity.exe"
$markerDir = "C:\Users\franc\Desktop\TileStories"

# Step 1: Compile check
Write-Host "STEP 1: Compile check..."
& $unityPath -batchmode -nographics -projectPath $projectPath -quit -logFile "$markerDir\compile_log.txt"
$compileExit = $LASTEXITCODE
$compileLog = Get-Content "$markerDir\compile_log.txt" -ErrorAction SilentlyContinue
$compileErrors = $compileLog | Select-String "error CS"
$compileOk = ($compileExit -eq 0) -or ($null -eq $compileErrors -or $compileErrors.Count -eq 0)
Write-Host "Compile exit code: $compileExit"
Write-Host "Compile errors found: $(if($compileErrors){$compileErrors.Count}else{'0'})"
Write-Output "COMPILE_OK=$compileOk" | Out-File "$markerDir\__test_result_step1.txt"

# Step 2: Run EditMode tests
Write-Host "STEP 2: EditMode tests..."
& $unityPath -batchmode -nographics -projectPath $projectPath -runTests -testPlatform EditMode -testResults "$markerDir\editmode_results.xml" -logFile "$markerDir\editmode_log.txt" -quit
$editExit = $LASTEXITCODE
Write-Host "EditMode tests exit code: $editExit"
Write-Output "EDITMODE_EXIT=$editExit" | Out-File "$markerDir\__test_result_step2.txt"

# Step 3: Run PlayMode tests
Write-Host "STEP 3: PlayMode tests..."
& $unityPath -batchmode -nographics -projectPath $projectPath -runTests -testPlatform PlayMode -testResults "$markerDir\playmode_results.xml" -logFile "$markerDir\playmode_log.txt" -quit
$playExit = $LASTEXITCODE
Write-Host "PlayMode tests exit code: $playExit"
Write-Output "PLAYMODE_EXIT=$playExit" | Out-File "$markerDir\__test_result_step3.txt"

Write-Host "ALL DONE"
