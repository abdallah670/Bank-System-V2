-- ============================================
-- BankSystemV2 Complete Database Setup
-- Run this single file to set up the entire database
-- ============================================

USE master;
GO

-- Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BankSystemV2')
BEGIN
    CREATE DATABASE BankSystemV2;
    PRINT 'Database BankSystemV2 created.';
END
ELSE
BEGIN
    PRINT 'Database BankSystemV2 already exists.';
END
GO

USE BankSystemV2;
GO

-- ============================================
-- TABLE: Users
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId INT PRIMARY KEY IDENTITY(1,1),
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        Role NVARCHAR(20) NOT NULL DEFAULT 'BankAdmin',
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastModified DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastLoginAt DATETIME2 NULL
    );
    PRINT 'Table Users created.';
END
GO

-- ============================================
-- TABLE: Customers
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE Customers (
        CustomerId INT PRIMARY KEY IDENTITY(1,1),
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20) NOT NULL,
        Address NVARCHAR(255) NULL,
        City NVARCHAR(100) NULL,
        Country NVARCHAR(100) NOT NULL DEFAULT 'United States',
        DateOfBirth DATE NULL,
        IdentificationNumber NVARCHAR(50) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastModified DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Table Customers created.';
END
GO

-- ============================================
-- TABLE: Accounts
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Accounts')
BEGIN
    CREATE TABLE Accounts (
        AccountId INT PRIMARY KEY IDENTITY(1,1),
        AccountNumber NVARCHAR(20) NOT NULL UNIQUE,
        CustomerId INT NOT NULL,
        AccountType NVARCHAR(20) NOT NULL DEFAULT 'Savings',
        Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',
        Balance DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastModified DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Table Accounts created.';
END
GO

-- ============================================
-- TABLE: Transactions
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transactions')
BEGIN
    CREATE TABLE Transactions (
        TransactionId BIGINT PRIMARY KEY IDENTITY(1,1),
        Type NVARCHAR(20) NOT NULL,
        FromAccountId INT NULL,
        ToAccountId INT NULL,
        Amount DECIMAL(18,4) NOT NULL,
        BalanceAfter DECIMAL(18,2) NOT NULL,
        ReferenceNumber NVARCHAR(50) NOT NULL UNIQUE,
        Description NVARCHAR(255) NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Completed',
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Table Transactions created.';
END
GO

-- ============================================
-- TABLE: AuditLogs
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        AuditId BIGINT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        Action NVARCHAR(50) NOT NULL,
        EntityType NVARCHAR(50) NOT NULL,
        EntityId INT NULL,
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        IPAddress NVARCHAR(50) NULL,
        UserAgent NVARCHAR(255) NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Table AuditLogs created.';
END
GO

-- ============================================
-- TABLE: RefreshTokens
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        TokenId INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        Token NVARCHAR(255) NOT NULL UNIQUE,
        ExpiresAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        RevokedAt DATETIME2 NULL,
        IsRevoked BIT NOT NULL DEFAULT 0
    );
    PRINT 'Table RefreshTokens created.';
END
GO

-- ============================================
-- TABLE: LoginAttempts
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoginAttempts')
BEGIN
    CREATE TABLE LoginAttempts (
        AttemptId INT PRIMARY KEY IDENTITY(1,1),
        Username NVARCHAR(50) NOT NULL,
        IPAddress NVARCHAR(50) NOT NULL,
        Success BIT NOT NULL DEFAULT 0,
        AttemptedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        FailureReason NVARCHAR(100) NULL
    );
    PRINT 'Table LoginAttempts created.';
END
GO

-- ============================================
-- PHASE 2 TABLES
-- ============================================

-- TwoFactorAuths
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
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Table TwoFactorAuths created.';
END
GO

-- Notifications
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
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Table Notifications created.';
END
GO

-- PasswordHistories
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PasswordHistories')
BEGIN
    CREATE TABLE PasswordHistories (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Table PasswordHistories created.';
END
GO

-- Sessions
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
        IsRevoked BIT NOT NULL DEFAULT 0
    );
    PRINT 'Table Sessions created.';
END
GO

-- ============================================
-- FOREIGN KEYS
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Accounts_Customers')
BEGIN
    ALTER TABLE Accounts ADD CONSTRAINT FK_Accounts_Customers 
        FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId) ON DELETE RESTRICT;
    PRINT 'Foreign key FK_Accounts_Customers created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Transactions_FromAccount')
