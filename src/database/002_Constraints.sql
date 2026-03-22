-- ============================================
-- BankSystemV2 Database Constraints
-- Additional constraints after schema creation
-- ============================================

USE BankSystemV2;
GO

-- ============================================
-- Add CHECK Constraints for additional validation
-- ============================================

-- Users table constraints
ALTER TABLE Users
ADD CONSTRAINT CK_Users_Username_Length CHECK (LEN(Username) >= 3);

-- Customers table constraints  
ALTER TABLE Customers
ADD CONSTRAINT CK_Customers_Name_Length CHECK (
    LEN(FirstName) >= 2 AND LEN(LastName) >= 2
);

ALTER TABLE Customers
ADD CONSTRAINT CK_Customers_Phone_Length CHECK (LEN(Phone) >= 10);

-- Accounts table constraints
ALTER TABLE Accounts
ADD CONSTRAINT CK_Accounts_AccountNumber_Format CHECK (
    AccountNumber LIKE '[0-9]%' AND LEN(AccountNumber) >= 8
);

ALTER TABLE Accounts
ADD CONSTRAINT CK_Accounts_Currency CHECK (
    Currency IN ('USD', 'EUR', 'GBP', 'JPY', 'CAD', 'AUD')
);

-- Transactions table constraints
ALTER TABLE Transactions
ADD CONSTRAINT CK_Transactions_DifferentAccounts CHECK (
    FromAccountId <> ToAccountId OR (FromAccountId IS NULL AND ToAccountId IS NULL)
);

ALTER TABLE Transactions
ADD CONSTRAINT CK_Transactions_ReferenceNumber_Format CHECK (
    ReferenceNumber LIKE 'TRX-%' OR ReferenceNumber LIKE 'TRF-%' OR 
    ReferenceNumber LIKE 'DEP-%' OR ReferenceNumber LIKE 'WDL-%'
);

-- ============================================
-- Add Default Values
-- ============================================

ALTER TABLE Transactions
ADD CONSTRAINT DF_Transactions_ReferenceNumber DEFAULT (
    'TRX-' + CONVERT(NVARCHAR(20), GETUTCDATE(), 112) + 
    RIGHT('0000' + CAST(NEXT VALUE FOR dbo.ReferenceNumberSeq AS NVARCHAR(10)), 4)
) FOR ReferenceNumber;

PRINT 'Constraints added successfully!';
