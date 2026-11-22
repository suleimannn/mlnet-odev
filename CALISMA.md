# ML.NET Ödevi: Kapsamlı Çalışma Rehberi

Bu rehber projeyi baştan sona anlaman için hazırlandı. Önce temel kavramlar, sonra ML.NET, sonra üç örneğin her biri, en sonda sistem analizi bakış açısı ve sorular var.

> **Çok vaktin yoksa:** 1. bölümü, kendi örneğinin bölümünü ve 8. bölümdeki soruları oku.

---

## İçindekiler

1. [Temel kavramlar](#1-temel-kavramlar)
2. [ML.NET nedir, nasıl çalışır?](#2-mlnet-nedir-nasıl-çalışır)
3. [Örnek 1: İkinci el araba fiyatı (Regresyon)](#3-örnek-1-ikinci-el-araba-fiyatı-regresyon)
4. [Örnek 2: Spam SMS (İkili sınıflandırma)](#4-örnek-2-spam-sms-ikili-sınıflandırma)
5. [Örnek 3: Atık türü (Görüntü sınıflandırma)](#5-örnek-3-atık-türü-görüntü-sınıflandırma)
6. [Başarı ölçüleri sözlüğü](#6-başarı-ölçüleri-sözlüğü)
7. [Sistem analizi bakış açısı](#7-sistem-analizi-bakış-açısı)
8. [Olası sorular ve cevapları](#8-olası-sorular-ve-cevapları)
9. [Kendini test et](#9-kendini-test-et)

---

## 1. Temel kavramlar

### Makine öğrenmesi nedir?
Normal programlamada kuralları biz yazarız: *"km 200 binden fazlaysa fiyatı düşür"* gibi.
Makine öğrenmesinde kuralları yazmayız. Bilgisayara **çok sayıda örnek** veririz, kuralları **kendisi bulur**.

| Normal programlama | Makine öğrenmesi |
|---|---|
| Kural + veri → cevap | Veri + cevaplar → **kural (model)** |

### Denetimli öğrenme (supervised learning)
Bizim üç örneğimiz de **denetimli öğrenme**. Yani modele hem soruyu hem doğru cevabı veriyoruz:
- "Bu araba şu özelliklerde, **fiyatı 94.000 TL**"
- "Bu mesaj şu, **spam**"
- "Bu fotoğraf, **plastik**"

Model bu örneklerden genelleme yapıp daha önce hiç görmediği yeni örneklere cevap veriyor.

### Temel terimler

| Terim | Anlamı | Bizim projede |
|---|---|---|
| **Özellik (Feature)** | Modelin baktığı bilgiler | Marka, yıl, km / mesaj metni / fotoğraf |
| **Etiket (Label)** | Tahmin edilecek doğru cevap | Fiyat / spam mı? / atık türü |
| **Model** | Veriden öğrenilmiş kurallar bütünü | `modeller/*.zip` dosyaları |
| **Eğitim (Training)** | Modelin örneklerden öğrenmesi | `dotnet run --project src/Egitim` |
| **Tahmin (Prediction)** | Modelin yeni bir örneğe cevap vermesi | Web arayüzünde butona basınca |
| **Eğitim verisi** | Modelin öğrendiği kısım (%80) | |
| **Test verisi** | Modelin **hiç görmediği**, ölçmek için ayrılan kısım (%20) | |

### Problem türleri

| Tür | Çıktı | Örneğimiz |
|---|---|---|
| **Regresyon** | Bir **sayı** | Araba fiyatı |
| **İkili sınıflandırma** | **İki** seçenekten biri (evet/hayır) | Spam mı, değil mi |
| **Çok sınıflı sınıflandırma** | **İkiden fazla** seçenekten biri | 6 atık türünden biri |

### Ezber (overfitting) nedir?
Bir öğrenci soruları anlamadan cevapları ezberlerse, sınavda farklı bir soru gelince yapamaz. Model de eğitim verisini ezberleyebilir.
Bunu anlamak için verinin **%20'sini ayırıp** modele hiç göstermiyoruz. Rakamlarımızın **hepsi bu görülmemiş test verisinden** geliyor.

---

## 2. ML.NET nedir, nasıl çalışır?

**ML.NET**, Microsoft'un **C# ve .NET** için yazdığı ücretsiz, açık kaynaklı makine öğrenmesi kütüphanesi.
- Python bilmeden, .NET projesinin **içinde** model eğitip kullanabilirsin.
- Eğitilen model bir `.zip` dosyası olarak kaydedilir, herhangi bir .NET uygulamasında (web, masaüstü, mobil) kullanılabilir.
- Bizim projede **ML.NET 3.0.1** ve **.NET 7** kullanıldı.

### ML.NET'in 5 temel parçası

| Parça | Ne işe yarar | Benzetme |
|---|---|---|
| **MLContext** | Her şeyin başlangıç noktası | Mutfak |
| **IDataView** | ML.NET'in veri tablosu | Malzemeler |
| **Pipeline (Estimator)** | Sırayla uygulanacak adımlar | Tarif |
| **Fit()** | Tarifi veriye uygulayıp modeli eğitir | Yemeği pişirmek |
| **Model (Transformer)** | Eğitilmiş, kullanıma hazır sonuç | Pişmiş yemek |

### Her örnekte aynı 7 adım

```
1. Veriyi oku ve temizle
2. %80 eğitim, %20 test olarak böl          → ml.Data.TrainTestSplit(...)
3. Pipeline kur (dönüşümler + algoritma)
4. Eğit                                     → pipeline.Fit(egitimVerisi)
5. Test verisiyle ölç                        → ml.Regression.Evaluate(...) vb.
6. Kaydet                                   → ml.Model.Save(..., "araba_model.zip")
7. Kullan (web'de yükle ve tahmin yap)        → PredictionEngine.Predict(...)
```

### Seed (tohum) neden 42?
`new MLContext(seed: 42)` yazdık. Bu sayede veri her seferinde **aynı şekilde** karıştırılıyor ve sonuçlar her çalıştırmada aynı çıkıyor. Sunumdaki rakamlar değişmesin diye. 42 sadece geleneksel bir sayı.

### Projenin yapısı

```
mlnet-odev/
├── src/Ortak/Modeller.cs    → Modele giren ve çıkan veri sınıfları (ArabaIlani, SmsMesaji...)
├── src/Egitim/Program.cs    → Üç modelin eğitimi ve testi (ASIL ML.NET KODU)
├── src/Web/Program.cs       → Modelleri yükleyip tahmin yapan web sunucusu
├── src/Web/wwwroot/index.html → Tarayıcıda görünen arayüz
├── modeller/                → Eğitilmiş modeller (.zip) ve sonuçlar (sonuclar.json)
└── data/                    → Ham veriler (GitHub'da yok, veri-indir.ps1 ile iner)
```

**Akış:** `Egitim` programı veriden modeli üretir ve `.zip` olarak kaydeder → `Web` programı bu `.zip` dosyasını yükler → kullanıcı tarayıcıdan bilgi girer → model tahmin eder → sonuç ekranda görünür.

---

## 3. Örnek 1: İkinci el araba fiyatı (Regresyon)

### Problem
Bir arabanın özelliklerine bakarak **kaç TL edeceğini** tahmin etmek. Çıktı bir sayı → **regresyon**.

### Veri
- **Kaynak:** 2020 yılında Türkiye'de bir ilan sitesinden toplanmış ikinci el araba ilanları (GitHub'da açık paylaşılan veri seti).
- **Ham hali:** 9.044 ilan → **temizlendikten sonra 8.686 ilan**.
- **Ortalama fiyat:** yaklaşık 134.500 TL, ortanca (medyan) fiyat 94.000 TL (2020 fiyatları).

**Kullandığımız özellikler:**

| Özellik | Tür | Örnek |
|---|---|---|
| Marka | Yazı | Renault |
| Seri | Yazı | Clio |
| Model yılı | Sayı | 2015 |
| Kilometre | Sayı | 120.000 |
| Yakıt | Yazı | Dizel |
| Vites | Yazı | Düz Vites |
| Motor hacmi | Yazı (aralık) | 1301-1600 cc |
| Kasa tipi | Yazı | Sedan |
| Kimden | Yazı | Sahibinden / Galeriden |

**Kullanmadıklarımız ve nedeni:**
- *Beygir gücü:* ilanların %60'ında "Bilmiyorum" yazıyordu.
- *Renk:* fiyata etkisi çok az.
- *İlan tarihi:* hepsi 2020'nin aynı birkaç ayından.

### Veri temizliği
- Sadece **"2. El"** ilanları aldık. Sıfır km, hasarlı ve klasik araçları çıkardık, çünkü fiyat mantıkları farklı.
- **10.000 TL altı ve 2 milyon TL üstü** ilanları attık. Bunlar genelde hatalı ya da uç ilanlar.
- 1 milyon km'den fazla ve 1990'dan eski araçları çıkardık.

> **Neden önemli?** "Çöp girerse çöp çıkar". Hatalı veriyle eğitilen model hatalı öğrenir.

### Pipeline (kod)

```csharp
var pipeline = ml.Transforms.Categorical.OneHotEncoding(...)       // 1) Yazıları sayıya çevir
    .Append(ml.Transforms.Concatenate("Features", ...))            // 2) Hepsini tek sütunda birleştir
    .Append(ml.Regression.Trainers.FastTree(                       // 3) Algoritma
        numberOfLeaves: 31, numberOfTrees: 300, learningRate: 0.1));
```

**1) One-Hot Encoding:** Bilgisayar "Renault" yazısıyla hesap yapamaz. Her marka için ayrı bir sütun açılır:

| Marka | Renault? | Fiat? | BMW? | ... |
|---|---|---|---|---|
| Renault | **1** | 0 | 0 | |
| BMW | 0 | 0 | **1** | |

**Neden "Renault=1, Fiat=2, BMW=3" yapmadık?** Çünkü o zaman model "BMW, Renault'nun 3 katıdır" gibi anlamsız bir sıralama varsayar. One-hot'ta her marka birbirinden bağımsız.

**2) Concatenate:** ML.NET algoritmaları tüm özellikleri tek bir `Features` sütununda bekler. Bu adım hepsini yan yana koyuyor.

**3) FastTree:** **Gradient boosting** denen bir yöntem.
- Önce basit bir **karar ağacı** kurulur: *"Yıl 2012'den yeni mi? Evetse → km 100 binden az mı? ..."*
- Bu ağacın hataları hesaplanır. **İkinci ağaç** sadece bu hataları düzeltmeye çalışır.
- Üçüncüsü kalan hataları düzeltir... **300 ağaç** böyle art arda kurulur.
- Son tahmin, tüm ağaçların katkılarının toplamıdır.
- `numberOfLeaves: 31` → her ağaç en fazla 31 yaprağa (sonuca) ayrılır.
- `learningRate: 0.1` → her ağacın katkısı küçültülür, böylece model yavaş ama sağlam öğrenir.

> **Benzetme:** Bir grup uzman sırayla fiyat söylüyor. Her yeni uzman bir öncekinin nerede yanıldığına bakıp düzeltme yapıyor.

### Sonuçlar (test verisinde, yaklaşık 1.700 ilan)

| Ölçü | Değer | Anlamı |
|---|---|---|
| **R²** | **0,921** | Fiyatlardaki farkların %92'sini açıklıyor |
| **MAE** | **15.395 TL** | Tahminler gerçek fiyattan ortalama ±15 bin TL sapıyor |

Ortalama fiyat 134.500 TL olduğuna göre ortalama hata **yaklaşık %11**. İkinci el araba gibi pazarlığı, hasar kaydı ve donanım farkı olan bir piyasa için iyi bir sonuç.

### Demo için örnekler
| Araba | Tahmin (2020 fiyatı) |
|---|---|
| Renault Clio, 2010, 200.000 km | ~57.000 TL |
| Renault Clio, 2015, 120.000 km | ~85.000 TL |
| Renault Clio, 2019, 20.000 km | ~114.000 TL |
| BMW 5 Serisi, 2018, 40.000 km, otomatik | ~577.000 TL |

**Göstermen gereken:** yıl yükselince ve km düşünce fiyatın arttığını model kendisi öğrendi. Biz bu kuralı hiç yazmadık.

---

## 4. Örnek 2: Spam SMS (İkili sınıflandırma)

### Problem
Gelen bir SMS'in **spam mı yoksa normal mi** olduğunu bulmak. İki seçenek → **ikili sınıflandırma**.

### Veri
- **Kaynak:** Türkçe SMS Koleksiyonu (Karasoy ve Ballı, 2021, akademik çalışma).
- **4.751 gerçek Türkçe SMS:** 2.536 spam, 2.215 normal. Dengeli bir veri seti, iki sınıf da yaklaşık eşit.

### Pipeline (kod)

```csharp
var pipeline = ml.Transforms.Text.FeaturizeText("Features", "Mesaj")    // 1) Metni sayılara çevir
    .Append(ml.BinaryClassification.Trainers.SdcaLogisticRegression()); // 2) Algoritma
```

Sadece iki satır. ML.NET metin işini büyük ölçüde kendisi hallediyor.

**1) FeaturizeText:** Metni sayılara çevirir. Arka planda şunları yapar:
- Küçük harfe çevirir ve harflerin üstündeki işaretleri kaldırır (ş→s, ç→c, ü→u, ö→o, ğ→g).
- Mesajı **kelimelere** ve **kelime çiftlerine** ayırır: "bonus", "kazandınız", "bonus kazandınız".
- Ayrıca **3'lü harf gruplarına** ayırır: "bon", "onu", "nus"... Böylece kelimenin tamamı aynı olmasa da ortak parçalar yakalanır (kazandınız / kazandın / kazanın).
- Her parçanın mesajda kaç kez geçtiğini sayar ve normalleştirir.
- Sonuçta her mesaj binlerce sayıdan oluşan bir **vektöre** dönüşür.

**2) Lojistik regresyon (SDCA ile):**
- Her kelimeye ve harf grubuna bir **ağırlık** verir.
  - "bonus", "kazandınız", "tıklayın", "kampanya", "www" → **artı** ağırlık (spam'e doğru çeker)
  - "annem", "hocam", "akşam", "geliyor musun" → **eksi** ağırlık (normale doğru çeker)
- Ağırlıklar toplanır ve **0 ile 1 arasında bir olasılığa** çevrilir (sigmoid fonksiyonu).
- **0,5'ten büyükse spam**, küçükse normal.
- **SDCA**, bu ağırlıkları hızlıca bulan optimizasyon yönteminin adı.

### Sonuçlar (test verisinde, 974 mesaj)

| Ölçü | Değer | Anlamı |
|---|---|---|
| **Doğruluk (Accuracy)** | **%99,3** | 974 mesajın 967'sini doğru bildi |
| **Kesinlik (Precision)** | **%99,2** | "Spam" dediklerinin %99,2'si gerçekten spam |
| **Duyarlılık (Recall)** | **%99,4** | Gerçek spamlerin %99,4'ünü yakaladı |

**Karışıklık matrisi (confusion matrix):**

| | Model: Spam | Model: Normal |
|---|---|---|
| **Gerçekte spam (519)** | ✅ 516 | ❌ 3 (kaçırdı) |
| **Gerçekte normal (455)** | ❌ 4 (yanlış alarm) | ✅ 451 |

Toplam sadece **7 hata**.

> **Hangi hata daha kötü?** Normal bir mesajı spam sanmak (yanlış alarm), spam'i kaçırmaktan genelde daha kötüdür. Annenin mesajı spam klasörüne düşerse görmezsin. Bizde yanlış alarm sadece 4 tane.

### Demo için örnekler
| Mesaj | Sonuç |
|---|---|
| Tebrikler! 5000 TL deneme bonusu kazandınız, hemen üye olun: bit.ly/xyz | SPAM (%94) |
| Size özel %0 faizli kredi fırsatı! Başvuru için KREDI yazıp 4455'e gönderin | SPAM (%95) |
| Akşam yemeğe geliyor musun? Annem sordu | Normal (%1) |
| Hocam yarınki ders saat kaçta başlıyor? | Normal (%4) |

---

## 5. Örnek 3: Atık türü (Görüntü sınıflandırma)

### Problem
Bir fotoğraftaki atığın **6 türden hangisi** olduğunu bulmak: kağıt, karton, plastik, cam, metal, diğer çöp. İkiden fazla seçenek → **çok sınıflı sınıflandırma**.

### Veri
- **Kaynak:** TrashNet (Stanford Üniversitesi öğrencilerinin hazırladığı açık veri seti, MIT lisansı).
- **2.527 fotoğraf**, beyaz zemin üzerinde tek bir atık.

| Sınıf | Fotoğraf |
|---|---|
| Kağıt | 594 |
| Cam | 501 |
| Plastik | 482 |
| Metal | 410 |
| Karton | 403 |
| Diğer çöp | 137 |

### Asıl fikir: Transfer learning
Sıfırdan bir görüntü tanıma modeli eğitmek **milyonlarca fotoğraf** ve günlerce güçlü donanım ister. Bizde 2.500 fotoğraf ve sıradan bir bilgisayar var.

**Çözüm:** başkasının eğittiği modeli alıp kendi işimize uyarlamak.
- **ResNet-50**, ImageNet adlı 1,2 milyon fotoğraflık veri setiyle eğitilmiş, 50 katmanlı hazır bir sinir ağı.
- İlk katmanlar **kenarları**, sonraki katmanlar **dokuları ve şekilleri**, son katmanlar **nesneleri** tanıyor.
- Biz ResNet'in öğrendiği bu bilgiyi **olduğu gibi tutup** sadece **son katmanını** kendi 6 sınıfımıza göre yeniden eğittik.

> **Benzetme:** Araba kullanmayı bilen birine kamyon kullanmayı öğretmek, hiç araç kullanmamış birine öğretmekten çok daha kolaydır. Temel beceriler zaten var.

### Pipeline (kod)

```csharp
var pipeline = ml.Transforms.Conversion.MapValueToKey("Label", "Sinif")      // 1) Sınıf adlarını numaraya çevir
    .Append(ml.MulticlassClassification.Trainers.ImageClassification(...))  // 2) ResNet + yeni son katman
    .Append(ml.Transforms.Conversion.MapKeyToValue("PredictedLabel"));       // 3) Numarayı tekrar ada çevir
```

**Ayarlar:**
- `Arch = ResnetV250` → kullanılan hazır model (ResNet-50, sürüm 2).
- `Epoch = 50` → veri üzerinden en fazla 50 tur geçilir. Gelişme durunca eğitim **erken durur** (early stopping). Bizde yaklaşık 20-25. turda durdu.
- `BatchSize = 10` → her adımda 10 fotoğraf birlikte işlenir.
- `LearningRate = 0.01` → her adımda ne kadar büyük düzeltme yapılacağı.

**Arka planda ML.NET şunları yapıyor:**
1. Hazır ResNet modelini (yaklaşık 100 MB) internetten bir kere indiriyor.
2. Her fotoğrafı ResNet'ten geçirip son katmandan önceki çıktıyı (**darboğaz / bottleneck özellikleri**) hesaplıyor ve önbelleğe alıyor. En uzun süren kısım bu.
3. Sadece bu özellikler üzerinde yeni son katmanı eğitiyor. Bu kısım hızlı.
4. Eğitimin içinden %10'luk bir **doğrulama (validation)** parçası ayırıp her turda kontrol ediyor. Eğitim çıktısında her tur için iki satır görünmesinin nedeni bu.

### Sonuçlar (test verisinde, 505 fotoğraf)

**Genel doğruluk: %87,3**

| Sınıf | Doğru tanıma oranı (recall) |
|---|---|
| Karton | %96 |
| Cam | %91 |
| Kağıt | %91 |
| Plastik | %86 |
| Metal | %85 |
| Diğer çöp | %56 |

**Neden "Diğer çöp" düşük?**
- En az fotoğraf o sınıfta (137, diğerlerinin üçte biri kadar).
- İçinde her türden şey var (paket, poşet, yemek artığı...). Ortak bir görünüşü yok.

**En çok karıştırdığı ikili:** **cam ↔ plastik**. İkisi de parlak, şeffaf ve şişe şeklinde olabiliyor. İnsan bile fotoğrafta zaman zaman karıştırır.

### Demo ipuçları
- Atığı **tek başına, sade (tercihen beyaz) zeminde, yakından** çek. Model böyle fotoğraflarla eğitildi.
- Arayüz fotoğrafı otomatik olarak küçültüp JPEG'e çeviriyor. PNG ya da telefon fotoğrafı fark etmez.
- Sonuçta her sınıfın olasılığı çubuk grafikle gösteriliyor. Model emin değilse olasılıklar birbirine yakın çıkar. Bu da gösterilmeye değer bir şey.

---

## 6. Başarı ölçüleri sözlüğü

### Regresyon için

| Ölçü | Formül (kabaca) | Yorum |
|---|---|---|
| **R² (R-kare)** | 1 - (modelin hatası / ortalamayı tahmin etmenin hatası) | 1 = mükemmel, 0 = "hep ortalama fiyatı söylemek" kadar kötü. **Bizde 0,92** |
| **MAE** (Ortalama Mutlak Hata) | Tahmin ile gerçek arasındaki farkların ortalaması | TL cinsinden, anlaması kolay. **Bizde 15.395 TL** |
| RMSE | Hataların karelerinin ortalamasının karekökü | Büyük hataları daha çok cezalandırır |

### Sınıflandırma için

| Ölçü | Soru | Formül |
|---|---|---|
| **Doğruluk (Accuracy)** | Toplamda ne kadarını doğru bildi? | Doğrular / Hepsi |
| **Kesinlik (Precision)** | "Spam" dediklerinin ne kadarı gerçekten spam? | Doğru spam / Spam dediklerin |
| **Duyarlılık (Recall)** | Gerçek spamlerin ne kadarını yakaladı? | Doğru spam / Gerçek spamler |
| **F1** | Kesinlik ile duyarlılığın dengesi | 2 × P × R / (P + R) |
| **Karışıklık matrisi** | Hangi sınıfı hangisiyle karıştırıyor? | Satır = gerçek, sütun = tahmin |

> **Doğruluk neden tek başına yetmez?** 100 mesajın 95'i normal olsaydı, "her şey normal" diyen aptal bir model bile %95 doğruluk alırdı. Bu yüzden kesinlik ve duyarlılığa da bakıyoruz. Bizim veride iki sınıf zaten dengeli.

---

## 7. Sistem analizi bakış açısı

Ders **Sistem Analizi** olduğu için projeyi bir sistem olarak da anlatabilmelisin.

### Sistemin amacı
ML.NET kütüphanesinin üç farklı makine öğrenmesi görevini (regresyon, ikili sınıflandırma, görüntü sınıflandırma) gerçek Türkçe/yerel verilerle uygulamak ve sonuçları bir web arayüzünden denenebilir hale getirmek.

### Kullanıcılar (aktörler)
| Aktör | Ne yapar |
|---|---|
| **Son kullanıcı** | Arayüzden bilgi/mesaj/fotoğraf girer, tahmini görür |
| **Geliştirici / analist** | Veriyi hazırlar, modeli eğitir, sonuçları değerlendirir |

### Kullanım senaryoları (use case)
1. **Araba fiyatı tahmin et:** kullanıcı marka, seri, yıl, km vb. seçer → sistem tahmini fiyatı gösterir.
2. **SMS kontrol et:** kullanıcı mesaj yazar → sistem spam/normal sonucunu ve olasılığını gösterir.
3. **Atık türünü bul:** kullanıcı fotoğraf yükler → sistem atık türünü ve her sınıfın olasılığını gösterir.
4. **Modelleri yeniden eğit:** geliştirici eğitim programını çalıştırır → yeni modeller ve başarı sonuçları kaydedilir.

### Fonksiyonel gereksinimler
- Sistem araba özelliklerinden fiyat tahmini yapmalı.
- Sistem bir SMS metninin spam olup olmadığını olasılığıyla birlikte söylemeli.
- Sistem yüklenen fotoğraftaki atığı 6 sınıftan birine ayırmalı.
- Sistem her modelin test başarısını arayüzde göstermeli.
- Sistem geçersiz girişlerde (boş mesaj, resim olmayan dosya) anlaşılır hata mesajı vermeli.

### Fonksiyonel olmayan gereksinimler
- **Performans:** tahmin 1 saniyeden kısa sürmeli. Modeller sunucu açılınca bir kez yükleniyor.
- **Taşınabilirlik:** okul bilgisayarlarında kurulum gerektirmeden çalışmalı → .NET'i içinde taşıyan paket (`BASLAT.bat`).
- **Uyumluluk:** .NET 7 ile çalışmalı (okul bilgisayarları).
- **Tekrarlanabilirlik:** aynı veriyle eğitilince aynı sonuç çıkmalı → sabit seed (42).
- **Kullanılabilirlik:** Türkçe, sade, tek sayfalık arayüz.

### Veri akışı

```
[Ham veri: CSV / fotoğraflar]
        │
        ▼
[Eğitim programı] ── temizleme → %80/%20 bölme → pipeline → Fit → değerlendirme
        │
        ▼
[Model dosyaları: araba_model.zip, sms_model.zip, atik_model.zip + sonuclar.json]
        │
        ▼
[Web sunucusu] ◄── istek (JSON / fotoğraf) ── [Tarayıcı / kullanıcı]
        │                                          ▲
        └──────────── tahmin sonucu (JSON) ─────────┘
```

### Web sunucusunun uç noktaları (API)
| Adres | Yöntem | Ne yapar |
|---|---|---|
| `/api/araba/tahmin` | POST | Araba bilgilerini alır, fiyat döner |
| `/api/araba/secenekler` | GET | Açılır listeler için marka/seri listesi |
| `/api/sms/tahmin` | POST | Mesajı alır, spam sonucunu döner |
| `/api/atik/tahmin` | POST | Fotoğrafı alır, atık türünü döner |
| `/api/sonuclar` | GET | Modellerin test başarılarını döner |

### Kısıtlar ve sınırlamalar
- **Araba verisi 2020'den.** Fiyatlar bugünün piyasasını yansıtmaz.
- **Atık modeli** sade zeminde tek nesneye göre eğitildi. Karışık ve kalabalık fotoğraflarda zorlanır.
- **Atık modeli** sadece 6 sınıfı bilir. Alakasız bir fotoğrafta da bunlardan birini seçer.
- **SMS modeli** sadece 2021'e kadarki spam kalıplarını biliyor. Yeni tür dolandırıcılık mesajları gelirse güncellenmesi gerekir.

### Geliştirme fikirleri
- Güncel araba ilanlarıyla yeniden eğitmek.
- Atık için kendi çektiğimiz fotoğrafları eklemek (özellikle "diğer çöp" sınıfına).
- Modelin emin olmadığı durumlarda (olasılık %60'ın altındaysa) "emin değilim" demesi.
- Uygulamayı bir mobil uygulamaya ya da geri dönüşüm kutusuna kamera ile entegre etmek.

---

## 8. Olası sorular ve cevapları

### Genel
**ML.NET'i neden seçtiniz, Python varken?**
Ders konusu ML.NET. Ayrıca .NET ile yazılmış bir uygulamaya makine öğrenmesi eklemek istiyorsan, ayrı bir Python servisi kurmadan aynı dil ve aynı proje içinde yapabiliyorsun.

**Model Builder kullandınız mı?**
Hayır, kodla yazdık. Model Builder, Visual Studio içinde aynı işi arayüzle yapan bir araç ve arka planda yine bu tür ML.NET kodları üretiyor. Kodla yazmak her adımı anlamamızı ve anlatabilmemizi sağladı.

**Neden verinin %20'sini ayırdınız?**
Modeli hiç görmediği verilerle test etmek için. Eğitim verisinde iyi olup yeni veride kötü olan model ezber yapmıştır. Rakamlarımızın hepsi test verisinden.

**Model nasıl kaydediliyor ve kullanılıyor?**
`ml.Model.Save()` ile `.zip` dosyasına kaydediliyor. Web sunucusu açılınca `ml.Model.Load()` ile yükleyip `PredictionEngine` ile tek tek tahmin yapıyor. Eğitim bir kere yapılıyor, tahmin anında hızlı.

**Seed nedir?**
Rastgele işlemlerin (veriyi karıştırma, bölme) başlangıç sayısı. Sabit tuttuğumuz için her çalıştırmada aynı sonuç çıkıyor.

### Araba
**R² nedir, 0,92 iyi mi?**
Modelin fiyattaki değişimin ne kadarını açıkladığını gösterir. 1 mükemmel demek. 0,92 ikinci el araba gibi değişkenli bir piyasa için çok iyi.

**Fiyatlar neden düşük çıkıyor?**
Veri 2020'den, model o yılın piyasasını öğrendi. Güncel veriyle eğitilirse güncel fiyat verir. Kod aynı kalır, sadece veri değişir.

**One-hot encoding neden gerekli?**
Algoritmalar sadece sayılarla çalışır. Markaya 1, 2, 3 gibi sayı verseydik model aralarında olmayan bir büyüklük ilişkisi varsayardı.

**Neden FastTree?**
Tablo halindeki verilerde (sütun sütun özellikler) en başarılı yöntemlerden biri karar ağacı tabanlı boosting. Kategorik ve sayısal özellikleri birlikte iyi işler ve hızlı eğitilir.

### SMS
**%99 ezber olmasın?**
Bu oran modelin hiç görmediği 974 test mesajından geliyor. Spam mesajlarında "kazandınız", "bonus", "tıkla", link gibi çok belirgin kalıplar olduğu için oran yüksek.

**Hangi hata daha tehlikeli?**
Normal mesajı spam sanmak (önemli bir mesajı kaçırırsın). Bizde bu hatadan sadece 4 tane var.

**Model Türkçe karakterleri ve yazım hatalarını anlıyor mu?**
Harflerin üstündeki işaretleri kaldırdığı (ş→s, ü→u gibi) ve 3'lü harf gruplarına da baktığı için Türkçe karakterli ve karaktersiz yazımları, kelimenin farklı eklerini büyük ölçüde yakalıyor.

### Atık
**Transfer learning nedir?**
Başka bir iş için eğitilmiş bir modelin bilgisini yeni bir işte kullanmak. ResNet milyonlarca fotoğraftan şekil ve doku tanımayı öğrenmişti, biz sadece son katmanını atıklara göre eğittik.

**Neden diğerlerinden düşük (%87,3)?**
Fotoğraf tanımak tablo ya da metin verisinden daha zor. Ayrıca sadece 2.500 fotoğraf vardı ve "diğer çöp" sınıfı hem az hem karışık.

**Alakasız bir fotoğraf yüklersek ne olur?**
Model sadece 6 sınıfı bildiği için yine birini seçer, ama olasılıklar düşük ve dağınık çıkar. Gerçek bir sistemde bir eşik konup "emin değilim" dedirtilebilir.

**Eğitim ne kadar sürdü?**
Birkaç dakika. En uzun kısım fotoğrafları ResNet'ten bir kere geçirip özelliklerini çıkarmak. Sonraki eğitim turları hızlı.

### Teknik
**Okul bilgisayarında nasıl çalışıyor?**
.NET'i de içinde taşıyan, kurulum gerektirmeyen bir paket hazırladık (self-contained publish). `BASLAT.bat` dosyasına çift tıklamak yetiyor.

**Neden .NET 7 ve ML.NET 3.0.1?**
Okul bilgisayarları .NET 7 kullanıyor. ML.NET 3.0.1, .NET 7'yi destekleyen sürüm.

---

## 9. Kendini test et

Cevapları kapatıp kendini dene. Cevaplar bu rehberin içinde.

1. Regresyon ile sınıflandırma arasındaki fark nedir? Bizim örneklerimizden hangisi hangisi?
2. Özellik (feature) ve etiket (label) ne demek? Araba örneğinde etiket nedir?
3. Test verisi neden modele gösterilmez?
4. ML.NET'te `Fit()` ne yapar?
5. One-hot encoding'e neden ihtiyaç duyduk?
6. FastTree nasıl çalışır? Bir cümleyle anlat.
7. R² = 0,92 ne anlama geliyor?
8. FeaturizeText bir mesajı nasıl sayılara çeviriyor?
9. Kesinlik (precision) ile duyarlılık (recall) arasındaki fark nedir?
10. Transfer learning nedir, neden kullandık?
11. Atık modeli en çok hangi sınıfta zorlandı, neden?
12. Projenin iki fonksiyonel ve iki fonksiyonel olmayan gereksinimini söyle.
13. Modeller eğitildikten sonra nerede saklanıyor ve web uygulaması onları nasıl kullanıyor?
14. Araba fiyatları neden düşük çıkıyor ve bu nasıl düzeltilir?
