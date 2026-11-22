// ============================================================
// EĞİTİM PROGRAMI
// Üç modeli de veriden eğitir, test eder ve "modeller" klasörüne kaydeder.
// Çalıştırma:  dotnet run --project src/Egitim            (hepsi)
//              dotnet run --project src/Egitim -- araba   (sadece biri: araba | sms | atik)
// ============================================================
using System.Text;
using System.Text.Json;
using Microsoft.ML;
using Microsoft.ML.Vision;
using Ortak;

Console.OutputEncoding = Encoding.UTF8;

var kok = ProjeKokunuBul();
var veriKlasoru = Path.Combine(kok, "data");
var modelKlasoru = Path.Combine(kok, "modeller");
Directory.CreateDirectory(modelKlasoru);

// seed sabit: her çalıştırmada aynı sonuç çıksın (sunumda rakamlar değişmesin)
var ml = new MLContext(seed: 42);
var secim = args.Length > 0 ? args[0].ToLowerInvariant() : "hepsi";
var sonuclar = new Dictionary<string, object>();
var sonucDosyasi = Path.Combine(modelKlasoru, "sonuclar.json");
if (File.Exists(sonucDosyasi))
    foreach (var kv in JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(sonucDosyasi))!)
        sonuclar[kv.Key] = kv.Value;

if (secim is "hepsi" or "araba") sonuclar["araba"] = ArabaEgit();
if (secim is "hepsi" or "sms") sonuclar["sms"] = SmsEgit();
if (secim is "hepsi" or "atik") sonuclar["atik"] = AtikEgit();

File.WriteAllText(sonucDosyasi, JsonSerializer.Serialize(sonuclar, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"\nBitti. Modeller ve sonuçlar: {modelKlasoru}");

// ------------------------------------------------------------
// 1) ARABA FİYATI
// ------------------------------------------------------------
object ArabaEgit()
{
    Baslik("1) İkinci el araba fiyatı (Regresyon)");

    // 1. Veriyi oku ve temizle
    var satirlar = CsvOku(Path.Combine(veriKlasoru, "araba", "turkey_car_market.csv"), ',');
    var ilanlar = new List<ArabaIlani>();
    foreach (var s in satirlar.Skip(1))
    {
        // Sütunlar: 0 Tarih,1 Marka,2 Seri,3 Tip,4 Yıl,5 Yakıt,6 Vites,7 CCM,8 Beygir,9 Renk,10 Kasa,11 Kimden,12 Durum,13 Km,14 Fiyat
        if (s.Count != 15 || s[12].Trim() != "2. El") continue;            // sadece ikinci el
        if (!float.TryParse(s[4], System.Globalization.CultureInfo.InvariantCulture, out var yil)) continue;
        if (!float.TryParse(s[13], out var km) || !float.TryParse(s[14], out var fiyat)) continue;
        if (fiyat < 10_000 || fiyat > 2_000_000 || km > 1_000_000 || yil < 1990) continue; // uç/hatalı ilanlar

        ilanlar.Add(new ArabaIlani
        {
            Marka = s[1].Trim(), Seri = s[2].Trim(), ModelYili = yil, Yakit = s[5].Trim(), Vites = s[6].Trim(),
            MotorHacmi = s[7].Trim(), KasaTipi = s[10].Trim(), Kimden = s[11].Trim(), Km = km, Fiyat = fiyat,
        });
    }
    Console.WriteLine($"Temizlenmiş ilan sayısı: {ilanlar.Count}");

    // 2. %80 eğitim, %20 test
    var veri = ml.Data.LoadFromEnumerable(ilanlar);
    var bolum = ml.Data.TrainTestSplit(veri, testFraction: 0.2, seed: 42);

    // 3. Pipeline: yazıları sayıya çevir (one-hot) -> hepsini birleştir -> FastTree ile öğren
    var kategorikler = new[] { "Marka", "Seri", "Yakit", "Vites", "MotorHacmi", "KasaTipi", "Kimden" };
    var pipeline = ml.Transforms.Categorical.OneHotEncoding(kategorikler.Select(c => new InputOutputColumnPair(c)).ToArray())
        .Append(ml.Transforms.Concatenate("Features", kategorikler.Append("ModelYili").Append("Km").ToArray()))
        .Append(ml.Regression.Trainers.FastTree(numberOfLeaves: 31, numberOfTrees: 300, learningRate: 0.1));

    var model = pipeline.Fit(bolum.TrainSet);

    // 4. Test verisinde ölç
    var m = ml.Regression.Evaluate(model.Transform(bolum.TestSet));
    Console.WriteLine($"R² (1'e ne kadar yakınsa o kadar iyi): {m.RSquared:F3}");
    Console.WriteLine($"Ortalama hata (MAE): {m.MeanAbsoluteError:N0} TL");

    ml.Model.Save(model, veri.Schema, Path.Combine(modelKlasoru, "araba_model.zip"));

    // Web formundaki açılır listeler için: marka -> seriler, ve diğer seçenekler
    var secenekler = new
    {
        markalar = ilanlar.GroupBy(i => i.Marka).OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Select(i => i.Seri).Distinct().Order().ToList()),
        yakit = ilanlar.Select(i => i.Yakit).Distinct().Order(),
        vites = ilanlar.Select(i => i.Vites).Distinct().Order(),
        motorHacmi = ilanlar.Select(i => i.MotorHacmi).Distinct().Order(),
        kasaTipi = ilanlar.Select(i => i.KasaTipi).Distinct().Order(),
        kimden = ilanlar.Select(i => i.Kimden).Distinct().Order(),
    };
    File.WriteAllText(Path.Combine(modelKlasoru, "araba_secenekler.json"), JsonSerializer.Serialize(secenekler));

    return new { ilanSayisi = ilanlar.Count, r2 = Math.Round(m.RSquared, 3), ortalamaHataTL = Math.Round(m.MeanAbsoluteError) };
}

