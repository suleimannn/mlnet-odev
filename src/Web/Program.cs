// ============================================================
// WEB ARAYÜZÜ
// Eğitilmiş modelleri (modeller/*.zip) yükler ve tarayıcıdan denemeyi sağlar.
// Çalıştırma: dotnet run --project src/Web   ->  http://localhost:5080
// ============================================================
using Microsoft.ML;
using Microsoft.ML.Data;
using Ortak;

// Paketlenmiş (flash bellek) sürümde wwwroot ve modeller exe'nin yanında durur.
var paketli = Directory.Exists(Path.Combine(AppContext.BaseDirectory, "wwwroot"));
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = paketli ? AppContext.BaseDirectory : null,
});
builder.WebHost.UseUrls("http://localhost:5080");
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

var modelKlasoru = paketli
    ? Path.Combine(AppContext.BaseDirectory, "modeller")
    : Path.Combine(ProjeKokunuBul(app.Environment.ContentRootPath), "modeller");

// Paketli sürümde sunucu açılınca tarayıcıyı kendisi açsın
if (paketli)
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        Console.WriteLine("\nUygulama çalışıyor: http://localhost:5080  (kapatmak için bu pencereyi kapatın)");
        if (Environment.GetEnvironmentVariable("MLODEV_TARAYICI_ACMA") == "1") return;
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("http://localhost:5080") { UseShellExecute = true }); } catch { }
    });
var ml = new MLContext();

// Modeller ilk istekte yüklenir (dosya henüz yoksa sonraki istekte tekrar denenir).
// PredictionEngine aynı anda tek istek için güvenli olduğundan lock kullanıyoruz.
var araba = new ModelTutucu<PredictionEngine<ArabaIlani, ArabaTahmini>>(() => Yukle<ArabaIlani, ArabaTahmini>("araba_model.zip"));
var sms = new ModelTutucu<PredictionEngine<SmsMesaji, SmsTahmini>>(() => Yukle<SmsMesaji, SmsTahmini>("sms_model.zip"));
var atik = new ModelTutucu<PredictionEngine<AtikFotografi, AtikTahmini>>(() => Yukle<AtikFotografi, AtikTahmini>("atik_model.zip"));

app.MapGet("/api/sonuclar", () => Dosya("sonuclar.json"));
app.MapGet("/api/araba/secenekler", () => Dosya("araba_secenekler.json"));

app.MapPost("/api/araba/tahmin", (ArabaIlani ilan) =>
{
    if (araba.Value is not { } motor) return ModelYok("araba");
    lock (motor) return Results.Ok(new { fiyat = Math.Max(0, Math.Round(motor.Predict(ilan).TahminiFiyat / 250) * 250) });
});

app.MapPost("/api/sms/tahmin", (SmsMesaji mesaj) =>
{
    if (sms.Value is not { } motor) return ModelYok("sms");
    if (string.IsNullOrWhiteSpace(mesaj.Mesaj)) return Results.BadRequest(new { hata = "Mesaj boş olamaz." });
    lock (motor)
    {
        var t = motor.Predict(mesaj);
        return Results.Ok(new { spamMi = t.SpamMi, olasilik = Math.Round(t.Probability, 3) });
    }
});

// Formu elle okuyoruz: böylece hem .NET 7'de hem daha yeni sürümlerde (form güvenlik kuralına takılmadan) çalışır.
app.MapPost("/api/atik/tahmin", async (HttpRequest istek) =>
{
    if (atik.Value is not { } motor) return ModelYok("atik");
    if (!istek.HasFormContentType) return Results.BadRequest(new { hata = "Fotoğraf gönderilmedi." });
    var resim = (await istek.ReadFormAsync()).Files["resim"];
    if (resim is null) return Results.BadRequest(new { hata = "Fotoğraf gönderilmedi." });
    using var ms = new MemoryStream();
    await resim.CopyToAsync(ms);
    // Model sadece JPEG okuyabiliyor (arayüz her resmi zaten JPEG'e çevirip gönderiyor).
    // JPEG olmayan dosyada model hata vermeden saçma sonuç döndürdüğü için burada reddediyoruz.
    var b = ms.GetBuffer();
    if (ms.Length < 4 || b[0] != 0xFF || b[1] != 0xD8)
        return Results.BadRequest(new { hata = "Bu dosya okunamadı. Lütfen bir fotoğraf (JPG/PNG) yükleyin." });
    lock (motor)
    {
        var t = motor.Predict(new AtikFotografi { Resim = ms.ToArray() });

        // Score dizisindeki sıranın hangi sınıfa ait olduğunu modelin kendisinden okuyoruz
        VBuffer<ReadOnlyMemory<char>> adlar = default;
        motor.OutputSchema["Label"].GetKeyValues(ref adlar);
        var olasiliklar = adlar.DenseValues().Select((a, i) => new
        {
            sinif = AtikSiniflari.Turkce.GetValueOrDefault(a.ToString(), a.ToString()),
            oran = Math.Round(t.Score[i], 3),
        }).OrderByDescending(x => x.oran);

        return Results.Ok(new { sinif = AtikSiniflari.Turkce.GetValueOrDefault(t.Sinif, t.Sinif), olasiliklar });
    }
});

app.Run();

// ------------------------------------------------------------
PredictionEngine<TGirdi, TCikti>? Yukle<TGirdi, TCikti>(string dosya) where TGirdi : class where TCikti : class, new()
{
    var yol = Path.Combine(modelKlasoru, dosya);
    if (!File.Exists(yol)) return null;
    var model = ml.Model.Load(yol, out _);
    return ml.Model.CreatePredictionEngine<TGirdi, TCikti>(model);
}

IResult Dosya(string ad)
{
    var yol = Path.Combine(modelKlasoru, ad);
    return File.Exists(yol) ? Results.File(yol, "application/json") : Results.NotFound();
}

static IResult ModelYok(string ad) =>
    Results.Problem($"'{ad}' modeli bulunamadı. Önce eğitimi çalıştırın: dotnet run --project src/Egitim -- {ad}", statusCode: 503);

static string ProjeKokunuBul(string baslangic)
{
    var d = new DirectoryInfo(baslangic);
    while (d != null && !File.Exists(Path.Combine(d.FullName, "MlOdev.sln"))) d = d.Parent;
    return d?.FullName ?? throw new Exception("Proje kökü (MlOdev.sln) bulunamadı.");
}

/// <summary>Modeli bir kez yükler; dosya yoksa null döner ve sonraki çağrıda tekrar dener.</summary>
class ModelTutucu<T> where T : class
{
    private readonly Func<T?> yukleyici;
    public ModelTutucu(Func<T?> yukleyici) => this.yukleyici = yukleyici;
    private T? _deger;
    private readonly object _kilit = new();
    public T? Value { get { lock (_kilit) return _deger ??= yukleyici(); } }
}
