-- Skor kart (Pupa) servis istek log tablosu.
-- C#: ScoreCardController her isteği (başarılı / hatalı) kuyruğa atar, ScoreCardRequestLogWriter bu tabloya yazar.
-- Tablo DB tarafında elle oluşturulur; YoneticiRaporu veritabanında bir kez çalıştırın.

IF OBJECT_ID(N'dbo.ScoreCardRequestLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ScoreCardRequestLogs
    (
        Id                   BIGINT         IDENTITY(1, 1) NOT NULL CONSTRAINT PK_ScoreCardRequestLogs PRIMARY KEY,
        RequestedAt          DATETIME2(3)   NOT NULL,      -- İsteğin geldiği an (sunucu saati)
        Endpoint             NVARCHAR(500)  NOT NULL,      -- Ör. scorecard/cumulatives, scorecard/periods?periodTypes=1
        HttpMethod           NVARCHAR(10)   NOT NULL,      -- GET / POST
        RequestBody          NVARCHAR(MAX)  NULL,          -- Input (GET isteklerinde NULL; input Endpoint query'sindedir)
        ResponseBody         NVARCHAR(MAX)  NULL,          -- Output (skor kart servisinin döndüğü cevap)
        StatusCode           INT            NULL,          -- Dönen HTTP kodu; servise ulaşılamadıysa NULL
        IsSuccess            BIT            NOT NULL,      -- 1 = skor kart servisi 2xx döndü
        ErrorMessage         NVARCHAR(MAX)  NULL,          -- Hata mesajı (token hatası, servis hata cevabı, bağlantı hatası)
        DurationMs           BIGINT         NULL,          -- Skor kart servisinin yanıt süresi (ms); servise gidilmediyse NULL
        RequesterUserCode    NVARCHAR(100)  NULL,          -- İsteği atan kullanıcı (ExternalContext.UserCode)
        RequesterBranchCode  NVARCHAR(50)   NULL,          -- İsteği atan kullanıcının şubesi (ExternalContext.BranchCode)
        RegisterId           NVARCHAR(50)   NULL,          -- Seçili sicil
        RegionCode           NVARCHAR(50)   NULL,          -- Seçili bölge
        BranchCode           NVARCHAR(50)   NULL,          -- Seçili şube
        ScoreCardId          NVARCHAR(50)   NULL,          -- Skor kart
        ScoreCardTypeId      NVARCHAR(50)   NULL,          -- Skor kart tipi
        PupaType             NVARCHAR(50)   NULL           -- Pupa tipi
    );

    CREATE INDEX IX_ScoreCardRequestLogs_RequestedAt
        ON dbo.ScoreCardRequestLogs (RequestedAt DESC);

    CREATE INDEX IX_ScoreCardRequestLogs_Endpoint_RequestedAt
        ON dbo.ScoreCardRequestLogs (Endpoint, RequestedAt DESC);
END
