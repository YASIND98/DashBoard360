-- Generic key/value ayar tablosu.
-- ValueType: 1=String, 2=Int, 3=Decimal, 4=Boolean (bkz. DashboardTsy.Domain.AppSettings.AppSettingType).
-- Value her zaman NVARCHAR olarak saklanır; okuma tarafında ValueType'a göre parse edilir.
-- Bu projede tablolar DB tarafında elle oluşturulur; EF migration kullanılmaz.

IF OBJECT_ID(N'dbo.AppSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppSettings
    (
        Id            INT           IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        [Key]         NVARCHAR(200) NOT NULL,
        [Value]       NVARCHAR(MAX) NULL,
        ValueType     TINYINT       NOT NULL,
        [Description] NVARCHAR(500) NULL,
        IsActive      BIT           NOT NULL CONSTRAINT DF_AppSettings_IsActive DEFAULT (1),
        CreatedAtUtc  DATETIME2     NOT NULL CONSTRAINT DF_AppSettings_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedAtUtc  DATETIME2     NULL,
        CONSTRAINT UQ_AppSettings_Key UNIQUE ([Key]),
        CONSTRAINT CK_AppSettings_ValueType CHECK (ValueType IN (1, 2, 3, 4))
    );

    CREATE INDEX IX_AppSettings_IsActive
        ON dbo.AppSettings (IsActive)
        INCLUDE ([Key], [Value], ValueType);
END
GO

-- Örnek seed: verim raporları AI görünürlüğü.
IF NOT EXISTS (SELECT 1 FROM dbo.AppSettings WHERE [Key] = N'ProductivityReport.AiInsight.Visible')
BEGIN
    INSERT INTO dbo.AppSettings ([Key], [Value], ValueType, [Description])
    VALUES (N'ProductivityReport.AiInsight.Visible', N'true', 4, N'Verim raporları ekranında AI Insight bölümünün görünürlüğü.');
END
