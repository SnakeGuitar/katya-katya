# Memory Game Revival

Multiplayer memory game with WPF client and ASP.NET Core backend.

## Requirements

| Tool | Minimum Version |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 |
| [Docker Desktop](https://www.docker.com/products/docker-desktop) | Latest |
| Windows | 10 / 11 (client is WPF) |

---

## Quick Start

### 1. Clone the repository

```bash
git clone https://github.com/tu-usuario/MemoryGame-Revival.git
cd MemoryGame-Revival
```

### 2. Configure environment variables

Copy the example file and fill in the values:

```bash
cp .env.example .env
```

Edit `.env`:

```env
# PostgreSQL
POSTGRES_DB=memorygame
POSTGRES_USER=your_username
POSTGRES_PASSWORD=your_secure_password

# API
ASPNETCORE_ENVIRONMENT=Development
CONNECTION_STRING=Host=db;Port=5432;Database=memorygame;Username=your_username;Password=your_secure_password

# Email (SMTP) — required for account verification
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=youremail@gmail.com
SMTP_PASSWORD=your_google_app_password

# JWT
JWT_SECRET=secret-key-at-least-32-characters-long-here
JWT_ISSUER=memorygame-api
JWT_AUDIENCE=memorygame-client
JWT_EXPIRATION_MINUTES=60
JWT_REFRESH_EXPIRATION_DAYS=7
```

> **How to generate a Gmail App Password?**
> Google Account → Security → 2-Step Verification → App Passwords
> Or go directly to: https://myaccount.google.com/apppasswords

### 3. Start the database and API

```bash
docker-compose up -d
```

This starts two services:
- **db** — PostgreSQL 17 on port `5432`
- **api** — REST API + SignalR on port `5000`

### 4. Configure the WPF client

The client connects to the local server. Open `MemoryGame.Client/appsettings.json` (or similar) and verify the base URL points to `http://localhost:5059` if running the server locally, or `http://localhost:5000` if using Docker.

### 5. Run the client

Open a terminal in the client folder:

```bash
cd MemoryGame.Client
dotnet run
```

Or simply open it in Visual Studio and press F5.

---

## Local Development (without Docker for the server)

If you prefer to run the server directly with `dotnet watch`:

### 1. Start only the database

```bash
docker-compose up -d db
```

### 2. Configure `appsettings.Development.json`

Open `MemoryGame.Server/src/MemoryGame.API/appsettings.Development.json` and make sure it has:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=memorygame;Username=your_username;Password=your_secure_password"
  },
  "JWT_SECRET": "secret-key-at-least-32-characters-long-here",
  "JWT_EXPIRATION_MINUTES": "1440",
  "SMTP_HOST": "smtp.gmail.com",
  "SMTP_PORT": "587",
  "SMTP_USER": "youremail@gmail.com",
  "SMTP_PASSWORD": "your_google_app_password",
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "Cors": {
    "AllowedOrigins": [ "*" ]
  }
}
```

> SMTP keys must be at the **root level** of the JSON, not nested inside an object.

### 3. Run migrations

```bash
cd MemoryGame.Server/src/MemoryGame.API
dotnet ef database update
```

> If `dotnet ef` is not installed:
> ```bash
> dotnet tool install --global dotnet-ef
> ```
> Then close and reopen the terminal.

### 4. Run the server

```bash
dotnet watch
```

The server will be available at `http://localhost:5059`.

---

## Project Structure

```
MemoryGame-Revival/
├── MemoryGame.Server/
│   └── src/
│       ├── MemoryGame.API/          # Controllers, SignalR Hubs, Middleware
│       ├── MemoryGame.Application/  # Commands, Queries, DTOs (MediatR)
│       ├── MemoryGame.Domain/       # Entities and domain logic
│       └── MemoryGame.Infrastructure/ # EF Core, repositories, external services
├── MemoryGame.Client/               # WPF Client (Windows)
│   ├── Views/                       # XAML
│   ├── ViewModels/                  # MVVM + CommunityToolkit
│   ├── Services/                    # API client, navigation, music, etc.
│   └── Resources/                   # Images, fonts, music
├── docker-compose.yml
├── .env.example
└── docs/
    └── README.md
```

---

## Ports

| Service | Port | Description |
|---|---|---|
| API (Docker) | `5000` | REST + SignalR |
| API (local) | `5059` | `dotnet watch` |
| PostgreSQL | `5432` | Database |

---

## Registration Flow

1. User fills out the registration form
2. Server sends a 6-digit PIN to the email
3. User enters the PIN on the verification screen
4. Upon successful verification, the user is **automatically logged in**

---

## Troubleshooting Common Issues

**`SMTP_HOST not configured`**
SMTP keys are nested in an object in the JSON. They should be at the root level:
```json
{
  "SMTP_HOST": "smtp.gmail.com",  ← correct
  ...
}
```

**`password authentication failed for user "..."`**
The PostgreSQL username and password in `appsettings.Development.json` don't match the Docker container. Make sure they're the same as in `.env`.

**`duplicate key value violates unique constraint "ix_pending_registrations_email"`**
A pending registration with that email already exists. Use a different email or clean the table:
```bash
docker exec -it <database_container_name> psql -U your_username -d memorygame -c "DELETE FROM pending_registrations;"
```

**`dotnet-ef` not found**
```bash
dotnet tool install --global dotnet-ef
# Close and reopen the terminal
```

**Client cannot connect to server**
Verify that the base URL in the client points to the correct port:
- Docker: `http://localhost:5000`
- Local: `http://localhost:5059`

---

## Technology Stack

### Backend
- .NET 10.0 (C#)
- ASP.NET Core API
- SignalR (real-time communication)
- PostgreSQL 17
- Entity Framework Core

### Frontend
- WPF (Windows Presentation Foundation)
- .NET 10.0
- MVVM Community Toolkit
- SignalR Client
- Supports 4 languages: es-MX, ja-JP, zh-CN, ko-KR

### DevOps
- Docker
- Docker Compose
- Git

---

## Contributing

1. Create a feature branch (`git checkout -b feature/amazing-feature`)
2. Commit your changes (`git commit -m 'Add amazing feature'`)
3. Push to the branch (`git push origin feature/amazing-feature`)
4. Open a Pull Request

---

## License

This project is part of a semester coursework assignment.
