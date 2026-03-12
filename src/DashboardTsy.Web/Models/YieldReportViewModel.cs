namespace DenizBank360.Models;

public class YieldReportViewModel
{
    public string KullaniciAdi { get; set; } = "Ayşe Demir";
    public string Tarih { get; set; } = DateTime.Now.ToString("dd.MM.yyyy");
    public List<YieldMonthColumn> MonthColumns { get; set; } = new();
    public List<YieldRegionRow> Rows { get; set; } = new();
}

public class YieldMonthColumn
{
    public string Label { get; set; } = "";
}

public class YieldRegionRow
{
    public string BolgeAdi { get; set; } = "";
    public List<double> MonthlyPercents { get; set; } = new();
    public double KurumsalPercent { get; set; }
    public double TicariPercent { get; set; }
    public double KobiKbiPercent { get; set; }
    public double KobiObiPercent { get; set; }
    public double TarimPercent { get; set; }
    public double BireyselKitlePercent { get; set; }
    public double BireyselAfiliPercent { get; set; }
    public double BireyselObPercent { get; set; }
}

public static class YieldReportDataHelper
{
    public static YieldReportViewModel GetSampleData()
    {
        return new YieldReportViewModel
        {
            MonthColumns = new List<YieldMonthColumn>
            {
                new() { Label = "Ara '25" },
                new() { Label = "Oca '26" },
                new() { Label = "Şub '26" }
            },
            Rows = new List<YieldRegionRow>
            {
                new() {
                    BolgeAdi = "Güney Ege",
                    MonthlyPercents = new() { 72, 85, 91 },
                    KurumsalPercent = 88, TicariPercent = 76,
                    KobiKbiPercent = 82, KobiObiPercent = 69,
                    TarimPercent = 55,
                    BireyselKitlePercent = 90, BireyselAfiliPercent = 78, BireyselObPercent = 65
                },
                new() {
                    BolgeAdi = "Kuzey Ege",
                    MonthlyPercents = new() { 68, 79, 84 },
                    KurumsalPercent = 81, TicariPercent = 72,
                    KobiKbiPercent = 75, KobiObiPercent = 63,
                    TarimPercent = 60,
                    BireyselKitlePercent = 85, BireyselAfiliPercent = 71, BireyselObPercent = 58
                },
                new() {
                    BolgeAdi = "Marmara",
                    MonthlyPercents = new() { 90, 93, 97 },
                    KurumsalPercent = 95, TicariPercent = 88,
                    KobiKbiPercent = 91, KobiObiPercent = 84,
                    TarimPercent = 42,
                    BireyselKitlePercent = 96, BireyselAfiliPercent = 89, BireyselObPercent = 77
                },
                new() {
                    BolgeAdi = "İç Anadolu",
                    MonthlyPercents = new() { 65, 71, 78 },
                    KurumsalPercent = 74, TicariPercent = 68,
                    KobiKbiPercent = 70, KobiObiPercent = 58,
                    TarimPercent = 80,
                    BireyselKitlePercent = 79, BireyselAfiliPercent = 65, BireyselObPercent = 52
                },
                new() {
                    BolgeAdi = "Akdeniz",
                    MonthlyPercents = new() { 78, 82, 88 },
                    KurumsalPercent = 85, TicariPercent = 79,
                    KobiKbiPercent = 80, KobiObiPercent = 72,
                    TarimPercent = 75,
                    BireyselKitlePercent = 88, BireyselAfiliPercent = 74, BireyselObPercent = 61
                }
            }
        };
    }
}
