-- Kullanıcı aktivite log tablosu (C# model: UserActivityLog / DbContext: UserActivityLogDbContext).
-- Bu projede tablolar genelde DB tarafında elle oluşturulur; EF migration kullanılmaz.
-- Uygun veritabanında (ör. oturum / rapor DB) bir kez çalıştırın.

IF OBJECT_ID(N'dbo.UserActivityLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserActivityLogs
    (
        Id               BIGINT         IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        UserId           INT            NOT NULL,
        UserDisplayName  NVARCHAR(200)  NULL,
        EventType        NVARCHAR(64)   NOT NULL,
        ActionName       NVARCHAR(500)  NULL,
        Route            NVARCHAR(500)  NULL,
        PageUrl          NVARCHAR(2000) NULL,
        Details          NVARCHAR(MAX)  NULL,
        IpAddress        NVARCHAR(45)   NULL,
        UserAgent        NVARCHAR(500)  NULL,
        CreatedAtUtc     DATETIME2      NOT NULL CONSTRAINT DF_UserActivityLogs_CreatedAtUtc DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_UserActivityLogs_UserId_CreatedAtUtc
        ON dbo.UserActivityLogs (UserId, CreatedAtUtc DESC);

    CREATE INDEX IX_UserActivityLogs_CreatedAtUtc
        ON dbo.UserActivityLogs (CreatedAtUtc DESC);
END
