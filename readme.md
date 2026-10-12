# FinTech Wallet - Event-Driven Microservices (.NET 10)

This repository contains an enterprise-grade microservices ecosystem designed according to modern banking and FinTech requirements. It features **security (JWT & User Secrets)**, **distributed caching (Redis)**, an **event-driven asynchronous architecture (RabbitMQ & MassTransit)**, and a **distributed locking mechanism (RedLock)** to prevent double-spending.

All microservices are exposed to external clients through a single entry point via **YARP Api Gateway**.

---

## Architecture Overview

```text
                           [ Client / Flutter / Postman ]
                                         │
                                         ▼
                            [ Gateway.API (YARP) ] :5000
                                         │
         ┌───────────────────────────────┼───────────────────────────────┐
         │                               │                               │
         ▼                               ▼                               ▼
  [ Auth.API ] :5198              [ Account.API ] :5290           [ Transfer.API ] :5225
  • JWT Generation                • CQRS / MediatR                • RedLock (Double-Spending)
  • User Secrets                  • Redis Cache (Read-Aside)      • MassTransit Publisher
                                                                         │
                                                                         ▼
                                                               [ RabbitMQ Queue ]
                                                                         │
                                                                         ▼
                                                            [ MoneyTransferConsumer ]
```

---

## Tech Stack & Infrastructure

* **Framework:** .NET 10 Web API
* **Architecture:** CQRS (MediatR), Microservices, Clean Architecture
* **Gateway:** YARP (Yet Another Reverse Proxy)
* **Messaging & Events:** RabbitMQ & MassTransit
* **Caching & Locking:** Redis, StackExchange.Redis, RedLock.net
* **Database & ORM:** SQLite, Entity Framework Core
* **Security:** JWT (JSON Web Token), .NET User Secrets
* **Containerization:** Docker & Docker Compose

---

## Local Setup & Running Instructions

Follow these step-by-step instructions after cloning the repository to get the entire ecosystem up and running locally.

### Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Must be installed and running)

---

### 1️⃣ Start Docker Containers (RabbitMQ & Redis)

From the project root directory, launch the infrastructure containers:

```bash
docker compose -f docker/docker-compose.yml up -d
```

> **Health Checks:**
> * **RabbitMQ Management UI:** `http://localhost:15672` *(Username: guest / Password: guest)*
> * **Redis:** `localhost:6379`

---

### 2️⃣ Configure Security Keys (.NET User Secrets)

Set up the symmetric JWT signing secret across all dependent microservices:

```bash
# Auth.API
cd src/Services/Auth.API
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:Secret" "3bf30fc1-c49c-4aa8-b0c3-62993246b463"

# Account.API
cd ../Account.API
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:Secret" "3bf30fc1-c49c-4aa8-b0c3-62993246b463"

# Transfer.API
cd ../Transfer.API
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:Secret" "3bf30fc1-c49c-4aa8-b0c3-62993246b463"

cd ../../../
```

---

### 3️⃣ Apply Database Migrations

Initialize database schemas for each service:

```bash
# Auth.API DB
cd src/Services/Auth.API
dotnet ef database update

# Account.API DB
cd ../Account.API
dotnet ef database update

# Transfer.API DB
cd ../Transfer.API
dotnet ef database update

cd ../../../
```

---

### 4️⃣ Launch Microservices

Run each service in a **separate terminal tab**:

* **Tab 1 (Auth.API):**
  ```bash
  cd src/Services/Auth.API
  dotnet run --urls "http://localhost:5198"
  ```

* **Tab 2 (Account.API):**
  ```bash
  cd src/Services/Account.API
  dotnet run --urls "http://localhost:5290"
  ```

* **Tab 3 (Transfer.API):**
  ```bash
  cd src/Services/Transfer.API
  dotnet run --urls "http://localhost:5225"
  ```

* **Tab 4 (Gateway.API):**
  ```bash
  cd src/Services/Gateway.API
  dotnet run --urls "http://localhost:5000"
  ```

---

## 🧪 End-to-End (E2E) API Testing Guide

> 💡 **Important:** External requests (from Postman, cURL, or Flutter) must **ONLY** target the Gateway port (`http://localhost:5000`).

### 1. User Registration (Auth.API)
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "johndoe@banka.com",
    "password": "Password123!",
    "fullName": "John Doe"
  }'
```

### 2. User Authentication & JWT Acquisition
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "johndoe@banka.com",
    "password": "Password123!"
  }'
```

> Copy the generated `token` string from the response. It will be passed in subsequent requests as an `Authorization: Bearer <TOKEN>` header.

### 3. Open a New Bank Account (Account.API)
```bash
curl -X POST http://localhost:5000/api/accounts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '"TRY"'
```

### 4. Fetch User Accounts (Redis Cache Verification)
```bash
curl http://localhost:5000/api/accounts \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```
*(The first call reads from SQLite and caches to Redis; subsequent calls are served directly from Redis in milliseconds).*

### 5. Asynchronous Money Transfer (Transfer.API)
```bash
curl -X POST http://localhost:5000/api/transfers \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "fromAccount": "TR789596289",
    "toAccount": "TR636812718",
    "amount": 250.0
  }'
```
*(RedLock acquires a lock on `fromAccount` to prevent double-spending, returns `202 Accepted`, and publishes `MoneyTransferSubmittedEvent` to RabbitMQ).*