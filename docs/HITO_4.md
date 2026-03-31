# Hito 4 — Lobby en Tiempo Real Operativo

**Período:** 06/05/2026 – 20/05/2026
**Completado:** 30/03/2026
**Estado:** ✅ Completado (documentación a cargo del compañero)

---

## Descripción

Reemplazar la arquitectura WCF duplex del legacy (`IGameLobbyService` + `IGameLobbyCallback`) por un hub SignalR con gestión completa de lobbies en memoria: ciclo de vida de partidas, chat, volteo de cartas, votación de expulsión e invitaciones a amigos por notificación en tiempo real o correo electrónico como fallback.

---

## Tareas completadas

- [x] **`GameLobbyHub`** — Hub SignalR con `[Authorize]` que reemplaza el servicio WCF duplex
- [x] **9 métodos de hub** — equivalentes a las operaciones del legacy

  | Método de hub | Equivalente WCF |
  |---|---|
  | `CreateLobby` | `CreateGame` |
  | `JoinLobby` | `JoinGame` |
  | `LeaveLobby` | `LeaveGame` |
  | `SendChatMessage` | `SendMessage` |
  | `StartGame` | `StartGame` |
  | `FlipCard` | `FlipCard` |
  | `VoteToKick` | `VoteKick` |
  | `GetPublicLobbies` | `GetAvailableGames` |
  | `InviteFriend` | `InvitePlayer` *(extendido)* |

- [x] **14 eventos de cliente** — equivalentes a los callbacks del legacy

  | Evento SignalR | Descripción |
  |---|---|
  | `LobbyCreated` | Confirmación al host |
  | `PlayerJoined` | Notifica a todos cuando alguien entra |
  | `PlayerLeft` | Notifica a todos cuando alguien sale |
  | `UpdatePlayerList` | Lista actualizada de jugadores |
  | `ReceiveChatMessage` | Mensaje de chat (jugador o sistema) |
  | `GameStarted` | Tablero inicial enviado a todos |
  | `UpdateTurn` | Jugador activo y tiempo de turno |
  | `ShowCard` | Carta volteada (optimistic: caller primero) |
  | `SetCardsAsMatched` | Par encontrado |
  | `HideCards` | Par no coincidente, cartas ocultas |
  | `UpdateScore` | Puntuación actualizada |
  | `GameFinished` | Ganador de la partida |
  | `Kicked` | Notificación al jugador expulsado |
  | `PublicLobbiesList` | Lista de salas públicas disponibles |
  | `LobbyInviteReceived` | Invitación en tiempo real |
  | `LobbyInviteSent` | Confirmación al invitador |
  | `Error` | Código de error al caller |

- [x] **Gestión de lobbies en memoria**
  - `Lobby` — `ConcurrentDictionary` de jugadores, votación de expulsión por mayoría, re-host automático, inicio de partida
  - `GameSession` — tablero shuffled (Fisher-Yates), `FlipCard`, `EvaluateMatch`, `AdvanceTurn`, `RemovePlayer`, `GetWinner`
  - `LobbyManager` — singleton `ConcurrentDictionary<string, Lobby>`, búsqueda por connectionId
  - `GameCard` / `LobbyPlayer` / DTOs de lobby

- [x] **Latencia optimizada en volteo de cartas**
  - `CardRevealDelay = 800ms` (legacy WCF ~1500ms)
  - Patrón optimista: el caller recibe `ShowCard` de inmediato vía `Clients.Caller`, los demás lo reciben en paralelo vía `Clients.GroupExcept`

- [x] **Invitaciones a amigos**
  - Si el target está **online** → `LobbyInviteReceived` directo a su `connectionId`
  - Si el target está **offline** → email HTML vía `IEmailService.SendLobbyInviteAsync`
  - El caller siempre recibe `LobbyInviteSent(username, wasOnline)`

- [x] **`PresenceTracker`** — singleton con dos `ConcurrentDictionary` inversos para lookup O(1) userId↔connectionId
  - Registro en `OnConnectedAsync`, desregistro en `OnDisconnectedAsync`
  - Cubre desconexiones limpias y abruptas

