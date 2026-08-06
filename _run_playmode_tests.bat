@echo off
REM Run Unity PlayMode tests in batchmode
"C:\Program Files\Unity\Hub\Editor\6000.3.19f1\Editor\Unity.exe" -batchmode -nographics -projectPath "c:\Users\franc\Desktop\TileStories" -runTests -testPlatform PlayMode -testResults "c:\Users\franc\Desktop\TileStories\playmode_results.xml" -logFile "c:\Users\franc\Desktop\TileStories\playmode_log.txt" -quit
exit /b %ERRORLEVEL%
