# Hito 2 — Autenticación y Perfil Operativos

**Período:** 14/04/2026 – 24/04/2026
**Completado:** 30/03/2026
**Estado:** ✅ Completado (parcialmente — faltan pruebas unitarias)

---

## Descripción

Implementar los 16 endpoints REST de autenticación y perfil de usuario, incluyendo autenticación JWT, hash de contraseñas con BCrypt, validadores robustos con FluentValidation y el pipeline de validación de MediatR. La documentación API está a cargo del compañero de equipo.

---

## Tareas completadas

- [x] **CQRS — Commands de autenticación**
  - `RegisterUserCommand` — registro con email de verificación (MailKit)
  - `VerifyEmailCommand` — confirmación de PIN de 6 dígitos
  - `LoginCommand` — login con JWT + refresh token en UserSession
  - `LogoutCommand` — invalida la sesión activa
  - `RefreshTokenCommand` — rota el access token
  - `ChangePasswordCommand` — requiere contraseña actual y nueva con complejidad
  - `ForgotPasswordCommand` — envía PIN de recuperación por email
  - `ResetPasswordCommand` — restablece contraseña con PIN válido
  - `ResendVerificationEmailCommand`
  - `DeleteAccountCommand`
- [x] **CQRS — Queries de autenticación**
  - `GetUserByUsernameQuery` — lookup de usuario por username
- [x] **CQRS — Commands de perfil**
  - `UpdateProfileCommand` — actualiza username, bio, country
  - `UpdateAvatarCommand` — valida magic bytes (JPG/PNG/BMP), máx 5 MB
- [x] **CQRS — Queries de perfil**
  - `GetProfileByIdQuery`
  - `GetUserAvatarQuery`
- [x] **Validadores FluentValidation** (robustos, corrigiendo errores del legacy)
  - 10 validadores para Auth, 4 para Profile
  - `ValidationConstants.cs` — constantes centralizadas (email ≤ 50, password 8-100, username 3-30, PIN 6 dígitos exactos, avatar ≤ 5 MB)
  - `SharedValidationRules.cs` — extensiones reutilizables: `ValidEmail()`, `ValidPassword()`, `ValidUsername()`, `ValidPin()`, `ValidId()`
- [x] **Pipeline MediatR — `ValidationBehavior<TRequest, TResponse>`**
  - Recolecta todos los fallos de FluentValidation y lanza `ValidationException`
- [x] **Infraestructura de autenticación**
  - `JwtService` — genera access token (claims: NameIdentifier, username, isGuest) y refresh token
  - `PasswordService` — BCrypt hash + verify
  - `EmailService` — envío de PIN por MailKit / SMTP
- [x] **REST API — `AuthController`** (12 endpoints)

  | Método | Ruta | Descripción |
  |--------|------|-------------|
  | POST | `/api/auth/register` | Registro de usuario |
  | POST | `/api/auth/register/guest` | Registro de invitado |
  | POST | `/api/auth/verify-email` | Verificación de PIN |
  | POST | `/api/auth/resend-verification` | Reenviar PIN |
  | POST | `/api/auth/login` | Login (JWT + refresh) |
  | POST | `/api/auth/logout` | Logout |
  | POST | `/api/auth/refresh` | Rotar access token |
  | POST | `/api/auth/change-password` | Cambiar contraseña |
  | POST | `/api/auth/forgot-password` | Olvidé mi contraseña |
  | POST | `/api/auth/reset-password` | Restablecer contraseña |
  | GET  | `/api/auth/username/{username}` | Lookup por username |
  | DELETE | `/api/auth/account` | Eliminar cuenta |

- [x] **REST API — `ProfileController`** (4 endpoints)

  | Método | Ruta | Auth | Descripción |
  |--------|------|------|-------------|
  | GET | `/api/profile/{id}` | ✅ | Obtener perfil |
  | PUT | `/api/profile` | ✅ | Actualizar perfil |
  | GET | `/api/profile/{id}/avatar` | ❌ | Obtener avatar (público) |
  | PUT | `/api/profile/avatar` | ✅ | Subir avatar |

- [x] **Middleware de manejo de excepciones**
  - `ValidationException` → HTTP 422 con lista de errores por campo
  - `DomainException` → HTTP 400 con `{ errorCode, message }`
  - No controladas → HTTP 500
- [x] **Configuración JWT en `Program.cs`**
  - Validación estricta: issuer, audience, lifetime, signing key
  - Token por query string (`access_token`) para WebSocket / SignalR
