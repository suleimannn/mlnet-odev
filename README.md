# ML.NET ile 3 Örnek

Sistem Analizi dersi, ML.NET grubu. ML.NET'in üç farklı görevini gösteren üç küçük örnek:

| # | Örnek | ML.NET görevi | Veri |
|---|---|---|---|
| 1 | İkinci el araba fiyatı | Regresyon (sayı tahmini) | 2020 Türkiye ikinci el ilanları, ~8.700 ilan ([kaynak](https://github.com/ozyuzer/turkeycarmarket)) |
| 2 | Spam SMS | İkili sınıflandırma (evet/hayır) | Türkçe SMS koleksiyonu, 4.751 mesaj (Karasoy ve Ballı, 2021) |
| 3 | Atık türü | Görüntü sınıflandırma (transfer learning) | TrashNet, 2.527 fotoğraf, 6 sınıf ([kaynak](https://github.com/garythung/trashnet)) |

## Çalıştırma

Proje **.NET 7** ve **ML.NET 3.0.1** ile yazıldı. Gereken tek şey .NET 7 SDK ya da daha yeni bir .NET SDK. Repoyu `git clone` ile ya da GitHub'daki **Code → Download ZIP** ile indirebilirsin.

Eğitilmiş modeller repoda hazır, yani **direkt arayüzü açabilirsin**:

**Visual Studio 2022 ile:** `MlOdev.sln` dosyasını aç. **Web** projesine sağ tıklayıp **Set as Startup Project** de, sonra ▶ butonuna bas. Tarayıcı kendiliğinden açılır.

**Komut satırından:**

```powershell
dotnet run --project src/Web
```

Sonra tarayıcıda http://localhost:5080 adresini aç.

### .NET kurulu olmayan bilgisayar için paket

Kurulum gerektirmeyen bir sürüm hazırlanabilir (içinde .NET de var, hedef bilgisayarda sadece 64-bit Windows yeterli):

```powershell
powershell -ExecutionPolicy Bypass -File paketle.ps1
```

`paket\MlOdev` klasörünü istediğin bilgisayara kopyala ve **BASLAT.bat** dosyasına çift tıkla. Tarayıcı kendiliğinden açılır.

### Modelleri yeniden eğitmek istersen

Ham veriler repoda yok (boyut ve lisans nedeniyle). Önce indir, sonra eğit:

```powershell
powershell -ExecutionPolicy Bypass -File veri-indir.ps1

# Modelleri eğit (modeller/ klasörüne kaydeder). Atık modeli birkaç dakika sürer.
dotnet run --project src/Egitim

# Sadece birini eğitmek için: araba | sms | atik
dotnet run --project src/Egitim -- sms
```

## Klasörler

- `data/` indirilen ham veriler (repoda yok, `veri-indir.ps1` oluşturur)
- `src/Ortak/Modeller.cs` modele giren ve modelden çıkan veri sınıfları
- `src/Egitim/Program.cs` üç modelin eğitimi ve testi (asıl ML.NET kodu burada)
- `src/Web/` tahmin yapan küçük web sunucusu ve arayüz (`wwwroot/index.html`)
- `modeller/` eğitilmiş modeller (`*.zip`) ve başarı sonuçları (`sonuclar.json`)
- `SUNUM.md` sunumda söylenecekler ve olası sorular
- `CALISMA.md` kapsamlı çalışma rehberi (kavramlar, her örneğin detayı, sistem analizi, sorular)
