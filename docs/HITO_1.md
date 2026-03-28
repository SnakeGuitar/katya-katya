# Hito 1 — Fundación del Proyecto

**Período:** 30/03/2026 – 13/04/2026
**Completado:** 28/03/2026
**Estado:** ✅ Completado

---

## Descripción

Establecer la infraestructura base del proyecto v2 siguiendo Domain Driven Design (DDD), migrando las 10 entidades del legacy WCF a modelos ricos con lógica de negocio, configurando EF Core con PostgreSQL y levantando el entorno con Docker.

---

## Tareas completadas

- [x] Tag `v1.0.0` creado en el repo legacy y publicado en GitHub
- [x] Repositorio `MemoryGame-Revival` creado en `C:\Users\snake\Workspace\Memory-Game-Revival`
- [x] Solución .NET 10 + ASP.NET Core con estructura DDD
- [x] 7 proyectos creados (4 src + 3 tests)
- [x] Referencias entre proyectos configuradas (flujo de dependencias DDD)
- [x] 14 paquetes NuGet instalados
- [x] Docker Compose con PostgreSQL 17-alpine
- [x] `.env` y `.env.example` configurados
- [x] 10 entidades legacy migradas a modelos ricos DDD
- [x] 5 interfaces de repositorio + 5 implementaciones
- [x] `MemoryGameDbContext` con todas las relaciones y conversiones
- [x] Migración `InitialCreate` generada y aplicada
- [x] 12 tablas creadas en PostgreSQL ✅
- [x] `Dockerfile` para `MemoryGame.API`

---

## Estructura del proyecto

```
MemoryGame-Revival/
├── .env                        ← secrets locales (ignorado por git)
├── .env.example                ← plantilla sin secrets
├── docker-compose.yml
├── .gitignore
├── docs/
│   └── HITO_1.md
├── MemoryGame.Server/
│   ├── MemoryGame.Server.sln
│   ├── dependencies.txt
│   ├── src/
│   │   ├── MemoryGame.Domain/
│   │   │   ├── Common/         (BaseEntity, DomainException, Enums)
│   │   │   ├── Users/          (User, UserSession, PendingRegistration)
│   │   │   ├── Social/         (FriendRequest, Friendship, SocialNetwork)
│   │   │   ├── Matches/        (Match, MatchParticipation)
│   │   │   ├── Cards/          (Card, Deck)
│   │   │   └── Penalties/      (Penalty)
│   │   ├── MemoryGame.Application/
│   │   ├── MemoryGame.Infrastructure/
│   │   │   ├── Persistence/    (MemoryGameDbContext)
│   │   │   ├── Repositories/   (5 repositorios)
│   │   │   └── Migrations/     (InitialCreate)
│   │   └── MemoryGame.API/
│   │       └── Dockerfile
│   └── tests/
│       ├── MemoryGame.Domain.Tests/
│       ├── MemoryGame.Application.Tests/
│       └── MemoryGame.Infrastructure.Tests/
└── MemoryGame.Client/          ← vacío hasta Hito 5
```

---

## Entidades migradas

| Legacy (WCF + EF6) | Revival (DDD) | Agregado |
|---|---|---|
| `user` | `User` | Users |
| `userSession` | `UserSession` | Users |
| `pendingRegistration` | `PendingRegistration` | Users |
| `FriendRequest` | `FriendRequest` | Social |
| *(self-referencing M2M)* | `Friendship` *(nueva)* | Social |
| `socialNetwork` | `SocialNetwork` | Social |
| `match` | `Match` | Matches |
| `matchHistory` | `MatchParticipation` | Matches |
| `card` | `Card` | Cards |
| `inventory` | `Deck` | Cards |
| `penalty` | `Penalty` | Penalties |

---

## Paquetes NuGet instalados

