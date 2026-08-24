# DashboardTsy API — Endpoint Referans Dökümanı

> Mobil ekip için hazırlanmıştır. Kaynak: `src/DashboardTsy.Api/Controllers/*` (2026-08-12 itibarıyla).
> Örnek request/response gövdeleri, DTO tanımlarından üretilen **şema-doğru statik örneklerdir** (gerçek prod verisi değildir).

> **⚠️ 2026-08-12 güncellemesi — Mobile Auth eklendi:** iOS uygulaması için Kutup Yıldızı login akışına proxy'lenen yeni endpoint'ler ve JWT tabanlı erişim koruması geldi. Detay: [`0. Mobile Auth`](#0-mobile-auth-ios-login-akışı) ve güncellenmiş [Genel Bilgiler](#genel-bilgiler).

## İçindekiler

1. [Genel Bilgiler](#genel-bilgiler)
2. [Mobile Auth (iOS Login Akışı)](#0-mobile-auth-ios-login-akışı)
3. [AiInsight](#1-aiinsight)
4. [AppSettings](#2-appsettings)
5. [CacheAdmin](#3-cacheadmin)
6. [ExchangeRate](#4-exchangerate)
7. [NplReport](#5-nplreport)
8. [Public (Auth/Login) — web-only, mobilde kullanılmaz](#6-public-authlogin)
9. [SalaryCustomerReport](#7-salarycustomerreport)
10. [ScoreCard](#8-scorecard)
11. [TargetReport](#9-targetreport)
12. [ProductivityReport](#10-productivityreport)
13. [Bilinen Kısıtlar ve Uyarılar](#bilinen-kısıtlar-ve-uyarılar)

---

## Genel Bilgiler

### Base URL ve `/mobile` Prefix'i (mobil ekip için kritik)

DashboardTsy API iki paralel erişim yolu sunar:

| İstemci | Path şablonu | Envelope | Auth |
|---|---|---|---|
| **iOS uygulaması (mobil)** | `https://<host>/mobile/<Controller>/<Action>` | Var — [`MobileEnvelope<T>`](#mobileenveloped) sarmalayıcısı otomatik | JWT Bearer zorunlu (login endpoint'leri hariç) |
| Web (tarayıcı) | `https://<host>/<Controller>/<Action>` | Yok — çıplak DTO/liste | Windows/Negotiate (ağ seviyesi) |

**Aynı controller iki taraftan da erişilebilir** — arada bir middleware var (`MobileEnvelopeMiddleware`) ve şunu yapar:
1. `/mobile` prefix'ini kaldırıp path'i controller route'una çevirir (`/mobile/TargetReport/... → /TargetReport/...`)
2. Login endpoint'leri (`/mobile/api/Token`, `/mobile/api/Login2`, `/mobile/api/SendSmsCode`) hariç, **JWT Bearer token'ı zorunlu olarak doğrular** — yoksa `401 { status:false, message:"Yetkisiz", data:null }` döner.
3. Controller'ın döndürdüğü JSON'u `MobileEnvelope<T>` içine sarar.

Mobil ekip **her zaman `/mobile/*` prefix'ini kullanmalıdır.** Prefix'siz path'ler web akışıdır ve iOS'tan gelen istek Windows auth'a girip çalışmaz.

Örnek:
```
POST https://<host>/mobile/TargetReport/GetDailyTargetReport
Authorization: Bearer eyJhbGciOi...   ← Login akışından alınan JWT
Content-Type: application/json
```

> Not: Controller XML-doc yorumlarında bazen `/api/...` prefix'i geçer; **gerçek route bu değil.** Route attribute'una ve yukarıdaki tabloya güvenin.

### Kimlik Doğrulama (Auth)

**Mobil için iki katmanlı auth vardır:**

**Katman 1 — JWT Bearer (transport-level, zorunlu).**
- iOS ilk olarak [`0. Mobile Auth`](#0-mobile-auth-ios-login-akışı) altındaki üç endpoint'e sırasıyla istek atar:
  1. `POST /mobile/api/Token` — ClientId/ClientSecret ile anonim JWT alır.
  2. `POST /mobile/api/Login2` — anonim JWT ile kullanıcı adı/şifre + SMS OTP challenge alır.
  3. `POST /mobile/api/SendSmsCode` — OTP kodu doğrulanır, **kullanıcıya özel JWT** döner.
- Bu son JWT, `/mobile/*` altındaki **tüm** endpoint'lerde `Authorization: Bearer <token>` header'ıyla gönderilir. Yoksa 401.
- Token süresi Kutup Yıldızı config'ine bağlıdır (prod'da 30 dk, dev'de 480 dk). Süresi dolduğunda tekrar login gerekir; şu an refresh token akışı yoktur.

**Katman 2 — Business-level `SessionId` (rapor endpoint'leri).**
- JWT geçerli olsa dahi, rapor endpoint'lerinin çoğu request body'sinde `sessionId` alanı bekler ve bu SP çağrılarına parametre olarak geçer.
- `SessionId`'nin nasıl elde edildiği/hangi alandan türetildiği DTO'da açık değil — **backend ekibine sorun.** JWT içindeki `ChannelSessionId` claim'i olabilir, ya da ayrı bir endpoint'ten döner.

**Web tarafı (referans, mobil için değil):** ASP.NET seviyesinde `[Authorize]` yok, erişim ağ/Windows Auth ile korunuyor. Web mobil değildir, mobil ekip bu bölümü göz ardı edebilir.

### Ortak Header'lar

| Header | Zorunlu mu | Nerede | Açıklama |
|---|---|---|---|
| `Authorization: Bearer <jwt>` | **Evet**, login endpoint'leri hariç | `/mobile/*` altındaki tüm istekler | [`/mobile/api/SendSmsCode`](#post-mobileapisendsmscode) çağrısından dönen `accessToken` alanının değeri. Yoksa 401. |
| `Content-Type: application/json` | POST body olan tüm endpoint'lerde evet | Tümü | Standart JSON body |
| `ExternalContext` | Opsiyonel | Sadece `ScoreCard/*` (14 endpoint) | Değer varsa, ScoreCard controller'ı bunu upstream Pupa API'sine olduğu gibi forward ediyor. Mobil taraf bir context/correlation id göndermek isterse burayı kullanabilir — semantiği için backend'e sorun. |

### Genel Hata Davranışı

Mobil (`/mobile/*`) tarafında **tüm yanıtlar `MobileEnvelope<T>` içinde döner** — hata mı başarı mı ayrımı envelope'un `status` alanından yapılır, HTTP status code ise ek bilgi verir.

| HTTP status | `status` | Ne zaman |
|---|---|---|
| `200 OK` | `true` | Başarılı |
| `400 Bad Request` | `false` | Eksik/geçersiz body, validation hatası. `message` alanı hatayı açıklar. |
| `401 Unauthorized` | `false` | JWT yok, süresi dolmuş, ya da geçersiz. `message: "Yetkisiz"` |
| `404 Not Found` | `false` | Veri bulunamadı (bazı `TargetReport` endpoint'leri) |
| `500 Internal Server Error` | `false` | Beklenmedik sunucu hatası. `message: "Sunucu hatası"` |
| `502 Bad Gateway` | `false` | Sadece `ScoreCard/*` — upstream Pupa API'sine ulaşılamıyor |

### Ortak Response Sarmalayıcı Tipleri

#### MobileEnvelope&lt;T&gt;

Mobil tarafta (`/mobile/*` altında) **her yanıt** bu zarfa sarılır. Middleware otomatik yapar; controller'lar farkında değildir.

```json
{
  "status": true,
  "message": "OK",
  "data": { "...": "T tipine göre değişir" }
}
```

Başarısız örnek:
```json
{
  "status": false,
  "message": "Yetkisiz",
  "data": null
}
```

| Alan | Tip | Not |
|---|---|---|
| `status` | `boolean` | `true` = HTTP 2xx başarılı, `false` = 4xx/5xx |
| `message` | `string` | Başarıda `"OK"`, hatada okunabilir mesaj (varsa upstream'den, yoksa default: "Yetkisiz", "Sunucu hatası", "Bulunamadı", vs.) |
| `data` | `T \| null` | Asıl payload — controller'ın döndürdüğü DTO. Hatada `null`. |

> Controller'lar hâlâ ham DTO/liste döndürür (kodda değişmedi). Envelope sadece `/mobile/*` altında middleware tarafından eklenir. Web tarafı çıplak DTO görmeye devam eder.

#### ApiResponse&lt;T&gt;

Sadece `/Public/*` (web tarafı login) endpoint'lerinde kullanılıyor. **Mobil ekip bu tipe direkt hiç dokunmaz** — mobil auth için [`0. Mobile Auth`](#0-mobile-auth-ios-login-akışı) bölümüne bakın.

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

Tanımlı **değil**. Native mobil (iOS) için sorun olmaz. Web istemci bu API'ye tarayıcıdan çağrı yapacaksa backend'e CORS eklenmesi gerekir.

---

## 0. Mobile Auth (iOS Login Akışı)

iOS uygulamasının login akışı KutupYıldızı'na (mevcut mobil bankacılık backend'i) pass-through proxy ile bağlanır. DashboardTsy Api sadece isteği/response'u aynen forward eder — kimlik doğrulama, şifre decrypt, OTP mantığı Kutup tarafında yapılır. iOS'un görmesi gereken sözleşme aşağıda.

> **Akış özeti (sıralı):**
> `1) POST /mobile/api/Token` (anonim JWT) → `2) POST /mobile/api/Login2` (kullanıcı adı/şifre, OTP challenge alır) → `3) POST /mobile/api/SendSmsCode` (OTP doğrular, **user JWT** döner) → `4) Bearer <userJWT>` ile diğer endpoint'ler çağrılır.

> **Şifreleme:** `UserName`, `EPassword` ve `CustomerNo` alanları RSA-OAEP ile şifrelenmiş base64 string gelir. Şifreleme anahtarı Kutup'un yayınladığı public.key ile yapılır; mobil taraf bu anahtarı ayrıca alır. Backend ekibine sorup public key'i temin edin.

### 🧪 Mock Mode (`AuthMock:Enabled=true`)

DashboardTsy Api iki modda çalışır — hangisinin aktif olduğu `appsettings.json:AuthMock:Enabled` değerine bağlıdır. **Response şeması iki modda birebir aynıdır.** iOS kodu değişmez; sadece backend flag değişir.

| Davranış | `false` (prod/UAT — default) | `true` (mock — geliştirme) |
|---|---|---|
| Nereye gider | KutupYıldızı `/api/*` | Kutup'a gitmez, in-memory yanıt |
| RSA şifreleme | Kutup decrypt eder — public.key gerekir | Kontrol edilmez, alanlar opaque geçer |
| SMS gönderilir mi | Evet — Kutup gerçek SMS OTP gönderir | Hayır — sabit değer döner |
| Geçerli OTP kodu | Gerçek SMS'e gelen kod | **`"111111"`** (sabit) |
| Access token | Kutup üretir | DashboardTsy üretir (Kutup ile aynı symmetric key, geçerli JWT) |
| Yanıt HTTP status | Kutup ne döndürürse | Kutup davranışıyla eşleşen değer |

**Mock modda sabit değerler:**

| Alan | Mock değeri |
|---|---|
| `EncryptData` (Login2'den dönen CustomerNo) | `"1000000001"` (düz string, RSA-şifreli değil) |
| `SmsGuid` | `"mock-sms-guid-11111111-1111-1111-1111-111111111111"` |
| `Key` | `"mock-verify-key"` |
| `CustomerIdentity` (SendSmsCode'dan) | `"9000000001"` |
| `ChannelSessionId` (JWT claim) | `"mock-channel-session-1234567890abcdef"` |
| Geçerli OTP | `"111111"` — başka her kod `"Sms kodu doğru değil."` hatası |

**Mock modda validation:**

- `Token` → `deviceId` boş ise 400. ClientId/ClientSecret **kontrol edilmez** (mock).
- `Login2` → `parameters[0]` var ise başarılı sayılır. UserName/EPassword içeriği **kontrol edilmez** — her şey OK dönebilir. (Kutup mode'da kimlik doğrulama gerçek yapılır.)
- `SendSmsCode` → 6 zorunlu alan (`smsGuid`, `password`, `key`, `customerNo`, `userName`, `deviceId`) boş ise 400. Doluysa ve `password="111111"` ise başarılı; başka kod → `"Sms kodu doğru değil."`.

**Mock modda üretilen JWT gerçek geçerli bir token'dır.** `MobileJwt:SymmetricKey` ile imzalanır, DashboardTsy'nin JWT Bearer middleware'i tarafından kabul edilir; mock login sonrası iOS diğer `/mobile/*` korumalı endpoint'lerine bu token ile normal şekilde erişebilir.

> **Prod'a çıkarken:** `AuthMock:Enabled` **kesinlikle `false`** olmalı. `KutupYildizi:BaseUrl` de gerçek adres olmak zorunda; mock kapalıyken bu config boş ise uygulama startup'ta fail-fast bir exception atar.

### POST /mobile/api/Token

Login akışına başlamadan önce alınması gereken **anonim JWT**. `Login2` ve `SendSmsCode` çağrılarında bu token `Authorization` header'ında gitmez zaten (login endpoint'leri auth-muaf), ama Kutup'un iç kontrolü için de bilgi vermesi gerekir; şu an sadece Kutup üretiyor, iOS opsiyonel olarak header'a ekleyebilir.

- **Auth:** yok (anonim)
- **Envelope:** var

**Request** (`AnonymousTokenHttpRequest`):
```json
{
  "parameters": [
    {
      "clientId": "<Kutup'tan gelen ClientId>",
      "clientSecret": "<Kutup'tan gelen ClientSecret>",
      "deviceId": "<cihazın benzersiz ID'si (UUID önerilir)>"
    }
  ]
}
```

**Response** (`200 OK`):
```json
{
  "status": true,
  "message": "OK",
  "data": {
    "Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

**Hata (`400`)** — ClientId/ClientSecret eşleşmezse veya `deviceId` boş ise:
```json
{ "status": false, "message": "Geçersiz istek", "data": null }
```

### POST /mobile/api/Login2

Kullanıcı adı ve şifreyi Kutup'a gönderir; başarılıysa Kutup **kullanıcının kayıtlı telefonuna SMS OTP kodu** yollar ve doğrulama için gerekli `smsGuid` + `key` ikilisini döner. `encryptData` alanında müşteri numarası şifreli olarak geri döner — bir sonraki adımda bu değer aynen kullanılır.

- **Auth:** yok (anonim). Opsiyonel olarak `Authorization: Bearer <anonimJWT>` gönderilebilir; Kutup'un iç doğrulaması için.
- **Envelope:** var

**Request** (`MobileLoginHttpRequest`):
```json
{
  "header": {
    "channelRequestId": "<opsiyonel; boş bırakılabilir>",
    "appKey": "<Kutup config>",
    "channel": "<Kutup config>",
    "channelSessionId": "<opsiyonel; anonim JWT'den de çekilir>"
  },
  "parameters": [
    {
      "applicationVersion": "1.0.0",
      "userAgentString": "iOS/17.5 iPhone15,3",
      "clientId": "<Kutup ClientId>",
      "softwareVersion": "17.5",
      "deviceType": "iPhone",
      "operatingSystem": "iOS",
      "userName": "<RSA-şifreli TC no veya müşteri no>",
      "ePassword": "<RSA-şifreli şifre>",
      "clientSecret": "<Kutup ClientSecret>",
      "deviceId": "<cihaz UUID>"
    }
  ]
}
```

**Response — başarılı** (`200 OK`):
```json
{
  "status": true,
  "message": "OK",
  "data": {
    "IsError": false,
    "ErrorMessage": null,
    "SmsLength": 6,
    "IsAlphaNumericCode": false,
    "EncryptData": "<RSA-şifreli CustomerNo — SendSmsCode'a geri gidecek>",
    "SmsGuid": "b3a1c8e2-4f5d-4a6b-9c1e-1f2a3b4c5d6e",
    "Key": "verify-key-string"
  }
}
```

**Response — kimlik doğrulanamadı / şifre yanlış** (`200 OK`, `IsError=true`):
```json
{
  "status": true,
  "message": "OK",
  "data": {
    "IsError": true,
    "ErrorMessage": "Girmiş olduğunuz bilgiler doğrulanamadı. Lütfen tüm bilgileri kontrol ederek tekrar deneyiniz."
  }
}
```

> **Dikkat:** HTTP status kodu başarılı olsa bile Kutup response body'sinde `IsError=true` gelebilir (Kutup davranışı). Mobil taraf **hem `envelope.status`'ü hem `data.IsError`'u kontrol etmelidir.**

### POST /mobile/api/SendSmsCode

OTP kodu doğrulama. Başarılıysa **kullanıcıya özel JWT** (`accessToken`) döner — sonraki tüm çağrılarda `Authorization: Bearer <accessToken>` olarak kullanılır.

- **Auth:** yok (anonim). Opsiyonel `Authorization: Bearer <anonimJWT>` gönderilebilir.
- **Envelope:** var

**Request** (`SmsVerifyHttpRequest`) — tüm alanlar zorunlu:
```json
{
  "smsGuid": "<Login2 response'undan>",
  "password": "<kullanıcının SMS'e gelen OTP kodu (plaintext, 6 hane)>",
  "key": "<Login2 response'undan>",
  "customerNo": "<Login2 response'undaki EncryptData — aynen gönder>",
  "userName": "<kullanıcı adı — plaintext, JWT claim'ine gidecek>",
  "deviceId": "<cihaz UUID>",
  "deviceToken": "<opsiyonel: push notification token>"
}
```

**Response — başarılı** (`200 OK`):
```json
{
  "status": true,
  "message": "OK",
  "data": {
    "State": "Success",
    "CustomerNo": "<RSA-şifreli — Login2'de dönenle aynı>",
    "CustomerIdentity": "1234567890",
    "AccessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

**Response — eksik alan** (`400`):
```json
{
  "status": false,
  "message": "DeviceId, UserName, SmsGuid, Password, Key ve CustomerNo zorunludur.",
  "data": null
}
```

**Response — OTP kodu yanlış / süresi doldu / max deneme** (`200 OK`, `IsError=1`):
```json
{
  "status": true,
  "message": "OK",
  "data": {
    "IsError": 1,
    "ErrorMessage": "Sms kodu doğru değil.",
    "CustomerNo": "<Login2'deki EncryptData>"
  }
}
```

Bilinen `ErrorMessage` değerleri:
- `"Sms kodu doğru değil."` — yanlış kod
- `"Maksimum doğrulama denemesine ulaşıldı."` — üst üste hatalı giriş
- `"Doğrulama süresi doldu."` — OTP süresi geçti, `Login2`'den başlanmalı
- `"Sms datası bulunamadı."` — session cache expired (10 dk üzeri gecikme)

### `accessToken`'in kullanımı

Bu bölümdeki 3 endpoint (Token/Login2/SendSmsCode) dışında **her `/mobile/*` çağrısında** aşağıdaki header eklenmeli:

```
Authorization: Bearer <SendSmsCode.data.AccessToken>
```

Yoksa middleware `401 { status:false, message:"Yetkisiz", data:null }` döndürür.

Token içeriği (informational — mobil parse etmesine gerek yok):
- `CustomerNo` — müşteri numarası (plaintext)
- `UserId` — müşteri identity ID
- `DeviceId`, `DeviceToken`, `ChannelSessionId` — Kutup için context claim'leri
- `role: "User"`
- `exp` — token bitiş zamanı

Token süresi dolunca (`exp` geçtiğinde) her istek 401 dönmeye başlar; mobil taraf tekrar login akışına yönlendirmelidir. Şu an refresh-token akışı **yoktur**.

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

> ⚠️ **Bu bölüm web tarafı içindir; iOS uygulaması bu endpoint'leri ÇAĞIRMAZ.** Mobil login için [`0. Mobile Auth`](#0-mobile-auth-ios-login-akışı) bölümünü kullanın. Aşağıdaki notlar sadece bağlam/referans amaçlıdır.

Web'in oturum başlangıç noktası. Bu endpoint'lerden dönen `UsersDto.userId`/oturum bilgisi web akışındaki sonraki rapor çağrılarında `SessionId` olarak kullanılır — DTO'da ayrı bir `SessionId` alanı yok, muhtemelen ayrı bir mekanizma (cookie/token) var; backend ile teyit edilmeli.

> ⚠️ **Güvenlik notu:** `/Public/Login` şifreyi **query string** üzerinden GET ile alıyor (`?username=...&password=...`). Bu, şifrenin proxy/access loglarına düşmesi riski taşır — mobil ekipte de olsa web ekibinde de olsa `Login`'in son çare olması önerilir.

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

### GET /Public/GetUserBySession?sessionId={sessionId}

Web login akışı için. `sessionId` query parametresi ile kullanıcıyı çözer ve `UsersDto` döner;
Web tarafı sonucu sunucu-tarafı session'a yazar. Response şeması aynıdır (`ApiResponse<UsersDto>`).

### GET /Public/GetCurrentUser

Mobil için. `Authorization: Bearer <kutup-jwt>` header'ı zorunludur. Sunucu JWT'nin
`ChannelSessionId` claim'ini okuyup `UserLogin` üzerinden kullanıcıyı çözer — client parametre
göndermez, sessionId spoof edilemez. Token yoksa/geçersizse `401`; claim yoksa `ApiResponse<UsersDto>`
zarfında "Geçersiz session bilgisi" mesajı döner.

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

### GET /TargetReport/GetTargetReportMenu?sessionId={sessionId}

GetTargetReportMenuTexts ile **aynı kaynağı** (`SP_RP_GetTargetReportMenuTexts`) tabs/subTabs hiyerarşisinde döner.
Mobil istemciler için eklendi — web tarafı hâlâ GetTargetReportMenuTexts'i kullanır, bu endpoint onu değiştirmez.

- **404:** kayıt bulunamazsa
- Her tab/subTab'ın i18n'den bağımsız stabil bir `key`'i vardır (örn. `"sme"`, `"retail.private"`).
- `tabId`, `tabs` dizisindeki sırayı yansıtır (0'dan başlar). `subTabId`, her tab'ın kendi `subTabs`
  dizisindeki sırayı yansıtır — **global unique değildir**, her tab kendi listesinde 0'dan başlar.
- `subTabs` alt sekmesi olmayan tab'larda boş dizi (`[]`) olarak döner, asla `null` olmaz.

**Response** (`GetTargetReportMenuResponse`):
```json
{
  "screenTitle": "Hedef Raporları",
  "tabs": [
    { "tabId": 0, "key": "all", "title": "Tümü", "subTabs": [] },
    { "tabId": 1, "key": "corporate", "title": "Kurumsal", "subTabs": [] },
    { "tabId": 2, "key": "commercial", "title": "Ticari", "subTabs": [] },
    {
      "tabId": 3,
      "key": "sme",
      "title": "KOBİ",
      "subTabs": [
        { "subTabId": 0, "key": "sme.all", "title": "Tümü" },
        { "subTabId": 1, "key": "sme.kbi", "title": "KBİ" },
        { "subTabId": 2, "key": "sme.obi", "title": "OBİ" }
      ]
    },
    { "tabId": 4, "key": "agriculture", "title": "Tarım", "subTabs": [] },
    {
      "tabId": 5,
      "key": "retail",
      "title": "Bireysel",
      "subTabs": [
        { "subTabId": 0, "key": "retail.all", "title": "Tümü" },
        { "subTabId": 1, "key": "retail.general", "title": "Genel" },
        { "subTabId": 2, "key": "retail.affiliate", "title": "Bağlı" },
        { "subTabId": 3, "key": "retail.private", "title": "Private" }
      ]
    }
  ]
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
- `key`, kök tab'dan bu tab'a kadarki yolu nokta ile birleştiren stabil bir tanımlayıcıdır (örn.
  `"count.customer.all"`). Aynı `tabName` ("Tümü", "Kurumsal", ...) birden çok parent altında
  tekrar edebildiği için tekillik parent zinciriyle sağlanır — sadece `tabName`'e bakarak
  ayırt edilemez. Hedef ekranındaki `GetTargetReportMenu` endpoint'inin `key` pattern'iyle tutarlıdır.

**Response** (`GetProductivityReportTabItem[]`):
```json
[
  { "tabId": 1, "key": "general", "tabName": "Genel", "parentId": 0, "tabLevel": 1 },
  { "tabId": 60, "key": "general.all", "tabName": "Tümü", "parentId": 1, "tabLevel": 2 },
  { "tabId": 61, "key": "general.corporate", "tabName": "Kurumsal", "parentId": 1, "tabLevel": 2 },
  { "tabId": 3, "key": "volume", "tabName": "Hacim", "parentId": 0, "tabLevel": 1 },
  { "tabId": 4, "key": "profit", "tabName": "Karlılık", "parentId": 0, "tabLevel": 1 },
  { "tabId": 40, "key": "profit.total", "tabName": "Toplam", "parentId": 4, "tabLevel": 2 },
  { "tabId": 41, "key": "profit.spread-management", "tabName": "Spread Yönetimi", "parentId": 4, "tabLevel": 2 },
  { "tabId": 2, "key": "count", "tabName": "Adet", "parentId": 0, "tabLevel": 1 },
  { "tabId": 10, "key": "count.customer", "tabName": "Müşteri", "parentId": 2, "tabLevel": 2 },
  { "tabId": 11, "key": "count.payment-systems", "tabName": "Ödeme Sistemleri", "parentId": 2, "tabLevel": 2 },
  { "tabId": 12, "key": "count.cash-management", "tabName": "Nakit Yönetimi", "parentId": 2, "tabLevel": 2 },
  { "tabId": 20, "key": "count.customer.all", "tabName": "Tümü", "parentId": 10, "tabLevel": 3 },
  { "tabId": 50, "key": "count.cash-management.all", "tabName": "Tümü", "parentId": 12, "tabLevel": 3 }
]
```
> Tam liste 27 satırdır (her segment grubu Tümü/Kurumsal/Ticari/KOBİ/Tarım/Bireysel kırılımını içerir);
> yukarıda `key` şemasını göstermek için temsili bir alt küme verildi.

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

1. **Refresh token yok.** JWT süresi dolunca (prod 30 dk, dev 480 dk, mock 480 dk) tüm istekler 401 döner. Mobil uygulama 401 aldığında **kullanıcıyı tekrar login ekranına yönlendirmeli**. Refresh akışı ilerideki bir sürüme bırakıldı.

   > **Mock mode:** `AuthMock:Enabled=true` iken login akışı KutupYıldızı'na gitmeden mock yanıt döner. Detay: [`Mock Mode`](#-mock-mode-authmockenabledtrue). iOS geliştirici SMS OTP olarak sabit **`111111`** kullanır. Prod'a çıkışta flag `false` olmalıdır.
2. **JWT symmetric key iki projede paylaşılıyor.** DashboardTsy'nin token doğrulaması Kutup'un `JwtManager.SymmetricKey` değeriyle birebir aynı base64 key'i kullanır (`appsettings.MobileJwt.SymmetricKey`). Kutup tarafı key'i değiştirirse DashboardTsy config'i de güncellenmelidir; aksi halde tüm mobil istekler 401 alır.
3. **Business-level `SessionId` hâlâ belirsiz.** JWT ile transport güvenliği çözüldü, ama rapor endpoint'lerinin body'sinde beklenen `sessionId` alanının **hangi kaynaktan üretileceği DTO'da açık değil**. Backend ekibiyle netleştirin — JWT'deki `ChannelSessionId` claim'i mi, yoksa ayrı bir endpoint çıktısı mı olduğu iOS istemcinin bilmesi gereken şey.
4. **`Login2` ve `SendSmsCode` — HTTP 200 hata çelişkisi.** Şifre yanlış veya OTP hatalı olduğunda HTTP status 200 dönebilir ama `data.IsError` alanı `true` ya da `1` olur. Mobil taraf **envelope.status + data.IsError'u birlikte kontrol etmelidir.**
5. **`/mobile/api/Token` gerekli mi?** Şu an DashboardTsy'nin JWT middleware'i `Login2`/`SendSmsCode`'u anonim kabul ediyor — pratikte `Token` çağrılmadan da bu ikisi çalışır. Kutup iç doğrulaması için header'a eklemek isteniyorsa akış budur; yoksa iOS bu adımı atlayabilir. Backend ile teyit edin.
6. **RSA public key mobil tarafta gerekli.** `UserName`, `EPassword` ve `CustomerNo` alanları RSA-OAEP ile şifrelenmiş halde gönderilir. Public key **repoda yok**; backend'den ayrıca alınmalıdır. iOS'ta `Security.framework` üzerinden encryption yapılabilir.
7. **Session cache TTL 10 dk.** `Login2` ile `SendSmsCode` arasında 10 dakikadan uzun süre geçerse `"Sms datası bulunamadı."` hatası alınır — akış baştan başlatılmalıdır. Kullanıcı OTP ekranında 10 dk beklerse bu durum tetiklenir.
8. **XML-doc yorumlarında `/api/...` prefix'i geçiyor.** Bunlar Kutup'tan gelen yorumlardır; gerçek DashboardTsy route'ları farklı. Bu döküman ve controller `[Route(...)]` attribute'larına güvenin, XML yorumlara değil.
9. **`ScoreCard/*` (14 endpoint) tamamen farklı bir model** — sabit DTO yok, request/response Pupa API'nin ham JSON'u. Gerçek şema için Pupa API dokümantasyonuna bakılmalı.
10. **`ExternalContext` header'ı** sadece `ScoreCard/*` tarafından okunuyor/forward ediliyor; diğer endpoint'lerde etkisi yok.
11. **ProductivityReport grubunun büyük çoğunluğu mock data dönüyor** — şema kesin, veri henüz gerçek değil. Gerçek veri entegrasyonu tamamlanınca bu döküman güncellenmeli.
12. **`GetReportDates` bir POST ama body almıyor** — muhtemelen ileride GET'e çevrilebilir, mobil tarafın body göndermemesi yeterli.
13. **CORS tanımlı değil** — native iOS için sorun değil, ama backend web istemciye açılırsa eklenmesi gerekir.
14. **404 davranışı endpoint'e göre değişken** — bazı `TargetReport` endpoint'leri veri bulunamazsa 404 dönerken, çoğu endpoint boş liste/obje dönüyor. Envelope'lu yanıtta bu `{ status:false, message:"Bulunamadı", data:null }` şeklinde gelir.

---

*Bu döküman DTO tanımlarından statik olarak üretilmiştir; örnek değerler gerçek üretim verisi değildir. Şema hataları veya eksik endpoint fark edilirse backend ekibine (bugrahan.akbas@mobven.com) bildirilmelidir.*

