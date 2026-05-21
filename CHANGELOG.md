# Changelog

All notable changes to this project will be documented in this file.

## [0.1.0] - 2026-05-21 (Memory Game Revival - Final Checkpoint)

### Features
- **Multiplayer Mode**: Create/join lobbies, real-time gameplay via SignalR, turn-based card matching with vote-to-kick moderation
- **SinglePlayer Mode**: Play against difficulty presets (Easy/Medium/Hard/Custom) with customizable time limits
- **Authentication System**: Email verification flow, guest login, JWT-based sessions with refresh tokens
- **User Profiles**: Avatar upload, match history, player statistics, social networking (friends, follows)
- **Social Features**: Friend requests, follow system, user search
- **Moderation System**: Vote-to-kick mechanism, match statistics tracking, player reports
- **Localization**: Full support for es-MX, ja-JP, zh-CN, ko-KR with resource dictionaries
- **Performance Optimization**: GPU-accelerated backgrounds, optimized rendering for 1920x1080 fullscreen

### Architecture
- **Server**: Clean Architecture pattern (Domain/Application/Infrastructure/API)
  - ASP.NET Core with MediatR for request handling
  - PostgreSQL 17 with Entity Framework Core (Code-First)
  - SignalR for real-time lobby communication
  - JWT Bearer authentication with custom query-param support for WebSockets
- **Client**: WPF MVVM with CommunityToolkit.Mvvm
  - .NET 10.0 target framework
  - Responsive navigation with history stack
  - Network services for HTTP + SignalR integration
  - Multi-language support with culture-specific resources

### Known Limitations
- **SinglePlayer Score Persistence**: Scores are session-scoped and not saved to the database
- **Lobby State**: In-memory only (singletons); lost on server restart
- **Offline Mode**: Requires active connection to server; no offline game play
- **Database Seeding**: Initial user/character data requires manual insertion or migration seed

### Project Status
This release marks the **completion of the Memory Game Revival phase**. 

The project is **pivoting to "Katya Katya"** (dating simulator with character progression, idle mechanics, and gift system) starting with the next major version. Development of Katya Katya will focus on:
- Character affinity/relationship tracking
- Currency and progression systems
- Server persistence for game statistics
- Dating hub UI and character interactions

**Namespace and project names** (`MemoryGame*`) will be refactored in a future major release to reflect the new "Katya Katya" branding.

### Migration Path
Users of MemoryGame Revival v0.1.0 can migrate their accounts to Katya Katya v1.0 without data loss. Existing match histories will be preserved.

## [0.1.0-alpha] - Previous Milestones
Earlier alpha versions added core features incrementally:
- Authentication & email verification
- Multiplayer lobby system
- SinglePlayer card matching
- Real-time SignalR communication
- Localization framework
- UI/UX refinements
