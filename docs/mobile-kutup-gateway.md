# Mobil Entegrasyon Rehberi — KutupYıldızı Üzerinden DashboardTsy Erişimi

Bu doküman mobil (iOS) uygulamanın **yeni** erişim modelini anlatır:

- **Login** doğrudan KutupYıldızı'na yapılır.
- **Dashboard raporları** da KutupYıldızı üzerinden, `/yntc` önekiyle çağrılır. KutupYıldızı bu
  istekleri DashboardTsy'ye olduğu gibi iletir.

Mobil uygulama artık DashboardTsy host'una **doğrudan istek atmaz**. Tek muhatap KutupYıldızı'dır.

> **Eski modelden geçiş:** Daha önce login dahil her şey `https://<dashboard-host>/mobile/...`
> adresine gidiyordu ([mobile-auth-endpoints.md](mobile-auth-endpoints.md)). Bu yol artık
> kullanılmamalı. Değişiklik özeti için [Bölüm 5](#5-eski-modelden-geçiş-kontrol-listesi)'e bakın.

## İçindekiler

1. [Mimari](#1-mimari)
2. [Login Akışı (KutupYıldızı)](#2-login-akışı-kutupyıldızı)
3. [Dashboard Endpoint'lerine Erişim](#3-dashboard-endpointlerine-erişim)
4. [Hata Yönetimi](#4-hata-yönetimi)
5. [Eski Modelden Geçiş Kontrol Listesi](#5-eski-modelden-geçiş-kontrol-listesi)
6. [Ortam ve Config (backend)](#6-ortam-ve-config-backend)

---

## 1. Mimari

```
                                  ┌──────────────────────── KutupYıldızı ────────────────────────┐
 iOS ── /mobile/api/Token ───────▶│  MobileEnvelopeMiddleware ──(öneki siler)──▶ LoginController │
     ── /mobile/api/Login2 ──────▶│  (response'u envelope'a sarar)               CustomerSearch  │
     ── /mobile/api/SendSmsCode ─▶│                                              (JWT üretir)    │
     ── /mobile/api/GetAuth ─────▶│                                                              │
                                  │                                                              │
     ── /yntc/mobile/... ────────▶│  RequestForwardingMiddleware ──(/yntc'yi siler, aynen iletir)┼──▶ DashboardTsy.Api
                                  └──────────────────────────────────────────────────────────────┘    /mobile/...
                                                                                                      (JWT doğrular,
                                                                                                       envelope'a sarar)
```

| İstek tipi | Mobilin çağırdığı adres | Kim cevaplar | Response formatı |
|---|---|---|---|
| Login (Token, Login2, SendSmsCode, GetAuth) | `https://<kutup-host>/mobile/api/<Action>` | KutupYıldızı | `{ status, message, data }` envelope |
| Dashboard raporları | `https://<kutup-host>/yntc/mobile/<Controller>/<Action>` | DashboardTsy (Kutup aracılığıyla) | `{ status, message, data }` envelope |

Her iki durumda da response aynı zarfla gelir. Mobil tek kurala güvenir: **`status` false ise hata.**

> `/mobile` öneki Kutup'ta **yalnızca bu 4 login endpoint'i** için tanımlıdır. Kutup'un diğer
> endpoint'leri `/mobile` ile çağrılırsa 404 döner. Önek'siz `/api/...` path'leri mevcut
> KutupYıldızı uygulaması için olduğu gibi (envelope'suz) çalışmaya devam eder.

**Token ortaktır:** KutupYıldızı'nın ürettiği JWT, DashboardTsy'de de aynı imza anahtarıyla
doğrulanır. Login sonunda alınan `AccessToken` hem Kutup'un korumalı endpoint'lerinde hem de
`/yntc/...` altındaki tüm Dashboard endpoint'lerinde kullanılır.

---

## 2. Login Akışı (KutupYıldızı)

Login endpoint'leri KutupYıldızı'nın mevcut endpoint'leridir; `/mobile` öneki yalnızca
response'un envelope'a sarılmasını sağlar. Request body'leri, şifreleme ve `X-Checksum`
kuralları önek'siz çağrıyla birebir aynıdır.

### Ortak header'lar

| Header | Zorunlu | Açıklama |
|---|---|---|
| `Content-Type: application/json` | Evet | |
| `X-Checksum` | Evet (Development dışındaki tüm ortamlarda) | Ham request body'nin HMAC-SHA256'sı, küçük harf hex. Mevcut uygulamadaki checksum anahtarıyla hesaplanır. Eksik/yanlışsa Kutup **401** döner. |
| `Authorization: Bearer <jwt>` | Adıma göre | Aşağıdaki tabloya bakın. |

### Adımlar

| # | Endpoint | Authorization | Dönen değer |
|---|---|---|---|
| 1 | `POST /mobile/api/Token` | Yok | `data.Token` — **anonim** JWT (rol: `Anonymous`) |
| 2 | `POST /mobile/api/Login2` | `Bearer <Token>` | `data.SmsGuid`, `data.Key`, `data.EncryptData` — OTP gönderildi |
| 3 | `POST /mobile/api/SendSmsCode` | `Bearer <Token>` | `data.AccessToken` — **kullanıcı** JWT'si (rol: `User`) |
| 4 | `POST /mobile/api/GetAuth` | `Bearer <AccessToken>` | Personel/oturum bilgisi (`data.SessionId`, `data.IsAdmin`, `data.EmergencyToken`…) |

Request body'lerinin ve `data` içindeki alanların ayrıntısı
[mobile-auth-endpoints.md](mobile-auth-endpoints.md) Bölüm 2–5'te. `data` içindeki alan adları
Kutup'tan geldiği gibi PascalCase'dir; envelope alanları (`status`, `message`, `data`) camelCase'dir.

### Envelope kuralları

Kutup iş hatalarını HTTP 200 + `IsError` ile döner. Middleware bunu tanır ve `status: false`
yapar; mobilin `data.IsError`'a bakmasına gerek kalmaz.

| Kutup'un döndüğü | `status` | `message` | `data` |
|---|---|---|---|
| HTTP 2xx, `IsError` yok / `false` / `0` | `true` | `"OK"` | Kutup'un body'si |
| HTTP 2xx, `IsError: 1` veya `true` | `false` | Body'deki `Error` ya da `ErrorMessage` (yoksa `"Hata"`) | Kutup'un body'si (örn. `RemainingTryCount` okunabilsin diye) |
| HTTP 4xx/5xx | `false` | Body'deki `Message` / `Error` / `ErrorMessage`, yoksa duruma göre: `"Geçersiz istek"`, `"Yetkisiz"`, `"Erişim yok"`, `"Bulunamadı"`, `"Sunucu hatası"` | `null` |

HTTP status kodu değiştirilmez: iş hatası 200, checksum/JWT hatası 401 olarak gelir.

**Örnek — SendSmsCode başarılı:**

```json
{
  "status": true,
  "message": "OK",
  "data": {
    "Type": "ServiceResponseMessage",
    "State": "Success",
    "CustomerNo": "<request'teki customerNo>",
    "CustomerIdentity": "9000000001",
    "AccessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

**Örnek — hatalı OTP (HTTP 200):**

```json
{
  "status": false,
  "message": "OTP doğrulaması başarısız.",
  "data": {
    "Type": "ServiceResponseMessage",
    "IsError": 1,
    "Error": "OTP doğrulaması başarısız.",
    "CustomerNo": "<request'teki customerNo>",
    "RemainingTryCount": 2
  }
}
```

**Örnek — checksum eksik/yanlış (HTTP 401):**

```json
{ "status": false, "message": "Yetkisiz", "data": null }
```

### Hangi token nerede?

| Token | Nereden gelir | Nerede kullanılır |
|---|---|---|
| `Token` (anonim) | Adım 1 | Yalnızca Adım 2 ve 3 |
| `AccessToken` (kullanıcı) | Adım 3 | Adım 4, Kutup'un diğer korumalı endpoint'leri ve **tüm `/yntc/...` Dashboard istekleri** |
| `EmergencyToken` | Adım 4 | Yalnızca Kutup acil durum özellikleri. **Dashboard'da geçersizdir** (farklı anahtarla imzalı) ve 401 alırsınız. |

**Token süresi:** Prod'da 30 dakika, Development'ta 480 dakika. Refresh token akışı yoktur.
Süre dolunca Dashboard 401 döner ve login akışı baştan yapılmalıdır.

---

## 3. Dashboard Endpoint'lerine Erişim

### URL kuralı

```
https://<kutup-host>/yntc/mobile/<Controller>/<Action>[?query]
```

KutupYıldızı `/yntc` önekini siler ve kalan path + query string'i DashboardTsy'ye iletir:

```
Mobil   → POST https://<kutup-host>/yntc/mobile/TargetReport/GetDailyTargetReport
Kutup   → POST https://<dashboard-host>/mobile/TargetReport/GetDailyTargetReport
```

Kullanılabilir controller/action listesi ve request/response şemaları için:
[API-REFERENCE.md](API-REFERENCE.md). Oradaki her `/mobile/...` path'inin başına `/yntc`
eklemeniz yeterli.

> `/yntc/mobile/...`'daki `/mobile` segmenti **zorunludur**. `/yntc/TargetReport/...` gibi
> `/mobile`'sız bir istek Dashboard'un web (Windows auth) tarafına düşer, envelope almaz ve
> çalışmaz.

### Header'lar

| Header | Zorunlu | Açıklama |
|---|---|---|
| `Authorization: Bearer <AccessToken>` | Evet | SendSmsCode'dan alınan token. |
| `Content-Type: application/json` | POST body varsa | |
| `X-Checksum` | Hayır | `/yntc` istekleri Kutup'un checksum kontrolüne girmez. Gönderilirse zararı yoktur. |
| Diğer custom header'lar (örn. `ExternalContext`) | Endpoint'e göre | Kutup tüm header'ları Dashboard'a aynen iletir. |

### Örnek istek

```bash
curl -X POST "https://<kutup-host>/yntc/mobile/TargetReport/GetDailyTargetReport" \
  -H "Authorization: Bearer <AccessToken>" \
  -H "Content-Type: application/json" \
  -d '{ "sessionId": "<GetAuth.SessionId>", "...": "..." }'
```

### Response

DashboardTsy'nin envelope'u değişmeden mobil uygulamaya ulaşır:

```json
{
  "status": true,
  "message": "OK",
  "data": { "...": "endpoint'e özgü DTO" }
}
```

| Alan | Tip | Açıklama |
|---|---|---|
| `status` | bool | HTTP 2xx ise `true`, 4xx/5xx ise `false` |
| `message` | string | Başarıda `"OK"`, hatada okunabilir mesaj |
| `data` | object \| null | Asıl veri. Hatada `null`. |

---

## 4. Hata Yönetimi

`/yntc/...` isteklerinde hata iki farklı katmandan gelebilir. **Body'nin boş olup olmadığına** bakarak
ayırt edin:

| HTTP | Body | Kaynak | Anlamı / Mobilin yapacağı |
|---|---|---|---|
| 200 | Envelope, `status: true` | Dashboard | Başarılı |
| 400 | Envelope, `status: false` | Dashboard | Geçersiz istek. `message`'ı gösterin. |
| 401 | Envelope, `message: "Yetkisiz"` | Dashboard | Token yok, süresi dolmuş veya geçersiz. **Yeniden login.** |
| 404 | Envelope, `status: false` | Dashboard | Veri/route bulunamadı |
| 500 | Envelope, `message: "Sunucu hatası"` | Dashboard | Beklenmedik hata |
| 502 | Envelope, `status: false` | Dashboard | Yalnızca `ScoreCard/*`: Dashboard'un arkasındaki Pupa API'sine erişilemedi |
| **502** | **Boş** | **KutupYıldızı** | Kutup, Dashboard'a ulaşamadı. Genel "servis şu an kullanılamıyor" hatası gösterin. |
| **504** | **Boş** | **KutupYıldızı** | Dashboard 100 sn içinde cevap vermedi (timeout) |

Login endpoint'lerinde (Bölüm 2) de response her zaman envelope'ludur. İş hataları (yanlış OTP
gibi) **HTTP 200 + `status: false`**, checksum/JWT hataları **HTTP 401 + `status: false`** olarak
gelir. Ayrıntı: [Envelope kuralları](#envelope-kuralları).

---

## 5. Eski Modelden Geçiş Kontrol Listesi

| Konu | Eski | Yeni |
|---|---|---|
| Login host'u | `<dashboard-host>/mobile/api/...` | `<kutup-host>/mobile/api/...` (yalnızca host değişir) |
| Login response'u | Envelope; iş hatası `status: true` + `data.IsError` | Envelope; iş hatası artık **`status: false`**. `data.IsError` kontrolüne gerek yok |
| Login'de `X-Checksum` | Gerekmiyordu | **Gerekli** (Development dışında) |
| Rapor host'u | `<dashboard-host>/mobile/...` | `<kutup-host>/yntc/mobile/...` |
| Rapor response'u | Envelope | Envelope (değişmedi) |
| Token | Kutup JWT'si | Kutup JWT'si (değişmedi) |
| Yeni hata durumu | — | Boş body'li 502/504 (Kutup → Dashboard bağlantı sorunu) |

---

## 6. Ortam ve Config (backend)

Mobil ekibin bilmesi gerekmez. Ortam kurulumu yapan backend/DevOps için:

**KutupYıldızı — `appsettings.json`**

```json
"RequestForwardingConfiguration": {
  "PathPrefix": "/yntc",
  "TargetBaseUrl": "https://<dashboard-host>"
}
```

- `TargetBaseUrl` her ortam (dev/UAT/prod) için DashboardTsy.Api'nin Kutup sunucusundan erişilebilen
  adresi olmalıdır. Repodaki değer (`http://localhost:5219`) yalnızca local geliştirme içindir.
- `PathPrefix` boş bırakılırsa forwarding tamamen kapanır.
- Login envelope'u `MobileEnvelopeMiddleware` ile sağlanır; config gerektirmez. `/mobile` önekli
  4 endpoint listesi middleware içinde sabittir. Path rewrite'ın route eşleşmesine yansıması için
  `UseRouting()`'den önce, `/yntc` isteklerini iki kez sarmamak için `UseRequestForwarding()`'den
  sonra çalışır.
- Forwarding, Kutup pipeline'ında authentication, checksum, CORS ve request-log middleware'lerinden
  **önce** çalışır. `/yntc` istekleri Kutup'ta doğrulanmaz ve loglanmaz; JWT doğrulaması
  DashboardTsy'de yapılır. İstek/hata logları Dashboard tarafında aranmalıdır. Kutup yalnızca
  502/504 durumlarını loglar.

**DashboardTsy.Api — `appsettings.json`**

```json
"MobileJwt": {
  "SymmetricKey": "<Kutup JwtManager.cs'deki base64 anahtarın birebir aynısı>"
}
```

- Bu anahtar Kutup'taki `JwtManager` anahtarıyla aynı olmalıdır. Biri değişirse diğeri de
  güncellenmelidir, aksi halde tüm `/yntc` istekleri 401 alır.
- Anahtar boş bırakılırsa JWT doğrulaması register edilmez ve `/mobile` istekleri 401 yerine
  **500** döner.