- [x] **Autenticación JWT en WebSocket**
  - Token leído del query string `?access_token=` cuando la ruta comienza con `/hub`
  - `[Authorize]` en el hub obliga a que toda conexión esté autenticada

- [x] **`OnDisconnectedAsync`** — limpieza automática: salida del lobby, re-host, destrucción del lobby vacío, detección de fin de partida

- [x] **Pruebas unitarias** — 60 pruebas, 0 fallidas

---

## Pruebas unitarias

### Distribución

| Proyecto | Clase de prueba | Pruebas |
|---|---|---|
| `Application.Tests` | `GameSessionTests` | 22 |
| `Application.Tests` | `LobbyTests` | 16 |
| `Infrastructure.Tests` | `LobbyManagerTests` | 11 |
| `Infrastructure.Tests` | `PresenceTrackerTests` | 11 |
| **Total** | | **60** |

### Cobertura por área

**`GameSessionTests`**
- Generación de tablero: conteo exacto, pares, índices secuenciales, scores iniciales
- `FlipCard`: primer flip, segundo flip, jugador incorrecto, carta ya visible, carta emparejada, índice fuera de rango, partida terminada
- `EvaluateMatch`: par coincidente (score++), todos emparejados (IsFinished), no coincidente (oculta cartas, avanza turno)
- `AdvanceTurn`: avance normal, wrap circular
- `RemovePlayer`: jugador no activo, jugador activo (siguiente toma el turno), jugador activo en último índice (wrap), un solo jugador restante (IsFinished), limpieza de score
- `GetWinner`: ganador único, empate (null), scores vacíos (null)

**`LobbyTests`**
- `TryAddPlayer`: bajo capacidad, lleno, connectionId duplicado
- `RemovePlayer`: jugador existente, connectionId inexistente, host sale (re-host), no-host sale (host no cambia)
- `VoteToKick`: bajo umbral, mayoría alcanzada, mismo votante dos veces (cuenta una)
- `StartGame`: orden de turno correcto, `IsGameInProgress` activado
- `GetPlayerList`: DTOs correctos
- `IsGameInProgress`: falso antes de iniciar

**`LobbyManagerTests`**
- `CreateLobby`: código nuevo, código duplicado (null)
- `GetLobby`: existente, inexistente
- `RemoveLobby`: existente (true + eliminado), inexistente (false)
- `FindLobbyByConnection`: connectionId conocido, desconocido
- `GetPublicLobbies`: excluye privados, llenos, partidas en curso, manager vacío

**`PresenceTrackerTests`**
- `Track`: usuario nuevo (online), mismo usuario (sobrescribe connectionId), múltiples usuarios independientes
- `Untrack`: connectionId conocido (offline), desconocido (no lanza), no afecta a otros usuarios
- `GetConnectionId`: usuario online, usuario offline (null)
- `IsOnline`: usuario no registrado, tras `Untrack`

### Resultado

```
Pruebas totales: 60
  Correctas:     60
  Fallidas:       0
Tiempo total:  ~1.3 s
```

---

## Arquitectura — SignalR vs WCF

### 1. Diferencias de protocolo

| Aspecto | Legacy WCF | Revival SignalR |
|---|---|---|
| Transporte | TCP / Named Pipes (binario) | WebSocket → SSE → Long Polling (automático) |
| Modelo de comunicación | Duplex contract sincrónico | Pub/sub asíncrono basado en eventos |
| Serialización | BinaryFormatter / DataContract XML | JSON (System.Text.Json) |
| Callbacks del servidor al cliente | `IGameLobbyCallback` interface | `Clients.Group()` / `Clients.Client()` / `Clients.Caller()` |
| Autenticación | WCF identity / Windows Auth | JWT Bearer; query string `?access_token=` para WebSocket |
| Latencia carta | ~1500ms (overhead WCF + serialización) | 800ms (WebSocket <5ms + delay intencional de reveal) |
| Desconexión abrupta | Sin manejo automático | `OnDisconnectedAsync` siempre se dispara |
| Presencia de usuarios | No existía | `PresenceTracker` singleton (userId ↔ connectionId) |
| Escalabilidad horizontal | Proceso único, sin soporte | Backplane-ready (Redis SignalR Backplane) |
| Plataforma cliente | Solo .NET Framework (WCF proxy) | Cualquier cliente con WebSocket (WPF, Web, Mobile) |

