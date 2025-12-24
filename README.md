# 🎮 Bomberman Multiplayer

A real-time two-player online Bomberman game built with .NET 8, MonoGame, and SignalR, demonstrating 9 object-oriented design patterns.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![MonoGame](https://img.shields.io/badge/MonoGame-3.8.4-E73C00?logo=monogame)
![SignalR](https://img.shields.io/badge/SignalR-Real--time-00ADD8)
![SQLite](https://img.shields.io/badge/SQLite-3-003B57?logo=sqlite)

---

## 📋 Table of Contents

- [Features](#-features)
- [Design Patterns](#-design-patterns)
- [Architecture](#-architecture)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Running the Game](#-running-the-game)
- [How to Play](#-how-to-play)
- [Project Structure](#-project-structure)
- [Documentation](#-documentation)
- [Technologies](#-technologies)

---

## ✨ Features

### Core Gameplay
- ⚔️ **Two-player online multiplayer** via SignalR
- 🗺️ **Three unique map themes** (City, Desert, Forest)
- 💣 **Three wall types** including hard walls requiring 2 hits
- ⚡ **Power-up system** (Speed Boost, Bomb Count, Bomb Power)
- 🤖 **Enemy AI** with 3 different behaviors (Static, Chasing, Random)
- 💥 **Real-time synchronized explosions**

### User Features
- 🔐 **Authentication system** with login/register
- 🏆 **Leaderboard** with player statistics
- 📊 **Persistent stats** (Wins, Losses, Total Games, Kills)
- 🎨 **Theme preferences** saved per user
- 💾 **SQLite database** for data persistence

### Technical Features
- 🎯 **9 Design Patterns** (Strategy, Observer, State, Abstract Factory, Builder, Factory Method, Decorator, Adapter, Repository)
- 🌐 **Real-time communication** with SignalR
- 🎮 **Deterministic map generation** for synchronization
- 🏗️ **Modular architecture** with clean separation of concerns

---

## 🎨 Design Patterns

This project implements **9 design patterns**, exceeding the requirement:

### Behavioral Patterns (3)
1. **Strategy Pattern** - Enemy AI behaviors
2. **Observer Pattern** - Game event notifications
3. **State Pattern** - Player state management

### Creational Patterns (3)
4. **Abstract Factory** - Theme-specific asset creation
5. **Builder Pattern** - Complex map construction
6. **Factory Method** - Wall type instantiation

### Structural Patterns (2)
7. **Decorator Pattern** - Dynamic power-up abilities
8. **Adapter Pattern** - SignalR wrapper abstraction

### Architectural Pattern (1)
9. **Repository Pattern** - Database access layer

📖 **See [DESIGN_DOCUMENT.md](DESIGN_DOCUMENT.md) for detailed explanations and UML diagrams.**

---

## 🏗️ Architecture

```
┌─────────────────┐
│  Bomberman.UI   │  ← MonoGame client (rendering, input)
└────────┬────────┘
         │
┌────────▼────────┐
│ Bomberman.      │  ← Networking + Database
│   Services      │
└────────┬────────┘
         │
┌────────▼────────┐
│ Bomberman.Core  │  ← Pure game logic (no dependencies)
└─────────────────┘

        ┌─────────────────┐
        │ Bomberman.Server│  ← SignalR Hub (ASP.NET Core)
        └─────────────────┘
```

**Key Principles:**
- **Core** has zero external dependencies
- **Services** handles I/O (network, database)
- **UI** orchestrates game flow
- **Server** maintains game state

---

## 📦 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MonoGame dependencies (automatically restored via NuGet)
- SQLite (included)

### MonoGame Runtime Dependencies (Mac)

```bash
brew install mono-libgdiplus
```

For other platforms, see [MonoGame documentation](https://docs.monogame.net/articles/getting_started/1_setting_up_your_development_environment_ubuntu.html).

---

## 🚀 Installation

1. **Clone the repository:**
```bash
cd ~/Desktop
git clone <repository-url>
cd "bomberman 2/Bomberman"
```

2. **Restore NuGet packages:**
```bash
dotnet restore
```

3. **Build the solution:**
```bash
dotnet build
```

---

## 🎮 Running the Game

### Step 1: Start the Server

Open a terminal and run:

```bash
cd Bomberman.Server
dotnet run
```

The server will start on `http://localhost:5077`

### Step 2: Start Player 1

Open a second terminal:

```bash
cd Bomberman.UI
dotnet run
```

### Step 3: Start Player 2

Open a third terminal:

```bash
cd Bomberman.UI
dotnet run
```

### Step 4: Play!

1. Both players **register** or **login**
2. **First player selects a theme** (City, Desert, or Forest)
3. Both players click **"JOIN MATCH"**
4. Game starts automatically when 2 players are ready!

---

## 🎯 How to Play

### Controls

| Key | Action |
|-----|--------|
| **Arrow Keys** | Move player |
| **Space** | Place bomb |
| **Enter** | Confirm/Submit |
| **Escape** | Exit game |

### Objective

💣 **Destroy your opponent with bombs while avoiding explosions!**

### Game Elements

| Element | Description |
|---------|-------------|
| 🧱 **Unbreakable Walls** | Cannot be destroyed |
| 🟫 **Breakable Walls** | Destroyed in 1 hit |
| 🟧 **Hard Walls** | Require 2 hits to destroy |
| ⚡ **Speed Boost** | Increase movement speed |
| 💣 **Bomb Count** | Place more bombs simultaneously |
| 💥 **Bomb Power** | Increase explosion range |
| 👾 **Enemies** | AI-controlled obstacles |

### Winning

- Survive longer than your opponent
- Stats are automatically saved to the leaderboard

---

## 📁 Project Structure

```
Bomberman/
├── Bomberman.Core/              # Game logic
│   ├── Entities/                # Player, Enemy, Bomb
│   ├── GameLogic/               # GameMap, collision
│   ├── Patterns/                # Design pattern implementations
│   │   ├── Behavioral/          # Strategy, Observer, State
│   │   ├── Creational/          # Abstract Factory, Builder, Factory Method
│   │   └── Structural/          # Decorator, Adapter
│   └── Walls/                   # Wall types
│
├── Bomberman.UI/                # MonoGame client
│   ├── Scenes/                  # Login, Lobby, Game, Leaderboard
│   ├── View/                    # Rendering logic
│   └── Content/                 # Textures, fonts, audio
│
├── Bomberman.Services/          # Infrastructure
│   ├── Network/                 # SignalR client wrapper
│   └── Data/                    # SQLite repository
│
├── Bomberman.Server/            # SignalR Hub
│   └── GameHub.cs               # Multiplayer coordination
│
├── Shared/                      # DTOs
│   └── DTOs/                    # Network message models
│
├── DESIGN_DOCUMENT.md           # 📘 Detailed design documentation
└── README.md                    # This file
```

---

## 📚 Documentation

- **[DESIGN_DOCUMENT.md](DESIGN_DOCUMENT.md)** - Comprehensive design patterns explanation, UML diagrams, and architecture
- **Code Comments** - Inline documentation throughout the codebase

---

## 🛠️ Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 8.0 | Core framework |
| **C#** | 12.0 | Programming language |
| **MonoGame** | 3.8.4.1 | Game engine |
| **SignalR** | Latest | Real-time networking |
| **SQLite** | 3.x | Local database |
| **ASP.NET Core** | 8.0 | Server hosting |

---

## 🏆 Features Summary

| Category | Feature | Status |
|----------|---------|--------|
| **Multiplayer** | 2-player online | ✅ |
| **Maps** | 3 themes | ✅ |
| **Walls** | 3 types (including hard walls) | ✅ |
| **Power-ups** | 3 types | ✅ |
| **Enemies** | 3 AI behaviors | ✅ |
| **Database** | User stats, leaderboard, preferences | ✅ |
| **Design Patterns** | 9 patterns (4 required) | ✅ |
| **Authentication** | Login/Register | ✅ |

---

## 👨‍💻 Development

### Database Schema

The game uses SQLite with the following schema:

```sql
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT UNIQUE NOT NULL,
    PasswordHash TEXT,
    Wins INTEGER DEFAULT 0,
    Losses INTEGER DEFAULT 0,
    TotalGames INTEGER DEFAULT 0,
    Kills INTEGER DEFAULT 0,
    PreferredTheme TEXT DEFAULT 'Forest',
    CreatedAt TEXT
);
```

Database location: `Bomberman.Server/bomberman.db`

### Network Protocol

The game uses SignalR for real-time communication:

**Client → Server:**
- `Login(username, password)`
- `Register(username, password)`
- `JoinLobby(username, theme)`
- `SendMovement(PlayerStateDTO)`
- `PlaceBomb(BombDTO)`
- `ReportDeath(username)`

**Server → Client:**
- `GameStarted(GameStartDTO)`
- `ReceiveMovement(PlayerStateDTO)`
- `ReceiveBombPlacement(BombDTO)`
- `PlayerEliminated(username)`

---

## 📝 Assignment Requirements

✅ **All requirements met:**

- [x] Minimum 4 design patterns (9 implemented)
- [x] At least 2 behavioral patterns (3 implemented)
- [x] At least 2 creational patterns (3 implemented)
- [x] At least 2 structural patterns (2 implemented)
- [x] Multiplayer functionality
- [x] Multiple map themes
- [x] Hard walls (2-hit destruction)
- [x] Database integration
- [x] User authentication
- [x] Comprehensive design document

**Bonus Features Implemented:**
- +5 Professional UI/UX design
- +5 Additional design patterns

---

## 📄 License

This project was created as an assignment for the Object-Oriented Design Patterns course.

---

## 🙏 Acknowledgments

- **MonoGame** - Cross-platform game framework
- **SignalR** - Real-time web functionality
- **SQLite** - Embedded database

---

**Made with ❤️ by Sude Dincer**

*For detailed pattern explanations and UML diagrams, see [DESIGN_DOCUMENT.md](DESIGN_DOCUMENT.md)*
