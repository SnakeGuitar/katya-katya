# Memory Game Client

WPF desktop client for the Memory Game Revival multiplayer application.

> **Note**: This is v0.1.0, the **Memory Game phase**. The project is transitioning to **"Katya Katya"** (dating simulator) in the next major version. See [CHANGELOG.md](../CHANGELOG.md) for details on the pivot.

## Requirements

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- Windows 10/11

## Quick Start

### 1. Start the server

The client requires the backend server to be running. See the [root README](../README.md) for server setup instructions.

### 2. Configure the connection

Edit the API base URL in `App.xaml.cs` or the relevant configuration file to point to your server:

- Docker: `http://localhost:5000`
- Local dev: `http://localhost:5059`

### 3. Run the client

```bash
dotnet run
```

Or open the project in Visual Studio and press **F5**.

## Project Structure

```
MemoryGame.Client/
├── Views/          # XAML views
├── ViewModels/     # MVVM view models (CommunityToolkit)
├── Models/         # Data models & DTOs
├── Services/       # API client, SignalR, navigation, music, etc.
├── Converters/     # Value converters for XAML bindings
├── Behaviors/      # Attached behaviors
├── Localization/   # Language configuration
├── Resources/      # Images, fonts, music
│   ├── Images/
│   ├── Fonts/
│   └── Music/
└── App.xaml        # Entry point & DI configuration
```

## Features

- **MVVM Architecture** — Clean separation using CommunityToolkit.Mvvm
- **Multi-language Support** — es-MX, ja-JP, zh-CN, ko-KR
- **Real-time Communication** — SignalR for multiplayer gameplay
- **Dependency Injection** — Built-in .NET DI via `App.xaml`

## Tech Stack

| Technology | Purpose |
|---|---|
| WPF | UI Framework |
| .NET 10.0 | Runtime |
| CommunityToolkit.Mvvm | MVVM pattern |
| SignalR Client | Real-time server communication |
