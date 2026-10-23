param(
    [Parameter(Mandatory = $true)]
    [string]$PlantUmlJar
)

$ErrorActionPreference = 'Stop'
$jarPath = (Resolve-Path -LiteralPath $PlantUmlJar).Path
$sourcePath = Join-Path $PSScriptRoot 'fontes'
$imagePath = Join-Path $PSScriptRoot 'imagens'
New-Item -ItemType Directory -Force -Path $imagePath | Out-Null
$sources = @(Get-ChildItem -LiteralPath $sourcePath -Filter '*.puml' | Sort-Object Name)
if ($sources.Count -eq 0) { throw 'Nenhuma fonte PlantUML encontrada.' }

foreach ($format in @('svg', 'png')) {
    & java '-Djava.awt.headless=true' -jar $jarPath -charset UTF-8 "-t$format" -o $imagePath @($sources.FullName)
    if ($LASTEXITCODE -ne 0) { throw "PlantUML falhou ao gerar $format." }
    foreach ($source in $sources) {
        $output = Join-Path $imagePath ($source.BaseName + '.' + $format)
        if (!(Test-Path -LiteralPath $output)) { throw "Imagem ausente: $output" }
    }
}
Write-Output "Gerados $($sources.Count) SVG e $($sources.Count) PNG em $imagePath."
