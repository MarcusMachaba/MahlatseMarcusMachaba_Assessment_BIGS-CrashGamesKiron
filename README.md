# 🧑‍💻 Senior Backend Developer Assessment – Kiron Interactive

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

## 📄 SQL Scripts & Database Initialization/Seeding

All SQL scripts used for object creation—such as **tables**, **stored procedures**, and other **database objects**—are included in the `Documentation/` folder for reference and transparency.

This includes:

- Table definitions
- Stored procedure logic
- Any supporting DB objects

**However, there is no need to run these scripts manually.**  
The application uses a **code-first convention-based bootstrapping mechanism** (similar to Entity Framework’s `DropCreateDatabaseIfModelChanges`) to automatically set up the database schema on application startup.

### ✅ What You Need to Do

1. Restore the provided `KironTest.bak` database — this contains only the pre-populated `Navigation` table.
2. Set your connection string in `appsettings.json` to point to the restored database.
3. **Run the application** — all required tables, stored procedures, and initial setup will be created automatically via the custom database layer.

### 🗂️ Path
**Path:**  
/Documentation/ScriptsAndDatabaseBackup/
├── SQL-DatabaseBackup/
├── SQL-DBLayer-Setup-scripts/
├── SQL-Procs/ 
├── SQL-table-creation-scripts/
└── SQL-table-data-scripts/

---

## 🚀 Quick-start (developer workflow)
### Prerequisites
- .NET 6 SDK
- SQL Server 2017+ (version 14.0+)
- Postman (optional, for API testing) We have swagger integrated on the API
### Steps
```bash
# Clone + enter
GITHUB
git clone https://github.com/MarcusMachaba/MahlatseMarcusMachaba_Assessment_BIGS-CrashGamesKiron.git
cd MahlatseMarcusMachaba_Assessment_BIGS-CrashGamesKiron
or
AZURE DEVOPS
git clone https://MarcusMachabasDemos@dev.azure.com/MarcusMachabasDemos/MahlatseMarcusMachaba_Assessment_BIGS-CrashGamesKiron/_git/MahlatseMarcusMachaba_Assessment_BIGS-CrashGamesKiron
cd MahlatseMarcusMachaba_Assessment_BIGS-CrashGamesKiron

# --- Tests DatabaseLayer---
cd Applications\Src\Tests\DatabaseLayer.Custom.Tests
dotnet test            # builds & runs all DatabaseLayer.Custom requirement test-case unit tests

# --- Tests DatabaseLayer CRUD ---
cd ..\DebugTester
dotnet run            # builds & runs the DebugTester Console App & runs CRUD using the database layer

# --- KironTest.API  ---
cd ..\..\KironTest.API
dotnet run            # builds & runs the DebugTester Console App & runs CRUD using the database layer
```

```bash
# 💡 Alternative is through visual studio 2022 on solution explorer - Set KiroTest.API as startup project & start/run sln.
```

## ⚖️ Database Layer
- Connection pooling with **max 10 active connections**
- Stored procedures only – **no inline SQL**
- Transaction support (Begin/Commit/Rollback)
- Object deserialization from stored procedure result
 
## 🖥 Caching Layer
- Reusable, thread-safe in-memory cache
- Sliding expiration support
- Separate durations per domain (30–60 mins)

## 🧪 API Endpoints

| Endpoint | Description |
|----------|-------------|
| `POST /api/auth/register` | Register a new user (hashed password) |
| `POST /api/auth/login` | Authenticate and return JWT |
| `GET /api/navigation` | Recursive navigation tree from DB |
| `GET /api/bankholidays/start` | Starts background updater for UK Bank Holidays |
| `GET /api/bankholidays/regions` | Returns all UK regions |
| `GET /api/bankholidays/{region}` | Bank holidays for the region |
| `GET /api/dragonball/characters` | Proxies the Dragon Ball API with sliding cache |

All endpoints (except register/login) require a valid JWT token in request headers.

---

## 🖼 Authentication

- Passwords are stored as **one-way salted hashes**
- JWT token is issued on login (1-hour expiry)
- All secure endpoints require `Authorization: Bearer <token>` header

---

## 📬 Caching Behavior

| Domain | Expiry | Strategy |
|--------|--------|----------|
| Dragon Ball Characters | 1 hour | Sliding expiration: extends by 1hr on access |
| Bank Holidays | 30 mins | Regular cache |
| Navigation Structure | 30 mins | Standard cache |

---


## 🧑‍💻 More Info

Author: **Marcus Machaba**  
GitHub: [@MarcusMachaba](https://github.com/MarcusMachaba)
Email: smaka1236@gmail.com  

---
