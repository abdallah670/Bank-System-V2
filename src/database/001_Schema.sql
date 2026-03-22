-- ============================================
-- BankSystemV2 Database Schema
-- Version: 1.0.0
-- Date: 2024
-- ============================================

USE master;
GO

-- Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BankSystemV2')
BEGIN
    CREATE DATABASE BankSystemV2;
END
GO

USE BankSystemV2;
GO

-- ============================================
-- TABLE: Users
-- Description: System administrators and staff
-- ============================================
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
    LastLoginAt DATETIME2 NULL,
    
    CONSTRAINT CK_Users_Role CHECK (Role IN ('SuperAdmin', 'BankAdmin', 'Auditor')),
    CONSTRAINT CK_Users_Email CHECK (Email LIKE '%@%.%')
);

-- ============================================
-- TABLE: Customers
-- Description: Bank customers (clients)
-- ============================================
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
    LastModified DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT CK_Customers_Email CHECK (Email LIKE '%@%.%')
);

-- ============================================
-- TABLE: Accounts
-- Description: Customer bank accounts
-- ============================================
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
    LastModified DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Accounts_Customers FOREIGN KEY (CustomerId) 
        REFERENCES Customers(CustomerId) ON DELETE RESTRICT,
    CONSTRAINT CK_Accounts_Type CHECK (AccountType IN ('Savings', 'Checking', 'Business')),
    CONSTRAINT CK_Accounts_Balance CHECK (Balance >= 0)
);

-- ============================================
-- TABLE: Transactions
-- Description: All financial transactions
-- ============================================
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
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Transactions_FromAccount FOREIGN KEY (FromAccountId) 
        REFERENCES Accounts(AccountId) ON DELETE SET NULL,
    CONSTRAINT FK_Transactions_ToAccount FOREIGN KEY (ToAccountId) 
        REFERENCES Accounts(AccountId) ON DELETE SET NULL,
    CONSTRAINT FK_Transactions_Users FOREIGN KEY (CreatedBy) 
        REFERENCES Users(UserId) ON DELETE RESTRICT,
    CONSTRAINT CK_Transactions_Type CHECK (Type IN ('Deposit', 'Withdrawal', 'Transfer')),
    CONSTRAINT CK_Transactions_Status CHECK (Status IN ('Pending', 'Completed', 'Failed', 'Reversed')),
    CONSTRAINT CK_Transactions_Amount CHECK (Amount > 0),
    CONSTRAINT CK_Transactions_Accounts CHECK (
        (Type = 'Deposit' AND FromAccountId IS NULL) OR
        (Type = 'Withdrawal' AND ToAccountId IS NULL) OR
        (Type = 'Transfer' AND FromAccountId IS NOT NULL AND ToAccountId IS NOT NULL)
    )
);

-- ============================================
-- TABLE: AuditLogs
-- Description: System activity tracking
-- ============================================
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
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) 
        REFERENCES Users(UserId) ON DELETE RESTRICT
);

-- ============================================
-- TABLE: LoginAttempts
-- Description: Track login attempts for security
-- ============================================
CREATE TABLE LoginAttempts (
    AttemptId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    IPAddress NVARCHAR(50) NOT NULL,
    Success BIT NOT NULL DEFAULT 0,
    AttemptedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FailureReason NVARCHAR(100) NULL
);

-- ============================================
-- TABLE: RefreshTokens
-- Description: JWT refresh tokens storage
-- ============================================
CREATE TABLE RefreshTokens (
    TokenId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Token NVARCHAR(255) NOT NULL UNIQUE,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RevokedAt DATETIME2 NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) 
        REFERENCES Users(UserId) ON DELETE CASCADE
);

PRINT 'Schema created successfully!';
