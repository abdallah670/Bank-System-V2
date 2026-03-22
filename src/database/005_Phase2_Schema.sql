-- ============================================
-- BankSystemV2 Phase 2 - Additional Tables
-- Run after initial schema
-- ============================================

USE BankSystemV2;
GO

-- ============================================
-- TABLE: TwoFactorAuth
-- Description: Two-factor authentication settings
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TwoFactorAuths')
BEGIN
    CREATE TABLE TwoFactorAuths (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL UNIQUE,
        SecretKey NVARCHAR(255) NOT NULL,
        BackupCodes NVARCHAR(MAX) NULL,
        IsEnabled BIT NOT NULL DEFAULT 0,
        IsVerified BIT NOT NULL DEFAULT 0,
        LastUsedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_TwoFactorAuths_Users FOREIGN KEY (UserId) 
            REFERENCES Users(UserId) ON DELETE CASCADE
    );
END

-- ============================================
-- TABLE: TwoFactorTokens
-- Description: Temporary tokens for 2FA verification
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TwoFactorTokens')
BEGIN
    CREATE TABLE TwoFactorTokens (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        Token NVARCHAR(255) NOT NULL UNIQUE,
        Code NVARCHAR(10) NOT NULL,
        Type NVARCHAR(20) NOT NULL,
        ExpiresAt DATETIME2 NOT NULL,
        IsUsed BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_TwoFactorTokens_Users FOREIGN KEY (UserId) 
            REFERENCES Users(UserId) ON DELETE CASCADE
    );
END

-- ============================================
-- TABLE: Notifications
-- Description: User notifications
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE Notifications (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        Title NVARCHAR(100) NOT NULL,
        Message NVARCHAR(500) NOT NULL,
        Type NVARCHAR(20) NOT NULL DEFAULT 'Info',
        IsRead BIT NOT NULL DEFAULT 0,
        Link NVARCHAR(255) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) 
            REFERENCES Users(UserId) ON DELETE CASCADE
    );
END

-- ============================================
-- TABLE: PasswordHistories
-- Description: Track password changes for policy
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PasswordHistories')
BEGIN
    CREATE TABLE PasswordHistories (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_PasswordHistories_Users FOREIGN KEY (UserId) 
            REFERENCES Users(UserId) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_PasswordHistories_UserId ON PasswordHistories(UserId);
END

-- ============================================
-- TABLE: Sessions
-- Description: Active user sessions
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Sessions')
BEGIN
    CREATE TABLE Sessions (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        SessionId NVARCHAR(255) NOT NULL UNIQUE,
        IPAddress NVARCHAR(50) NULL,
        UserAgent NVARCHAR(255) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NOT NULL,
        LastActivityAt DATETIME2 NULL,
        IsRevoked BIT NOT NULL DEFAULT 0,
        
        CONSTRAINT FK_Sessions_Users FOREIGN KEY (UserId) 
            REFERENCES Users(UserId) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_Sessions_UserId ON Sessions(UserId);
    CREATE INDEX IX_Sessions_ExpiresAt ON Sessions(ExpiresAt) WHERE IsRevoked = 0;
END

PRINT 'Phase 2 tables created successfully!';
