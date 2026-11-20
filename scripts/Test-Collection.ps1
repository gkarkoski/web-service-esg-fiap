$ErrorActionPreference = 'Stop'
$path = Join-Path $PSScriptRoot '../collections/EsgTrainingManagement.postman_collection.json'
$collection = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
if ($collection.info.schema -ne 'https://schema.getpostman.com/json/collection/v2.1.0/collection.json') { throw 'Unexpected collection schema.' }
if ($collection.auth.bearer[0].value -ne '{{jwt_token}}') { throw 'Bearer variable is missing.' }
if ($collection.item.Count -ne 6) { throw 'Expected six workflow folders.' }
$requests = @($collection.item | ForEach-Object { $_.item })
if ($requests.Count -ne 16) { throw "Expected 16 requests, got $($requests.Count)." }
foreach ($item in $requests) {
    if ($item.request.url -notlike '{{base_url}}/api/*') { throw "Invalid request URL: $($item.name)" }
    if ($item.request.body) { $null = $item.request.body.raw.Replace('{{training_id}}', '1') | ConvertFrom-Json }
}
if (($collection.variable | Where-Object key -eq jwt_token).value -ne '') { throw 'The exported JWT must be empty.' }
Write-Output 'Collection JSON, workflow folders, variables and 16 requests validated. Actual Postman/Insomnia import is a separate check.'
