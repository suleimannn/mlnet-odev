using Microsoft.ML.Data;

namespace Ortak;

// ============================================================
// 1) İKİNCİ EL ARABA FİYATI  (Regresyon = sayı tahmini)
// ============================================================

/// <summary>Modele verilen bir araba ilanı. Fiyat = öğrenilecek değer (Label).</summary>
public class ArabaIlani
{
    public string Marka { get; set; } = "";
    public string Seri { get; set; } = "";       // Clio, Megane, Focus...
    public float ModelYili { get; set; }
    public string Yakit { get; set; } = "";
    public string Vites { get; set; } = "";
    public string MotorHacmi { get; set; } = "";
    public string KasaTipi { get; set; } = "";
    public string Kimden { get; set; } = "";     // Galeriden / Sahibinden
    public float Km { get; set; }

    [ColumnName("Label")]
    public float Fiyat { get; set; }
}

/// <summary>Modelin cevabı: tahmini fiyat.</summary>
public class ArabaTahmini
{
    [ColumnName("Score")]
    public float TahminiFiyat { get; set; }
}

// ============================================================
// 2) SPAM SMS  (İkili sınıflandırma = evet/hayır)
// ============================================================

public class SmsMesaji
{
    public string Mesaj { get; set; } = "";

    [ColumnName("Label")]
    public bool SpamMi { get; set; }
}

public class SmsTahmini
{
    [ColumnName("PredictedLabel")]
    public bool SpamMi { get; set; }

    /// <summary>0-1 arası: mesajın spam olma olasılığı.</summary>
    public float Probability { get; set; }
}

// ============================================================
// 3) ATIK SINIFLANDIRMA  (Görüntü sınıflandırma)
// ============================================================

public class AtikFotografi
{
    public byte[] Resim { get; set; } = [];
    public string Sinif { get; set; } = "";   // eğitimde kullanılır
}

public class AtikTahmini
{
    [ColumnName("PredictedLabel")]
    public string Sinif { get; set; } = "";

    /// <summary>Her sınıf için olasılıklar.</summary>
    public float[] Score { get; set; } = [];
}

public static class AtikSiniflari
{
    /// <summary>Klasör adı (İngilizce) -> ekranda görünen Türkçe ad.</summary>
    public static readonly Dictionary<string, string> Turkce = new()
    {
        ["cardboard"] = "Karton",
        ["glass"] = "Cam",
        ["metal"] = "Metal",
        ["paper"] = "Kağıt",
        ["plastic"] = "Plastik",
        ["trash"] = "Diğer çöp",
    };
}