- [x] **CORS** configurado (`Cors:AllowedOrigins` en appsettings)
- [x] **`appsettings.json` / `appsettings.Development.json`** actualizados

---

## Correcciones al legacy

| Bug legacy | Corrección aplicada |
|---|---|
| `ChangePassword` no validaba complejidad de la nueva contraseña | Validator exige mínimo 8 chars, mayúscula, número y carácter especial |
| PIN aceptaba hasta 10 caracteres | Validator exige exactamente 6 dígitos |
| Email máximo 255 chars | Limitado a 50 (consistente con el Value Object `Email` del dominio) |
| Constructor de `GetUserAvatarQueryHandler` tenía nombre incorrecto | Renombrado a `GetUserAvatarQueryHandler` |

---

## Tareas pendientes

- [ ] **Pruebas unitarias** — migración de `SessionServiceTest` del legacy

> La documentación API REST y la Guía JWT están a cargo del compañero de equipo.

---

## Plan de pruebas unitarias

### Marco de trabajo

```
MSTest + Moq + FluentAssertions
Proyecto: MemoryGame.Application.Tests
```

### Clases a migrar del legacy

| Clase legacy | Handlers / casos a cubrir |
|---|---|
| `SessionServiceTest.RegisterTest` | `RegisterUserCommandHandler` — éxito, email duplicado, username duplicado |
| `SessionServiceTest.LoginTest` | `LoginCommandHandler` — credenciales válidas, contraseña incorrecta, cuenta no verificada, cuenta baneada |
| `SessionServiceTest.LogoutTest` | `LogoutCommandHandler` — sesión activa, sesión no encontrada |
| `SessionServiceTest.ChangePasswordTest` | `ChangePasswordCommandHandler` — contraseña actual incorrecta, nueva contraseña débil |
| `SessionServiceTest.VerifyEmailTest` | `VerifyEmailCommandHandler` — PIN correcto, PIN expirado, PIN incorrecto |
| `SessionServiceTest.ResetPasswordTest` | `ResetPasswordCommandHandler` — flujo completo, PIN incorrecto |

### Estructura sugerida

```
MemoryGame.Application.Tests/
└── Auth/
    ├── Commands/
    │   ├── RegisterUserCommandHandlerTests.cs
    │   ├── LoginCommandHandlerTests.cs
    │   ├── LogoutCommandHandlerTests.cs
    │   ├── ChangePasswordCommandHandlerTests.cs
    │   ├── VerifyEmailCommandHandlerTests.cs
    │   ├── ResetPasswordCommandHandlerTests.cs
    │   ├── ForgotPasswordCommandHandlerTests.cs
    │   └── RefreshTokenCommandHandlerTests.cs
    └── Queries/
        └── GetUserByUsernameQueryHandlerTests.cs
```

### Patrón AAA — ejemplo

```csharp
[TestMethod]
public async Task Handle_ValidCredentials_ReturnsTokenDto()
{
    // Arrange
    var userRepo   = new Mock<IUserRepository>();
    var sessionRepo = new Mock<IUserSessionRepository>();
    var jwtService = new Mock<IJwtService>();
    var pwdService = new Mock<IPasswordService>();

    var user = User.Create("testuser", Email.Create("a@b.com"), "hashed", false);
    userRepo.Setup(r => r.GetByUsernameAsync("testuser")).ReturnsAsync(user);
    pwdService.Setup(s => s.Verify("plain", "hashed")).Returns(true);
    jwtService.Setup(s => s.GenerateAccessToken(user)).Returns("access.token");
    jwtService.Setup(s => s.GenerateRefreshToken()).Returns("refresh.token");

    var handler = new LoginCommandHandler(userRepo.Object, sessionRepo.Object,
                                          jwtService.Object, pwdService.Object, uow.Object);

    // Act
    var result = await handler.Handle(new LoginCommand("testuser", "plain"), default);

    // Assert
    result.AccessToken.Should().Be("access.token");
    result.RefreshToken.Should().Be("refresh.token");
}
```

### Cobertura mínima esperada

| Área | Objetivo |
|---|---|
| Caminos felices (happy paths) | 100 % de handlers |
| Errores de dominio conocidos | 100 % de `DomainErrors` lanzados en cada handler |
| Validadores | Al menos un caso inválido por regla de validación crítica |

---

## Próximo hito

**Hito 3 — Red social y estadísticas** (25/04/2026 – 05/05/2026)
