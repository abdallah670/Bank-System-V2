-- ============================================
-- BankSystemV2 Database Indexes
-- Performance optimization indexes
-- ============================================

USE BankSystemV2;
GO

-- ============================================
-- Users Table Indexes
-- ============================================
CREATE NONCLUSTERED INDEX IX_Users_Username 
ON Users(Username) 
WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Users_Email 
ON Users(Email);

CREATE NONCLUSTERED INDEX IX_Users_Role 
ON Users(Role) 
WHERE IsActive = 1 AND IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Users_IsActive 
ON Users(IsActive, IsDeleted) 
WHERE IsActive = 1 AND IsDeleted = 0;

-- ============================================
-- Customers Table Indexes
-- ============================================
CREATE NONCLUSTERED INDEX IX_Customers_Email 
ON Customers(Email);

CREATE NONCLUSTERED INDEX IX_Customers_Phone 
ON Customers(Phone);

CREATE NONCLUSTERED INDEX IX_Customers_Name 
ON Customers(LastName, FirstName);

CREATE NONCLUSTERED INDEX IX_Customers_IsActive 
ON Customers(IsActive, IsDeleted) 
WHERE IsActive = 1 AND IsDeleted = 0;

-- ============================================
-- Accounts Table Indexes
-- ============================================
CREATE NONCLUSTERED INDEX IX_Accounts_AccountNumber 
ON Accounts(AccountNumber) 
WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Accounts_CustomerId 
ON Accounts(CustomerId) 
WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Accounts_Type 
ON Accounts(AccountType, IsActive) 
WHERE IsActive = 1 AND IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Accounts_Balance 
ON Accounts(Balance) 
WHERE IsActive = 1 AND IsDeleted = 0;

-- Composite index for account lookups
CREATE NONCLUSTERED INDEX IX_Accounts_Customer_Type 
ON Accounts(CustomerId, AccountType) 
WHERE IsActive = 1 AND IsDeleted = 0;

-- ============================================
-- Transactions Table Indexes
-- ============================================
CREATE NONCLUSTERED INDEX IX_Transactions_FromAccountId 
ON Transactions(FromAccountId, CreatedAt DESC);

CREATE NONCLUSTERED INDEX IX_Transactions_ToAccountId 
ON Transactions(ToAccountId, CreatedAt DESC);

CREATE NONCLUSTERED INDEX IX_Transactions_Type 
ON Transactions(Type, CreatedAt DESC);

CREATE NONCLUSTERED INDEX IX_Transactions_Status 
ON Transactions(Status) 
WHERE Status = 'Pending';

CREATE NONCLUSTERED INDEX IX_Transactions_CreatedAt 
ON Transactions(CreatedAt DESC);

-- Composite index for date range queries
CREATE NONCLUSTERED INDEX IX_Transactions_DateRange 
ON Transactions(CreatedAt DESC, Type) 
INCLUDE (Amount, FromAccountId, ToAccountId);

-- Index for reference number lookups
CREATE NONCLUSTERED INDEX IX_Transactions_ReferenceNumber 
ON Transactions(ReferenceNumber);

-- Index for user audit trail
CREATE NONCLUSTERED INDEX IX_Transactions_CreatedBy 
ON Transactions(CreatedBy, CreatedAt DESC);

-- ============================================
-- AuditLogs Table Indexes
-- ============================================
CREATE NONCLUSTERED INDEX IX_AuditLogs_UserId 
ON AuditLogs(UserId, Timestamp DESC);

CREATE NONCLUSTERED INDEX IX_AuditLogs_Entity 
ON AuditLogs(EntityType, EntityId) 
WHERE EntityId IS NOT NULL;

CREATE NONCLUSTERED INDEX IX_AuditLogs_Action 
ON AuditLogs(Action, Timestamp DESC);

CREATE NONCLUSTERED INDEX IX_AuditLogs_Timestamp 
ON AuditLogs(Timestamp DESC);

-- ============================================
-- LoginAttempts Table Indexes
-- ============================================
CREATE NONCLUSTERED INDEX IX_LoginAttempts_Username 
ON LoginAttempts(Username, AttemptedAt DESC);

CREATE NONCLUSTERED INDEX IX_LoginAttempts_IPAddress 
ON LoginAttempts(IPAddress, AttemptedAt DESC);

CREATE NONCLUSTERED INDEX IX_LoginAttempts_Success 
ON LoginAttempts(Username, Success, AttemptedAt DESC);

-- ============================================
-- RefreshTokens Table Indexes
-- ============================================
CREATE NONCLUSTERED INDEX IX_RefreshTokens_Token 
ON RefreshTokens(Token) 
WHERE IsRevoked = 0;

CREATE NONCLUSTERED INDEX IX_RefreshTokens_UserId 
ON RefreshTokens(UserId, ExpiresAt);

CREATE NONCLUSTERED INDEX IX_RefreshTokens_ExpiresAt 
ON RefreshTokens(ExpiresAt) 
WHERE IsRevoked = 0;

PRINT 'Indexes created successfully!';
