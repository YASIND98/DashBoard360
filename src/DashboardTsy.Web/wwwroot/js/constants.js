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

// Skor Kart - Hedef Detayı kolon eşlemesi
var SCORE_CARD_DETAIL_COLUMN_LABELS = [
    { key: "ACILIS_TARIHI", label: "Açılış Tarihi" },
    { key: "KAZANIM_URUN_ADI", label: "Ürün 1" },
    { key: "KAZANIM_URUN_ADI_2", label: "Ürün 2" },
    { key: "KAZANIM_URUN_ADI_3", label: "Ürün 3" },
    { key: "ACCOUNT_NUMBER", label: "Hesap Numarası" },
    { key: "KAZANIM", label: "Kazanım" },
    { key: "MUST_DURUM", label: "Müşteri Durumu" },
    { key: "DOB_BAYI", label: "DOB Bayi" }
];

// scorecards/details servisi -> gönderilecek status kodu
var SCORE_CARD_DETAIL_STATUS = {
    realized: 1,
    pending: 0,
    unrealized: -1,
    offtarget: 2
};

var SCORE_CARD_BASE_URL = "http://api-pupa.v3.dev.intertech.com.tr/api";

// users/authorities servisi -> sabit istek parametreleri
var PUPA_USER_CODE = "INTERTECH\\FURKANAI";
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

// dashboard/score-cards statik roleCode
var PUPA_SCORECARDS_ROLE_CODE = 1038;

// dashboard/score-cards Key -> ön yüzde gösterilecek skor kart etiketi
var SCORE_CARD_LABELS = {
    1: "SY",
    2: "BD",
    3: "KB",
    4: "KBT",
    5: "KBİ",
    6: "TARIM",
    7: "NKOS",
    8: "GİŞE SATIŞ SORUMLUSU",
    9: "GİŞE",
    10: "Dinamik MİSS-NİSS",
    11: "ŞOY",
    12: "NAKİT İŞLEM SORUMLUSU",
    13: "OBI",
    14: "TİCARİ",
    15: "KURUMSAL",
    16: "MİS-NİS",
    17: "TARIM-ST",
    19: "Mobil Satış",
    20: "ÖB VE YATIRIM",
    21: "ŞUBE MÜDÜRÜ SKORKART",
    22: "KOBI DENIZ",
    23: "BÖLGE SORUMLUSU",
    24: "SATIŞ MÜDÜRÜ",
    25: "BİREYSEL MİKRO",
    26: "BİREYSEL KİTLE",
    27: "Merkezi Portföy Yönetimi",
    28: "BHT",
    29: "BD-2",
    31: "KIBRIS-BİREYSEL – SY",
    32: "KIBRIS-BİREYSEL – BD",
    33: "KIBRIS-TİCARİ",
    34: "KIBRIS-OBİ",
    35: "KIBRIS-KBİ",
    36: "KIBRIS-MİS-NİS",
    37: "BÖLGE MÜDÜRÜ",
    38: "KAMU PORTFÖY",
    39: "Ticari Merkez BD",
    40: "Maaş Müşterisi"
};

//Sub tab olarak gösterilecek skor kartlar
var SCORE_CARD_GROUPS = [
    { label: "Bireysel", keys: [1, 2] },
    { label: "KOBİ", keys: [5, 13] }
];

// Skor kart tablosu kolon başlıkları
var SCORE_CARD_REPORT_COLUMNS = ['', 'Ürün / Hedef Adı', 'Ürün Tipi', 'Hedef', 'Gerçekleşen', 'H/G %', 'Ağırlık %', 'Ağırlıklı H/G %', 'Bekleyen', 'Detay'];

// Trend analizi sekmesi -> scorecards/trends trendPeriod parametresi
var SCORE_CARD_TREND_PERIOD = {
    "bu-ay": 1,
    "son-3-ay": 3,
    "son-6-ay": 6,
    "son-12-ay": 12
};