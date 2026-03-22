-- ============================================
-- BankSystemV2 Seed Data
-- Initial admin users and reference data
-- ============================================

USE BankSystemV2;
GO

-- ============================================
-- Create SuperAdmin User
-- Password: Admin@123 (BCrypt hashed with cost factor 12)
-- ============================================

DECLARE @PasswordHash NVARCHAR(255);
SET @PasswordHash = '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4gHrN7mK5e.lnqG2';
-- Note: Hash above is for "Admin@123" with BCrypt cost factor 12
-- For production, generate a new hash

INSERT INTO Users (
    Username,
    Email,
    PasswordHash,
    FirstName,
    LastName,
    Role,
    IsActive,
    IsDeleted,
    CreatedAt,
    LastModified
)
VALUES (
    'admin',
    'admin@bank.com',
    @PasswordHash,
    'System',
    'Administrator',
    'SuperAdmin',
    1,
    0,
    GETUTCDATE(),
    GETUTCDATE()
);

-- ============================================
-- Create Sample BankAdmin User
-- Password: BankAdmin@123
-- ============================================

SET @PasswordHash = '$2a$12$rPY9.H7W1K3V5f5dK5hKBejK8mN5qL4jK8iI9hH8gG7fF6eE5dD4cC';
-- Note: This is a placeholder hash

INSERT INTO Users (
    Username,
    Email,
    PasswordHash,
    FirstName,
    LastName,
    Role,
    IsActive,
    IsDeleted,
    CreatedAt,
    LastModified
)
VALUES (
    'bankadmin',
    'bankadmin@bank.com',
    @PasswordHash,
    'John',
    'Smith',
    'BankAdmin',
    1,
    0,
    GETUTCDATE(),
    GETUTCDATE()
);

-- ============================================
-- Create Sample Auditor User
-- ============================================

INSERT INTO Users (
    Username,
    Email,
    PasswordHash,
    FirstName,
    LastName,
    Role,
    IsActive,
    IsDeleted,
    CreatedAt,
    LastModified
)
VALUES (
    'auditor',
    'auditor@bank.com',
    @PasswordHash,
    'Jane',
    'Doe',
    'Auditor',
    1,
    0,
    GETUTCDATE(),
    GETUTCDATE()
);

-- ============================================
-- Create Sample Customers
-- ============================================

INSERT INTO Customers (
    FirstName,
    LastName,
    Email,
    Phone,
    Address,
    City,
    Country,
    DateOfBirth,
    IdentificationNumber,
    IsActive,
    IsDeleted,
    CreatedAt,
    LastModified
)
VALUES 
('Michael', 'Johnson', 'michael.johnson@email.com', '+1-555-0101', '123 Main Street', 'New York', 'United States', '1985-03-15', 'ID-001-MJ', 1, 0, GETUTCDATE(), GETUTCDATE()),
('Sarah', 'Williams', 'sarah.williams@email.com', '+1-555-0102', '456 Oak Avenue', 'Los Angeles', 'United States', '1990-07-22', 'ID-002-SW', 1, 0, GETUTCDATE(), GETUTCDATE()),
('David', 'Brown', 'david.brown@email.com', '+1-555-0103', '789 Pine Road', 'Chicago', 'United States', '1978-11-30', 'ID-003-DB', 1, 0, GETUTCDATE(), GETUTCDATE()),
('Emily', 'Davis', 'emily.davis@email.com', '+1-555-0104', '321 Elm Street', 'Houston', 'United States', '1995-01-10', 'ID-004-ED', 1, 0, GETUTCDATE(), GETUTCDATE()),
('Robert', 'Miller', 'robert.miller@email.com', '+1-555-0105', '654 Maple Drive', 'Phoenix', 'United States', '1982-09-05', 'ID-005-RM', 1, 0, GETUTCDATE(), GETUTCDATE());

-- ============================================
-- Create Sample Accounts
-- ============================================

INSERT INTO Accounts (
    AccountNumber,
    CustomerId,
    AccountType,
    Currency,
    Balance,
    IsActive,
    IsDeleted,
    CreatedAt,
    LastModified
)
VALUES 
('1000000001', 1, 'Savings', 'USD', 15000.00, 1, 0, GETUTCDATE(), GETUTCDATE()),
('1000000002', 1, 'Checking', 'USD', 5000.00, 1, 0, GETUTCDATE(), GETUTCDATE()),
('1000000003', 2, 'Savings', 'USD', 25000.00, 1, 0, GETUTCDATE(), GETUTCDATE()),
('1000000004', 2, 'Checking', 'USD', 3500.00, 1, 0, GETUTCDATE(), GETUTCDATE()),
('1000000005', 3, 'Business', 'USD', 100000.00, 1, 0, GETUTCDATE(), GETUTCDATE()),
('1000000006', 4, 'Savings', 'USD', 8000.00, 1, 0, GETUTCDATE(), GETUTCDATE()),
('1000000007', 5, 'Checking', 'USD', 12000.00, 1, 0, GETUTCDATE(), GETUTCDATE());

