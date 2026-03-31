# Community Bank Teller API

> **Status: Complete** — all features implemented, 67 tests passing.

A RESTful backend API simulating core banking operations for a community bank teller system. Built to demonstrate professional backend development practices including layered architecture, repository pattern, Unit of Work, atomic database transactions, and a comprehensive test suite.

---

## Tech Stack

- **.NET 9 / C#** — Web API
- **Entity Framework Core 9** — ORM and migrations
- **PostgreSQL** — Database (via Docker)
- **Docker / Docker Compose** — Local development environment
- **xUnit + Moq** — Unit and integration testing
- **Respawn** — Database reset between integration tests
- **Swagger / Swashbuckle** — API documentation

---

## Architecture

The project follows a layered architecture pattern:
```
Controllers → Services → Repositories (Unit of Work) → AppDbContext → PostgreSQL
```

- **Controllers** — HTTP layer only. Receives requests, calls services, returns responses.
- **Services** — Business logic. Enforces all domain rules (overdraft protection, transfer atomicity, account closure).
- **Repositories** — EF Core data access. Wraps DbContext calls. Each entity has its own repository.
- **Unit of Work** — Coordinates repositories and wraps database transaction management. Services depend on `IUnitOfWork` rather than individual repositories directly.
- **Models** — EF Core entity classes mapped to database tables.
- **DTOs** — Request and response objects. API contract is fully decoupled from the database schema.
- **Data** — AppDbContext and EF Core migrations.
- **Middleware** — Global exception handling for consistent error responses.
- **Exceptions** — Custom exception types (e.g. `InsufficientFundsException`) for precise HTTP status mapping.

---

## Domain Models

| Model | Description |
|---|---|
| Customer | Bank customer with personal info and a collection of accounts |
| Account | Checking or savings account tied to a customer |
| Transaction | Deposit, withdrawal, or transfer record |
| LedgerEntry | Immutable audit trail entry written on every transaction |

### Enums

| Enum | Values |
|---|---|
| AccountType | Checking, Savings |
| AccountStatus | Active, Closed |
| TransactionType | Deposit, Withdrawal, Transfer |

---

## Business Rules

- **Overdraft protection** — Withdrawals that would bring a balance below $0.00 are rejected with 422 Unprocessable Entity
- **Transfer atomicity** — Both account balances update in a single DB transaction (`BeginTransactionAsync`) or neither does — rolled back on failure
- **Account closure** — Closed accounts cannot receive deposits, withdrawals, or transfers (409 Conflict)
- **Immutability** — Transactions and LedgerEntries are never updated or deleted
- **Audit trail** — Every transaction writes a LedgerEntry with balance before and after
- **Input validation** — Data Annotations on all request DTOs (email format, zip code format, state abbreviation length, positive amounts)

---

## API Endpoints

### Customers — `/api/customers`
| Method | Route | Description |
|---|---|---|
| POST | /api/customers | Create a new customer |
| GET | /api/customers/{id} | Get customer by ID |
| PUT | /api/customers/{id} | Update customer profile |
| GET | /api/customers/{id}/accounts | List all accounts for a customer |

### Accounts — `/api/accounts`
| Method | Route | Description |
|---|---|---|
| POST | /api/accounts | Open a new account |
| GET | /api/accounts/{id} | Get account details and balance |
| DELETE | /api/accounts/{id} | Soft-close an account |

### Transactions — `/api/transactions`
| Method | Route | Description |
|---|---|---|
| POST | /api/transactions/deposit | Deposit funds |
| POST | /api/transactions/withdraw | Withdraw funds (enforces overdraft protection) |
| POST | /api/transactions/transfer | Transfer between two accounts (atomic) |
| GET | /api/transactions/{accountId} | Get transaction history for an account |

### Ledger — `/api/ledger`
| Method | Route | Description |
|---|---|---|
| GET | /api/ledger/{accountId} | Get full immutable audit trail for an account |

---

## Testing

**67 tests total — all passing.**

### Unit Tests (27) — `CommunityBankTellerAPI.Tests/Unit/`

Tests the service layer in complete isolation using **xUnit** and **Moq**. Dependencies are mocked — no database involved. Covers both happy path and all failure/edge cases for `TransactionService`, `CustomerService`, and `AccountService`.

### Integration Tests (40) — `CommunityBankTellerAPI.Tests/Integration/`

Tests the full HTTP stack against a real **PostgreSQL test database** using **WebApplicationFactory**. Each test class shares a single factory instance via xUnit's `ICollectionFixture`. **Respawn** resets data between tests for isolation without rebuilding the schema.

Covers `CustomerController`, `AccountController`, and `TransactionController` including deposit, withdrawal, overdraft protection, and atomic transfers.

### How to Run Tests

**Prerequisites:** Docker running with the dev database container up.
```bash
# Unit tests only
dotnet test --filter "FullyQualifiedName~Unit"

# Integration tests only
dotnet test --filter "FullyQualifiedName~Integration"

# All tests
dotnet test
```

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

## Design Decisions

**Repository Pattern + Unit of Work** — Services depend on `IUnitOfWork` rather than `AppDbContext` directly. This keeps EF Core confined to the repository layer, makes the service layer fully testable with mocks, and follows the Dependency Inversion Principle.

**Custom InsufficientFundsException** — Rather than returning a generic 400, overdraft violations return 422 Unprocessable Entity. A custom exception type allows the controller to catch it specifically and map it to the correct status code.

**Immutable LedgerEntry** — The ledger table has no update or delete endpoints by design. In regulated banking environments every state change must be traceable. The ledger provides a complete, tamper-proof history of every balance change.

**Account Number Generation** — Account numbers are generated as `ACC{Id:D8}` (e.g. `ACC00000001`) after the initial save, using the auto-incremented primary key to guarantee uniqueness without additional logic.

**Integration Test Infrastructure** — Integration tests run against a real PostgreSQL test database using `WebApplicationFactory`. A static migration guard ensures migrations run exactly once per test session regardless of xUnit's factory instantiation behavior. Respawn handles per-test data cleanup without dropping the schema.

---

## Progress

### Completed
- [x] EF Core domain models (Customer, Account, Transaction, LedgerEntry)
- [x] Enums (AccountType, AccountStatus, TransactionType)
- [x] AppDbContext with decimal precision and relationship configuration
- [x] Repository pattern (CustomerRepository, AccountRepository, TransactionRepository, LedgerRepository)
- [x] Unit of Work pattern (IUnitOfWork / UnitOfWork)
- [x] Docker Compose with PostgreSQL
- [x] EF Core migrations
- [x] CustomerService, AccountService, TransactionService, LedgerService
- [x] All controllers with proper HTTP status codes
- [x] Global exception handling middleware
- [x] Data Annotations for input validation
- [x] Swagger with XML documentation and enum string serialization
- [x] Unit tests — 27 tests (xUnit + Moq)
- [x] Integration tests — 40 tests (WebApplicationFactory + PostgreSQL + Respawn)
- [x] README finalized

### Potential Future Enhancements
- [ ] Authentication and authorization (JWT)
- [ ] Account interest calculation
- [ ] Scheduled / recurring transfers
- [ ] Frontend teller dashboard (React)
- [ ] Deployment (Railway / Fly.io)
- [ ] CI/CD pipeline

---

## About

Built as a portfolio project targeting backend Software Engineer roles in the fintech space.

---

*Tanner Watson — [GitHub](https://github.com/TannerWatsonDev) — tanner.watsondev@gmail.com*