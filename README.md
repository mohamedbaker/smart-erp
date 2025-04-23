# SmartERP - Authentication Module

A secure, modular authentication system built with **.NET 8**, **Entity Framework Core**, **JWT**, and **MySQL**, designed for SmartERP. It supports registration, login, logout, session/token-based authentication, and **role-based access control (RBAC)**.

---

## 🔐 Features

- User registration and login with hashed passwords
- JWT-based token generation and validation
- Role-based authorization (Admin, User, etc.)
- Secure configuration with `.env` or Docker secrets
- Database migrations using EF Core
- Swagger API documentation
- Docker and Docker Compose support

---

## 🛠️ Tech Stack

- ASP.NET Core 8 Web API
- Entity Framework Core
- MySQL
- JWT (JSON Web Token)
- Docker + Docker Compose
- Swagger/OpenAPI
- DotNetEnv (.env loader)

---

## 🚀 Getting Started

### 🔧 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [MySQL](https://www.mysql.com/) or Docker
- [Docker + Docker Compose](https://docs.docker.com/compose/install/)

---

### 🔄 Clone the Project

```bash
git clone https://github.com/yourusername/SmartERP-Auth.git
cd SmartERP-Auth
```

---

## 🔧 Environment Configuration

### `.env` Example

```env
# MySQL
MYSQL_USER= userName
MYSQL_PASSWORD=your_mysql_password
MYSQL_DATABASE=SmartERP
MYSQL_ROOT_PASSWORD=rootpass

# JWT Settings
JWT_KEY=YourSuperSecureKeyThatIsAtLeast32CharsLong
JWT_ISSUER=Smart-ERP-API
JWT_AUDIENCE=ERPClient
JWT_EXPIRE_MINUTES=60
```

---

## 💻 Run Locally

### 1. Load environment variables

```bash
dotnet add package DotNetEnv
```

And in `Program.cs`:

```csharp
DotNetEnv.Env.Load();
```

### 2. Run the API

```bash
dotnet build
dotnet ef database update
dotnet run
```

Visit: `https://localhost:5001/swagger`

---

## 🐳 Run with Docker

### Dockerfile

```Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Smart_ERP.dll"]
```

---

### docker-compose.yml

```yaml
version: '3.8'

services:
  mysql:
    image: mysql:8
    restart: always
    environment:
      MYSQL_DATABASE: ${MYSQL_DATABASE}
      MYSQL_USER: ${MYSQL_USER}
      MYSQL_PASSWORD: ${MYSQL_PASSWORD}
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD}
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql

  api:
    build: .
    depends_on:
      - mysql
    environment:
      MYSQL_PASSWORD: ${MYSQL_PASSWORD}
      JWT_KEY: ${JWT_KEY}
      JWT_ISSUER: ${JWT_ISSUER}
      JWT_AUDIENCE: ${JWT_AUDIENCE}
      JWT_EXPIRE_MINUTES: ${JWT_EXPIRE_MINUTES}
    ports:
      - "5000:80"
    volumes:
      - .:/app

volumes:
  mysql_data:
```

### Run

```bash
docker compose up --build
```

---

## 📚 API Endpoints

### 🔑 Auth

| Method | Endpoint           | Description              |
|--------|--------------------|--------------------------|
| POST   | `/api/auth/register` | Register a new user     |
| POST   | `/api/auth/login`    | Login and get JWT token |

### User Management
| Method | Endpoint            | Description                     | Auth Required |
|--------|---------------------|---------------------------------|---------------|
| GET    | `/users`            | Get all users                   | Yes           |
| GET    | `/user/{id}`        | Get a user by ID                | Yes           |
| DELETE | `/user/{id}`        | Delete a user by ID             | Yes           |
| GET    | `/user/{id}/roles`  | Get the role assigned to a user | Yes           |
| POST   | `/user/{id}/roles`  | Assign a role to a user         | Yes           |

### Role Management
| Method | Endpoint       | Description              | Auth Required |
|--------|----------------|--------------------------|---------------|
| GET    | `/roles`       | Get all roles            | Yes           |
| POST   | `/role`        | Create a new role        | Yes           |
| PUT    | `/role/{id}`   | Update a role by ID      | Yes           |
| DELETE | `/role/{id}`   | Delete a role by ID      | Yes           |
| GET    | `/role/{id}`   | Get role details by ID   | Yes           |
---

## 🛡️ Role-Based Access

- Roles are seeded on first run (`Admin`, `User`)
- Claims include roles in the JWT
- `[Authorize(Roles = "Admin")]` can be used in controllers

---

## 📁 Project Structure

```
SmartERP/
│
├── Modules/
│   └── Auth/
│       ├── Controllers/
│       ├── Models/
│       └── Services/
│
├── Data/
│   └── ERPDbContext.cs
│
├── Program.cs
├── .env
├── Dockerfile
├── docker-compose.yml
├── appsettings.json
```

---

## ✍️ License

Open-Source
---

## 👨‍💻 Author

- [Mohamed Abo Bakr](https://github.com/mohamedbaker)
---

## ✅ Coming Next

- ✅ Refresh tokens
- ✅ Products Module
- ✅ Email verification
- ✅ Account lockout
