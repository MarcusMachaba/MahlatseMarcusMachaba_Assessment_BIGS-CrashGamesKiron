# Senior Backend Developer Assessment – Kiron Interactive

This project is a comprehensive backend solution built for Kiron Interactive as part of a senior backend developer assessment. It demonstrates scalable architecture, thread-safe service access, secure user authentication, recursive data structuring, and efficient API proxying with intelligent caching strategies.

---

## ?? Project Overview

This solution includes:

- A reusable and thread-safe **database access layer** using stored procedures and Dapper
- A generic **caching layer** with support for sliding expiration
- A **.NET Web API** exposing secure endpoints backed by JWT authentication
- Thread-safe integrations with **external APIs** for:
  - UK Bank Holidays (gov.uk)
  - Dragon Ball Characters (https://web.dragonball-api.com)

---

## ??? Technologies Used

- ASP.NET Core Web API (.NET 6+)
- Dapper ORM
- SQL Server (v14.0+)
- MemoryCache for local caching
- JWT Authentication
- Background Services (`IHostedService`)
- SemaphoreSlim (for thread-safe operations)
- Swagger (optional)

---

## ?? Solution Structure

- `Database/` – SQL scripts for schema creation, stored procedures, and initial data
- `Services/` – Caching, API proxying, and DB abstraction logic
- `Models/` – DTOs and database models
- `Controllers/` – Web API endpoints
- `Auth/` – User registration, login, and JWT handling
- `BackgroundTasks/` – Automated updater for bank holidays
- `README.md` – Project documentation

---

## ?? Features

### ? Database Layer
- Connection pooling with **max 10 active connections**
- Stored procedures only – **no inline SQL**
- Transaction support (Begin/Commit/Rollback)
- Object deserialization from stored procedure result
 
### ? Caching Layer
- Reusable, thread-safe in-memory cache
- Sliding expiration support
- Separate durations per domain (30–60 mins)

### ? API Endpoints

| Endpoint | Description |
|----------|-------------|
| `POST /api/auth/register` | Register a new user (hashed password) |
| `POST /api/auth/login` | Authenticate and return JWT |
| `GET /api/navigation` | Recursive navigation tree from DB |
| `GET /api/bankholidays/start` | Starts background updater for UK Bank Holidays |
| `GET /api/bankholidays/regions` | Returns all UK regions |
| `GET /api/bankholidays/{region}` | Bank holidays for the region |
| `GET /api/dragonball/characters` | Proxies the Dragon Ball API with sliding cache |

All endpoints (except register/login) require a valid JWT token.

---

## ?? Authentication

- Passwords are stored as **one-way salted hashes**
- JWT token is issued on login (1-hour expiry)
- All secure endpoints require `Authorization: Bearer <token>` header

---

## ?? Caching Behavior

| Domain | Expiry | Strategy |
|--------|--------|----------|
| Dragon Ball Characters | 1 hour | Sliding expiration: extends by 1hr on access |
| Bank Holidays | 30 mins | Regular cache |
| Navigation Structure | 30 mins | Standard cache |

---

## ?? Setup Instructions

### Prerequisites
- .NET 6 SDK
- SQL Server 2017+ (version 14.0+)
- Postman (optional, for API testing)

### Steps
1. **Clone the Repo**
   ```bash
   git clone https://dev.azure.com/your-org/your-project/_git/backend-assessment
