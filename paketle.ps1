# Flash bellek / okul bilgisayarı için kurulum gerektirmeyen paket hazırlar.
# İçinde .NET de olduğu için hedef bilgisayarda .NET kurulu olması gerekmez (Windows 64-bit yeterli).
# Çalıştırma (proje klasöründe):  powershell -ExecutionPolicy Bypass -File paketle.ps1
$ErrorActionPreference = 'Stop'
$kok = $PSScriptRoot
$cikti = Join-Path $kok 'paket\MlOdev'
$uygulama = Join-Path $cikti 'uygulama'   # yüzlerce .NET/TensorFlow dosyası buraya, dışarıda sadece BASLAT.bat kalır

if (Test-Path (Join-Path $kok 'paket')) { Remove-Item (Join-Path $kok 'paket') -Recurse -Force }
dotnet publish (Join-Path $kok 'src\Web') -c Release -r win-x64 --self-contained true -p:DebugType=none -o $uygulama
if ($LASTEXITCODE -ne 0) { throw 'Paketleme başarısız.' }

# Hata ayıklama dosyaları gereksiz yer kaplar
Get-ChildItem $uygulama -Filter *.pdb | Remove-Item -Force

Copy-Item (Join-Path $kok 'BASLAT.bat') $cikti

Write-Host 'Zip yapılıyor...'
Compress-Archive -Path $cikti -DestinationPath (Join-Path $kok 'paket\MlOdev.zip') -CompressionLevel Optimal
$mb = [math]::Round((Get-Item (Join-Path $kok 'paket\MlOdev.zip')).Length / 1MB)
Write-Host "Tamam: paket\MlOdev klasörü ve paket\MlOdev.zip ($mb MB)"
Write-Host 'Okulda: klasörü bilgisayara kopyala, içindeki BASLAT.bat dosyasına çift tıkla.'