---

### 2. Diagrama de flujo — Legacy WCF

```
Cliente WPF                    Servidor WCF
    │                               │
    │──── FlipCard(index) ─────────▶│
    │                               │  GameService.FlipCard()
    │                               │  ├─ valida turno
    │                               │  ├─ actualiza estado
    │                               │  └─ itera IGameLobbyCallback[]
    │◀─── callback.ShowCard() ──────│  (síncrono, uno por uno)
    │◀─── callback.ShowCard() ──────│
    │◀─── callback.ShowCard() ──────│
    │                               │  Thread.Sleep(1500ms)
    │◀─── callback.HideCards() ─────│
    │◀─── callback.UpdateTurn() ────│
```

**Problema:** los callbacks se hacen de forma secuencial y bloqueante. Un cliente lento retrasaba a todos los demás.

---

### 3. Diagrama de flujo — Revival SignalR

```
Cliente WPF                    GameLobbyHub                  Otros clientes
    │                               │                               │
    │──── FlipCard(index) ─────────▶│                               │
    │                               │  game.FlipCard()              │
    │◀─── ShowCard (inmediato) ─────│  Clients.Caller               │
    │                               │──── ShowCard ────────────────▶│ (paralelo)
    │                               │                               │
    │                               │  await Task.Delay(800ms)      │
    │                               │                               │
    │                               │  game.EvaluateMatch()         │
    │◀─── HideCards ────────────────│──── HideCards ───────────────▶│
    │◀─── UpdateTurn ───────────────│──── UpdateTurn ──────────────▶│
```

**Mejora:** el caller recibe `ShowCard` de inmediato (optimistic feedback). La broadcast a los demás ocurre en paralelo vía `Clients.GroupExcept`.

---

### 4. Migración de contratos — Callbacks WCF → Eventos SignalR

| Callback WCF (`IGameLobbyCallback`) | Evento SignalR | Destinatarios |
|---|---|---|
| `ShowCard(index, imageId)` | `ShowCard` | Group (caller inmediato + GroupExcept en paralelo) |
| `SetCardsAsMatched(i1, i2)` | `SetCardsAsMatched` | Group |
| `HideCards(i1, i2)` | `HideCards` | Group |
| `UpdateScore(user, score)` | `UpdateScore` | Group |
| `UpdateTurn(user, time)` | `UpdateTurn` | Group |
| `PlayerJoined(user, isGuest)` | `PlayerJoined` | Group |
| `PlayerLeft(user)` | `PlayerLeft` | Group |
| `UpdatePlayerList(list)` | `UpdatePlayerList` | Group |
| `ReceiveMessage(user, msg)` | `ReceiveChatMessage` | Group |
| `GameStarted(board)` | `GameStarted` | Group |
| `GameFinished(winner)` | `GameFinished` | Group |
| `Kicked()` | `Kicked` | Client (solo el expulsado) |
| *(no existía)* | `LobbyCreated` | Caller |
| *(no existía)* | `PublicLobbiesList` | Caller |
| *(no existía)* | `LobbyInviteReceived` | Client (target) |
| *(no existía)* | `LobbyInviteSent` | Caller |
| *(no existía)* | `Error` | Caller |

---

### 5. Migración de operaciones — Métodos WCF → Métodos de Hub

