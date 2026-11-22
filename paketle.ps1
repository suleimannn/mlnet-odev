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

# Kaynak kod zip'i (hocaya teslim için): Visual Studio'da açılıp çalıştırılabilir, ham veri ve derleme çıktıları yok
Write-Host 'Kaynak kod zip yapılıyor...'
$kod = Join-Path $kok 'paket\MlOdev-KaynakKod'
New-Item -ItemType Directory -Force $kod | Out-Null
foreach ($d in 'MlOdev.sln','README.md','SUNUM.md','CALISMA.md','veri-indir.ps1') { Copy-Item (Join-Path $kok $d) $kod }
New-Item -ItemType Directory -Force (Join-Path $kod 'modeller') | Out-Null
Get-ChildItem (Join-Path $kok 'modeller') -File | Where-Object { $_.Extension -in '.zip','.json' } | Copy-Item -Destination (Join-Path $kod 'modeller')
foreach ($p in 'Ortak','Egitim','Web') {
    $hedef = Join-Path $kod "src\$p"
    robocopy (Join-Path $kok "src\$p") $hedef /E /XD bin obj /NFL /NDL /NJH /NJS /NP | Out-Null
}
Compress-Archive -Path $kod -DestinationPath (Join-Path $kok 'paket\MlOdev-KaynakKod.zip') -CompressionLevel Optimal
Remove-Item $kod -Recurse -Force
$mb2 = [math]::Round((Get-Item (Join-Path $kok 'paket\MlOdev-KaynakKod.zip')).Length / 1MB)
Write-Host "Tamam: paket\MlOdev-KaynakKod.zip ($mb2 MB) - Visual Studio'da MlOdev.sln ile açılır"