// ------------------------------------------------------------
// 2) SPAM SMS
// ------------------------------------------------------------
object SmsEgit()
{
    Baslik("2) Spam SMS (İkili sınıflandırma)");

    var mesajlar = new List<SmsMesaji>();
    foreach (var satir in File.ReadLines(Path.Combine(veriKlasoru, "sms", "TurkishSMSCollection.csv")).Skip(1))
    {
        // Biçim: Mesaj;Grup;GrupYazisi  — mesajın içinde ';' olabilir, o yüzden sağdan bölüyoruz
        var p2 = satir.LastIndexOf(';');
        if (p2 < 0) continue;
        var p1 = satir.LastIndexOf(';', p2 - 1);
        if (p1 < 0) continue;
        var grup = satir[(p1 + 1)..p2].Trim();
        if (grup is not ("1" or "2")) continue;
        mesajlar.Add(new SmsMesaji { Mesaj = satir[..p1].Trim().Trim('"'), SpamMi = grup == "1" });
    }
    Console.WriteLine($"Mesaj sayısı: {mesajlar.Count}  (spam: {mesajlar.Count(m => m.SpamMi)}, normal: {mesajlar.Count(m => !m.SpamMi)})");

    var veri = ml.Data.LoadFromEnumerable(mesajlar);
    var bolum = ml.Data.TrainTestSplit(veri, testFraction: 0.2, seed: 42);

    // Pipeline: metni sayılara çevir (kelime + harf grupları) -> lojistik regresyon
    var pipeline = ml.Transforms.Text.FeaturizeText("Features", "Mesaj")
        .Append(ml.BinaryClassification.Trainers.SdcaLogisticRegression());

    var model = pipeline.Fit(bolum.TrainSet);
    var m = ml.BinaryClassification.Evaluate(model.Transform(bolum.TestSet));

    Console.WriteLine($"Doğruluk (Accuracy): %{m.Accuracy * 100:F1}");
    Console.WriteLine($"Kesinlik (spam dediklerinin ne kadarı gerçekten spam): %{m.PositivePrecision * 100:F1}");
    Console.WriteLine($"Duyarlılık (spamlerin ne kadarını yakaladı): %{m.PositiveRecall * 100:F1}");
    Console.WriteLine(m.ConfusionMatrix.GetFormattedConfusionTable());

    ml.Model.Save(model, veri.Schema, Path.Combine(modelKlasoru, "sms_model.zip"));
    return new
    {
        mesajSayisi = mesajlar.Count,
        dogruluk = Math.Round(m.Accuracy, 3),
        kesinlik = Math.Round(m.PositivePrecision, 3),
        duyarlilik = Math.Round(m.PositiveRecall, 3),
    };
}

