# Memory Game Revival

Juego de memoria multijugador con cliente WPF y backend ASP.NET Core.

## Requisitos

| Herramienta | Versión mínima |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 |
| [Docker Desktop](https://www.docker.com/products/docker-desktop) | cualquiera reciente |
| Windows | 10 / 11 (el cliente es WPF) |

---

## Inicio rápido

### 1. Clonar el repositorio

```bash
git clone https://github.com/tu-usuario/MemoryGame-Revival.git
cd MemoryGame-Revival
```

### 2. Configurar variables de entorno

Copia el archivo de ejemplo y rellena los valores:

```bash
cp .env.example .env
```

Edita `.env`:

```env
# PostgreSQL
POSTGRES_DB=memorygame
POSTGRES_USER=tu_usuario
POSTGRES_PASSWORD=tu_password_segura

# API
ASPNETCORE_ENVIRONMENT=Development
CONNECTION_STRING=Host=db;Port=5432;Database=memorygame;Username=tu_usuario;Password=tu_password_segura

# Email (SMTP) — requerido para verificación de cuenta
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=tucorreo@gmail.com
SMTP_PASSWORD=tu_app_password_de_google

# JWT
JWT_SECRET=clave-secreta-de-al-menos-32-caracteres-aqui
JWT_ISSUER=memorygame-api
JWT_AUDIENCE=memorygame-client
JWT_EXPIRATION_MINUTES=60
JWT_REFRESH_EXPIRATION_DAYS=7
```

> **¿Cómo generar una App Password de Gmail?**
> Google Account → Seguridad → Verificación en 2 pasos → Contraseñas de aplicaciones
> O entra directo a: https://myaccount.google.com/apppasswords

### 3. Levantar la base de datos y la API

```bash
docker-compose up -d
```

Esto levanta dos servicios:
- **db** — PostgreSQL 17 en el puerto `5432`
- **api** — API REST + SignalR en el puerto `5000`

### 4. Configurar el cliente WPF

El cliente se conecta al servidor local. Abre `MemoryGame.Client/appsettings.json` (o similar) y verifica que la URL base apunte a `http://localhost:5059` si corres el servidor localmente, o `http://localhost:5000` si usas Docker.

### 5. Correr el cliente

Abre una terminal en la carpeta del cliente:

```bash
cd MemoryGame.Client
dotnet run
```

O simplemente ábrelo en Visual Studio y presiona F5.

---

## Desarrollo local (sin Docker para el servidor)

Si prefieres correr el servidor directamente con `dotnet watch`:

### 1. Levantar solo la base de datos

```bash
docker-compose up -d db
```

### 2. Configurar `appsettings.Development.json`

Abre `MemoryGame.Server/src/MemoryGame.API/appsettings.Development.json` y asegúrate de que tenga:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=memorygame;Username=tu_usuario;Password=tu_password_segura"
  },
  "JWT_SECRET": "clave-secreta-de-al-menos-32-caracteres-aqui",
  "JWT_EXPIRATION_MINUTES": "1440",
  "SMTP_HOST": "smtp.gmail.com",
  "SMTP_PORT": "587",
  "SMTP_USER": "tucorreo@gmail.com",
  "SMTP_PASSWORD": "tu_app_password",
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

> Las claves SMTP deben estar al **nivel raíz** del JSON, no dentro de un objeto anidado.

### 3. Ejecutar las migraciones

```bash
cd MemoryGame.Server/src/MemoryGame.API
dotnet ef database update
```

> Si `dotnet ef` no está instalado:
> ```bash
> dotnet tool install --global dotnet-ef
> ```
> Luego cierra y reabre la terminal.

### 4. Correr el servidor

```bash
dotnet watch
```

El servidor estará disponible en `http://localhost:5059`.

---

## Estructura del proyecto

```
MemoryGame-Revival/
├── MemoryGame.Server/
│   └── src/
│       ├── MemoryGame.API/          # Controladores, Hubs SignalR, Middleware
│       ├── MemoryGame.Application/  # Comandos, Queries, DTOs (MediatR)
│       ├── MemoryGame.Domain/       # Entidades y lógica de dominio
│       └── MemoryGame.Infrastructure/ # EF Core, repositorios, servicios externos
├── MemoryGame.Client/               # Cliente WPF (Windows)
│   ├── Views/                       # XAML
│   ├── ViewModels/                  # MVVM + CommunityToolkit
│   ├── Services/                    # API client, navegación, música, etc.
│   └── Resources/                   # Imágenes, fuentes, música
├── docker-compose.yml
├── .env.example
└── README.md
```

---

## Puertos

| Servicio | Puerto | Descripción |
|---|---|---|
| API (Docker) | `5000` | REST + SignalR |
| API (local) | `5059` | `dotnet watch` |
| PostgreSQL | `5432` | Base de datos |

---

## Flujo de registro

1. El usuario llena el formulario de registro
2. El servidor envía un PIN de 6 dígitos al correo
3. El usuario ingresa el PIN en la pantalla de verificación
4. Al verificar correctamente, el usuario queda **logueado automáticamente**

---

## Solución de problemas comunes

**`SMTP_HOST not configured`**
Las claves SMTP están anidadas en un objeto en el JSON. Deben estar al nivel raíz:
```json
{
  "SMTP_HOST": "smtp.gmail.com",  ← correcto
  ...
}
```

**`password authentication failed for user "..."`**
El usuario y contraseña de PostgreSQL en `appsettings.Development.json` no coinciden con los del container Docker. Asegúrate de que sean los mismos que en `.env`.

**`duplicate key value violates unique constraint "ix_pending_registrations_email"`**
Ya existe un registro pendiente con ese email. Usa un email diferente o limpia la tabla:
```bash
docker exec -it <nombre_container_db> psql -U tu_usuario -d memorygame -c "DELETE FROM pending_registrations;"
```

**`dotnet-ef` no encontrado**
```bash
dotnet tool install --global dotnet-ef
# Cierra y reabre la terminal
```

**El cliente no conecta con el servidor**
Verifica que la URL base en el cliente apunte al puerto correcto:
- Docker: `http://localhost:5000`
- Local: `http://localhost:5059`
