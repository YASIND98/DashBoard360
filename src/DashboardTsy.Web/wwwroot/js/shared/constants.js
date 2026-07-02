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

var SCORE_CARD_BASE_URL = "https://api-pupa.apps.prod.deniz.denizbank.com/esbnode.asmx/api";

// ServiceBus OAuth (client_credentials) -> skor kart servis çağrılarına eklenecek Bearer token
var SERVICEBUS_TOKEN_URL = "https://esb.deniz.denizbank.com/ServiceBus";
var SERVICEBUS_CLIENT_ID = "YonetimRaporToPupa";
var SERVICEBUS_CLIENT_SECRET = "19ea09f7714271b98b79332c36b16908262ea5ddc9b8fdb8a687981f1318c97f";
// audience = skor kart servisinin origin'i; SCORE_CARD_BASE_URL'den türetilir (tek kaynak)
var SERVICEBUS_AUDIENCE = new URL(SCORE_CARD_BASE_URL).origin;

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
  3: "SDK",
  4: "Özel Bankacılık"
};

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
  17: "TARIM-ST",
  19: "Mobil Satış",
  20: "ÖB Ve Yatırım",
  21: "ŞUBE Müdürü SKORKART",
  22: "KOBI DENIZ",
  23: "BÖLGE Sorumlusu",
  24: "Satış Müdürü",
  25: "Bireysel Mikro",
  26: "Bireysel Kitle",
  27: "Merkezi Portföy Yönetimi",
  28: "BHT",
  29: "BD-2",
  31: "KIBRIS-BİREYSEL – SY",
  32: "KIBRIS-BİREYSEL – BD",
  33: "KIBRIS-TİCARİ",
  34: "KIBRIS-OBİ",
  35: "KIBRIS-KBİ",
  36: "KIBRIS-MİS-NİS",
  37: "BÖLGE Müdürü",
  38: "Kamu Portföy",
  39: "Ticari Merkez BD",
  40: "Maaş Müşterisi"
};

//Sub tab olarak gösterilecek skor kartlar
var SCORE_CARD_GROUPS = [
  { label: "Bireysel", keys: [1, 2] },
  { label: "KOBİ", keys: [5, 13] }
];

// "Genel Bakış" sekmesi (Key -1) skor kart servisinden dönmez; ön yüzde eklenir.
var SCORE_CARD_OVERVIEW_KEY = -1;

var SCORE_CARD_OVERVIEW_STATIC_COLUMNS = {
  SUBE_ADI: "Şube Adı",
  BOLGE_ADI: "Bölge Adı"
};

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
    { key: "UYMAYAN_KURAL", label: "Gerçekleşmeme Nedeni" }
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