// ------------------------------------------------------------
// 3) ATIK SINIFLANDIRMA
// ------------------------------------------------------------
object AtikEgit()
{
    Baslik("3) Atık sınıflandırma (Görüntü sınıflandırma, transfer learning)");

    var resimKlasoru = Path.Combine(veriKlasoru, "atik", "dataset-resized");
    var fotograflar = Directory.GetDirectories(resimKlasoru)
        .SelectMany(d => Directory.GetFiles(d, "*.jpg").Select(f => new AtikFotografi
        {
            Resim = File.ReadAllBytes(f),
            Sinif = Path.GetFileName(d),
        }))
        .ToList();
    foreach (var g in fotograflar.GroupBy(f => f.Sinif))
        Console.WriteLine($"  {AtikSiniflari.Turkce[g.Key],-10} {g.Count()} fotoğraf");

    var veri = ml.Data.ShuffleRows(ml.Data.LoadFromEnumerable(fotograflar), seed: 42);
    var bolum = ml.Data.TrainTestSplit(veri, testFraction: 0.2, seed: 42);

    // Hazır eğitilmiş ResNet modelinin son katmanını bizim 6 sınıfa göre yeniden eğitiyoruz (transfer learning)
    var secenekler = new ImageClassificationTrainer.Options
    {
        FeatureColumnName = "Resim",
        LabelColumnName = "Label",
        Arch = ImageClassificationTrainer.Architecture.ResnetV250,
        Epoch = 50,
        BatchSize = 10,
        LearningRate = 0.01f,
        WorkspacePath = Path.Combine(modelKlasoru, "atik_calisma"),
        MetricsCallback = x => { if (x.Train != null && x.Train.Epoch % 10 == 0) Console.WriteLine($"  tur {x.Train.Epoch}: doğruluk %{x.Train.Accuracy * 100:F1}"); },
    };

    var pipeline = ml.Transforms.Conversion.MapValueToKey("Label", "Sinif")
        .Append(ml.MulticlassClassification.Trainers.ImageClassification(secenekler))
        .Append(ml.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

    Console.WriteLine("Eğitim başlıyor (ilk seferde birkaç dakika sürer)...");
    var model = pipeline.Fit(bolum.TrainSet);
    var m = ml.MulticlassClassification.Evaluate(model.Transform(bolum.TestSet));

    Console.WriteLine($"Doğruluk (test fotoğraflarında): %{m.MicroAccuracy * 100:F1}");
    Console.WriteLine(m.ConfusionMatrix.GetFormattedConfusionTable());

    ml.Model.Save(model, veri.Schema, Path.Combine(modelKlasoru, "atik_model.zip"));
    return new { fotografSayisi = fotograflar.Count, dogruluk = Math.Round(m.MicroAccuracy, 3) };
}

// ------------------------------------------------------------
// Yardımcılar
// ------------------------------------------------------------
static void Baslik(string s) => Console.WriteLine($"\n==================== {s} ====================");

static string ProjeKokunuBul()
{
    var d = new DirectoryInfo(AppContext.BaseDirectory);
    while (d != null && !File.Exists(Path.Combine(d.FullName, "MlOdev.sln"))) d = d.Parent;
    return d?.FullName ?? throw new Exception("Proje kökü (MlOdev.sln) bulunamadı.");
}

/// <summary>Tırnak içinde virgül ve satır sonu olabilen CSV dosyasını okur.</summary>
static List<List<string>> CsvOku(string yol, char ayrac)
{
    var sonuc = new List<List<string>>();
    var metin = File.ReadAllText(yol, Encoding.UTF8);
    var satir = new List<string>();
    var alan = new StringBuilder();
    var tirnakta = false;
    for (int i = 0; i < metin.Length; i++)
    {
        var c = metin[i];
        if (tirnakta)
        {
            if (c == '"' && i + 1 < metin.Length && metin[i + 1] == '"') { alan.Append('"'); i++; }
            else if (c == '"') tirnakta = false;
            else alan.Append(c);
        }
        else if (c == '"') tirnakta = true;
        else if (c == ayrac) { satir.Add(alan.ToString()); alan.Clear(); }
        else if (c == '\n')
        {
            satir.Add(alan.ToString().TrimEnd('\r')); alan.Clear();
            sonuc.Add(satir); satir = new List<string>();
        }
        else alan.Append(c);
    }
    if (alan.Length > 0 || satir.Count > 0) { satir.Add(alan.ToString()); sonuc.Add(satir); }
    return sonuc;
}
