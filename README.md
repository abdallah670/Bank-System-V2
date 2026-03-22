# BankSystem V2

A modern banking management system built with Angular 17 and ASP.NET Core 8.

## Project Structure

```
BankSystem.V2/
├── src/
│   ├── api/                          # ASP.NET Core Web API
│   │   ├── BankSystem.Api/          # Controllers, Middleware
│   │   ├── BankSystem.Core/          # Entities, Interfaces, DTOs
│   │   ├── BankSystem.Infrastructure/ # Repositories, EF Core
│   │   └── BankSystem.Services/     # Business Logic
│   ├── web/                          # Angular Frontend
│   │   └── bank-admin/
│   └── database/                     # SQL Scripts
└── docs/
```

## Prerequisites

- **.NET 8 SDK**
- **SQL Server 2019+**
- **Node.js 18+**
- **Angular CLI 17+**

## Setup Instructions

### 1. Database Setup

1. Open SQL Server Management Studio
2. Run the SQL scripts in order:
   ```
   src/database/001_Schema.sql
   src/database/002_Constraints.sql
   src/database/003_Indexes.sql
   src/database/004_Seed_Data.sql
   ```

### 2. Backend Setup

```bash
cd src/api

# Restore packages
dotnet restore

# Update connection string in appsettings.json if needed

# Run the API
dotnet run --project BankSystem.Api
```

API will be available at: `https://localhost:7000`

### 3. Frontend Setup

```bash
cd src/web/bank-admin

# Install dependencies
npm install

# Run the application
npm start
```

Frontend will be available at: `http://localhost:4200`

## Default Admin Credentials

| Username | Password | Role |
|----------|----------|------|
| admin | Admin@123 | SuperAdmin |

**Important:** Change the default password after first login!

## Features

### Core Features
- Customer Management (CRUD, Search)
- Account Management
- Transactions (Deposit, Withdrawal, Transfer)
- User Management (SuperAdmin only)
- Dashboard with Analytics

### Security
- JWT Authentication
- Role-Based Authorization
- BCrypt Password Hashing
- Audit Logging
- Input Validation

### Roles
| Role | Permissions |
|------|-------------|
| SuperAdmin | Full access, can manage other admins |
| BankAdmin | Customer, Account, Transaction management |
| Auditor | Read-only access to all data |

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login
- `POST /api/auth/refresh` - Refresh token
- `POST /api/auth/logout` - Logout

### Customers
- `GET /api/customers` - List customers (paginated)
- `GET /api/customers/{id}` - Get customer
- `POST /api/customers` - Create customer
- `PUT /api/customers/{id}` - Update customer
- `DELETE /api/customers/{id}` - Delete customer

### Accounts
- `GET /api/accounts` - List accounts
- `GET /api/accounts/{id}` - Get account
- `POST /api/accounts` - Create account

### Transactions
- `GET /api/transactions` - List transactions
- `POST /api/transactions/deposit` - Deposit funds
- `POST /api/transactions/withdraw` - Withdraw funds
- `POST /api/transactions/transfer` - Transfer funds

### Dashboard
- `GET /api/dashboard/summary` - Dashboard KPIs
- `GET /api/dashboard/chart-data` - Chart data

### Users (SuperAdmin)
- `GET /api/users` - List users
- `POST /api/users` - Create user
- `PUT /api/users/{id}/role` - Update user role
- `DELETE /api/users/{id}` - Delete user

## Technology Stack

### Backend
- ASP.NET Core 8
- Entity Framework Core 8
- SQL Server
- JWT Authentication
- BCrypt

### Frontend
- Angular 17
- Angular Material
- Angular Google Charts
- RxJS

## Development

### Running Tests
```bash
# Backend
cd src/api
dotnet test

# Frontend
cd src/web/bank-admin
npm test
```

### Building for Production
```bash
# Backend
dotnet publish

# Frontend
npm run build
```

## License

Private - All rights reserved
