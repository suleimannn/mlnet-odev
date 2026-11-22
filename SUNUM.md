# Sunum Metni

Toplam süre yaklaşık 10 dakika. Her örnek ayrı bir kişiye verilebilir.
Sunumdan önce: `dotnet run --project src/Web` komutunu çalıştır ve tarayıcıda http://localhost:5080 adresini aç.

---

## 0. Giriş (1 dakika)

> "ML.NET, Microsoft'un C# için hazırladığı makine öğrenmesi kütüphanesi. Python bilmeden,
> .NET projelerinin içinde model eğitmeyi ve kullanmayı sağlıyor.
>
> Biz ML.NET'in üç farklı görevini denedik: **sayı tahmini**, **metin sınıflandırma** ve **görüntü sınıflandırma**.
> Üçünde de adımlar aynı: **veriyi yükle, eğitim ve test diye ikiye böl, modeli eğit, test verisiyle ölç, kaydet ve kullan.**"

Gösterilecek: `src/Egitim/Program.cs` dosyası. Üç fonksiyon var, her biri bir örnek.

**ML.NET'te her şey bir "pipeline" (boru hattı):** veri bir uçtan giriyor, sırayla dönüştürülüyor, en sonda model öğreniyor.

---

## 1. İkinci el araba fiyatı (Regresyon, 3 dakika)

> "Amacımız, bir arabanın özelliklerine bakarak fiyatını tahmin etmek. Çıktı bir sayı olduğu için bu bir **regresyon** problemi.
>
> Veri olarak 2020 yılına ait yaklaşık 8.700 Türkiye ikinci el araba ilanını kullandık. Marka, seri, yıl, kilometre, yakıt, vites,
> motor hacmi, kasa tipi ve ilanın kimden verildiği bilgileri var.
>
> **Temizlik:** sıfır km ve hasarlı araçları çıkardık. Fiyatı 10 bin TL'nin altında olan hatalı ilanları da attık.
>
> **Pipeline:** Bilgisayar 'Renault' gibi yazıları anlayamaz, o yüzden **One-Hot Encoding** ile bunları sayılara çevirdik.
> Sonra tüm özellikleri tek bir sütunda birleştirdik ve **FastTree** algoritmasıyla eğittik.
> FastTree, art arda yüzlerce küçük karar ağacı kurar. Her yeni ağaç bir öncekinin hatasını düzeltmeye çalışır.
>
> **Sonuç:** R² = 0,92. Yani fiyatlardaki değişimin %92'sini açıklayabiliyor. Ortalama hata yaklaşık 15 bin TL."

**Demo:** Renault Clio, 2015 model, 120.000 km. Sonra yılı 2010 yap, sonra km'yi artır. Fiyatın düştüğünü göster.
Sınıftan birine "senin araban ne?" diye sorup onunkini dene.

---

## 2. Spam SMS (İkili sınıflandırma, 3 dakika)

> "Amacımız, gelen bir SMS'in spam mı yoksa normal mesaj mı olduğunu bulmak. İki seçenek olduğu için buna **ikili sınıflandırma** deniyor.
>
> Veri: bir akademik çalışmada toplanmış 4.751 gerçek Türkçe SMS. Yaklaşık yarısı spam, yarısı normal.
>
> **Pipeline:** **FeaturizeText** metni sayılara çeviriyor. Hangi kelimeler ve harf grupları geçiyor, kaç kere geçiyor, bunlara bakıyor.
> Sonra **lojistik regresyon** her kelimeye bir ağırlık veriyor. 'Bonus', 'kazandınız', 'tıklayın' spam'e doğru çekiyor,
> 'annem', 'hocam' gibi kelimeler normale doğru çekiyor. Sonuçta 0 ile 1 arasında bir spam olasılığı çıkıyor.
>
> **Sonuç:** test mesajlarının %99,3'ünü doğru bildi. 974 test mesajından sadece 7'sinde yanıldı."