BEGIN
    ALTER TABLE Transactions ADD CONSTRAINT FK_Transactions_FromAccount 
        FOREIGN KEY (FromAccountId) REFERENCES Accounts(AccountId) ON DELETE SET NULL;
    PRINT 'Foreign key FK_Transactions_FromAccount created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Transactions_ToAccount')
BEGIN
    ALTER TABLE Transactions ADD CONSTRAINT FK_Transactions_ToAccount 
        FOREIGN KEY (ToAccountId) REFERENCES Accounts(AccountId) ON DELETE SET NULL;
    PRINT 'Foreign key FK_Transactions_ToAccount created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Transactions_Users')
BEGIN
    ALTER TABLE Transactions ADD CONSTRAINT FK_Transactions_Users 
        FOREIGN KEY (CreatedBy) REFERENCES Users(UserId) ON DELETE RESTRICT;
    PRINT 'Foreign key FK_Transactions_Users created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AuditLogs_Users')
BEGIN
    ALTER TABLE AuditLogs ADD CONSTRAINT FK_AuditLogs_Users 
        FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE RESTRICT;
    PRINT 'Foreign key FK_AuditLogs_Users created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_RefreshTokens_Users')
BEGIN
    ALTER TABLE RefreshTokens ADD CONSTRAINT FK_RefreshTokens_Users 
        FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE;
    PRINT 'Foreign key FK_RefreshTokens_Users created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Notifications_Users')
BEGIN
    ALTER TABLE Notifications ADD CONSTRAINT FK_Notifications_Users 
        FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE;
    PRINT 'Foreign key FK_Notifications_Users created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PasswordHistories_Users')
BEGIN
    ALTER TABLE PasswordHistories ADD CONSTRAINT FK_PasswordHistories_Users 
        FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE;
    PRINT 'Foreign key FK_PasswordHistories_Users created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Sessions_Users')
BEGIN
    ALTER TABLE Sessions ADD CONSTRAINT FK_Sessions_Users 
        FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE;
    PRINT 'Foreign key FK_Sessions_Users created.';
END
GO

-- ============================================
-- INDEXES
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username' AND object_id = OBJECT_ID('Users'))
    CREATE INDEX IX_Users_Username ON Users(Username) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('Users'))
    CREATE INDEX IX_Users_Email ON Users(Email);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_Email' AND object_id = OBJECT_ID('Customers'))
    CREATE INDEX IX_Customers_Email ON Customers(Email);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_Name' AND object_id = OBJECT_ID('Customers'))
    CREATE INDEX IX_Customers_Name ON Customers(LastName, FirstName);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Accounts_AccountNumber' AND object_id = OBJECT_ID('Accounts'))
    CREATE INDEX IX_Accounts_AccountNumber ON Accounts(AccountNumber) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Accounts_CustomerId' AND object_id = OBJECT_ID('Accounts'))
    CREATE INDEX IX_Accounts_CustomerId ON Accounts(CustomerId) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Transactions_CreatedAt' AND object_id = OBJECT_ID('Transactions'))
    CREATE INDEX IX_Transactions_CreatedAt ON Transactions(CreatedAt DESC);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Transactions_ReferenceNumber' AND object_id = OBJECT_ID('Transactions'))
    CREATE INDEX IX_Transactions_ReferenceNumber ON Transactions(ReferenceNumber);

PRINT 'Indexes created.';
GO

-- ============================================
-- SEED DATA - SuperAdmin User
-- Password: Admin@123 (BCrypt hash with cost factor 12)
-- ============================================

IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, Role, IsActive, IsDeleted, CreatedAt, LastModified)
    VALUES (
        'admin',
        'admin@bank.com',
        '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4gHrN7mK5e.lnqG2',
        'System',
        'Administrator',
        'SuperAdmin',
        1,
        0,
        GETUTCDATE(),
        GETUTCDATE()
    );
    PRINT 'SuperAdmin user created.';
END
ELSE
BEGIN
    PRINT 'Admin user already exists.';
END
GO

-- Sample Customers
IF NOT EXISTS (SELECT * FROM Customers WHERE Email = 'michael.johnson@email.com')
BEGIN
    INSERT INTO Customers (FirstName, LastName, Email, Phone, Address, City, Country, DateOfBirth, IdentificationNumber, IsActive, IsDeleted, CreatedAt, LastModified)
    VALUES 
    ('Michael', 'Johnson', 'michael.johnson@email.com', '+1-555-0101', '123 Main Street', 'New York', 'United States', '1985-03-15', 'ID-001-MJ', 1, 0, GETUTCDATE(), GETUTCDATE()),
    ('Sarah', 'Williams', 'sarah.williams@email.com', '+1-555-0102', '456 Oak Avenue', 'Los Angeles', 'United States', '1990-07-22', 'ID-002-SW', 1, 0, GETUTCDATE(), GETUTCDATE()),
    ('David', 'Brown', 'david.brown@email.com', '+1-555-0103', '789 Pine Road', 'Chicago', 'United States', '1978-11-30', 'ID-003-DB', 1, 0, GETUTCDATE(), GETUTCDATE()),
    ('Emily', 'Davis', 'emily.davis@email.com', '+1-555-0104', '321 Elm Street', 'Houston', 'United States', '1995-01-10', 'ID-004-ED', 1, 0, GETUTCDATE(), GETUTCDATE()),
    ('Robert', 'Miller', 'robert.miller@email.com', '+1-555-0105', '654 Maple Drive', 'Phoenix', 'United States', '1982-09-05', 'ID-005-RM', 1, 0, GETUTCDATE(), GETUTCDATE());
    PRINT 'Sample customers created.';
END
GO

-- Sample Accounts
IF NOT EXISTS (SELECT * FROM Accounts WHERE AccountNumber = '1000000001')
BEGIN
    INSERT INTO Accounts (AccountNumber, CustomerId, AccountType, Currency, Balance, IsActive, IsDeleted, CreatedAt, LastModified)
    VALUES 
    ('1000000001', 1, 'Savings', 'USD', 15000.00, 1, 0, GETUTCDATE(), GETUTCDATE()),
    ('1000000002', 1, 'Checking', 'USD', 5000.00, 1, 0, GETUTCDATE(), GETUTCDATE()),
    ('1000000003', 2, 'Savings', 'USD', 25000.00, 1, 0, GETUTCDATE(), GETUTCDATE()),
    ('1000000004', 2, 'Checking', 'USD', 3500.00, 1, 0, GETUTCDATE(), GETUTCDATE()),
    ('1000000005', 3, 'Business', 'USD', 100000.00, 1, 0, GETUTCDATE(), GETUTCDATE()),
    ('1000000006', 4, 'Savings', 'USD', 8000.00, 1, 0, GETUTCDATE(), GETUTCDATE()),
    ('1000000007', 5, 'Checking', 'USD', 12000.00, 1, 0, GETUTCDATE(), GETUTCDATE());
    PRINT 'Sample accounts created.';
END
GO

PRINT '';
PRINT '============================================';
PRINT '   BankSystemV2 Database Setup Complete!';
PRINT '============================================';
PRINT '';
PRINT 'DEFAULT CREDENTIALS:';
PRINT '  Username: admin';
PRINT '  Password: Admin@123';
PRINT '  Role: SuperAdmin';
PRINT '';
PRINT 'IMPORTANT: Change the default password after first login!';
PRINT '';
GO
