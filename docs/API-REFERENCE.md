# DashboardTsy API — Endpoint Referans Dökümanı

> Mobil ekip için hazırlanmıştır. Kaynak: `src/DashboardTsy.Api/Controllers/*` (2026-08-06 itibarıyla).
> Örnek request/response gövdeleri, DTO tanımlarından üretilen **şema-doğru statik örneklerdir** (gerçek prod verisi değildir).

## İçindekiler

1. [Genel Bilgiler](#genel-bilgiler)
2. [AiInsight](#1-aiinsight)
3. [AppSettings](#2-appsettings)
4. [CacheAdmin](#3-cacheadmin)
5. [ExchangeRate](#4-exchangerate)
6. [NplReport](#5-nplreport)
7. [Public (Auth/Login)](#6-public-authlogin)
8. [SalaryCustomerReport](#7-salarycustomerreport)
9. [ScoreCard](#8-scorecard)
10. [TargetReport](#9-targetreport)
11. [ProductivityReport](#10-productivityreport)
12. [Bilinen Kısıtlar ve Uyarılar](#bilinen-kısıtlar-ve-uyarılar)

---

## Genel Bilgiler

### Base URL

Route'larda **`/api` prefix'i yoktur.** Controller'lardaki bazı XML yorumlarda `/api/...` geçse de gerçek route budur:

```
https://<host>/<Controller>/<Action>
```

Örnek: `POST https://<host>/TargetReport/GetDailyTargetReport`

### Kimlik Doğrulama (Auth)

- **ASP.NET seviyesinde `[Authorize]` YOK.** Hiçbir controller/action'da auth attribute'u bulunmuyor — API katmanı kendi başına tüm endpoint'leri anonim kabul ediyor.
- Oturum yönetimi uygulama (business) seviyesinde **`sessionId`** üzerinden yürüyor:
  1. Mobil/istemci önce [`/Public`](#6-public-authlogin) altındaki login endpoint'lerinden birine istek atar (`WindowsLogin`, `DomainLogin`, `Login`, `SessionLogin`).
  2. Dönen `UsersDto` içinden alınan oturum bilgisi, sonraki tüm rapor çağrılarının **request body'sindeki `SessionId` alanına** konur.
  3. Sunucu tarafında bu `SessionId` her rapor endpoint'inde zorunlu bir parametre olarak geçiyor (SP çağrılarına parametre olarak veriliyor).
- Gerçek erişim kontrolü (network/gateway seviyesi, Windows Auth vs.) API'nin dışında bir yerde yapılıyor olabilir — **bunu backend ekibiyle teyit edin.**
- **Header bazlı auth yok** — `Authorization: Bearer ...` gibi bir header hiçbir endpoint'te (`ScoreCard` hariç, o da farklı bir amaçla) okunmuyor.

### Ortak Header'lar

| Header | Zorunlu mu | Nerede | Açıklama |
|---|---|---|---|
| `Content-Type: application/json` | POST body olan tüm endpoint'lerde evet | Tümü | Standart JSON body |
| `ExternalContext` | Opsiyonel | Sadece `ScoreCard/*` (14 endpoint) | Değer varsa, ScoreCard controller'ı bunu upstream Pupa API'sine olduğu gibi forward ediyor. Mobil taraf bir context/correlation id göndermek isterse burayı kullanabilir — semantiği için backend'e sorun. |

### Genel Hata Davranışı

- `null`/boş body gönderilirse çoğu endpoint **`400 Bad Request`** döner.
- Bazı endpoint'ler (`TargetReport` içinde işaretliler) veri bulunamazsa **`404 Not Found`** döner.
- `ScoreCard` proxy endpoint'leri, upstream Pupa API token alamazsa **`502 Bad Gateway`** döner; diğer durumlarda upstream'in status code'unu olduğu gibi forward eder.
- Genel response sarmalayıcısı **yok** — bazı endpoint'ler çıplak DTO/list döner, `Public` controller'ı ise `ApiResponse<UsersDto>` ile sarmalıyor (bkz. [ApiResponse<T>](#apiresponset)).

### Ortak Response Sarmalayıcı Tipleri

#### ApiResponse&lt;T&gt;
Sadece `Public` (login) endpoint'lerinde kullanılıyor.

```json
{
  "result": { "...": "T tipine göre değişir" },
  "message": {
    "message": "İşlem başarılı.",
    "message2": null
  }
}
```

| Alan | Tip | Not |
|---|---|---|
| `result` | `T \| null` | Asıl veri |
| `message` | `MessageResult \| null` | `message` ve `message2` (her ikisi de nullable string) |

### Tarih/Kültür Formatı

API `tr-TR` culture ile çalışıyor: ondalık ayraç `.`, binlik ayraç `,`, tarih formatı `dd.MM.yyyy`. JSON'da `DateTime` alanları ISO-8601 (`2026-08-06T00:00:00`) olarak serialize edilir (ASP.NET Core varsayılanı; culture ayarı sadece server-side model binding/formatting için).

### CORS

Tanımlı **değil**. Mobil native istemciler için sorun olmaz; bir web istemci bu API'ye tarayıcıdan çağrı yapacaksa backend'e CORS eklenmesi gerekir.

---

## 1. AiInsight

### POST /AiInsight/GetBranchAiInsights

Şube bazlı AI-üretimi özet/yorum kayıtlarını döner.

- **Auth:** yok (business-level SessionId bu endpoint'te kullanılmıyor)
- **400 durumu:** `RegionCode` veya `BranchCode` boşsa

**Request** (`GetBranchAiInsightRequest`):
```json
{
  "regionCode": "35",
  "branchCode": "1234"
}
```

**Response** (`200 OK` → `GetBranchAiInsightResponse`):
```json
{
  "items": [
    {
      "id": 1,
      "region": "İzmir Bölgesi",
      "branchName": "Alsancak Şubesi",
      "branchCode": "1234",
      "summaryDate": "2026-08-05T00:00:00",
      "summary": "Şube bu hafta kredi kartı hedefinin %112'sini gerçekleştirdi.",
      "modelName": "gpt-4o-mini",
      "createdAt": "2026-08-06T08:00:00"
    }
  ]
}
```

---

## 2. AppSettings

### GET /app-settings

Tüm feature-flag / uygulama ayarlarını döner.

**Response** (`200 OK` → `AppSettingDto[]`):
```json
[
  {
    "key": "ENABLE_AI_INSIGHTS",
    "value": true,
    "type": 4,
    "description": "AI Insight kartlarını göster/gizle"
  },
  {
    "key": "DEFAULT_REPORT_DATE_RANGE_DAYS",
    "value": 30,
    "type": 2,
    "description": "Rapor ekranlarında varsayılan tarih aralığı"
  }
]
```

`type` alanı `AppSettingType` enum'udur: `1=String, 2=Int, 3=Decimal, 4=Boolean`.

### GET /app-settings/{key}

Tek bir ayarı `key` ile döner.

- **404:** key bulunamazsa

**Response** (`200 OK` → `AppSettingDto`):
```json
{
  "key": "ENABLE_AI_INSIGHTS",
  "value": true,
  "type": 4,
  "description": "AI Insight kartlarını göster/gizle"
}
```

---

## 3. CacheAdmin

> Not: Yorumda "Windows Auth ile API dışında korunuyor" deniyor ama kod seviyesinde auth attribute'u yok. Mobil ekibin normal akışta kullanması gerekmez — admin/ops amaçlı.

### POST /cache/invalidate?scope={scope}

Sunucu içi memory cache'i temizler.

- **Query param:** `scope` — `Target`, `Productivity`, `all`, veya boş. Tanınmayan değer → `400`.

**Response** (`200 OK`):
```json
{ "cleared": "all" }
```

---

## 4. ExchangeRate

### POST /ExchangeRate/GetUsdExchangeRates

Belirli bir tarihe göre T-1/T-2/T-7/T-365 USD kurlarını döner. **Şu an mock data dönüyor.**

- **400:** body null ise

**Request** (`GetUsdExchangeRatesRequest`):
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "reportDate": "2026-08-06T00:00:00"
}
```

**Response** (`200 OK` → `GetUsdExchangeRatesResponse`):
```json
{
  "yesterdayDate": "2026-08-05T00:00:00",
  "yesterdayRate": 41.235,
  "previousDayDate": "2026-08-04T00:00:00",
  "previousDayRate": 41.180,
  "previousWeekDate": "2026-07-30T00:00:00",
  "previousWeekRate": 40.910,
  "previousYearDate": "2025-08-06T00:00:00",
  "previousYearRate": 33.420
}
```

---

## 5. NplReport

### POST /NplReport/GetNplBalanceRatio

NPL (Non-Performing Loan) bakiye ve oran raporu — tarih bazlı satırlar.

**Request** (`GetNplBalanceRatioRequest`):
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "subeKodu": 1234,
  "bolgeKodu": 35,
  "yil": 2026,
  "parameters": {}
}
```
> `subeKodu`, `bolgeKodu`, `yil` nullable — filtre uygulanmayacaksa gönderilmeyebilir (`null`). `parameters` serbest key/value ek filtre alanı.

**Response** (`200 OK` → `GetNplBalanceRatioItem[]`):
```json
[
  {
    "reportDate": "2026-08-01T00:00:00",
    "balanceAnapara": 1250000.50,
    "balanceKof": 34500.75,
    "balanceToplam": 1284501.25,
    "ratioAnapara": 2.35,
    "ratioKof": 0.08
  }
]
```

### POST /NplReport/GetNplFilters

NPL ekranındaki filtre/dropdown seçeneklerini döner (Ürün, Yetki, İş Kolu, Tahsis Hattı grupları).

**Request** (`GetNplFiltersRequest`):
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000" }
```

**Response** (`200 OK` → `GetNplFiltersItem[]`):
```json
[
  {
    "filterGroupId": 1,
    "filterCode": "PRODUCT",
    "filterName": "Ürün",
    "displayOrder": 1,
    "isMultiSelect": true,
    "filterItemId": 101,
    "itemCode": "IHTIYAC",
    "itemName": "İhtiyaç Kredisi",
    "itemOrder": 1,
    "isDefault": true
  }
]
```

### POST /NplReport/GetNplProducts

NPL raporlarında kullanılan ürün lookup listesi (İhtiyaç, KMH, KK, ÜK, Traktör, Diğer).

**Request** (`GetNplProductsRequest`):
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000" }
```

**Response** (`200 OK` → `GetNplProductsItem[]`):
```json
[
  { "productId": 1, "productCode": "IHTIYAC", "productName": "İhtiyaç Kredisi" },
  { "productId": 2, "productCode": "KMH", "productName": "KMH" }
]
```

> Tüm NplReport endpoint'lerinde body `null` ise `400`.

---

## 6. Public (Auth/Login)

Oturum akışının başlangıç noktası. Bu endpoint'lerden dönen `UsersDto.userId`/oturum bilgisini sonraki tüm rapor çağrılarında `SessionId` olarak kullanacaksınız — **backend ekibiyle `SessionId`'nin tam olarak hangi alandan üretildiğini teyit edin**, DTO'da ayrı bir `SessionId` alanı yok, muhtemelen ayrı bir mekanizma (cookie/token) var.

> ⚠️ **Güvenlik notu:** `/Public/Login` şifreyi **query string** üzerinden GET ile alıyor (`?username=...&password=...`). Bu, şifrenin proxy/access loglarına düşmesi riski taşır. Mobil tarafta mümkünse `WindowsLogin`/`DomainLogin`/`SessionLogin` akışlarının kullanılması, `Login`'in son çare olması önerilir — bunu backend ile netleştirin.

### GET /Public/WindowsLogin?username={username}

**Response** (`200 OK` → `ApiResponse<UsersDto>`):
```json
{
  "result": {
    "userId": 42,
    "nameSurname": "Ayşe Yılmaz",
    "email": "ayse.yilmaz@denizbank.com",
    "password": null,
    "isAdmin": false,
    "createdDate": "2025-01-15T09:00:00",
    "type": 1,
    "passChange": false,
    "profilePhoto": null,
    "department": "Şube Operasyon",
    "branchCode": 1234,
    "branchName": "Alsancak Şubesi",
    "authority": "Şube Müdürü",
    "regionCode": 35,
    "domainName": "AYSEY",
    "isBlock": false,
    "updateSeen": true
  },
  "message": { "message": "Giriş başarılı.", "message2": null }
}
```

### GET /Public/DomainLogin?domainName={domainName}

Response şeması `WindowsLogin` ile aynıdır (`ApiResponse<UsersDto>`).

### GET /Public/Login?username={username}&password={password}

Response şeması aynıdır (`ApiResponse<UsersDto>`).

### GET /Public/SessionLogin?sessionId={sessionId}

Var olan bir oturumu `sessionId` ile devam ettirir. Response şeması aynıdır (`ApiResponse<UsersDto>`).

---

## 7. SalaryCustomerReport

Maaş/Emekli müşteri raporları ekranı. Tüm endpoint'ler `POST`, body `null` ise `400`.

### POST /SalaryCustomerReport/GetSalaryCustomerReportTabs

Ekranın üst/alt tab ağacını döner.

**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000" }
```
**Response** (`GetSalaryCustomerReportTabItem[]`):
```json
[
  { "tabId": 1, "tabName": "Hacim", "parentId": 0, "tabLevel": 1 },
  { "tabId": 2, "tabName": "Çapraz Satış Gelişimi", "parentId": 0, "tabLevel": 1 }
]
```

### POST /SalaryCustomerReport/GetSalaryCustomerVolumeReportHeaders

**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000", "reportDate": "2026-08-06T00:00:00" }
```
**Response** (`GetSalaryCustomerVolumeReportHeadersResponse`):
```json
{
  "productColumnName": "Ürün",
  "lastYearColumnName": "Geçen Yıl",
  "lastYearColumnDate": "2025-08-06T00:00:00",
  "lastYearDifferenceColumnName": "Fark",
  "lastWeekColumnName": "Geçen Hafta",
  "lastWeekColumnDate": "2026-07-30T00:00:00",
  "lastWeekDifferenceColumnName": "Fark",
  "previousDayColumnName": "Önceki Gün",
  "previousDayColumnDate": "2026-08-04T00:00:00",
  "previousDayDifferenceColumnName": "Fark",
  "yesterdayColumnName": "Dün",
  "yesterdayColumnDate": "2026-08-05T00:00:00",
  "salaryLabel": "Maaş",
  "retiredLabel": "Emekli"
}
```

### POST /SalaryCustomerReport/GetSalaryCustomerVolumeReport

**Request** (`GetSalaryCustomerVolumeReportRequest`):
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "regionCode": "35",
  "branchCode": "1234",
  "subTabId": 1,
  "reportDate": "2026-08-06T00:00:00",
  "showDifferences": true,
  "sortBy": 2,
  "isAscending": false
}
```
> `sortBy`: 1=Ürün Adı, 2=Geçen Yıl, 3=Geçen Hafta ... `regionCode`/`branchCode` opsiyonel filtre.

**Response** (`GetSalaryCustomerVolumeReportItem[]`, kısaltılmış — tüm alanlar için DTO katalogunu bkz.):
```json
[
  {
    "id": 1,
    "sortOrder": 1,
    "productName": "Vadesiz Mevduat",
    "lastYearTotalAmount": 1500000.00,
    "lastYearSalaryAmount": 1100000.00,
    "lastYearSalaryRate": 73.33,
    "lastYearRetiredAmount": 400000.00,
    "lastYearRetiredRate": 26.67,
    "lastYearTotalDifference": 125000.00,
    "lastYearTotalDifferenceStatus": 1,
    "lastYearSalaryDifference": 90000.00,
    "lastYearSalaryDifferenceStatus": 1,
    "lastYearRetiredDifference": 35000.00,
    "lastYearRetiredDifferenceStatus": 1,
    "lastWeekTotalAmount": 1600000.00,
    "lastWeekSalaryAmount": 1180000.00,
    "lastWeekSalaryRate": 73.75,
    "lastWeekRetiredAmount": 420000.00,
    "lastWeekRetiredRate": 26.25,
    "lastWeekTotalDifference": 5000.00,
    "lastWeekTotalDifferenceStatus": 1,
    "lastWeekSalaryDifference": 4000.00,
    "lastWeekSalaryDifferenceStatus": 1,
    "lastWeekRetiredDifference": 1000.00,
    "lastWeekRetiredDifferenceStatus": 1,
    "previousDayTotalAmount": 1610000.00,
    "previousDaySalaryAmount": 1185000.00,
    "previousDaySalaryRate": 73.6,
    "previousDayRetiredAmount": 425000.00,
    "previousDayRetiredRate": 26.4,
    "previousDayTotalDifference": 2000.00,
    "previousDayTotalDifferenceStatus": 1,
    "previousDaySalaryDifference": 1500.00,
    "previousDaySalaryDifferenceStatus": 1,
    "previousDayRetiredDifference": 500.00,
    "previousDayRetiredDifferenceStatus": 1,
    "yesterdayTotalAmount": 1612000.00,
    "yesterdaySalaryAmount": 1186500.00,
    "yesterdaySalaryRate": 73.6,
    "yesterdayRetiredAmount": 425500.00,
    "yesterdayRetiredRate": 26.4
  }
]
```

### POST /SalaryCustomerReport/GetSalaryCustomerCrossSellReportHeaders

**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000", "reportDate": "2026-08-06T00:00:00" }
```
**Response** (`GetSalaryCustomerCrossSellReportHeadersResponse`):
```json
{
  "productColumnName": "Ürün",
  "lastYearColumnName": "Geçen Yıl",
  "lastYearColumnDate": "2025-08-06T00:00:00",
  "lastYearDifferenceColumnName": "Fark",
  "twoMonthsAgoColumnName": "İki Ay Önce",
  "twoMonthsAgoColumnDate": "2026-06-06T00:00:00",
  "twoMonthsAgoDifferenceColumnName": "Fark",
  "lastMonthColumnName": "Geçen Ay",
  "lastMonthColumnDate": "2026-07-06T00:00:00",
  "salaryLabel": "Maaş",
  "retiredLabel": "Emekli"
}
```

### POST /SalaryCustomerReport/GetSalaryCustomerCrossSellReport

**Request** (`GetSalaryCustomerCrossSellReportRequest`) — `GetSalaryCustomerVolumeReportRequest` ile aynı şema:
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "regionCode": "35",
  "branchCode": "1234",
  "subTabId": 1,
  "reportDate": "2026-08-06T00:00:00",
  "showDifferences": true,
  "sortBy": 2,
  "isAscending": false
}
```

**Response** (`GetSalaryCustomerCrossSellReportItem[]`, kısaltılmış):
```json
[
  {
    "id": 1,
    "sortOrder": 1,
    "productName": "Kredi Kartı",
    "lastYearTotalAmount": 800000.00,
    "lastYearSalaryAmount": 600000.00,
    "lastYearSalaryRate": 75.0,
    "lastYearRetiredAmount": 200000.00,
    "lastYearRetiredRate": 25.0,
    "lastYearTotalDifference": 50000.00,
    "lastYearTotalDifferenceStatus": 1,
    "lastYearSalaryDifference": 40000.00,
    "lastYearSalaryDifferenceStatus": 1,
    "lastYearRetiredDifference": 10000.00,
    "lastYearRetiredDifferenceStatus": 1,
    "twoMonthsAgoTotalAmount": 820000.00,
    "twoMonthsAgoSalaryAmount": 615000.00,
    "twoMonthsAgoSalaryRate": 75.0,
    "twoMonthsAgoRetiredAmount": 205000.00,
    "twoMonthsAgoRetiredRate": 25.0,
    "twoMonthsAgoTotalDifference": 3000.00,
    "twoMonthsAgoTotalDifferenceStatus": 1,
    "twoMonthsAgoSalaryDifference": 2000.00,
    "twoMonthsAgoSalaryDifferenceStatus": 1,
    "twoMonthsAgoRetiredDifference": 1000.00,
    "twoMonthsAgoRetiredDifferenceStatus": 1,
    "lastMonthTotalAmount": 825000.00,
    "lastMonthSalaryAmount": 618000.00,
    "lastMonthSalaryRate": 74.9,
    "lastMonthRetiredAmount": 207000.00,
    "lastMonthRetiredRate": 25.1
  }
]
```

### POST /SalaryCustomerReport/GetSalaryCustomerBankShareReportHeaders

**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000", "reportDate": "2026-08-06T00:00:00" }
```
**Response** (`GetSalaryCustomerBankShareReportHeadersResponse`):
```json
{
  "productColumnName": "Ürün",
  "denizbankCreditGroupName": "DenizBank",
  "otherBanksCreditGroupName": "Diğer Bankalar",
  "walletShareGroupName": "Cüzdan Payı",
  "firstMonthName": "Temmuz",
  "secondMonthName": "Ağustos",
  "salaryCustomersLabel": "Maaş Müşterileri",
  "retiredCustomersLabel": "Emekli Müşterileri"
}
```

### POST /SalaryCustomerReport/GetSalaryCustomerBankShareReport

**Request** (`GetSalaryCustomerBankShareReportRequest`):
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "regionCode": "35",
  "branchCode": "1234",
  "subTabId": 1,
  "reportDate": "2026-08-06T00:00:00",
  "customerType": 1
}
```
> `customerType`: 1=Maaş Müşterileri, 2=Emekli Müşterileri

**Response** (`GetSalaryCustomerBankShareReportItem[]`):
```json
[
  {
    "id": 1,
    "sortOrder": 1,
    "productName": "Kredi Kartı",
    "valueType": 2,
    "denizbankFirstMonthValue": 450000.00,
    "denizbankSecondMonthValue": 470000.00,
    "otherBanksFirstMonthValue": 550000.00,
    "otherBanksSecondMonthValue": 530000.00,
    "walletShareFirstMonthRate": 45.0,
    "walletShareSecondMonthRate": 47.0,
    "walletShareSecondMonthRateStatus": 1
  }
]
```
> `valueType`: 1=Adet, 2=Hacim

---

## 8. ScoreCard

> **Bu grup diğerlerinden farklı çalışır.** Tüm 14 endpoint, Pupa API'sine yapılan bir **pass-through proxy**'dir:
> - Request body'si serbest JSON (`JsonElement`) — sabit bir DTO şeması yok, upstream Pupa API'nin beklediği şemaya göre gönderilmeli.
> - Response, upstream'den gelen ham JSON'un olduğu gibi forward edilmesidir.
> - API, arka planda `IScoreCardTokenService` ile Pupa API için bir OAuth Bearer token alır (mobil tarafın bilmesi/göndermesi gerekmez). **Token alınamazsa `502 Bad Gateway`.**
> - Upstream'in döndüğü her status code olduğu gibi forward edilir.
> - `ExternalContext` header'ı (varsa) upstream'e forward edilir.
>
> ⚠️ **Mobil ekip için önemli:** Bu endpoint'lerin gerçek request/response şemaları **Pupa API dokümantasyonundan** alınmalı — DashboardTsy tarafında DTO yok. Aşağıda sadece route/method/upstream path haritası verilmiştir.

| # | Method | Route | Upstream Pupa Path | Not |
|---|---|---|---|---|
| 1 | POST | `/scorecard/authorities` | `scorecard/authorities` | |
| 2 | GET | `/scorecard/periods?periodTypes={int}` | `scorecard/periods?periodTypes={int}` | Query param zorunlu |
| 3 | POST | `/scorecard/pupa-types` | `scorecard/pupa-types` | |
| 4 | POST | `/scorecard/score-cards` | `scorecard/score-cards` | |
| 5 | POST | `/scorecard/regions` | `scorecard/regions` | |
| 6 | POST | `/scorecard/branches` | `scorecard/branches` | |
| 7 | POST | `/scorecard/registers` | `scorecard/registers` | |
| 8 | POST | `/scorecard/cumulatives` | `scorecard/cumulatives` | |
| 9 | POST | `/scorecard/main-view-regions` | `scorecard/main-view-regions` | |
| 10 | POST | `/scorecard/main-view-branches` | `scorecard/main-view-branches` | |
| 11 | POST | `/scorecard/employee-order-summaries` | `scorecard/employee-order-summaries` | |
| 12 | POST | `/scorecard/details` | `scorecard/details` | |
| 13 | POST | `/scorecard/trends/product-sale-realized` | `scorecard/trends/product-sale-realized` | |
| 14 | POST | `/scorecard/types` | `scorecard/types` | |

**Örnek — GET /scorecard/periods?periodTypes=1**

Response, Pupa API'nin döndüğü ham JSON'dur, örnek (gerçek şema Pupa dokümantasyonundan doğrulanmalı):
```json
{
  "periods": [
    { "id": 1, "name": "2026-Q3", "startDate": "2026-07-01", "endDate": "2026-09-30" }
  ]
}
```

---

## 9. TargetReport

Günlük/aylık hedef gerçekleşme raporları. Ürün ağaçları **self-referential** (`subProducts` alanı kendi tipinden liste — ana ürün → alt ürün hiyerarşisi).

### GET /TargetReport/GetTargetReportMenuTexts?sessionId={sessionId}

Ekran başlıkları/menü metinlerini döner (localization amaçlı).

- **404:** kayıt bulunamazsa

**Response** (`GetTargetReportMenuTextsResponse`):
```json
{
  "screenTitle": "Hedef Raporları",
  "tabAllTitle": "Tümü",
  "tabCorporateTitle": "Kurumsal",
  "tabCommercialTitle": "Ticari",
  "tabSmeTitle": "KOBİ",
  "tabAgricultureTitle": "Tarım",
  "tabRetailTitle": "Perakende",
  "smeSubTabAllTitle": "Tümü",
  "smeSubTabKbiTitle": "KBİ",
  "smeSubTabObiTitle": "OBİ",
  "retailSubTabAllTitle": "Tümü",
  "retailSubTabGeneralTitle": "Genel",
  "retailSubTabAffiliateTitle": "Bağlı",
  "retailSubTabPrivateTitle": "Private",
  "amountSubTabTitle": "Tutar"
}
```

### POST /TargetReport/GetTargetReportFilters

**Request** (`GetTargetReportFiltersRequest`):
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "filterId": 1,
  "filterCode": ["12", "23", "45"]
}
```
> `filterCode` CSV benzeri kod listesi (SP'ye virgülle ayrılmış string olarak gidiyor, JSON'da array olarak gönderin).

**Response** (`GetTargetReportFiltersItem[]`):
```json
[
  { "code": "12", "name": "İzmir Bölgesi" },
  { "code": "23", "name": "Ankara Bölgesi" }
]
```

### POST /TargetReport/GetDailyTargetReport

**Request** (`GetDailyTargetReportRequest`):
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "tabId": 1,
  "subTabId": null,
  "reportDate": "2026-08-06T00:00:00",
  "regionId": [35],
  "branchId": [1234],
  "portfolioId": null,
  "searchText": null,
  "showDifferences": true,
  "sortBy": null,
  "isAscending": false,
  "productId": null,
  "userCode": null
}
```

**Response** (`GetDailyTargetReportResponse`):
```json
{
  "products": [
    {
      "productId": 1,
      "productName": "Vadesiz Mevduat",
      "parentProductId": null,
      "lastYearAmount": 1500000.00,
      "lastYearDate": "2025-08-06T00:00:00",
      "lastWeekAmount": 1600000.00,
      "lastWeekDate": "2026-07-30T00:00:00",
      "prevDayAmount": 1610000.00,
      "prevDayDate": "2026-08-04T00:00:00",
      "yesterdayAmount": 1612000.00,
      "yesterdayDate": "2026-08-05T00:00:00",
      "todayDate": "2026-08-06T00:00:00",
      "diffByPrevDayAmount": 2000.00,
      "diffByLastYearAmount": 112000.00,
      "diffByLastWeekAmount": 12000.00,
      "subProducts": []
    }
  ]
}
```

### POST /TargetReport/GetDailyTargetReportTableHeaders

**Request** (`GetDailyTargetReportTableHeadersRequest`):
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000" }
```

- **404:** kayıt bulunamazsa

**Response** (`GetDailyTargetReportTableHeadersResponse`):
```json
{
  "productNameTitle": "Ürün",
  "lastYearTitle": "Geçen Yıl",
  "lastYearDate": "2025-08-06T00:00:00",
  "lastWeekTitle": "Geçen Hafta",
  "lastWeekDate": "2026-07-30T00:00:00",
  "prevDayTitle": "Önceki Gün",
  "prevDayDate": "2026-08-04T00:00:00",
  "yesterdayTitle": "Dün",
  "yesterdayDate": "2026-08-05T00:00:00",
  "todayTitle": "Bugün",
  "todayDate": "2026-08-06T00:00:00",
  "diffByPrevDayTitle": "Önceki Güne Göre Fark",
  "diffByLastYearTitle": "Geçen Yıla Göre Fark",
  "diffByLastWeekTitle": "Geçen Haftaya Göre Fark"
}
```

### POST /TargetReport/GetDailyQuantityTargetReport

**Request** (`GetDailyQuantityTargetReportRequest`):
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "tabId": 1,
  "subTabId": null,
  "reportDate": "2026-08-06T00:00:00",
  "regionId": [35],
  "branchId": [1234],
  "portfolioId": null,
  "searchText": null,
  "showDifferences": true,
  "sortBy": null,
  "isAscending": false,
  "productId": null,
  "userCode": null
}
```

**Response** (`GetDailyQuantityTargetReportResponse`):
```json
{
  "products": [
    {
      "productId": 1,
      "productName": "Kredi Kartı Adedi",
      "parentProductId": null,
      "lastYearAmount": 320,
      "lastYearDate": "2025-08-06T00:00:00",
      "lastMonthAmount": 410,
      "lastMonthDate": "2026-07-06T00:00:00",
      "lastTwoMonthEarlierAmount": 395,
      "lastTwoMonthEarlierDate": "2026-06-06T00:00:00",
      "todayDate": "2026-08-06T00:00:00",
      "diffByLastTwoMonthEarlierAmount": 25,
      "diffByLastYearAmount": 105,
      "diffByLastMonthAmount": 15,
      "subProducts": []
    }
  ]
}
```

### POST /TargetReport/GetDailyQuantityTargetReportTableHeaders

**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000" }
```

**Response** (`GetDailyQuantityTargetReportTableHeadersResponse`):
```json
{
  "productNameTitle": "Ürün",
  "lastYearTitle": "Geçen Yıl",
  "lastYearDate": "2025-08-06T00:00:00",
  "lastMonthTitle": "Geçen Ay",
  "lastMonthDate": "2026-07-06T00:00:00",
  "lastTwoMonthEarlierTitle": "İki Ay Önce",
  "lastTwoMonthEarlierDate": "2026-06-06T00:00:00",
  "todayTitle": "Bugün",
  "todayDate": "2026-08-06T00:00:00",
  "diffByLastTwoMonthEarlierTitle": "İki Ay Öncesine Göre Fark",
  "diffByLastYearTitle": "Geçen Yıla Göre Fark",
  "diffByLastMonthTitle": "Geçen Aya Göre Fark"
}
```

### POST /TargetReport/GetMonthlyTargetReport

**Request** (`GetMonthlyTargetReportRequest`):
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "tabId": 1,
  "subTabId": null,
  "reportDate": "2026-08-06T00:00:00",
  "regionId": [35],
  "branchId": [1234],
  "portfolioId": null,
  "searchText": null,
  "sortBy": null,
  "isAscending": false,
  "productId": null,
  "userCode": null
}
```

- **404:** kayıt bulunamazsa

**Response** (`GetMonthlyTargetReportResponse`):
```json
{
  "products": [
    {
      "productId": 1,
      "productName": "Vadesiz Mevduat",
      "parentProductId": null,
      "monthActualAmount": 1250000.00,
      "monthTargetAmount": 1400000.00,
      "monthRatio": 89.29,
      "yearActualAmount": 14500000.00,
      "yearTargetAmount": 16000000.00,
      "yearRatio": 90.63,
      "subProducts": []
    }
  ]
}
```

### POST /TargetReport/GetMonthlyTargetReportTableHeaders

**Request** (`GetMonthlyTargetReportTableHeadersRequest`):
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000", "reportDate": "2026-08-06T00:00:00" }
```

- **404:** kayıt bulunamazsa

**Response** (`GetMonthlyTargetReportTableHeadersResponse`):
```json
{
  "productNameTitle": "Ürün",
  "monthGroupTitle": "Ay",
  "yearGroupTitle": "Yıl",
  "monthActualTitle": "Gerçekleşen",
  "monthTargetTitle": "Hedef",
  "monthHGTitle": "H/G %",
  "yearActualTitle": "Gerçekleşen",
  "yearTargetTitle": "Hedef",
  "yearHGTitle": "H/G %"
}
```

### POST /TargetReport/GetProductTop10DailyAndWeeklyDifferences

**Request** (`GetProductTop10DailyAndWeeklyDifferencesRequest`):
```json
{
  "productId": 1,
  "filterType": 1,
  "regionId": [35],
  "branchId": [1234],
  "tabId": 1,
  "subTabId": null
}
```

**Response** (`ProductTop10DifferencesResponse`):
```json
{
  "first10": [
    { "companyId": 1001, "companyName": "Alsancak Şubesi", "value": 125000.50 }
  ],
  "last10": [
    { "companyId": 1050, "companyName": "Bornova Şubesi", "value": -85000.25 }
  ]
}
```

### POST /TargetReport/GetVolumeTrendAnalysis

**Request** (`GetTrendAnalysisRequest`):
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "bolge": "35",
  "subeKodu": "1234",
  "isKolu": "Perakende",
  "segment": "Mass",
  "urun": "Vadesiz Mevduat",
  "productId": null,
  "userCode": null
}
```

**Response** (`GetVolumeTrendAnalysisItem[]`):
```json
[
  { "productName": "Vadesiz Mevduat", "reportDate": "2026-08-01T00:00:00", "amount": 1250000.50 },
  { "productName": "Vadesiz Mevduat", "reportDate": "2026-08-02T00:00:00", "amount": 1262000.75 }
]
```

### POST /TargetReport/GetQuantityTrendAnalysis

**Request** (`GetTrendAnalysisRequest`) — şema `GetVolumeTrendAnalysis` ile aynı.

**Response** (`GetQuantityTrendAnalysisItem[]`):
```json
[
  { "productName": "Kredi Kartı Adedi", "reportDate": "2026-08-01T00:00:00", "count": 12 },
  { "productName": "Kredi Kartı Adedi", "reportDate": "2026-08-02T00:00:00", "count": 15 }
]
```

---

## 10. ProductivityReport

> ⚠️ Bu grubun **çoğu endpoint'i şu an mock data dönüyor** (backend henüz gerçek veri kaynağına bağlanmamış). Şemalar kesindir, değerler örnek amaçlıdır. Tüm endpoint'ler `POST`, body `null` ise `400` (istisna: `GetReportDates`, body almaz).

### 10.1 Tab / Header / Filter / Sidebar (genel ekran iskeleti)

#### POST /ProductivityReport/GetProductivityReportTabs
**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000", "filterType": 1 }
```
**Response** (`GetProductivityReportTabItem[]`):
```json
[
  { "tabId": 1, "tabName": "Genel", "parentId": 0, "tabLevel": 1 },
  { "tabId": 2, "tabName": "Kredi Kartı / POS", "parentId": 0, "tabLevel": 1 }
]
```

#### POST /ProductivityReport/GetProductivityReportTableHeaders
**Request:**
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "mainTabId": 1,
  "midTabId": null,
  "subTabId": null,
  "filterType": 1,
  "reportDate": "2026-08-06T00:00:00"
}
```
> `filterType`: 1=Region, 2=Branch

**Response** (`GetProductivityReportTableHeaderItem[]`):
```json
[
  { "id": 1, "headerName": "Ürün", "parentId": 0, "orderNo": 1, "sortable": false },
  { "id": 2, "headerName": "Gerçekleşen", "parentId": 0, "orderNo": 2, "sortable": true }
]
```

#### POST /ProductivityReport/GetProductivityScoreCardReportHeaders
**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000", "filterType": 1, "reportDate": "2026-08-06T00:00:00" }
```
**Response** (`GetProductivityScoreCardReportHeaderItem[]`):
```json
[
  { "id": 1, "headerName": "Bölge/Şube", "parentId": 0, "orderNo": 1 },
  { "id": 2, "headerName": "NPS", "parentId": 0, "orderNo": 2 }
]
```

#### POST /ProductivityReport/GetReportRegionFilters
**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000" }
```
**Response** (`GetReportRegionFilterItem[]`):
```json
[ { "code": "35", "name": "İzmir Bölgesi" }, { "code": "06", "name": "Ankara Bölgesi" } ]
```

#### POST /ProductivityReport/GetReportBranchFilters
**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000" }
```
**Response** (`GetReportBranchFilterItem[]`):
```json
[ { "code": "1234", "name": "Alsancak Şubesi", "regionCode": "35" } ]
```

#### POST /ProductivityReport/GetReportSidebarItems
**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000" }
```
**Response** (`GetReportSidebarItem[]`):
```json
[
  { "code": "PRODUCTIVITY", "name": "Verimlilik Raporları", "url": "/productivity", "isVisible": true, "orderNo": 1 },
  { "code": "TARGET", "name": "Hedef Raporları", "url": "/target", "isVisible": true, "orderNo": 2 }
]
```

#### POST /ProductivityReport/GetReportDates
**Request:** body **yok** (boş POST).

**Response** (`GetReportDatesItem[]`):
```json
[
  { "reportDate": "2026-08-06T00:00:00", "isDefault": true },
  { "reportDate": "2026-08-05T00:00:00", "isDefault": false }
]
```

---

### 10.2 Genel Bölge Raporu

#### POST /ProductivityReport/GetProductivityGeneralRegionReport
> Not: Bu request `BaseReportRequest`'tan miras almıyor; `sessionId`/`reportDate` alanı **yok**.

**Request** (`GetProductivityGeneralRegionReportRequest`):
```json
{ "isKoluAdi": "Perakende", "segment": "Mass" }
```
**Response** (`GetProductivityGeneralRegionReportResponse`):
```json
{
  "getProductivityGeneralRegionReports": [
    {
      "urun": "Vadesiz Mevduat",
      "bankaGecenYil": 14500000.00,
      "bankaGerceklesen": 16200000.00,
      "bankaOrt": 15800000.00,
      "bankaHedef": 17000000.00,
      "hgYuzde": 95.29,
      "netBuyumeBanka": 1700000.00,
      "netBuyumeBankaOrt": 1600000.00,
      "ytdBanka": 11.72,
      "qtdBanka": 3.15
    }
  ]
}
```

---

### 10.3 Kredi Kartı / POS Sayı Raporları (Region & Branch)

#### POST /ProductivityReport/GetProductivityCountCardPosRegionReport
**Request** (`GetProductivityCountCardPosRegionReportRequest`):
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "regionCode": "35",
  "tabId": 1,
  "reportDate": "2026-08-06T00:00:00",
  "sortBy": 1,
  "isAscending": false,
  "productId": null,
  "userCode": null
}
```
> `tabId`: 1=Kredi Kartı, 2=POS

**Response** (`GetProductivityCountCardPosRegionReportResponse`):
```json
{
  "getProductivityCountCardPosRegionReports": [
    {
      "id": 1,
      "productName": "Kredi Kartı Adedi",
      "currentMonthRegionValue": 4200,
      "currentMonthBankAverage": 3950,
      "currentMonthBankAverageDiff": 250,
      "threeMonthHgRegion": 12300,
      "threeMonthHgBankAverage": 11800,
      "threeMonthHgBankAverageDiff": 500,
      "subProducts": []
    }
  ]
}
```

#### POST /ProductivityReport/GetProductivityCountCardPosBranchReport
**Request** (`GetProductivityCountCardPosBranchReportRequest`) — Region ile aynı şema, `regionCode` yerine `branchCode`:
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "branchCode": "1234",
  "tabId": 1,
  "reportDate": "2026-08-06T00:00:00",
  "sortBy": 1,
  "isAscending": false,
  "productId": null,
  "userCode": null
}
```
**Response** (`GetProductivityCountCardPosBranchReportResponse`):
```json
{
  "getProductivityCountCardPosBranchReports": [
    {
      "id": 1,
      "productName": "Kredi Kartı Adedi",
      "currentPeriodBranchValue": 145,
      "currentPeriodRegionAverageValue": 130,
      "currentPeriodRegionAverageValueDiff": 15,
      "currentPeriodBankAverageValue": 125,
      "currentPeriodBankAverageValueDiff": 20,
      "threeMonthHgBranchValue": 410,
      "threeMonthHgRegionAverageValue": 390,
      "threeMonthHgRegionAverageValueDiff": 20,
      "threeMonthHgBankAverageValue": 380,
      "threeMonthHgBankAverageValueDiff": 30,
      "subProducts": []
    }
  ]
}
```

#### POST /ProductivityReport/GetProductivityCountCardPosRatioRegionReport
**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000", "regionCode": "35", "tabId": 1, "reportDate": "2026-08-06T00:00:00" }
```
**Response** (`GetProductivityCountCardPosRatioRegionReportItem[]`):
```json
[
  {
    "id": 1,
    "ratioName": "Aktif Kart Oranı",
    "previousQuarterRegionValue": 62.5,
    "currentRegionValue": 65.2,
    "currentBankAverageValue": 63.8,
    "currentRegionDiff": 2.7,
    "currentBankAverageDiff": 1.4
  }
]
```

#### POST /ProductivityReport/GetProductivityCountCardPosRatioRegionReportTableHeaders
**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000", "tabId": 1, "reportDate": "2026-08-06T00:00:00" }
```
**Response** (`GetProductivityCountCardPosRatioRegionReportTableHeadersItem`):
```json
{
  "rowNumberTitle": "#",
  "ratioNameTitle": "Oran Adı",
  "previousQuarterRegionTitle": "Önceki Çeyrek",
  "currentRegionTitle": "Bölge",
  "currentBankAverageTitle": "Banka Ort."
}
```

#### POST /ProductivityReport/GetProductivityCountCardPosRatioBranchReport
**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000", "branchCode": "1234", "tabId": 1, "reportDate": "2026-08-06T00:00:00" }
```
**Response** (`GetProductivityCountCardPosRatioBranchReportItem[]`):
```json
[
  {
    "id": 1,
    "ratioName": "Aktif Kart Oranı",
    "previousQuarterBranchValue": 60.0,
    "currentBranchValue": 64.0,
    "currentRegionAverageValue": 65.2,
    "currentBankAverageValue": 63.8,
    "currentBranchValueDiff": 4.0,
    "currentRegionAverageValueDiff": -1.2,
    "currentBankAverageValueDiff": 0.2
  }
]
```

#### POST /ProductivityReport/GetProductivityCountCardPosRatioBranchReportTableHeaders
**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000", "tabId": 1, "reportDate": "2026-08-06T00:00:00" }
```
**Response** (`GetProductivityCountCardPosRatioBranchReportTableHeadersItem`):
```json
{
  "rowNumberTitle": "#",
  "ratioNameTitle": "Oran Adı",
  "previousQuarterBranchTitle": "Önceki Çeyrek",
  "currentBranchTitle": "Şube",
  "currentRegionAverageTitle": "Bölge Ort.",
  "currentBankAverageTitle": "Banka Ort."
}
```

---

### 10.4 Müşteri Sayısı Raporları (Region & Branch)

#### POST /ProductivityReport/GetProductivityCountCustomerRegionReport
**Request:**
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "regionCode": "35",
  "subTabId": 1,
  "reportDate": "2026-08-06T00:00:00",
  "sortBy": null,
  "isAscending": false,
  "productId": null,
  "userCode": null
}
```
> `subTabId` müşteri segmentini seçer: Tümü/Kurumsal/Ticari/KOBİ/Tarım/Perakende.

**Response** (`GetProductivityCountCustomerRegionReportResponse`):
```json
{
  "getProductivityCountCustomerRegionReports": [
    {
      "id": 1,
      "productName": "Aktif Müşteri Sayısı",
      "realizationRegion": 125000,
      "realizationRegionDiff": 3200,
      "realizationBankAverage": 118000,
      "realizationBankAverageDiff": 2100,
      "ytdChangeRegion": 8500,
      "ytdChangeRegionDiff": 500,
      "ytdChangeBankAverage": 7900,
      "ytdChangeBankAverageDiff": 300,
      "qtdChangeRegion": 2100,
      "qtdChangeRegionDiff": 150,
      "qtdChangeBankAverage": 1950,
      "qtdChangeBankAverageDiff": 100,
      "subProducts": []
    }
  ]
}
```

#### POST /ProductivityReport/GetProductivityCountCustomerBranchReport
**Request** — Region ile aynı şema, `regionCode`→`branchCode`:
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "branchCode": "1234",
  "subTabId": 1,
  "reportDate": "2026-08-06T00:00:00",
  "sortBy": null,
  "isAscending": false,
  "productId": null,
  "userCode": null
}
```
**Response** (`GetProductivityCountCustomerBranchReportResponse`):
```json
{
  "getProductivityCountCustomerBranchReports": [
    {
      "id": 1,
      "productName": "Aktif Müşteri Sayısı",
      "realizationBranchValue": 4200,
      "realizationRegionAverageValue": 4000,
      "realizationRegionAverageValueDiff": 200,
      "realizationBankAverageValue": 3900,
      "realizationBankAverageValueDiff": 300,
      "ytdNominalChangeBranchValue": 280,
      "ytdNominalChangeRegionAverageValue": 260,
      "ytdNominalChangeRegionAverageValueDiff": 20,
      "ytdNominalChangeBankAverageValue": 250,
      "ytdNominalChangeBankAverageValueDiff": 30,
      "qtdNominalChangeBranchValue": 70,
      "qtdNominalChangeRegionAverageValue": 65,
      "qtdNominalChangeRegionAverageValueDiff": 5,
      "qtdNominalChangeBankAverageValue": 60,
      "qtdNominalChangeBankAverageValueDiff": 10,
      "subProducts": []
    }
  ]
}
```

---

### 10.5 Nakit Yönetimi Sayı Raporları (Region & Branch)

#### POST /ProductivityReport/GetProductivityCountCashManagementRegionReport
**Request:**
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "regionCode": "35",
  "subTabId": 1,
  "reportDate": "2026-08-06T00:00:00",
  "sortBy": null,
  "isAscending": false
}
```
**Response** (`GetProductivityCountCashManagementRegionReportResponse`):
```json
{
  "getProductivityCountCashManagementRegionReports": [
    {
      "id": 1,
      "productName": "Nakit Yönetimi Sözleşme Sayısı",
      "realizationRegionValue": 320,
      "realizationRegionAverageValue": 300,
      "realizationRegionAverageValueDiff": 20,
      "realizationBankAverageValue": 290,
      "realizationBankAverageValueDiff": 30,
      "ytdNominalChangeRegionValue": 45,
      "ytdNominalChangeRegionAverageValue": 40,
      "ytdNominalChangeRegionAverageValueDiff": 5,
      "ytdNominalChangeBankAverageValue": 38,
      "ytdNominalChangeBankAverageValueDiff": 7,
      "qtdNominalChangeRegionValue": 12,
      "qtdNominalChangeRegionAverageValue": 10,
      "qtdNominalChangeRegionAverageValueDiff": 2,
      "qtdNominalChangeBankAverageValue": 9,
      "qtdNominalChangeBankAverageValueDiff": 3,
      "subProducts": []
    }
  ]
}
```
> Not: Controller, provider `null` dönerse boş response'a fallback ediyor (`{"getProductivityCountCashManagementRegionReports": []}`), hata fırlatmıyor.

#### POST /ProductivityReport/GetProductivityCountCashManagementBranchReport
**Request** — Region ile aynı şema, `regionCode`→`branchCode`.
**Response** (`GetProductivityCountCashManagementBranchReportResponse`) — alan adları Region ile aynı yapıda, `Branch` prefix'i ile:
```json
{
  "getProductivityCountCashManagementBranchReports": [
    {
      "id": 1,
      "productName": "Nakit Yönetimi Sözleşme Sayısı",
      "realizationBranchValue": 18,
      "realizationRegionAverageValue": 16,
      "realizationRegionAverageValueDiff": 2,
      "realizationBankAverageValue": 15,
      "realizationBankAverageValueDiff": 3,
      "ytdNominalChangeBranchValue": 3,
      "ytdNominalChangeRegionAverageValue": 2,
      "ytdNominalChangeRegionAverageValueDiff": 1,
      "ytdNominalChangeBankAverageValue": 2,
      "ytdNominalChangeBankAverageValueDiff": 1,
      "qtdNominalChangeBranchValue": 1,
      "qtdNominalChangeRegionAverageValue": 1,
      "qtdNominalChangeRegionAverageValueDiff": 0,
      "qtdNominalChangeBankAverageValue": 0,
      "qtdNominalChangeBankAverageValueDiff": 1,
      "subProducts": []
    }
  ]
}
```

---

### 10.6 Hacim Raporları (Region & Branch)

#### POST /ProductivityReport/GetProductivityVolumeRegionReport
**Request:**
```json
{
  "sessionId": "a1b2c3d4-0000-0000-0000-000000000000",
  "regionCode": "35",
  "subTabId": 1,
  "reportDate": "2026-08-06T00:00:00",
  "sortBy": null,
  "isAscending": false,
  "productId": null,
  "userCode": null
}
```
**Response** (`GetProductivityVolumeRegionReportResponse`):
```json
{
  "getProductivityVolumeRegionReports": [
    {
      "id": 1,
      "productName": "Vadesiz Mevduat",
      "realizationRegionValue": 16200000.00,
      "realizationRegionDiff": 1700000.00,
      "realizationRegionLastYearValue": 14500000.00,
      "realizationRegionAverageValue": 15800000.00,
      "realizationBankValue": 210000000.00,
      "realizationBankAverageValue": 15500000.00,
      "realizationBankAverageDiff": 700000.00,
      "targetValue": 17000000.00,
      "hgRate": 95.29,
      "netGrowthRegionValue": 1700000.00,
      "netGrowthRegionDiff": 150000.00,
      "netGrowthRegionAverageValue": 1600000.00,
      "netGrowthBankValue": 22000000.00,
      "netGrowthBankAverageValue": 1550000.00,
      "netGrowthBankAverageDiff": 150000.00,
      "ytdRegionValue": 11.72,
      "ytdRegionDiff": 0.5,
      "ytdBankAverageValue": 11.2,
      "ytdBankAverageDiff": 0.3,
      "qtdRegionValue": 3.15,
      "qtdRegionDiff": 0.2,
      "qtdBankAverageValue": 3.0,
      "qtdBankAverageDiff": 0.1,
      "subProducts": []
    }
  ]
}
```

#### POST /ProductivityReport/GetProductivityVolumeBranchReport
**Request** — Region ile aynı şema, `branchCode` ile.
**Response** (`GetProductivityVolumeBranchReportResponse`) — Region'a benzer, ek olarak `realizationBranchLastYearValue`, `netGrowthBranchValue` gibi branch-özel alanlar:
```json
{
  "getProductivityVolumeBranchReports": [
    {
      "id": 1,
      "productName": "Vadesiz Mevduat",
      "realizationBranchLastYearValue": 480000.00,
      "realizationBranchValue": 540000.00,
      "realizationBranchDiff": 60000.00,
      "realizationRegionValue": 16200000.00,
      "realizationRegionAverageValue": 510000.00,
      "realizationRegionAverageValueDiff": 30000.00,
      "realizationBankValue": 210000000.00,
      "realizationBankAverageValue": 500000.00,
      "realizationBankAverageValueDiff": 40000.00,
      "targetValue": 560000.00,
      "hgRate": 96.43,
      "netGrowthBranchValue": 60000.00,
      "netGrowthBranchDiff": 5000.00,
      "netGrowthRegionValue": 1700000.00,
      "netGrowthRegionAverageValue": 53000.00,
      "netGrowthRegionAverageValueDiff": 7000.00,
      "netGrowthBankValue": 22000000.00,
      "netGrowthBankAverageValue": 51000.00,
      "netGrowthBankAverageValueDiff": 9000.00,
      "ytdBranchValue": 12.5,
      "ytdRegionValue": 11.72,
      "ytdRegionValueDiff": 0.78,
      "ytdBankValue": 11.2,
      "ytdBankValueDiff": 1.3,
      "qtdBranchValue": 3.4,
      "qtdRegionValue": 3.15,
      "qtdRegionValueDiff": 0.25,
      "qtdBankValue": 3.0,
      "qtdBankValueDiff": 0.4,
      "subProducts": []
    }
  ]
}
```

---

### 10.7 Kâr Oranı Raporları (Region & Branch)

#### POST /ProductivityReport/GetProductivityProfitRatioRegionReport
**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000", "regionCode": "35", "reportDate": "2026-08-06T00:00:00", "sortBy": null, "isAscending": false }
```
**Response** (`GetProductivityProfitRatioRegionReportItem[]`):
```json
[
  {
    "id": 1,
    "ratioName": "Net Kâr Marjı",
    "targetValue": 25.0,
    "regionValue": 26.3,
    "regionValueDiff": 1.3,
    "bankValue": 24.8,
    "bankValueDiff": 0.5,
    "retailValue": 22.1,
    "kobiValue": 27.4,
    "agricultureValue": 19.8,
    "agricultureValueDiff": -1.2,
    "commercialValue": 28.9,
    "commercialValueDiff": 2.1,
    "subProducts": []
  }
]
```

#### POST /ProductivityReport/GetProductivityProfitRatioBranchReport
**Request** — aynı şema, `branchCode` ile.
**Response** (`GetProductivityProfitRatioBranchReportItem[]`) — alan adları aynı yapıda (`GetProductivityProfitRatioRegionReportItem` ile birebir aynı şema).

---

### 10.8 Toplam Kâr Raporları (Region & Branch)

#### POST /ProductivityReport/GetProductivityProfitTotalRegionReport
**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000", "regionCode": "35", "reportDate": "2026-08-06T00:00:00", "sortBy": null, "isAscending": false, "productId": null, "userCode": null }
```
**Response** (`GetProductivityProfitTotalRegionReportResponse`):
```json
{
  "getProductivityProfitTotalRegionReports": [
    {
      "id": 1,
      "description": "Toplam Net Kâr",
      "targetValue": 4200000.00,
      "realizationRegionValue": 4450000.00,
      "realizationRegionValueDiff": 250000.00,
      "regionAverageValue": 4300000.00,
      "bankAverageValue": 4100000.00,
      "realizationBankAverageValue": 4180000.00,
      "realizationBankAverageValueDiff": 80000.00,
      "bankBudgetValue": 4000000.00,
      "hgRegionValue": 105.95,
      "hgRegionValueDiff": 5.95,
      "hgBankAverageValue": 104.50,
      "hgBankAverageValueDiff": 4.50,
      "retailValue": 1200000.00,
      "kobiValue": 1800000.00,
      "agricultureValue": 450000.00,
      "commercialValue": 1000000.00,
      "commercialValueDiff": 50000.00,
      "partnerValue": 0.00,
      "subProducts": []
    }
  ]
}
```

#### POST /ProductivityReport/GetProductivityProfitTotalBranchReport
**Request** — aynı şema, `branchCode` ile.
**Response** (`GetProductivityProfitTotalBranchReportResponse`) — ek olarak `branchBudgetValue`, `regionBudgetValue` alanları:
```json
{
  "getProductivityProfitTotalBranchReports": [
    {
      "id": 1,
      "description": "Toplam Net Kâr",
      "targetValue": 140000.00,
      "realizationBranchValue": 148000.00,
      "realizationBranchValueDiff": 8000.00,
      "regionAverageValue": 143000.00,
      "realizationRegionAverageValue": 145000.00,
      "realizationRegionAverageValueDiff": 2000.00,
      "bankAverageValue": 138000.00,
      "realizationBankAverageValue": 140000.00,
      "realizationBankAverageValueDiff": 2000.00,
      "branchBudgetValue": 135000.00,
      "regionBudgetValue": 140000.00,
      "bankBudgetValue": 133000.00,
      "hgBranchValue": 105.71,
      "hgBranchValueDiff": 5.71,
      "hgRegionAverageValue": 103.57,
      "hgRegionAverageValueDiff": 3.57,
      "hgBankAverageValue": 105.26,
      "hgBankAverageValueDiff": 5.26,
      "retailValue": 40000.00,
      "kobiValue": 60000.00,
      "agricultureValue": 15000.00,
      "commercialValue": 33000.00,
      "commercialValueDiff": 2000.00,
      "partnerValue": 0.00,
      "subProducts": []
    }
  ]
}
```

---

### 10.9 Spread Yönetimi Kâr Raporları (Region & Branch)

#### POST /ProductivityReport/GetProductivityProfitSpreadManagementRegionReport
**Request:**
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000", "regionCode": "35", "reportDate": "2026-08-06T00:00:00", "sortBy": null, "isAscending": false, "productId": null, "userCode": null }
```
**Response** (`GetProductivityProfitSpreadManagementRegionReportResponse`):
```json
{
  "getProductivityProfitSpreadManagementRegionReports": [
    {
      "id": 1,
      "description": "TL Kredi Spreadi",
      "spreadValue": 12.5,
      "ratioRegionValue": 13.1,
      "ratioRegionValueDiff": 0.6,
      "ratioBankAverageValue": 12.8,
      "ratioBankAverageValueDiff": 0.3,
      "netReturnRegionValue": 850000.00,
      "netReturnRegionValueDiff": 45000.00,
      "netReturnBankAverageValue": 820000.00,
      "netReturnBankAverageValueDiff": 30000.00,
      "netReturnHgRegionValue": 108.2,
      "netReturnHgRegionValueDiff": 8.2,
      "netReturnHgBankAverageValue": 105.0,
      "netReturnHgBankAverageValueDiff": 5.0,
      "subProducts": []
    }
  ]
}
```

#### POST /ProductivityReport/GetProductivityProfitSpreadManagementBranchReport
**Request** — aynı şema, `branchCode` ile.
**Response** (`GetProductivityProfitSpreadManagementBranchReportResponse`):
```json
{
  "getProductivityProfitSpreadManagementBranchReports": [
    {
      "id": 1,
      "description": "TL Kredi Spreadi",
      "spreadValue": 12.8,
      "ratioBranchValue": 13.4,
      "ratioRegionAverageValue": 13.1,
      "ratioRegionAverageValueDiff": 0.3,
      "ratioBankAverageValue": 12.8,
      "ratioBankAverageValueDiff": 0.6,
      "netReturnBranchValue": 32000.00,
      "netReturnRegionAverageValue": 30000.00,
      "netReturnRegionAverageValueDiff": 2000.00,
      "netReturnBankAverageValue": 29000.00,
      "netReturnBankAverageValueDiff": 3000.00,
      "netReturnHgBranchValue": 110.0,
      "netReturnHgRegionAverageValue": 108.2,
      "netReturnHgRegionAverageValueDiff": 1.8,
      "netReturnHgBankAverageValue": 105.0,
      "netReturnHgBankAverageValueDiff": 5.0,
      "subProducts": []
    }
  ]
}
```

---

### 10.10 ScoreCard (Productivity ekranı — Pupa proxy'sinden ayrı, kendi DTO'su var)

#### POST /ProductivityReport/GetProductivityBranchScoreCardReport
**Request** (`GetProductivityBranchScoreCardReportRequest`):
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000", "branchCode": "1234", "reportDate": "2026-08-06T00:00:00" }
```
**Response** (`GetProductivityBranchScoreCardReportItem` — tekil obje, liste değil):
```json
{
  "managerName": "Ayşe Yılmaz",
  "firstMonthScore": 88.5,
  "secondMonthScore": 91.2,
  "thirdMonthScore": 93.0,
  "corporateScore": null,
  "commercialScore": 85.0,
  "kobiScore": 90.5,
  "obiScore": 87.0,
  "agricultureScore": null,
  "massScore": 92.0,
  "affluentScore": 89.5,
  "privateBankingScore": null,
  "branchNpsScore": 74.5,
  "bankNpsScore": 71.2,
  "phoneGreetingScore": 95.0,
  "branchManagerPhoto": "https://cdn.denizbank.com/photos/managers/42.jpg",
  "branchPhoto": "https://cdn.denizbank.com/photos/branches/1234.jpg"
}
```

#### POST /ProductivityReport/GetProductivityRegionScoreCardReport
**Request** (`GetProductivityRegionScoreCardReportRequest`):
```json
{ "sessionId": "a1b2c3d4-0000-0000-0000-000000000000", "regionCode": "35", "branchCode": null, "reportDate": "2026-08-06T00:00:00" }
```
**Response** (`GetProductivityRegionScoreCardReportItem` — tekil obje):
```json
{
  "managerName": "Mehmet Kaya",
  "firstMonthScore": 86.0,
  "secondMonthScore": 89.5,
  "thirdMonthScore": 90.8,
  "corporateScore": 84.0,
  "commercialScore": 87.5,
  "kobiScore": 91.0,
  "obiScore": 88.0,
  "agricultureScore": 80.5,
  "massScore": 90.0,
  "affluentScore": 88.0,
  "privateBankingScore": null,
  "regionNpsScore": 72.8,
  "bankNpsScore": 71.2,
  "regionManagerPhoto": "https://cdn.denizbank.com/photos/managers/17.jpg"
}
```

---

## Bilinen Kısıtlar ve Uyarılar

Mobil ekiple entegrasyona başlamadan önce backend ekibiyle netleştirilmesi önerilen noktalar:

1. **Auth akışı netleşmeli.** ASP.NET seviyesinde `[Authorize]` yok; erişim kontrolü ağ/gateway seviyesinde olabilir. `SessionId`'nin login response'undan (`UsersDto`) tam olarak nasıl türetildiği DTO'da açık değil — backend'e sorulmalı.
2. **`/Public/Login` şifreyi query string'de taşıyor** (GET). Mümkünse bu endpoint mobil tarafta kullanılmamalı; `WindowsLogin` / `DomainLogin` / `SessionLogin` tercih edilmeli.
3. **XML-doc yorumlarında `/api/...` prefix'i geçiyor ama gerçek route'larda yok.** Route attribute'larına güvenin, yorumlara değil.
4. **`ScoreCard/*` (14 endpoint) tamamen farklı bir model** — sabit DTO yok, request/response Pupa API'nin ham JSON'u. Gerçek şema için Pupa API dokümantasyonuna bakılmalı.
5. **`ExternalContext` header'ı** sadece `ScoreCard/*` tarafından okunuyor/forward ediliyor; diğer endpoint'lerde etkisi yok.
6. **ProductivityReport grubunun büyük çoğunluğu mock data dönüyor** — şema kesin, veri henüz gerçek değil. Gerçek veri entegrasyonu tamamlanınca bu döküman güncellenmeli.
7. **`GetReportDates` bir POST ama body almıyor** — muhtemelen ileride GET'e çevrilebilir, mobil tarafın body göndermemesi yeterli.
8. **CORS tanımlı değil** — sadece native mobil için sorun değil, ama backend tarafı bir web istemciye açılırsa eklenmesi gerekir.
9. **Genel bir response sarmalayıcı (envelope) yok** — sadece `Public/*` `ApiResponse<T>` kullanıyor, diğerleri çıplak DTO/array dönüyor. Hata durumunda status code'a bakmak gerekiyor, response body'de standart bir `success`/`error` alanı yok.
10. **404 davranışı endpoint'e göre değişken** — bazı `TargetReport` endpoint'leri veri bulunamazsa 404 dönerken, çoğu endpoint boş liste/obje dönüyor. Her endpoint için yukarıdaki tablo/notlar dikkatle takip edilmeli.

---

*Bu döküman DTO tanımlarından statik olarak üretilmiştir; örnek değerler gerçek üretim verisi değildir. Şema hataları veya eksik endpoint fark edilirse backend ekibine (bugrahan.akbas@mobven.com) bildirilmelidir.*

