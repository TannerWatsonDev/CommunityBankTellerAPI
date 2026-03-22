# Community Bank Teller API

> **Status: Actively in development** — core infrastructure complete, features in progress.

A RESTful backend API simulating core banking operations used by a community bank teller system. Built to demonstrate real-world backend development practices including layered architecture, business rule enforcement, and a full test suite.

---

## Tech Stack

- **.NET 9 / C#** — Web API
- **Entity Framework Core 9** — ORM and migrations
- **PostgreSQL** — Database (via Docker)
- **Docker / Docker Compose** — Local development environment
- **xUnit + Moq** — Unit and integration testing *(in progress)*
- **Swagger / Swashbuckle** — API documentation

---

## Architecture

The project follows a layered architecture pattern:

```
Controllers → Services → Repositories → AppDbContext → PostgreSQL
```

- **Controllers** — HTTP layer only. Receives requests, calls services, returns responses.
- **Services** — Business logic. Enforces all domain rules (overdraft protection, transfer atomicity, account closure).
- **Repositories** — EF Core data access. Wraps DbContext calls.
- **Models** — EF Core entity classes mapped to database tables.
- **DTOs** — Request and response objects. API contract is decoupled from the database schema.
- **Data** — AppDbContext and EF Core migrations.
- **Middleware** — Global exception handling for consistent error responses.

---

## Domain Models

| Model | Description |
|---|---|
| Customer | Bank customer with personal info and a collection of accounts |
| Account | Checking or savings account tied to a customer |
| Transaction | Deposit, withdrawal, or transfer record |
| LedgerEntry | Immutable audit trail entry written on every transaction |

---

## Business Rules

- **Overdraft protection** — Withdrawals that would bring a balance below $0.00 are rejected with 422
- **Transfer atomicity** — Both account balances update in a single DB transaction or neither does
- **Account closure** — Closed accounts cannot receive deposits, withdrawals, or transfers
- **Immutability** — Transactions and ledger entries are never updated or deleted
- **Audit trail** — Every transaction writes a LedgerEntry with balance before and after

---

## API Endpoints *(in progress)*

### Customers
| Method | Route | Description |
|---|---|---|
| POST | /api/customers | Create a new customer |
| GET | /api/customers/{id} | Get customer by ID |
| PUT | /api/customers/{id} | Update customer profile |
| GET | /api/customers/{id}/accounts | List all accounts for a customer |

### Accounts
| Method | Route | Description |
|---|---|---|
| POST | /api/accounts | Open a new account |
| GET | /api/accounts/{id} | Get account details and balance |
| DELETE | /api/accounts/{id} | Soft-close an account |

### Transactions
| Method | Route | Description |
|---|---|---|
| POST | /api/transactions/deposit | Deposit funds |
| POST | /api/transactions/withdraw | Withdraw funds |
| POST | /api/transactions/transfer | Transfer between accounts |
| GET | /api/transactions/{accountId} | Get transaction history |

### Ledger
| Method | Route | Description |
|---|---|---|
| GET | /api/ledger/{accountId} | Get full audit trail for an account |

---

## How to Run

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 9 SDK](https://dotnet.microsoft.com/download)

### Steps

1. Clone the repo
```bash
git clone https://github.com/TannerWatsonDev/CommunityBankTellerAPI.git
cd CommunityBankTellerAPI
```

2. Start the database
```bash
docker-compose up -d
```

3. Apply migrations
```bash
cd CommunityBankTellerAPI
dotnet ef database update
```

4. Run the API
```bash
dotnet run
```

5. Open Swagger
```
https://localhost:{port}/swagger
```

---

## Current Progress

- [x] Solution and project setup
- [x] EF Core domain models (Customer, Account, Transaction, LedgerEntry)
- [x] Enums (AccountType, AccountStatus, TransactionType)
- [x] AppDbContext with decimal precision and relationship configuration
- [x] Docker Compose with PostgreSQL
- [x] Initial database migration
- [ ] DTOs (request/response objects)
- [x] Customer endpoints
- [x] Account endpoints
- [x] Transaction endpoints with business rule enforcement
- [ ] Ledger endpoints
- [ ] Unit tests
- [ ] Integration tests
- [ ] Global error handling middleware
- [ ] README finalized

---

## About

Built as a portfolio project targeting backend Software Engineer roles in the fintech space.

---

*Tanner Watson — [GitHub](https://github.com/TannerWatsonDev) — tanner.watsondev@gmail.com*