| Proyecto | Paquete | Versión |
|---|---|---|
| Application | MediatR | 14.1.0 |
| Application | FluentValidation | 12.1.1 |
| Application | AutoMapper | 16.1.1 |
| Infrastructure | Microsoft.EntityFrameworkCore | 10.0.5 |
| Infrastructure | Npgsql.EntityFrameworkCore.PostgreSQL | 10.x |
| Infrastructure | Microsoft.EntityFrameworkCore.Design | 10.0.5 |
| Infrastructure | Microsoft.EntityFrameworkCore.Tools | 10.0.5 |
| Infrastructure | EFCore.NamingConventions | 10.0.1 |
| Infrastructure | MailKit | latest |
| Infrastructure | BCrypt.Net-Next | latest |
| API | Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.5 |
| API | Swashbuckle.AspNetCore | 10.1.7 |
| API | MediatR | 14.1.0 |
| API | Microsoft.EntityFrameworkCore.Design | 10.0.5 |
| *.Tests | Moq | 4.20.72 |
| *.Tests | FluentAssertions | 8.9.0 |
| *.Tests | Microsoft.EntityFrameworkCore.InMemory | 10.0.5 |

---

## Comandos ejecutados

### Tag legacy

```bash
git tag v1.0.0
git push origin v1.0.0
```

### Crear solución y proyectos

```bash
cd MemoryGame.Server/

dotnet new sln -n MemoryGame.Server
dotnet new classlib -n MemoryGame.Domain      -o src/MemoryGame.Domain
dotnet new classlib -n MemoryGame.Application -o src/MemoryGame.Application
dotnet new classlib -n MemoryGame.Infrastructure -o src/MemoryGame.Infrastructure
dotnet new webapi   -n MemoryGame.API         -o src/MemoryGame.API --use-controllers
dotnet new mstest   -n MemoryGame.Domain.Tests      -o tests/MemoryGame.Domain.Tests
dotnet new mstest   -n MemoryGame.Application.Tests -o tests/MemoryGame.Application.Tests
dotnet new mstest   -n MemoryGame.Infrastructure.Tests -o tests/MemoryGame.Infrastructure.Tests
```

### Agregar proyectos a la solución

```bash
dotnet sln add src/MemoryGame.Domain/MemoryGame.Domain.csproj
dotnet sln add src/MemoryGame.Application/MemoryGame.Application.csproj
dotnet sln add src/MemoryGame.Infrastructure/MemoryGame.Infrastructure.csproj
dotnet sln add src/MemoryGame.API/MemoryGame.API.csproj
dotnet sln add tests/MemoryGame.Domain.Tests/MemoryGame.Domain.Tests.csproj
dotnet sln add tests/MemoryGame.Application.Tests/MemoryGame.Application.Tests.csproj
dotnet sln add tests/MemoryGame.Infrastructure.Tests/MemoryGame.Infrastructure.Tests.csproj
```

### Configurar referencias entre proyectos

```bash
dotnet add src/MemoryGame.Application/MemoryGame.Application.csproj \
    reference src/MemoryGame.Domain/MemoryGame.Domain.csproj

dotnet add src/MemoryGame.Infrastructure/MemoryGame.Infrastructure.csproj \
    reference src/MemoryGame.Domain/MemoryGame.Domain.csproj

dotnet add src/MemoryGame.Infrastructure/MemoryGame.Infrastructure.csproj \
    reference src/MemoryGame.Application/MemoryGame.Application.csproj

dotnet add src/MemoryGame.API/MemoryGame.API.csproj \
    reference src/MemoryGame.Application/MemoryGame.Application.csproj

dotnet add src/MemoryGame.API/MemoryGame.API.csproj \
    reference src/MemoryGame.Infrastructure/MemoryGame.Infrastructure.csproj

dotnet add tests/MemoryGame.Domain.Tests/MemoryGame.Domain.Tests.csproj \
    reference src/MemoryGame.Domain/MemoryGame.Domain.csproj

dotnet add tests/MemoryGame.Application.Tests/MemoryGame.Application.Tests.csproj \
    reference src/MemoryGame.Application/MemoryGame.Application.csproj

dotnet add tests/MemoryGame.Infrastructure.Tests/MemoryGame.Infrastructure.Tests.csproj \
    reference src/MemoryGame.Infrastructure/MemoryGame.Infrastructure.csproj
```

