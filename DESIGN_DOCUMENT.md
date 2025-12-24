# Bomberman Multiplayer - Design Document

**Student:** Sude Dincer  
**Date:** December 24, 2024  
**Course:** Object-Oriented Design Patterns

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [System Architecture](#system-architecture)
3. [Design Patterns](#design-patterns)
4. [Database Design](#database-design)
5. [Technology Stack](#technology-stack)
6. [Multiplayer Architecture](#multiplayer-architecture)

---

## Executive Summary

This document describes the design and implementation of a real-time multiplayer Bomberman game built using .NET 8, MonoGame, and SignalR. The project demonstrates the application of **9 design patterns** across behavioral, creational, and structural categories, exceeding the requirement of 4 patterns.

### Key Features
- Two-player online multiplayer gameplay
- Three distinct map themes (City, Desert, Forest)
- Multiple wall types including hard walls requiring 2 hits
- Power-up system with 3 types
- Enemy AI with 3 different behaviors
- Persistent player statistics and leaderboard
- User authentication system

---

## System Architecture

### Module Structure

The project follows a modular architecture with clear separation of concerns:

```
Bomberman/
├── Bomberman.Core/          # Game logic, entities, patterns
├── Bomberman.UI/            # MonoGame rendering, scenes
├── Bomberman.Services/      # Networking, database
├── Bomberman.Server/        # SignalR Hub host
└── Shared/                  # DTOs, shared models
```

### Dependency Flow

```
UI → Services → Core
     ↓
   Server (SignalR Hub)
```

**Key Design Decisions:**
- **Core** contains no external dependencies - pure game logic
- **Services** handles I/O (network, database)
- **UI** orchestrates game flow and rendering
- **Server** is a standalone ASP.NET Core host

---

## Design Patterns

This project implements **9 design patterns**, demonstrating mastery across all three GoF categories plus architectural patterns.

### Behavioral Patterns (3/3)

#### 1. Strategy Pattern ⭐

**Purpose:** Enable dynamic selection of enemy AI behaviors at runtime.

**Implementation:**

```
IMovementStrategy (Interface)
├── StaticMovement      # Enemy doesn't move
├── ChasingMovement     # Follows nearest player
└── RandomMovement      # Moves randomly
```

**Code Location:** `Bomberman.Core/Patterns/Behavioral/Strategy/`

**Usage:**
```csharp
public class Enemy
{
    private IMovementStrategy _movementStrategy;
    
    public void SetStrategy(IMovementStrategy strategy)
    {
        _movementStrategy = strategy;
    }
    
    public void Update(GameTime gameTime)
    {
        _movementStrategy.Move(this, gameTime);
    }
}
```

**Benefits:**
- Easy to add new AI behaviors without modifying Enemy class
- Behaviors can be changed at runtime
- Clear separation of movement logic

---

#### 2. Observer Pattern ⭐

**Purpose:** Notify multiple subscribers when game events occur (explosions, power-ups, player deaths).

**Implementation:**

```
GameMap (Subject)
├── OnExplosion event
├── OnPowerUpSpawned event
└── Observers: GameScene, NetworkClient
```

**Code Location:** `Bomberman.Core/GameLogic/GameMap.cs`

**Usage:**
```csharp
// Publisher
public class GameMap
{
    public event Action<int, int, int> OnExplosion;
    
    private void TriggerExplosion(int x, int y, int power)
    {
        OnExplosion?.Invoke(x, y, power);
    }
}

// Subscriber
public class GameScene
{
    _map.OnExplosion += HandleExplosion;
    
    private void HandleExplosion(int x, int y, int power)
    {
        // Play animation, send to network
    }
}
```

**Benefits:**
- Loose coupling between game logic and presentation
- Multiple systems can react to same event
- Easy to add new observers

---

#### 3. State Pattern ⭐

**Purpose:** Manage player states (Normal, Powered, Invincible) with different behaviors.

**Implementation:**

```
IPlayerState (Interface)
├── NormalState         # Default state
├── PoweredState        # Enhanced abilities
└── InvincibleState     # Temporary immunity
```

**Code Location:** `Bomberman.Core/Patterns/Behavioral/State/`

**Usage:**
```csharp
public class Player
{
    private IPlayerState _state = new NormalState();
    
    public void ChangeState(IPlayerState newState)
    {
        _state = newState;
    }
    
    public void TakeDamage()
    {
        _state.HandleDamage(this); // Behavior changes per state
    }
}
```

**Benefits:**
- State-specific behavior encapsulated in state classes
- Easy to add new states
- Eliminates complex conditional logic

---

### Creational Patterns (3/2 Required)

#### 4. Abstract Factory Pattern ⭐

**Purpose:** Create families of related theme objects (walls, backgrounds, sprites) without specifying concrete classes.

**Implementation:**

```
IThemeFactory (Interface)
├── CityThemeFactory      # Urban theme assets
├── DesertThemeFactory    # Desert theme assets
└── ForestThemeFactory    # Forest theme assets
```

**Code Location:** `Bomberman.Core/Patterns/Creational/AbstractFactory/`

**Usage:**
```csharp
public interface IThemeFactory
{
    Texture2D CreateBackground();
    Texture2D CreateWallTexture();
    Texture2D CreateFloorTexture();
}

// Usage
IThemeFactory factory = new DesertThemeFactory();
var background = factory.CreateBackground();
var walls = factory.CreateWallTexture();
```

**Benefits:**
- Ensures theme consistency (all assets from same family)
- Easy to add new themes
- Client code doesn't depend on concrete factories

---

#### 5. Builder Pattern ⭐

**Purpose:** Construct complex GameMap objects step-by-step with different configurations.

**Implementation:**

```
IMapBuilder (Interface)
└── ClassicMapBuilder
    ├── SetDimensions()
    ├── PlaceWalls()
    ├── SpawnEnemies()
    ├── AddPowerUps()
    └── Build() → GameMap
```

**Code Location:** `Bomberman.Core/Patterns/Creational/Builder/`

**Usage:**
```csharp
GameMap map = new ClassicMapBuilder(factory)
    .SetDimensions(15, 13)
    .PlaceWalls(seed)
    .SpawnEnemies(3)
    .AddPowerUps()
    .Build();
```

**Benefits:**
- Separates map construction from representation
- Same builder process can create different representations
- More readable than complex constructors

---

#### 6. Factory Method Pattern ⭐

**Purpose:** Delegate wall object creation to subclasses based on wall type.

**Implementation:**

```
Wall (Abstract Base)
├── UnbreakableWall  # Cannot be destroyed
├── BreakableWall    # Destroyed in 1 hit
└── HardWall         # Requires 2 hits
```

**Code Location:** `Bomberman.Core/Walls/`

**Usage:**
```csharp
public abstract class Wall
{
    public abstract bool CanBeDestroyed();
}

// Factory method in MapBuilder
private Wall CreateWall(WallType type)
{
    return type switch
    {
        WallType.Unbreakable => new UnbreakableWall(),
        WallType.Breakable => new BreakableWall(),
        WallType.Hard => new HardWall(hitsRequired: 2),
    };
}
```

**Benefits:**
- Eliminates tight coupling to concrete wall classes
- Easy to add new wall types
- Centralizes wall creation logic

---

### Structural Patterns (2/2 Required)

#### 7. Decorator Pattern ⭐

**Purpose:** Dynamically add power-up abilities to players without modifying their class.

**Implementation:**

```
IPlayer (Component)
├── BasePlayer (Concrete)
└── PowerUpDecorator (Decorator)
    ├── SpeedBoostDecorator
    ├── BombCountDecorator
    └── BombPowerDecorator
```

**Code Location:** `Bomberman.Core/Patterns/Structural/Decorator/`

**Usage:**
```csharp
IPlayer player = new BasePlayer();

// Add power-ups dynamically
player = new SpeedBoostDecorator(player);        // Speed +1
player = new BombCountDecorator(player);         // +1 bomb
player = new BombPowerDecorator(player);         // Explosion range +1

// Player now has all 3 power-ups without modification
```

**Benefits:**
- Abilities can be added/removed at runtime
- Multiple decorators can be stacked
- Open-Closed Principle: extend without modifying

---

#### 8. Adapter Pattern ⭐

**Purpose:** Wrap SignalR HubConnection with a domain-specific interface for easier testing and abstraction.

**Implementation:**

```
IGameClient (Target Interface)
└── SignalRGameClient (Adapter)
    └── HubConnection (Adaptee)
```

**Code Location:** `Bomberman.Services/Network/GameClient.cs`

**Usage:**
```csharp
public interface IGameClient
{
    Task<bool> LoginAsync(string username, string password);
    Task JoinLobbyAsync(string username, string theme);
    event Action<PlayerStateDTO> MovementReceived;
}

// Adapter wraps SignalR complexity
public class SignalRGameClient : IGameClient
{
    private HubConnection _connection;
    
    public async Task<bool> LoginAsync(string u, string p)
    {
        return await _connection.InvokeAsync<bool>("Login", u, p);
    }
}
```

**Benefits:**
- Abstracts SignalR implementation details
- Makes networking layer testable
- Easy to swap networking library

---

### Architectural Pattern

#### 9. Repository Pattern ⭐

**Purpose:** Abstract database access behind a clean interface, enabling testability and data source flexibility.

**Implementation:**

```
IUserRepository (Interface)
└── SqliteUserRepository (Concrete)
    └── SQLite Database
```

**Code Location:** `Bomberman.Services/Data/`

**Usage:**
```csharp
public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> AddUserAsync(User user);
    Task<bool> UpdateStatsAsync(string username, bool isWin);
    Task<IEnumerable<User>> GetTopPlayersAsync(int count);
}

// Injected into GameHub
public class GameHub : Hub
{
    private readonly IUserRepository _userRepository;
    
    public async Task ReportDeath(string username)
    {
        await _userRepository.UpdateStatsAsync(username, false);
    }
}
```

**Benefits:**
- Database logic separated from business logic
- Easy to switch from SQLite to SQL Server
- Repository can be mocked for testing

---

## Database Design

### Entity-Relationship Diagram

```
┌─────────────────┐
│     Users       │
├─────────────────┤
│ Id (PK)         │
│ Username        │──┐ Unique
│ PasswordHash    │  │
│ Wins            │  │ Statistics
│ Losses          │  │
│ TotalGames      │  │
│ Kills           │──┘
│ PreferredTheme  │─── References: City/Desert/Forest
│ CreatedAt       │
└─────────────────┘
```

### Schema Definition

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

### Database Operations

| Operation | Method | Purpose |
|-----------|--------|---------|
| **Create** | `AddUserAsync()` | Register new user |
| **Read** | `GetByUsernameAsync()` | Authentication, profile |
| **Update** | `UpdateStatsAsync()` | Record game results |
| **Update** | `UpdateKillsAsync()` | Track enemy kills |
| **Update** | `UpdatePreferencesAsync()` | Save theme choice |
| **Read** | `GetTopPlayersAsync()` | Leaderboard |

---

## Technology Stack

### Core Technologies

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Game Engine** | MonoGame | 3.8.4.1 | Rendering, input, game loop |
| **Framework** | .NET | 8.0 | Core runtime |
| **Networking** | SignalR Core | Latest | Real-time multiplayer |
| **Database** | SQLite | 3 | Local data persistence |
| **Language** | C# | 12.0 | Primary language |

### Project Structure

```
Bomberman.Core          - Game logic (no dependencies)
Bomberman.UI            - MonoGame client
Bomberman.Services      - Network + DB
Bomberman.Server        - SignalR Hub (ASP.NET Core)
Shared                  - DTOs
```

### Key Libraries

- **Microsoft.AspNetCore.SignalR.Client** - Client-side SignalR
- **Microsoft.Data.Sqlite** - Database access
- **MonoGame.Framework.DesktopGL** - Cross-platform MonoGame

---

## Multiplayer Architecture

### Client-Server Model

```
┌──────────┐         ┌──────────┐         ┌──────────┐
│ Client 1 │◄───────►│  Server  │◄───────►│ Client 2 │
│ (Player) │  SignalR│ (Hub)    │ SignalR │ (Player) │
└──────────┘         └──────────┘         └──────────┘
     │                     │                     │
     └─────── Deterministic Map (Same Seed) ────┘
```

### Sequence Diagram: Game Start

```
Player1          GameHub          Player2
   │                │                │
   │──JoinLobby────►│                │
   │◄─LobbyUpdate───│                │
   │                │◄──JoinLobby────│
   │◄─LobbyUpdate───┼──LobbyUpdate──►│
   │                │                │
   │   (2 players ready)             │
   │                │                │
   │◄─GameStarted───┼──GameStarted──►│
   │  (Seed: 12345) │  (Seed: 12345) │
   │                │                │
   └──Both players generate identical map──┘
```

### Synchronization Strategy

**Problem:** Keep two game instances in sync over network.

**Solution:**
1. **Seed-based Map Generation** - Same seed = same map
2. **Input Synchronization** - Send player actions, not state
3. **Server Authority** - Hub validates and broadcasts

**Events Synchronized:**
- Player movement
- Bomb placement & explosion
- Power-up spawn & collection
- Enemy movement
- Player death

### Network Protocol

```csharp
// From Client to Server
SendMovement(PlayerStateDTO)
PlaceBomb(BombDTO)
CollectPowerUp(PowerUpDTO)
ReportDeath(username)

// From Server to Clients
ReceiveMovement(PlayerStateDTO)
ReceiveBombPlacement(BombDTO)
ReceivePowerUpSpawn(PowerUpDTO)
PlayerEliminated(username)
```

---

## Conclusion

This Bomberman multiplayer game demonstrates comprehensive application of object-oriented design patterns across all three GoF categories:

- **3 Behavioral Patterns** - Strategy, Observer, State
- **3 Creational Patterns** - Abstract Factory, Builder, Factory Method
- **2 Structural Patterns** - Decorator, Adapter
- **1 Architectural Pattern** - Repository

The modular architecture ensures separation of concerns, testability, and maintainability. The multiplayer system uses SignalR for real-time communication with deterministic map generation for consistency.

**Total Design Patterns Implemented: 9** (exceeding requirement of 4)

---

*End of Design Document*