-- ============================================
-- Create Sample Transactions
-- ============================================

DECLARE @AdminUserId INT = 1; -- SuperAdmin

INSERT INTO Transactions (
    Type,
    FromAccountId,
    ToAccountId,
    Amount,
    BalanceAfter,
    ReferenceNumber,
    Description,
    Status,
    CreatedBy,
    CreatedAt
)
VALUES 
-- Deposits
('Deposit', NULL, 1, 5000.00, 15000.00, 'DEP-20240115-0001', 'Initial deposit', 'Completed', @AdminUserId, DATEADD(DAY, -10, GETUTCDATE())),
('Deposit', NULL, 2, 2000.00, 5000.00, 'DEP-20240115-0002', 'Initial deposit', 'Completed', @AdminUserId, DATEADD(DAY, -10, GETUTCDATE())),
('Deposit', NULL, 3, 10000.00, 25000.00, 'DEP-20240115-0003', 'Initial deposit', 'Completed', @AdminUserId, DATEADD(DAY, -10, GETUTCDATE())),
('Deposit', NULL, 4, 3500.00, 3500.00, 'DEP-20240115-0004', 'Initial deposit', 'Completed', @AdminUserId, DATEADD(DAY, -10, GETUTCDATE())),
('Deposit', NULL, 5, 50000.00, 100000.00, 'DEP-20240115-0005', 'Initial deposit', 'Completed', @AdminUserId, DATEADD(DAY, -10, GETUTCDATE())),
('Deposit', NULL, 6, 8000.00, 8000.00, 'DEP-20240115-0006', 'Initial deposit', 'Completed', @AdminUserId, DATEADD(DAY, -10, GETUTCDATE())),
('Deposit', NULL, 7, 12000.00, 12000.00, 'DEP-20240115-0007', 'Initial deposit', 'Completed', @AdminUserId, DATEADD(DAY, -10, GETUTCDATE())),

-- Withdrawals
('Withdrawal', 1, NULL, 500.00, 14500.00, 'WDL-20240116-0001', 'ATM withdrawal', 'Completed', @AdminUserId, DATEADD(DAY, -5, GETUTCDATE())),
('Withdrawal', 3, NULL, 1000.00, 24000.00, 'WDL-20240116-0002', 'ATM withdrawal', 'Completed', @AdminUserId, DATEADD(DAY, -3, GETUTCDATE())),
('Withdrawal', 7, NULL, 2000.00, 10000.00, 'WDL-20240116-0003', 'ATM withdrawal', 'Completed', @AdminUserId, DATEADD(DAY, -1, GETUTCDATE())),

-- Transfers
('Transfer', 2, 1, 1000.00, 4000.00, 'TRF-20240117-0001', 'Transfer to savings', 'Completed', @AdminUserId, DATEADD(DAY, -2, GETUTCDATE())),
('Transfer', 5, 3, 5000.00, 95000.00, 'TRF-20240117-0002', 'Business transfer', 'Completed', @AdminUserId, DATEADD(DAY, -1, GETUTCDATE()));

-- ============================================
-- Create Audit Log Entries
-- ============================================

INSERT INTO AuditLogs (
    UserId,
    Action,
    EntityType,
    EntityId,
    OldValues,
    NewValues,
    IPAddress,
    Timestamp
)
VALUES 
(@AdminUserId, 'CREATE', 'User', 1, NULL, '{"Username":"admin","Role":"SuperAdmin"}', '127.0.0.1', GETUTCDATE()),
(@AdminUserId, 'CREATE', 'Customer', 1, NULL, '{"FirstName":"Michael","LastName":"Johnson"}', '127.0.0.1', GETUTCDATE()),
(@AdminUserId, 'CREATE', 'Account', 1, NULL, '{"AccountNumber":"1000000001","Balance":15000}', '127.0.0.1', GETUTCDATE());

PRINT 'Seed data inserted successfully!';
PRINT '';
PRINT '========================================';
PRINT 'DEFAULT ADMIN CREDENTIALS';
PRINT '========================================';
PRINT 'Username: admin';
PRINT 'Password: Admin@123';
PRINT 'Role: SuperAdmin';
PRINT '========================================';
PRINT '';
PRINT 'Please change this password after first login!';