### Instalar paquetes NuGet

```bash
# Application
dotnet add src/MemoryGame.Application/MemoryGame.Application.csproj package MediatR
dotnet add src/MemoryGame.Application/MemoryGame.Application.csproj package FluentValidation
dotnet add src/MemoryGame.Application/MemoryGame.Application.csproj package AutoMapper

# Infrastructure
dotnet add src/MemoryGame.Infrastructure/MemoryGame.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add src/MemoryGame.Infrastructure/MemoryGame.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/MemoryGame.Infrastructure/MemoryGame.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/MemoryGame.Infrastructure/MemoryGame.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Tools
dotnet add src/MemoryGame.Infrastructure/MemoryGame.Infrastructure.csproj package EFCore.NamingConventions
dotnet add src/MemoryGame.Infrastructure/MemoryGame.Infrastructure.csproj package MailKit
dotnet add src/MemoryGame.Infrastructure/MemoryGame.Infrastructure.csproj package BCrypt.Net-Next

# API
dotnet add src/MemoryGame.API/MemoryGame.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/MemoryGame.API/MemoryGame.API.csproj package Swashbuckle.AspNetCore
dotnet add src/MemoryGame.API/MemoryGame.API.csproj package MediatR
dotnet add src/MemoryGame.API/MemoryGame.API.csproj package Microsoft.EntityFrameworkCore.Design

# Tests (repetir para los 3 proyectos de test)
dotnet add tests/MemoryGame.Domain.Tests/MemoryGame.Domain.Tests.csproj package Moq
dotnet add tests/MemoryGame.Domain.Tests/MemoryGame.Domain.Tests.csproj package FluentAssertions
dotnet add tests/MemoryGame.Domain.Tests/MemoryGame.Domain.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory
# (idem para Application.Tests e Infrastructure.Tests)
```

### Docker

```bash
# Levantar solo la base de datos
docker compose up -d db

# Verificar que PostgreSQL está listo
docker exec memorygame-revival-db-1 pg_isready -U admin -d memorygame
```

### EF Core — Migraciones

```bash
# Generar migración inicial
dotnet ef migrations add InitialCreate \
    --project src/MemoryGame.Infrastructure \
    --startup-project src/MemoryGame.API \
    --context MemoryGameDbContext

# Aplicar migración a la base de datos
dotnet ef database update \
    --project src/MemoryGame.Infrastructure \
    --startup-project src/MemoryGame.API \
    --context MemoryGameDbContext

# Verificar tablas creadas
docker exec memorygame-revival-db-1 psql -U admin -d memorygame -c "\dt"
```

### Verificación final

```bash
dotnet build MemoryGame.Server.sln
# Resultado esperado: Compilación correcta. 0 Errores.
```

---

## Tablas creadas en PostgreSQL

```
 Schema |         Name          | Type
--------+-----------------------+-------
 public | __EFMigrationsHistory | table
 public | cards                 | table
 public | decks                 | table
 public | friend_requests       | table
 public | friendships           | table
 public | match_participations  | table
 public | matches               | table
 public | penalties             | table
 public | pending_registrations | table
 public | social_networks       | table
 public | user_sessions         | table
 public | users                 | table
```

---

## Próximo hito

**Hito 2 — Autenticación y perfil operativos** (14/04/2026 – 24/04/2026)

- 16 endpoints REST (registro con verificación por email, login, perfil)
- Autenticación JWT
- Hash de contraseñas con BCrypt
- Pruebas de SessionServiceTest migradas
- Documentación API REST + Guía JWT
