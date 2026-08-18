$maxWait = 120 # 10 minutes in 5-second intervals
for ($i = 0; $i -lt $maxWait; $i++) {
    Start-Sleep -Seconds 5
    if (Test-Path "C:\Users/franc/Desktop/TileStories\editmode_results.xml") {
        [xml]$results = Get-Content "C:\Users/franc/Desktop/TileStories\editmode_results.xml"
        $total = $results.testsuite.tests
        $failures = $results.testsuite.failures
        $errors = $results.testsuite.errors
        $output = "=== EditMode Test Results ===" + "`n" +
                  "Total tests: $total`n" +
                  "Failures: $failures`n" +
                  "Errors: $errors`n" +
                  "Time: $($results.testsuite.time)`n" +
                  "Result: $(if ($failures -eq 0 -and $errors -eq 0) { 'ALL PASSED' } else { 'FAILURES DETECTED' })"
        $output | Out-File "C:\Users/franc/Desktop/TileStories\__editmode_result.txt"
        Write-Host $output
        exit 0
    }
}
"EditMode test results not found after 10 minutes" | Out-File "C:\Users/franc/Desktop/TileStories\__editmode_result.txt"
