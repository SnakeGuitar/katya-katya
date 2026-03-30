# Hito 3 — Red Social y Estadísticas

**Período:** 25/04/2026 – 05/05/2026
**Completado:** 30/03/2026
**Estado:** ✅ Completado (parcialmente — faltan pruebas unitarias)

---

## Descripción

Implementar los endpoints REST de red social (amigos, solicitudes, redes sociales externas), historial de partidas y moderación. Incluye los comandos y queries CQRS correspondientes, validadores robustos y controladores. La documentación API está a cargo del compañero de equipo.

---

## Tareas completadas

- [x] **CQRS — Commands de red social**
  - `AddSocialNetworkCommand` — vincula una red social externa al perfil
  - `RemoveSocialNetworkCommand` — desvincula una red social
  - `SendFriendRequestCommand` — envía solicitud de amistad (lookup por username)
  - `AnswerFriendRequestCommand` — acepta o rechaza solicitud; crea `Friendship` si se acepta
  - `RemoveFriendCommand` — elimina amistad verificando que existan como amigos
- [x] **CQRS — Queries de red social**
  - `GetSocialNetworksQuery` — lista las redes vinculadas de un usuario
  - `GetFriendsListQuery` — lista amigos con estado en línea
  - `GetPendingFriendRequestsQuery` — solicitudes recibidas pendientes
- [x] **CQRS — Queries de estadísticas**
  - `GetMatchHistoryQuery` — historial de partidas con score e indicador de victoria
- [x] **CQRS — Commands de moderación**
  - `ReportUserCommand` — crea una `Penalty` de tipo `Warning` contra el usuario reportado
- [x] **DTOs**
  - `SocialNetworkDto`, `FriendDto`, `FriendRequestDto`
  - `MatchHistoryDto` — `(MatchId, Score, IsWinner)`
- [x] **Validadores FluentValidation**
  - 5 validadores para Social (incluyendo verificación de que el caller no sea el target)
  - 1 validador para Moderation (caller ≠ target)
- [x] **Nuevos `DomainErrors`**

  ```csharp
  Social.FriendRequestNotFound
  Social.FriendRequestAlreadySent
  Social.AlreadyFriends
  Social.NotFriends
  Social.SocialNetworkNotFound
  Match.NotFound
  ```

- [x] **REST API — `SocialController`** (8 endpoints)

  | Método | Ruta | Descripción |
  |--------|------|-------------|
  | GET    | `/api/social/networks`             | Mis redes sociales |
  | POST   | `/api/social/networks`             | Agregar red social |
  | DELETE | `/api/social/networks/{networkId}` | Eliminar red social |
  | GET    | `/api/social/friends`              | Lista de amigos |
  | DELETE | `/api/social/friends/{friendId}`   | Eliminar amigo |
  | GET    | `/api/social/friend-requests`      | Solicitudes pendientes |
  | POST   | `/api/social/friend-requests`      | Enviar solicitud |
  | PUT    | `/api/social/friend-requests/{id}` | Aceptar / rechazar solicitud |

- [x] **REST API — `MatchesController`** (1 endpoint)

  | Método | Ruta | Descripción |
  |--------|------|-------------|
  | GET | `/api/matches/history` | Historial de partidas del usuario autenticado |

- [x] **REST API — `ModerationController`** (1 endpoint)

  | Método | Ruta | Descripción |
  |--------|------|-------------|
  | POST | `/api/moderation/report` | Reportar a un usuario |

---

## Correcciones al legacy

| Situación legacy | Solución aplicada |
|---|---|
| `FriendRequest` y `Friendship` compartían tabla self-referencing M2M | Se separaron en dos entidades distintas con tabla `friendships` propia |
| No había validación de que el caller y el target no fueran el mismo | `RemoveFriendCommandValidator` y `ReportUserCommandValidator` verifican `CallerId != TargetId` |
| `SendFriendRequest` usaba el ID directamente; posible envío duplicado sin validar | El handler verifica `FriendRequestAlreadySent` y `AlreadyFriends` antes de persistir |

---

## Tareas pendientes

- [ ] **Pruebas unitarias** — migración de `SocialCoreTest` y `StatisticsCoreTest` del legacy

> La documentación API REST está a cargo del compañero de equipo.

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
| `SocialCoreTest.SendFriendRequestTest` | `SendFriendRequestCommandHandler` — éxito, ya amigos, solicitud duplicada, usuario no encontrado |
| `SocialCoreTest.AnswerFriendRequestTest` | `AnswerFriendRequestCommandHandler` — aceptar (crea Friendship), rechazar, solicitud no encontrada |
| `SocialCoreTest.RemoveFriendTest` | `RemoveFriendCommandHandler` — éxito, no son amigos |
| `SocialCoreTest.SocialNetworkTest` | `AddSocialNetworkCommandHandler` y `RemoveSocialNetworkCommandHandler` |
| `StatisticsCoreTest.MatchHistoryTest` | `GetMatchHistoryQueryHandler` — lista con resultados, lista vacía |

### Estructura sugerida

```
MemoryGame.Application.Tests/
├── Social/
│   ├── Commands/
│   │   ├── SendFriendRequestCommandHandlerTests.cs
│   │   ├── AnswerFriendRequestCommandHandlerTests.cs
│   │   ├── RemoveFriendCommandHandlerTests.cs
│   │   ├── AddSocialNetworkCommandHandlerTests.cs
│   │   └── RemoveSocialNetworkCommandHandlerTests.cs
│   └── Queries/
│       ├── GetFriendsListQueryHandlerTests.cs
│       ├── GetPendingFriendRequestsQueryHandlerTests.cs
│       └── GetSocialNetworksQueryHandlerTests.cs
├── Matches/
│   └── Queries/
│       └── GetMatchHistoryQueryHandlerTests.cs
└── Moderation/
    └── Commands/
        └── ReportUserCommandHandlerTests.cs
```

### Patrón AAA — ejemplo

```csharp
[TestMethod]
public async Task Handle_AlreadyFriends_ThrowsDomainException()
{
    // Arrange
    var socialRepo = new Mock<ISocialRepository>();
    socialRepo
        .Setup(r => r.AreFriendsAsync(1, 2))
        .ReturnsAsync(true);

    var handler = new SendFriendRequestCommandHandler(socialRepo.Object, uow.Object);

    // Act
    Func<Task> act = () => handler.Handle(
        new SendFriendRequestCommand(CallerId: 1, ReceiverUsername: "target"), default);

    // Assert
    await act.Should()
        .ThrowAsync<DomainException>()
        .WithMessage(DomainErrors.Social.AlreadyFriends);
}
```

### Cobertura mínima esperada

| Área | Objetivo |
|---|---|
| Caminos felices (happy paths) | 100 % de handlers |
| Errores de dominio conocidos | 100 % de `DomainErrors` lanzados en cada handler |
| Validadores | Al menos un caso inválido por regla de validación crítica |
| Consistencia de estado | Verificar que `IUnitOfWork.SaveChangesAsync` se llama en cada command |

---

## Próximo hito

**Hito 4 — Juego en tiempo real** (06/05/2026 – 20/05/2026)