**Demo:** Hazır örnek butonlara tıkla. Sonra sınıftan birine kendi telefonundaki bir spam mesajı okut ve yaz.

---

## 3. Atık türü (Görüntü sınıflandırma, 3 dakika)

> "Amacımız, fotoğraftaki atığın kağıt, karton, plastik, cam, metal ya da diğer çöp olduğunu bulmak.
>
> Sıfırdan bir görüntü modeli eğitmek milyonlarca fotoğraf ister. Biz bunun yerine **transfer learning** kullandık:
> milyonlarca fotoğrafla önceden eğitilmiş hazır bir model aldık (**ResNet**). Bu model kenarları, şekilleri ve dokuları
> zaten tanıyor. Biz sadece son katmanını kendi 6 sınıfımıza göre yeniden eğittik. Bu sayede 2.500 fotoğraf yetti.
>
> **Sonuç:** daha önce hiç görmediği 505 test fotoğrafının %88,8'ini doğru bildi. En zorlandığı sınıf 'diğer çöp' oldu,
> çünkü bu sınıfta en az fotoğraf var (137) ve içinde her türden şey bulunuyor. En kolay tanıdığı ise karton (%97)."

**Demo:** Telefonla bir pet şişe ya da kağıt fotoğrafı çek, bilgisayara at ve sürükle bırak. Olasılık çubuklarını göster.
İpucu: Atık tek başına ve sade bir zeminde olsun. Model bu tür fotoğraflarla eğitildi.

---

## 4. Kapanış (30 saniye)

> "Özetle ML.NET ile, C# bilen biri ayrı bir dil öğrenmeden makine öğrenmesini kendi uygulamasına ekleyebiliyor.
> Üç farklı görevde de aynı adımları izledik: veri, pipeline, eğitim, test, kaydetme ve kullanma."

---

## Olası sorular

**Neden verinin %20'sini ayırdınız?**
Modeli hiç görmediği verilerle test etmek için. Ezberleyip ezberlemediğini böyle anlıyoruz.

**R² nedir?**
Modelin fiyatlardaki değişimi ne kadar açıkladığını gösterir. 1 mükemmel, 0 ise hiçbir şey açıklayamıyor demek. Bizimki 0,92.

**Araba fiyatları neden düşük çıkıyor?**
Veri 2020 yılından. Model o yılın piyasasını öğrendi. Güncel veriyle yeniden eğitilirse güncel fiyat verir.

**SMS'te %99 çok yüksek değil mi, ezber olmasın?**
Bu sonuç modelin hiç görmediği test mesajlarından geliyor. Spam mesajlar genelde çok belirgin kalıplar içerdiği için bu oran normal.

**Transfer learning nedir?**
Başka bir iş için eğitilmiş bir modelin bilgisini yeni bir işte kullanmak. Biz ResNet'in görüntü tanıma bilgisini atık tanımaya uyarladık.

**Atık modeli neden diğerlerinden daha düşük (%88,8)?**
Fotoğraf tanımak metin ya da tablo verisinden daha zor. Ayrıca elimizde sadece 2.500 fotoğraf vardı. Daha fazla fotoğrafla oran yükselir.

**Atık olmayan bir şeyin fotoğrafını (mesela bir insanı) yüklesek ne olur?**
Model yine 6 sınıftan birini seçer, çünkü sadece bu 6 sınıfı biliyor. Ama olasılık düşük çıkar (%40-50 gibi). Bu, modelin emin olmadığını gösterir.

**Model Builder kullandınız mı?**
Hayır, kodla yazdık. Model Builder aynı işi arayüzle yapan Visual Studio aracı. Arka planda yine bu ML.NET kodlarını üretiyor.

**Modeli nasıl kullanıyorsunuz?**
Eğitilen model `.zip` dosyası olarak kaydediliyor. Web uygulaması bu dosyayı yükleyip `PredictionEngine` ile tahmin yapıyor.