| Método WCF (`IGameLobbyService`) | Método de Hub | Cambios |
|---|---|---|
| `CreateGame(code, isPublic)` | `CreateLobby(code, isPublic)` | Devuelve `LobbyCreated` en lugar de retorno directo |
| `JoinGame(code)` | `JoinLobby(code)` | Agrega al grupo SignalR automáticamente |
| `LeaveGame()` | `LeaveLobby()` | El hub busca el lobby por connectionId |
| `SendMessage(msg)` | `SendChatMessage(msg)` | Límite de 500 chars, validado en hub |
| `StartGame(settings)` | `StartGame(settings)` | Valida `CardCount` (4-36, par) y `TurnTimeSeconds` (5-120) |
| `FlipCard(index)` | `FlipCard(index)` | Optimistic feedback al caller |
| `VoteKick(target)` | `VoteToKick(targetUsername)` | Umbral de mayoría: `(n/2) + 1` |
| `GetAvailableGames()` | `GetPublicLobbies()` | Excluye privados, llenos y partidas en curso |
| `InvitePlayer(id)` | `InviteFriend(targetUserId)` | Notif. en tiempo real si online; email si offline |

---

### 6. Guía de conexión desde el cliente WPF

#### Instalar el paquete

```bash
dotnet add package Microsoft.AspNetCore.SignalR.Client
```

#### Establecer conexión

```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:5001/hub/lobby", options =>
    {
        // JWT se pasa por query string porque WebSocket
        // no soporta cabeceras HTTP personalizadas
        options.AccessTokenProvider = () => Task.FromResult<string?>(jwtToken);
    })
    .WithAutomaticReconnect()
    .Build();
```

#### Suscribirse a eventos

```csharp
connection.On<int, string>("ShowCard", (index, imageId) =>
{
    // voltear carta index con imagen imageId
});

connection.On<int, int>("SetCardsAsMatched", (i1, i2) =>
{
    // marcar cartas como emparejadas
});

connection.On<int, int>("HideCards", (i1, i2) =>
{
    // ocultar cartas no coincidentes
});

connection.On<string, int>("UpdateScore", (username, score) =>
{
    // actualizar marcador
});

connection.On<string, int>("UpdateTurn", (username, seconds) =>
{
    // mostrar turno activo y temporizador
});

connection.On<string>("GameFinished", winner =>
{
    // mostrar pantalla de fin de partida
});

connection.On<string>("Error", code =>
{
    // manejar error (ej. LOBBY_FULL, LOBBY_NOT_FOUND)
});
```

#### Iniciar conexión y enviar acciones

```csharp
await connection.StartAsync();

// Crear lobby
await connection.InvokeAsync("CreateLobby", "ABCD", isPublic: true);

// Unirse a lobby
await connection.InvokeAsync("JoinLobby", "ABCD");

// Voltear carta
await connection.InvokeAsync("FlipCard", cardIndex);

// Enviar chat
await connection.InvokeAsync("SendChatMessage", "¡Hola!");

// Invitar amigo
await connection.InvokeAsync("InviteFriend", targetUserId);

// Cerrar conexión
await connection.StopAsync();
```

#### Manejo de reconexión

`WithAutomaticReconnect()` reintenta la conexión con backoff exponencial (0s, 2s, 10s, 30s). Al reconectar, el cliente debe volver a llamar `JoinLobby` porque el `connectionId` cambia y el servidor ya no lo tiene en el grupo.

```csharp
connection.Reconnected += async connectionId =>
{
    await connection.InvokeAsync("JoinLobby", lastGameCode);
};
```

---

### 7. Gestión de estado en memoria

```
PresenceTracker (singleton)
├── _userToConnection: { userId → connectionId }
└── _connectionToUser: { connectionId → userId }

LobbyManager (singleton)
└── _lobbies: { gameCode → Lobby }
              └── Lobby
                  ├── _players: { connectionId → LobbyPlayer }
                  ├── _kickVotes: { targetUsername → HashSet<voterUsername> }
                  └── Game: GameSession?
                            ├── Board: List<GameCard>
                            ├── TurnOrder: List<string>
                            └── Scores: ConcurrentDictionary<string, int>
```

**Nota de producción:** este estado es efímero (se pierde al reiniciar el servidor). Para alta disponibilidad con múltiples instancias se requiere un backplane Redis y persistencia del estado de partida en base de datos.

---

## Próximo hito

**Hito 5 — Cliente WPF** (21/05/2026 – 04/06/2026)
