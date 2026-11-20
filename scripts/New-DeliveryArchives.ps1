$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$output = Join-Path $root 'artifacts/delivery'
New-Item -ItemType Directory -Force -Path $output | Out-Null
$tracked = @(git -C $root ls-files)
if ($LASTEXITCODE -ne 0) { throw 'Cannot enumerate tracked source files.' }
$files = @($tracked | Where-Object { $_ -notmatch '(^|/)(bin|obj|\.git|\.idea|\.vs|artifacts)/|(^|/)AGENTS\.md$|(^|/)\.env|\.Local\.json$|secrets\.json$|\.(zip|pfx|p12|key)$' })
$collection = 'collections/EsgTrainingManagement.postman_collection.json'
$sourceZip = Join-Path $output 'EsgTrainingManagement.Source.zip'
$collectionZip = Join-Path $output 'EsgTrainingManagement.Collection.zip'

function Write-Archive([string]$destination, [string[]]$entries) {
    $stream = [System.IO.File]::Open($destination, [System.IO.FileMode]::Create)
    $archive = [System.IO.Compression.ZipArchive]::new($stream, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($entry in $entries) {
            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, (Join-Path $root $entry), $entry) | Out-Null
        }
    } finally { $archive.Dispose(); $stream.Dispose() }
}

foreach ($required in @('web-service-esg-fiap.sln', 'Dockerfile', '.dockerignore', 'README.md', 'src/EsgTrainingManagement.Api/EsgTrainingManagement.Api.csproj', 'tests/EsgTrainingManagement.Tests/EsgTrainingManagement.Tests.csproj', $collection)) {
    if ($required -notin $files) { throw "Missing tracked delivery file: $required" }
}
if (-not ($files -match 'Data/Migrations/.*InitialCreate.cs$')) { throw 'Initial Oracle migration is missing.' }
Write-Archive $sourceZip $files
Write-Archive $collectionZip @($collection)
foreach ($zip in @($sourceZip, $collectionZip)) {
    $archive = [System.IO.Compression.ZipFile]::OpenRead($zip)
    try {
        if ($archive.Entries.Count -eq 0) { throw "Empty archive: $zip" }
        $listing = @($archive.Entries | ForEach-Object { '{0} ({1} bytes)' -f $_.FullName, $_.Length })
        $listing | Set-Content -LiteralPath ($zip + '.contents.txt')
        Write-Output "$zip : $($archive.Entries.Count) entries"
        $listing | Write-Output
    } finally { $archive.Dispose() }
}
