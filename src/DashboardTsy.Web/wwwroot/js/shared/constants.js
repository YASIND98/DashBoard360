var TOP10_PRODUCT_NAMES = [
/*    "Çalışma Büyüklüğü",
    "Aktif Büyüklük",
  
    "TL Nakit Krediler",
    "TL Bireysel Takasitli Krediler",
    "TL İhtiyaç Kredileri - GPL",
    "Oto",
    "Konut",
    "TL Ticari Taksitli Krediler",
    "YP Nakit Krediler",
    "TL G.Nakit Krediler",
    "YP G.Nakit Krediler",
    "Yatırım Fonları",
    "Leasing",
    "Factoring", */
    "KMH - Kurtaran Hesap",
    "TL İhtiyaç Kredileri - GPL",
    "Oto",
    "Konut",
    "Kredi Kartı",
    "TL Rotatif Spot Diğer Krediler",
    "Üretici Kart",
    "Vadesiz TL",
    "Vadesiz YP",
    "Vadeli TL",
    "Vadeli YP",
    "Mevduat"
];

// Skor kart servisleri relative path ile Web controller (proxy) üzerinden çağrılır (ör. /scorecard/...).

// scorcard/authorities servisi -> sabit istek parametreleri
var PUPA_APPLICATION_CODE = "Surfacepluspupa";

// Period tipi: aylik/ceyreklik/yillik -> periodTypes query parametresi
var PUPA_PERIOD_TYPE = {
  aylik: 1,
  ceyreklik: 2,
  yillik: 3
};

// Pupa tipi: Key -> ön yüzde gösterilecek statik etiket
var PUPA_TYPE_LABELS = {
  1: "Pazarlama",
  2: "Operasyon",
  3: "ŞDK",
  4: "Özel Bankacılık",
  "-1": "Yönetim"
};

// "Yönetim" (Key -1) pupa-types servisinden dönmez; ön yüzde statik olarak en sona eklenir.
var PUPA_TYPE_MANAGEMENT_KEY = -1;

// "Özel Bankacılık" (Key 4): bu pupa tipinde skor kart sekmelerine "Genel Bakış" eklenmez.
var PUPA_TYPE_PRIVATE_BANKING_KEY = 4;

// dashboard/score-cards Key -> ön yüzde gösterilecek skor kart etiketi
var SCORE_CARD_LABELS = {
  "-1": "Genel Bakış",
  1: "SY",
  2: "BD",
  3: "KB",
  4: "KBT",
  5: "KBİ",
  6: "Tarım",
  7: "NKOS",
  8: "Gişe Satış Sorumlusu",
  9: "Gişe",
  10: "Dinamik MİSS-NİSS",
  11: "ŞOY",
  12: "Nakit İşlem Sorumlusu",
  13: "OBI",
  14: "Ticari",
  15: "Kurumsal",
  16: "MİS-NİS",
  17: "Tarım-ST",
  19: "Mobil Satış",
  20: "ÖB Ve Yatırım",
  21: "Şube Müdürü",
  22: "KOBI Deniz",
  23: "Bölge Sorumlusu",
  24: "Bölge Pazarlama Müdürü",
  25: "Bireysel Mikro",
  26: "Bireysel Kitle",
  27: "Merkezi Portföy Yönetimi",
  28: "BHT",
  29: "BD-2",
  31: "Kıbrıs-Bireysel – SY",
  32: "Kıbrıs-Bireysel – BD",
  33: "Kıbrıs-Ticari",
  34: "Kıbrıs-OBİ",
  35: "Kıbrıs-KBİ",
  36: "Kıbrıs-MİS-NİS",
  37: "Bölge Müdürü",
  38: "Kamu Portföy",
  39: "Ticari Merkez BD",
  40: "Maaş Müşterisi"
};

//Sub tab olarak gösterilecek skor kartlar
var SCORE_CARD_GROUPS = [
  { label: "Bireysel", keys: [1, 2] },
  { label: "KOBİ", keys: [5, 13] },
  { label: "Kıbrıs", keys: [31, 32, 33, 34, 35, 36] }
];

