# Senior Backend Developer Assessment – Kiron Interactive

This project is a comprehensive backend solution built for Kiron Interactive as part of a senior backend developer assessment. It demonstrates scalable architecture, thread-safe service access, secure user authentication, recursive data structuring, and efficient API proxying with intelligent caching strategies.

---

## 🧩 Project Overview

This solution includes:
<ul class="list-disc pl-4 my-0">
<li class="my-0">🛠️ <strong>Database Management:</strong> Automated schema validation, index management, and stored procedures for reliable data integrity.</li>
<li class="my-0">📊 <strong>Logging:</strong> Centralized, multi-channel logging with log4net for effective monitoring and troubleshooting.</li>
<li class="my-0">⚡ <strong>Caching:</strong> In-memory caching layer to boost data retrieval speed and reduce latency.</li>
<li class="my-0">🌐 <strong>API Integration:</strong> Modular RESTful API endpoints, navigation, user management, and external data sources integrations like Dragon Ball characters etc.</li>
<li class="my-0">🔧 <strong>Developer-Friendly:</strong> Modular architecture with clear separation of concerns, supporting scalable and maintainable codebases.</li>
</ul>

---

## 🛠️ Technologies Used

- ASP.NET Core Web API (.NET 8 )
- SQL Server (v14.0+)
- MemoryCache for local caching
- JWT Authentication
- Background Services (`IHostedService`)
- SemaphoreSlim (for thread-safe operations)
- Swagger
- Ado.Net framework

---

## 📁 Solution Structure

- `Src/Common/DatabaseLayer.Custom` – 🛠️ SQL for schema creation, stored procedures, and initial data. (CodeFirst no manual running of scripts - it bootstraps itself - you just run the API).
- `Src/Common/Caching` – 📊 Caching layer (InMemoryCache).
- `Src/Common/Core` – ⚡ DTOs, Utility method helpers and database models.
- `Src/Common/Logger` – 🌐 Centralized, multi-channel logging with log4net for effective monitoring and troubleshooting.
- `Src/KironTestAPI` – 🎯 User registration, login, and JWT handling (Modular RESTful API endpoints, navigation, user management, & external data sources integrations.)
- `Src/KironTestAPI/Hosting/TimeHostedService` – Automated updater/ background scheduler for bank holidays
- `README.md` – Project documentation4

  
## 📁  layout
```text
├── Documentation/                            # Documentation
│   ├── KironTest.bak                         # DatabaseBackup
│   └── README.md                             # Readme doc
│
├── Common/                                   # Generic reusable projects
│   ├── CachingLayer/                         
│   └── Core/        
│   └── DatabaseLayer.Custom/
│   └── Logger/
│
├── Tests/
│   └── DatabaseLayer.Custom.Tests/           # xUnit tests
│   └── DebugTester/                          # demo ConsoleApp for quick testing of DatabaseLayer.Custom proj
│
├── KironTest.API/                            # NET Core API .NET 8
│   └── Controllers/                          # Contains endpoints
│       │   ├── DragonBallCharacters/
│       │   ├── Navigation/
│       │   └── UKBankHolidays
│       │   └── UserManagement
│
│
└── Directory.Build.props                     # Analyzer rules for all C# projects & global C# language version
``` 

---

## 🚀 Quick-start (developer workflow)

```bash
# Clone + enter
GITHUB
git clone https://github.com/MarcusMachaba/MahlatseMarcusMachaba_Assessment_BIGS-CrashGamesKiron.git
cd MahlatseMarcusMachaba_Assessment_BIGS-CrashGamesKiron
or
AZURE DEVOPS
git clone https://MarcusMachabasDemos@dev.azure.com/MarcusMachabasDemos/MahlatseMarcusMachaba_Assessment_BIGS-CrashGamesKiron/_git/MahlatseMarcusMachaba_Assessment_BIGS-CrashGamesKiron
cd MahlatseMarcusMachaba_Assessment_BIGS-CrashGamesKiron


```

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
