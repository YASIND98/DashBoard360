# Mobil Auth Entegrasyon Rehberi

**DashboardTsy.Api · MobileAuthController**
KutupYıldızı.Net → DashboardTsy.Api proxy köprüsü — Token · Login2 · SendSmsCode · GetAuth

Bu doküman, mobil (iOS) client'ın giriş yapmak için sırayla çağırması gereken dört endpoint'i
eksiksiz tanımlar: her biri için tam route, header, request/response şeması, hata senaryoları ve
çalışan bir curl örneği. Dördü de aynı controller'da, aynı pass-through / mock mimarisiyle yaşıyor.

## İçindekiler

1. [Genel Mimari](#1-genel-mimari)
2. [Token — Anonim Oturum Açma](#2-token--anonim-oturum-açma)
3. [Login2 — Kimlik Doğrulama ve OTP Tetikleme](#3-login2--kimlik-doğrulama-ve-otp-tetikleme)
4. [SendSmsCode — OTP Doğrulama](#4-sendsmscode--otp-doğrulama)
5. [GetAuth — Personel Yetki ve Oturum Bilgisi](#5-getauth--personel-yetki-ve-oturum-bilgisi)
6. [Uçtan Uca Akış](#6-uçtan-uca-akış)
7. [Ortam ve Config](#7-ortam-ve-config)
8. [Mimari Notlar](#8-mimari-notlar)

---

## 1. Genel Mimari

`MobileAuthController` (`src/DashboardTsy.Api/Controllers/MobileAuthController.cs`) dört action
barındırır. Her biri ortak bir `HandleAsync` altyapısını kullanır ve iki modda çalışır:

- **Pass-through (varsayılan, `AuthMock:Enabled = false`)** — istek body'si ve `Authorization`
  header'ı byte-birebir KutupYıldızı'nın (`KutupYildizi:BaseUrl`) ilgili `/api/…` endpoint'ine
  forward edilir; Kutup'un döndüğü response aynen mobil client'a iletilir.
- **Mock (`AuthMock:Enabled = true`)** — Kutup'a hiç gidilmez, `MockMobileAuthScenario`
  (`src/DashboardTsy.Api/Services/MockMobileAuthScenario.cs`) sabit senaryolarla aynı şemada yanıt
  üretir. iOS ekibi Kutup ortamına bağımlı kalmadan geliştirme yapabilir.

Mobil client tüm isteklerini `/mobile` önekiyle atar. `MobileEnvelopeMiddleware` öneki kaldırıp
controller'a yönlendirir, sonra controller'ın döndüğü JSON'u `{ status, message, data }` zarfına
sararak client'a döner.

| Endpoint | Mobil path | JWT gerekli mi |
|---|---|---|
| Token | `/mobile/api/Token` | Hayır |
| Login2 | `/mobile/api/Login2` | Hayır |
| SendSmsCode | `/mobile/api/SendSmsCode` | Hayır |
| GetAuth | `/mobile/api/GetAuth` | **Evet** — Bearer token zorunlu |

> **Neden ilk üçü anonim, GetAuth değil?**
> Token/Login2/SendSmsCode login akışının kendisi — henüz elde bir token yokken çağrılırlar, bu
> yüzden `MobileEnvelopeMiddleware.AnonymousPaths` listesinde JWT kontrolünden muaflar. GetAuth ise
> Kutup'ta `[Authorize(Roles="User")]` ile korunur; SendSmsCode'dan alınan `AccessToken` olmadan
> çağrılamaz, dolayısıyla bu listeye bilerek eklenmedi.

---

## 2. Token — Anonim Oturum Açma

```
POST /mobile/api/Token
```
JWT gerekmiyor.

Login akışının ilk adımı. Client kimliğini (ClientId/ClientSecret) doğrulatıp, sonraki adımlarda
kullanılacak **anonim bir JWT** alır.

### Header'lar

| Header | Değer |
|---|---|
| Content-Type | `application/json` |

### Request Body

Kutup, bu üçlüde `Header` + `Parameters` (dizi) zarfını kullanır — `Parameters[0]` okunur, diziye
ikinci eleman eklemenin bir etkisi yoktur.

| Alan | Tip | Açıklama |
|---|---|---|
| `header` | object | Opsiyonel meta bilgi zarfı (aşağıya bakınız). Mock modda okunmaz. |
| `parameters[0].deviceId` | string | Zorunlu. Cihaz kimliği. |
| `parameters[0].clientId` | string | Zorunlu (pass-through modda). Kutup'ta sabit tanımlı client kimliği. |
| `parameters[0].clientSecret` | string | Zorunlu (pass-through modda). Kutup'ta sabit tanımlı client secret. |

`header` zarfının alt alanları (tüm dört endpoint'te aynı, opsiyonel): `channelRequestId`,
`appKey`, `channel`, `channelSessionId`.

**Örnek istek gövdesi**
```json
{
  "header": {
    "channelRequestId": "b6f1e2d0-4b2b-4e2a-9b0a-111111111111",
    "appKey": "ios-app-key",
    "channel": "MOBILE",
    "channelSessionId": ""
  },
  "parameters": [
    {
      "deviceId": "F1C2A3B4-D5E6-7890-ABCD-EF1234567890",
      "clientId": "9206BF12-31A1-476B-A5CB-A48E2A481227",
      "clientSecret": "<kutup-tarafinda-tanimli-client-secret>"
    }
  ]
}
```

### Yanıt Senaryoları

- **200 · Başarılı** — `clientId`/`clientSecret` doğrulandı → anonim JWT üretildi.
- **400 · Bad Request** — `deviceId` boş, veya `clientId`/`clientSecret` yanlış → boş body, 400.

**Başarılı yanıt (mobile envelope içinde)**
```json
{
  "status": true,
  "message": "OK",
  "data": {
    "Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

| Alan | Tip | Açıklama |
|---|---|---|
| `Token` | string | Anonim JWT. Sonraki adımlarda (Login2, SendSmsCode) Authorization header'ında kullanılır — client bu token'ı saklamalı. |

**curl örneği**
```bash
curl -X POST "https://<host>/mobile/api/Token" \
  -H "Content-Type: application/json" \
  -d '{
    "parameters": [
      {
        "deviceId": "F1C2A3B4-D5E6-7890-ABCD-EF1234567890",
        "clientId": "9206BF12-31A1-476B-A5CB-A48E2A481227",
        "clientSecret": "<client-secret>"
      }
    ]
  }'
```

> **Mock modu (AuthMock:Enabled = true)**
> Kutup'a gidilmez. Sadece `deviceId` dolu mu kontrol edilir — `clientId`/`clientSecret` değeri ne
> olursa olsun kabul edilir. Üretilen JWT, gerçek `MobileJwt:SymmetricKey` ile imzalanır; bu yüzden
> diğer korunan endpoint'lerde de geçerlidir.

---

## 3. Login2 — Kimlik Doğrulama ve OTP Tetikleme

```
POST /mobile/api/Login2
```
JWT middleware'de gerekmiyor*.

Kullanıcı adı/şifre ile kimlik doğrular ve başarılıysa kullanıcının telefonuna SMS OTP kodu
gönderilmesini tetikler.

> \* Middleware seviyesinde JWT şart koşulmaz (AnonymousPaths listesinde), ancak Kutup tarafında
> controller-level `[Authorize(Roles="Anonymous", AuthenticationSchemes="ApplicationSchema")]`
> vardır — yani pass-through modda **Token adımından alınan JWT'yi Authorization header'ında
> göndermek gerekir.**

### Header'lar

| Header | Değer |
|---|---|
| Content-Type | `application/json` |
| Authorization | `Bearer <Token adımından dönen JWT>` — pass-through modda zorunlu |

### Request Body

| Alan | Tip | Açıklama |
|---|---|---|
| `parameters[0].userName` | string | Zorunlu. Kullanıcı adı. |
| `parameters[0].ePassword` | string | Zorunlu. Şifrelenmiş parola. |
| `parameters[0].applicationVersion` | string | Uygulama versiyonu. |
| `parameters[0].userAgentString` | string | User-Agent bilgisi. |
| `parameters[0].clientId` | string | Client kimliği. |
| `parameters[0].softwareVersion` | string | Yazılım versiyonu. |
| `parameters[0].deviceType` | string | Cihaz tipi. |
| `parameters[0].operatingSystem` | string | İşletim sistemi. |

**Örnek istek gövdesi**
```json
{
  "header": {
    "channelRequestId": "b6f1e2d0-4b2b-4e2a-9b0a-222222222222",
    "appKey": "ios-app-key",
    "channel": "MOBILE",
    "channelSessionId": ""
  },
  "parameters": [
    {
      "userName": "bugrahan.akbas",
      "ePassword": "<sifrelenmis-parola>",
      "applicationVersion": "3.2.1",
      "userAgentString": "KutupYildiziApp/3.2.1",
      "clientId": "9206BF12-31A1-476B-A5CB-A48E2A481227",
      "softwareVersion": "iOS 17.4",
      "deviceType": "iPhone",
      "operatingSystem": "iOS"
    }
  ]
}
```

### Yanıt Senaryoları

Kutup'un legacy davranışı: hepsi **HTTP 200** döner, hata durumu `IsError` alanıyla body içinde
taşınır.

- **IsError: 1 · Şifre çözülemedi** — `userName`/`ePassword` decrypt edilemedi.
- **IsError: false · OTP gönderildi** — Kimlik doğrulandı, SMS gönderim isteği başarılı —
  `SmsGuid`/`Key` döner.
- **IsError: true · SMS gönderilemedi / login başarısız** — Dış servis hata döndürdü veya
  kullanıcı adı/şifre hatalı.

**Başarılı yanıt (mobile envelope içinde)**
```json
{
  "status": true,
  "message": "OK",
  "data": {
    "IsError": false,
    "ErrorMessage": null,
    "SmsLength": 6,
    "IsAlphaNumericCode": false,
    "EncryptData": "<sifrelenmis-customerNo>",
    "SmsGuid": "3f2b1a90-....",
    "Key": "a1b2c3d4e5f6..."
  }
}
```

| Alan | Tip | Açıklama |
|---|---|---|
| `IsError` | bool | false ise akış devam eder |
| `ErrorMessage` | string? | Hata varsa açıklama, yoksa null |
| `SmsLength` | int | Beklenen OTP kod uzunluğu (genelde 6) |
| `IsAlphaNumericCode` | bool | OTP kodu alfanumerik mi |
| `EncryptData` | string | **Opaque** — içeriğine bakılmadan SendSmsCode'a `customerNo` olarak geri gönderilir |
| `SmsGuid` | string | SendSmsCode'a aynen iletilecek |
| `Key` | string | SendSmsCode'a aynen iletilecek |

**Hatalı yanıt örneği**
```json
{
  "status": true,
  "message": "OK",
  "data": {
    "IsError": true,
    "ErrorMessage": "Girmiş olduğunuz bilgiler doğrulanamadı. Lütfen tüm bilgileri kontrol ederek tekrar deneyiniz"
  }
}
```

**curl örneği**
```bash
curl -X POST "https://<host>/mobile/api/Login2" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <Token-adimindan-donen-jwt>" \
  -d '{
    "parameters": [
      {
        "userName": "bugrahan.akbas",
        "ePassword": "<sifrelenmis-parola>",
        "applicationVersion": "3.2.1",
        "userAgentString": "KutupYildiziApp/3.2.1",
        "clientId": "9206BF12-31A1-476B-A5CB-A48E2A481227",
        "softwareVersion": "iOS 17.4",
        "deviceType": "iPhone",
        "operatingSystem": "iOS"
      }
    ]
  }'
```

> **Mock modu (AuthMock:Enabled = true)**
> Kredensiyel doğrulaması **yapılmaz** — `parameters[0]` doluysa her zaman başarılı OTP challenge
> döner: sabit `SmsGuid`, sabit `Key`, `EncryptData` olarak sabit bir mock müşteri numarası.

---

## 4. SendSmsCode — OTP Doğrulama

```
POST /mobile/api/SendSmsCode
```
JWT middleware'de gerekmiyor*.

Login2'de gönderilen SMS kodunu doğrular; başarılıysa kullanıcıya özel bir **AccessToken** (giriş
yapmış oturum JWT'si) döner.

> \* Login2 ile aynı durum: middleware'de anonim, ama Kutup'ta `[Authorize(Roles="Anonymous")]` —
> Token adımından alınan JWT'yi Authorization header'ında göndermek gerekir.

### Header'lar

| Header | Değer |
|---|---|
| Content-Type | `application/json` |
| Authorization | `Bearer <Token adımından dönen JWT>` — pass-through modda zorunlu |

### Request Body

Diğer ikisinden farklı olarak **düz obje** — `parameters` dizisi yok.

| Alan | Tip | Açıklama |
|---|---|---|
| `smsGuid` | string | Zorunlu. Login2'den dönen `SmsGuid`. |
| `password` | string | Zorunlu. Kullanıcının girdiği OTP kodu. (Alias: `smsCode` — ikisi de kabul edilir, aynı alana yazılır.) |
| `key` | string | Zorunlu. Login2'den dönen `Key`. |
| `customerNo` | string | Zorunlu. Login2'den dönen `EncryptData` değeri (opaque, aynen geri gönderilir). |
| `userName` | string | Zorunlu. Login2'de kullanılan kullanıcı adı. |
| `deviceId` | string | Zorunlu. Token adımında kullanılan cihaz kimliği. |
| `deviceToken` | string | Opsiyonel. Push bildirim cihaz token'ı. |

**Örnek istek gövdesi**
```json
{
  "smsGuid": "3f2b1a90-....",
  "password": "111111",
  "key": "a1b2c3d4e5f6...",
  "customerNo": "<Login2.EncryptData degeri>",
  "userName": "bugrahan.akbas",
  "deviceId": "F1C2A3B4-D5E6-7890-ABCD-EF1234567890",
  "deviceToken": "<push-token-varsa>"
}
```

### Yanıt Senaryoları

- **400 · Eksik alan** — `deviceId`, `userName`, `smsGuid`, `password`, `key` veya `customerNo` boş.
- **IsError: 1 · Kod yanlış** — Girilen OTP kodu doğru değil; kalan deneme hakkı
  `RemainingTryCount` ile döner.
- **State: "Success" · Doğrulandı** — OTP doğru — kullanıcıya özel `AccessToken` üretildi.

**Başarılı yanıt (mobile envelope içinde)**
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

| Alan | Tip | Açıklama |
|---|---|---|
| `State` | string | Sabit "Success" |
| `CustomerNo` | string | İstekte gönderilen değerin aynısı |
| `CustomerIdentity` | string | Müşteri kimlik numarası |
| `AccessToken` | string | **Bu token'ı saklayın** — GetAuth ve diğer tüm korunan `/mobile/*` endpoint'lerinde Authorization header'ında kullanılır. |

**Hatalı kod yanıtı**
```json
{
  "status": true,
  "message": "OK",
  "data": {
    "Type": "ServiceResponseMessage",
    "IsError": 1,
    "Error": "OTP doğrulaması başarısız.",
    "CustomerNo": "<request'teki customerNo>",
    "RemainingTryCount": 2
  }
}
```

**curl örneği**
```bash
curl -X POST "https://<host>/mobile/api/SendSmsCode" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <Token-adimindan-donen-jwt>" \
  -d '{
    "smsGuid": "3f2b1a90-....",
    "password": "111111",
    "key": "a1b2c3d4e5f6...",
    "customerNo": "<Login2.EncryptData degeri>",
    "userName": "bugrahan.akbas",
    "deviceId": "F1C2A3B4-D5E6-7890-ABCD-EF1234567890"
  }'
```

> **Mock modu (AuthMock:Enabled = true)**
> Test OTP kodu sabittir: **`password = "111111"`**. Bu kod gönderilirse başarılı sayılır ve gerçek
> `MobileJwt:SymmetricKey` ile imzalı bir `AccessToken` döner (rol: "User"). Başka her kod
> `{ "IsError": 1, "ErrorMessage": "Sms kodu doğru değil." }` döndürür.

---

## 5. GetAuth — Personel Yetki ve Oturum Bilgisi

```
POST /mobile/api/GetAuth
```
**JWT zorunlu.**

Personelin (banka çalışanının) müşteri bazlı yetkisini ve oturum bilgisini döndürür. Login
zincirinin **son adımı**.

> **Diğer üçünden farkı**
> Kutup'ta `[Authorize(Roles="User", AuthenticationSchemes="ApplicationSchema")]` ile korunur.
> `MobileEnvelopeMiddleware.AnonymousPaths` listesine **eklenmedi** — yani `/mobile/api/GetAuth`
> çağrısı önce DashboardTsy'nin kendi JWT kontrolünden geçer, sonra `Authorization` header'ı
> Kutup'a da forward edilir (Kutup kendi JWT'sini ayrıca doğrular).

### Header'lar

| Header | Değer |
|---|---|
| Content-Type | `application/json` |
| Authorization | `Bearer <SendSmsCode'dan dönen AccessToken>` — **zorunlu** |

### Request Body

| Alan | Tip | Açıklama |
|---|---|---|
| `hashCustomerNumber` | string | Zorunlu. Şifrelenmiş müşteri numarası — boşsa istek reddedilir. |
| `userName` | string | Kullanıcı adı. |
| `ePassword` | string | Şifrelenmiş parola. |
| `applicationVersion` | string | Uygulama versiyonu. |
| `clientId` | string | Client kimliği. |
| `softwareVersion` | string | Yazılım versiyonu. |
| `deviceType` | string | Cihaz tipi. |
| `operatingSystem` | string | İşletim sistemi. |
| `userAgentString` | string | User-Agent bilgisi. |

**Örnek istek gövdesi**
```json
{
  "hashCustomerNumber": "AbC123XyZ==",
  "userName": "bugrahan.akbas",
  "ePassword": "<sifrelenmis-parola>",
  "applicationVersion": "3.2.1",
  "clientId": "9206BF12-31A1-476B-A5CB-A48E2A481227",
  "softwareVersion": "iOS 17.4",
  "deviceType": "iPhone",
  "operatingSystem": "iOS",
  "userAgentString": "KutupYildiziApp/3.2.1"
}
```

### Yanıt Senaryoları

- **Result: "1" · Yetkili** — Personel doğrulandı — oturum ve yetki bilgisi döner.
- **IsError: 1 · Yetkisiz** — `hashCustomerNumber` boş veya doğrulama başarısız — **farklı bir
  şema** döner (Result alanı yok).

**Başarılı yanıt (mobile envelope içinde)**
```json
{
  "status": true,
  "message": "OK",
  "data": {
    "Result": "1",
    "PersonelName": "Bugrahan Akbas",
    "BranchName": "Merkez Sube",
    "SessionId": "a1b2c3d4-...",
    "NetmerExternalId": "9000000001",
    "HashUserName": "YnVncmFoYW4uYWtiYXM=",
    "IsAdmin": 0,
    "IsPerformanceAdmin": 0,
    "EmergencyToken": "eyJhbGciOiJIUzI1NiIs..."
  }
}
```

| Alan | Tip | Açıklama |
|---|---|---|
| `Result` | string | "1" ise başarılı |
| `PersonelName` | string | Personel adı |
| `BranchName` | string | Şube adı |
| `SessionId` | string | Oturum kimliği |
| `NetmerExternalId` | string | Netmera push-bildirim kimliği |
| `HashUserName` | string | Şifrelenmiş kullanıcı adı |
| `IsAdmin` | int | 0 / 1 |
| `IsPerformanceAdmin` | int | 0 / 1 |
| `EmergencyToken` | string | 3 ay geçerli acil durum JWT'si |

### Başarısız Yanıt

Kutup'un legacy davranışı: başarısız durumda **farklı bir şema** döner (Result alanı yok).
DashboardTsy bunu değiştirmeden aynen iletir.

```json
{
  "status": true,
  "message": "OK",
  "data": {
    "IsError": 1,
    "Error": "Uygulama için yetkiniz yoktur.",
    "HashUserName": "..."
  }
}
```

> **Önemli**
> `status: true` olmasına dikkat — HTTP seviyesinde 200 döner, hata `IsError` alanıyla body
> içinde taşınır. Mobil taraf HTTP status koduna değil, `data.IsError` alanına bakmalı.

**curl örneği**
```bash
curl -X POST "https://<host>/mobile/api/GetAuth" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <SendSmsCode-adimindan-donen-AccessToken>" \
  -d '{
    "hashCustomerNumber": "AbC123XyZ==",
    "userName": "bugrahan.akbas",
    "ePassword": "<sifrelenmis-parola>",
    "applicationVersion": "3.2.1",
    "clientId": "9206BF12-31A1-476B-A5CB-A48E2A481227",
    "softwareVersion": "iOS 17.4",
    "deviceType": "iPhone",
    "operatingSystem": "iOS",
    "userAgentString": "KutupYildiziApp/3.2.1"
  }'
```

> **Mock modu (AuthMock:Enabled = true)**
> - `hashCustomerNumber` boş gönderilirse → yetkisiz hatası döner.
> - Doluysa → sabit mock personel verisi (isim: gönderilen `userName`, şube: "Mock Sube") döner.
> - Üretilen `EmergencyToken`, gerçek `MobileJwt:SymmetricKey` ile imzalanır — mock modda da diğer
>   korunan endpoint'lerde geçerli bir token olarak kullanılabilir.

---

## 6. Uçtan Uca Akış

Mobil client'ın tam login zinciri, adım adım:

1. **`POST /mobile/api/Token`**
   deviceId + clientId/clientSecret ile anonim JWT alınır.

2. **`POST /mobile/api/Login2` — `Authorization: Bearer <Token>`**
   Kullanıcı adı/şifre ile OTP challenge tetiklenir; `SmsGuid`, `Key`, `EncryptData` döner.

3. **`POST /mobile/api/SendSmsCode` — `Authorization: Bearer <Token>`**
   Kullanıcının girdiği SMS kodu, Login2'den dönen `SmsGuid`/`Key`/`EncryptData` ile birlikte
   doğrulanır; başarılıysa `AccessToken` döner.

4. **`POST /mobile/api/GetAuth` — `Authorization: Bearer <AccessToken>`**
   Personel yetkisi ve oturum bilgisi (SessionId, EmergencyToken, IsAdmin…) alınır. Bu noktadan
   sonra mobil uygulama diğer korunan `/mobile/*` endpoint'lerine aynı `AccessToken` ile erişir.

> **İki farklı token'a dikkat**
> Adım 1'den dönen **Token** (anonim, rol: "Anonymous") sadece Adım 2 ve 3'te kullanılır. Adım
> 3'ten dönen **AccessToken** (kullanıcıya özel, rol: "User") Adım 4'te ve sonrasında kullanılır.
> İkisini karıştırmak 401/yetkisiz hatasına yol açar.

---

## 7. Ortam ve Config

| Config anahtarı | Anlamı |
|---|---|
| `AuthMock:Enabled` | true → mock mod, false (varsayılan) → Kutup'a pass-through. Dört endpoint için de geçerli tek anahtar. |
| `KutupYildizi:BaseUrl` | Pass-through modda proxy hedefi (Kutup'un gerçek URL'i). |
| `MobileJwt:SymmetricKey` | Kutup'un JwtManager'ıyla birebir aynı base64 key — mock modda üretilen token'ların DashboardTsy JWT middleware'inde de geçerli sayılması için. |

Gerçek host değeri ortam bazlı (dev/UAT/prod) config'ten alınmalı — bu doküman sabit bir URL
vermez, sadece path yapısını (`/mobile/api/…`) garanti eder.

---

## 8. Mimari Notlar

Dört endpoint de bilinçli olarak **Api katmanında** (Application/Domain/Infrastructure'a
sızmadan) tutuldu — çünkü iş mantığı gerçek bir domain kuralı değil, harici bir sisteme
(KutupYıldızı) byte-birebir proxy. Mevcut `MobileAuthController` zaten bu ilkeyle tasarlanmıştı;
`GetAuth` aynı sınırın içinde eklendi.

Eğer ileride bu endpoint'lerden biri DashboardTsy tarafında gerçek bir iş kuralı (örn. DB'den ek
yetki kontrolü) almayı gerektirirse, o zaman `PublicController` / `IWindowsAuthService` pattern'i
örnek alınarak Application katmanında bir port tanımlanmalı — bugünkü haliyle buna gerek yoktur.

---

*DashboardTsy.Api · MobileAuthController — 2026-08-24*