// "Genel Bakış" sekmesi (Key -1) skor kart servisinden dönmez; ön yüzde eklenir.
var SCORE_CARD_OVERVIEW_KEY = -1;

// "Yönetim" pupa tipinde score-cards servisine gidilmez; statik olarak bu skor kartları listelenir.
var SCORE_CARD_MANAGEMENT_KEYS = [21, 24, 37];

// Bu skor kartlar seçildiğinde alt sekmeler scorecard/types servisinden gelen skor kart tipleridir
var SCORE_CARD_TYPE_TAB_IDS = [19, 20, 22, 24, 27];

// scorecard/types servisinden dönse bile ön yüzde gösterilmeyecek skor kart tip id'leri
var SCORE_CARD_HIDDEN_TYPE_IDS = [9];

var SCORE_CARD_OVERVIEW_STATIC_COLUMNS = {
  SUBE_ADI: "Şube Adı",
  BOLGE_ADI: "Bölge Adı"
};

// Genel Bakış özet tablosunda servis dönse bile gizlenecek HG kolonları
var SCORE_CARD_OVERVIEW_HIDDEN_KEYS = {
  HG31: true,
  HG32: true,
  HG33: true,
  HG34: true,
  HG35: true,
  HG36: true,
};

// Genel Bakış bölge özetinde servis dönse bile gizlenecek bölge kodları (satır listelenmez)
var SCORE_CARD_OVERVIEW_HIDDEN_REGION_CODES = [62];

// Skor Kart - Hedef Detayı kolon eşlemesi
var SCORE_CARD_DETAIL_COLUMN_LABELS = [
    { key: "ACILIS_TARIHI", label: "Açılış Tarihi" },
    { key: "KAZANIM_URUN_ADI", label: "Ürün 1" },
    { key: "KAZANIM_URUN_ADI_2", label: "Ürün 2" },
    { key: "KAZANIM_URUN_ADI_3", label: "Ürün 3" },
    { key: "ACCOUNT_NUMBER", label: "Hesap Numarası" },
    { key: "KAZANIM", label: "Kazanım" },
    { key: "MUST_DURUM", label: "Müşteri Durumu" },
    { key: "MUST_NO", label: "Müşteri No" },
    { key: "UYMAYAN_KURAL", label: "Gerçekleşmeme Nedeni" },
    { key: "MUSTERI_GRUP", label: "Müşteri Grubu" },
    { key: "ONCEKI_TOPLAM_VARLIK", label: "Önceki Toplam Varlık" },
    { key: "KAZANIM_SONRASI_VARLIK", label: "Kazanım Sonrası Varlık" },
    { key: "ILAVE_ALINAN_VARLIK", label: "İlave Alınan Varlık" },
    { key: "SKORKARTA_YAZMA_TARIHI", label: "Skorkarta Yazma Tarihi" },
    { key: "VARLIK_KAZANIM_KONTROL_TARIHI", label: "Varlık Kazanım Kontrol Tarihi" },
    { key: "VARLIK_KAZANIM_TARIHI", label: "Varlık Kazanım Tarihi" },
    { key: "SKORKART_DETAYI", label: "Skorkart Detayı" },
    { key: "ELEME_GRUP", label: "Eleme Grubu" },
    { key: "URUN", label: "Ürün" },
    { key: "ACIKLAMA", label: "Açıklama" }
];

// Skor kart tablosu kolon başlıkları
var SCORE_CARD_REPORT_COLUMNS = ['', 'Ürün / Hedef Adı', 'Ürün Tipi', 'Hedef', 'Gerçekleşen', 'H/G %', 'Ağırlık %', 'Ağırlıklı H/G %', 'Bekleyen', 'Detay'];

// scorecards/details servisi -> gönderilecek status kodu
var SCORE_CARD_DETAIL_STATUS = {
    realized: 1,
    pending: 0,
    unrealized: -1
};

// Trend analizi sekmesi -> scorecards/trends trendPeriod parametresi
var SCORE_CARD_TREND_PERIOD = {
    "bu-ay": 1,
    "son-3-ay": 3,
    "son-6-ay": 6,
    "son-12-ay": 12
};