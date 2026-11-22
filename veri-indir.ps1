# Üç örneğin ham verisini data/ klasörüne indirir. Modeli yeniden eğitmek için gerekir.
# Çalıştırma (proje klasöründe):  powershell -ExecutionPolicy Bypass -File veri-indir.ps1
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$data = Join-Path $PSScriptRoot 'data'

$dosyalar = @(
    @{ Klasor = 'araba'; Ad = 'turkey_car_market.csv';    Url = 'https://raw.githubusercontent.com/ozyuzer/turkeycarmarket/HEAD/turkey_car_market.csv' },
    @{ Klasor = 'sms';   Ad = 'TurkishSMSCollection.csv'; Url = 'https://huggingface.co/spaces/NimaKL/spamd/resolve/main/TurkishSMSCollection.csv' },
    @{ Klasor = 'atik';  Ad = 'dataset-resized.zip';      Url = 'https://huggingface.co/datasets/garythung/trashnet/resolve/main/dataset-resized.zip' }
)

foreach ($d in $dosyalar) {
    $klasor = Join-Path $data $d.Klasor
    New-Item -ItemType Directory -Force $klasor | Out-Null
    $hedef = Join-Path $klasor $d.Ad
    if (Test-Path $hedef) { Write-Host "Zaten var: $($d.Ad)"; continue }
    Write-Host "İndiriliyor: $($d.Ad)"
    Invoke-WebRequest $d.Url -OutFile $hedef -UseBasicParsing
}

$resimler = Join-Path $data 'atik\dataset-resized'
if (-not (Test-Path $resimler)) {
    Write-Host 'Atık fotoğrafları açılıyor...'
    Expand-Archive (Join-Path $data 'atik\dataset-resized.zip') (Join-Path $data 'atik') -Force
    Remove-Item (Join-Path $data 'atik\__MACOSX') -Recurse -Force -ErrorAction SilentlyContinue
}
Write-Host 'Tamam. Şimdi eğitebilirsin: dotnet run --project src/Egitim'
