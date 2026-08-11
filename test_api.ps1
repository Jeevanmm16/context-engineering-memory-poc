$ErrorActionPreference = "Stop"

Write-Host "Starting API..."
$process = Start-Process dotnet "run --urls http://localhost:5261" -WorkingDirectory "c:\Users\costrategix\context-engineering-memory-poc\src\ContextEngineering.MemoryPOC.Api" -PassThru -WindowStyle Hidden

Write-Host "Waiting for API to boot up (10s)..."
Start-Sleep -Seconds 10

$url = "http://localhost:5261/api/chat"
$headers = @{ "Content-Type" = "application/json" }

try {
    Write-Host "`n[Test 1] Store a semantic memory"
    $body1 = @{ message = "My project uses SQL Server." } | ConvertTo-Json
    $response1 = Invoke-RestMethod -Uri $url -Method Post -Body $body1 -Headers $headers
    Write-Host "--- Response 1 ---"
    $response1 | ConvertTo-Json -Depth 5

    Write-Host "`n[Test 2] Chit chat to push it out of working memory"
    for ($i=2; $i -le 6; $i++) {
        Write-Host "Sending chit chat $i..."
        $bodyChat = @{ message = "This is random chitchat number $i" } | ConvertTo-Json
        Invoke-RestMethod -Uri $url -Method Post -Body $bodyChat -Headers $headers | Out-Null
    }

    Write-Host "`n[Test 3] Recall from Long-Term Memory"
    $body3 = @{ message = "Which database does my project use?" } | ConvertTo-Json
    $response3 = Invoke-RestMethod -Uri $url -Method Post -Body $body3 -Headers $headers
    Write-Host "--- Response 3 ---"
    $response3 | ConvertTo-Json -Depth 5
}
finally {
    Write-Host "`nStopping API process..."
    Stop-Process -Id $process.Id -Force
}
