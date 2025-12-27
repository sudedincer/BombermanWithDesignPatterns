# Bomberman Projesi Walkthrough

Bu dokümanda projenizin tüm özellikleri, design pattern'leri ve nasıl çalıştığı adım adım açıklanmıştır.

---

## 📁 Proje Yapısı

Projeniz 5 ana modülden oluşuyor:

```
Bomberman/
├── Bomberman.Core/              # ✅ Saf oyun mantığı
├── Bomberman.UI/                # ✅ MonoGame client
├── Bomberman.Services/          # ✅ Network + Database
├── Bomberman.Server/            # ✅ SignalR Hub
└── Shared/                      # ✅ DTOs
```

## 🎯 9 Tasarım Deseni - Lokasyonlar

### Behavioral Patterns (3)

#### 1. Strategy Pattern
**Dosya:** [Bomberman.Core/Patterns/Behavioral/Strategy/](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Behavioral/Strategy/)

**Dosyalar:**
- [IMovementStrategy.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Behavioral/Strategy/IMovementStrategy.cs)
- [StaticMovement.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Behavioral/Strategy/StaticMovement.cs)
- [ChasingMovement.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Behavioral/Strategy/ChasingMovement.cs)
- [RandomWalkMovement.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Behavioral/Strategy/RandomWalkMovement.cs)

**Kullanım:** Düşmanların 3 farklı hareket davranışı

#### 2. Observer Pattern
**Dosya:** [Bomberman.Core/Patterns/Behavioral/Observer/](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Behavioral/Observer/)

**Dosyalar:**
- [IExplosionObserver.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Behavioral/Observer/IExplosionObserver.cs)
- [IExplosionSubject.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Behavioral/Observer/IExplosionSubject.cs)

**Kullanım:** Bomba patlaması bildirimleri

#### 3. State Pattern
**Dosya:** [Bomberman.Core/Patterns/Behavioral/State/](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Behavioral/State/)

**Dosyalar:**
- [IPlayerState.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Behavioral/State/IPlayerState.cs)
- [AlivePlayerState.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Behavioral/State/AlivePlayerState.cs)
- [DeadPlayerState.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Behavioral/State/DeadPlayerState.cs)

**Kullanım:** Oyuncu durumları (Alive/Dead)

### Creational Patterns (3)

#### 4. Abstract Factory Pattern
**Dosya:** [Bomberman.Core/Patterns/Creational/](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Creational/)

**Dosyalar:**
- [IWallFactory.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Creational/IWallFactory.cs)
- [CityWallFactory.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Creational/CityWallFactory.cs)
- [DesertWallFactory.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Creational/DesertWallFactory.cs)
- [ForestWallFactory.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Creational/ForestWallFactory.cs)

**Kullanım:** 3 tema için tema-spesifik duvarlar

#### 5. Builder Pattern
**Dosya:** [Bomberman.Core/Patterns/Creational/Builder/](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Creational/Builder/)

**Dosyalar:**
- [IMapBuilder.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Creational/Builder/IMapBuilder.cs)
- [ClassicMapBuilder.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Creational/Builder/ClassicMapBuilder.cs)

**Kullanım:** Karmaşık harita oluşturma

#### 6. Factory Method Pattern
**Dosya:** [Bomberman.Core/Walls/](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Walls/)

**Dosyalar:**
- `Wall.cs` (abstract)
- `UnbreakableWall.cs`
- `BreakableWall.cs`
- `HardWall.cs` ⭐ (2 vuruş gerektiren!)

**Kullanım:** Duvar tipleri oluşturma

### Structural Patterns (2)

#### 7. Decorator Pattern
**Dosya:** [Bomberman.Core/Patterns/Structural/](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Structural/)

**Dosyalar:**
- [IPlayer.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Structural/IPlayer.cs)
- [PlayerDecorator.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Structural/PlayerDecorator.cs)
- [TimedPlayerDecorator.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Structural/TimedPlayerDecorator.cs)
- [SpeedBoostDecorator.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Structural/SpeedBoostDecorator.cs)
- [BombPowerDecorator.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Structural/BombPowerDecorator.cs)
- [ExtraBombDecorator.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Core/Patterns/Structural/ExtraBombDecorator.cs)

**Kullanım:** Power-up yetenekleri

#### 8. Adapter Pattern
**Dosya:** [Bomberman.Services/Network/](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Services/Network/)

**Dosyalar:**
- `IGameClient.cs` (target interface)
- `SignalRGameClient.cs` (adapter)

**Kullanım:** SignalR API wrapper

### Architectural Pattern (1)

#### 9. Repository Pattern
**Dosya:** [Bomberman.Services/Data/](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Services/Data/)

**Dosyalar:**
- [IUserRepository.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Services/Data/IUserRepository.cs)
- [SqliteUserRepository.cs](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/Bomberman.Services/Data/SqliteUserRepository.cs)

**Kullanım:** Veritabanı erişim katmanı

---

## 🎮 Projeyi Çalıştırma

### Adım 1: Server Başlat

```bash
cd /Users/sudedincer/Desktop/bomberman\ 2/Bomberman/Bomberman.Server
dotnet run
```

Server `http://localhost:5077` adresinde çalışacak.

### Adım 2: İlk Client'ı Başlat

```bash
cd /Users/sudedincer/Desktop/bomberman\ 2/Bomberman/Bomberman.UI
dotnet run
```

### Adım 3: İkinci Client'ı Başlat (Yeni Terminal)

```bash
cd /Users/sudedincer/Desktop/bomberman\ 2/Bomberman/Bomberman.UI
dotnet run
```

### Adım 4: Oyuna Giriş

1. Her iki client'ta da **Register** veya **Login** yapın
2. **İlk oyuncu** tema seçsin (City, Desert, veya Forest)
3. **Her iki oyuncu** "JOIN MATCH" butonuna tıklasın
4. Oyun otomatik başlar!

---

## 🎯 Design Pattern'leri Test Etme

### Strategy Pattern'i Görmek

Oyun başladığında düşmanlara bakın:
- Bazıları **yatay** gidip geliyor (StaticMovement)
- Bazıları **oyuncuyu takip ediyor** (ChasingMovement)
- Bazıları **rastgele** hareket ediyor (RandomWalkMovement)

### Observer Pattern'i Görmek

Bomba koyun ve bekleyin:
- Bomba patladığında **harita güncellenir**
- **Oyuncular** hasar alır
- **Düşmanlar** ölür
- **Tüm bunlar** Observer pattern sayesinde otomatik bildirim alıyor

### State Pattern'i Görmek

Karakterinizi bomba ile öldürün:
- **Alive** durumunda hareket edebiliyorsunuz
- Patlama aldığınızda **Dead** durumuna geçer
- **Dead** durumunda artık hareket edemezsiniz

### Abstract Factory Pattern'i Görmek

Farklı temaları deneyin:
- **City:** Beton ve tuğla duvarlar
- **Desert:** Taş ve kumtaşı duvarlar
- **Forest:** Ağaç ve ahşap sandık duvarlar

Her tema kendi factory'sini kullanıyor.

### Builder Pattern'i Görmek

Oyun başladığında harita oluşturulur:
1. `SetSize(15, 13)` - Boyut belirlenir
2. `PlaceBorders()` - Kenarlara duvarlar konur
3. `AddBreakableWalls()` - İçe duvarlar eklenir

Bu adımlar ClassicMapBuilder tarafından yürütülür.

### Factory Method - Hard Wall'u Görmek

Haritada **turuncu/sarı** renkli duvarları bulun:
- **İlk vuruşta:** Renk koyulaşır (hasar almış)
- **İkinci vuruşta:** Yıkılır

Bu Hard Wall özelliğidir!

### Decorator Pattern'i Görmek

Power-up toplayın:
- ⚡ **Speed Boost:** Hızınız %50 artar (10 saniye)
- 💣 **Bomb Count:** +1 bomba koyabilirsiniz (10 saniye)
- 💥 **Bomb Power:** Patlamalar +1 kare daha uzağa gider (10 saniye)

Süre bitince otomatik geri döner (TimedPlayerDecorator).

### Adapter Pattern'i Görmek

Network haberleşmesi SignalRGameClient üzerinden:
```csharp
// UI sadece temiz interface'i görüyor
await _gameClient.LoginAsync(username, password);
await _gameClient.JoinLobbyAsync(username, theme);

// SignalR detayları gizli
```

### Repository Pattern'i Görmek

Oyunu bitirin, leaderboard'a bakın:
- Kazanan oyuncunun **Wins** sayısı artar
- Kaybeden oyuncunun **Losses** sayısı artar
- Her şey SqliteUserRepository üzerinden kaydedilir

---

## 📊 Pattern Özeti

| Pattern | Dosya Sayısı | Kod Satırı (yaklaşık) |
|---------|--------------|----------------------|
| Strategy | 4 | ~150 |
| Observer | 2 | ~50 |
| State | 3 | ~200 |
| Abstract Factory | 4 | ~120 |
| Builder | 2 | ~150 |
| Factory Method | 4 | ~100 |
| Decorator | 6 | ~250 |
| Adapter | 2 | ~200 |
| Repository | 2 | ~300 |
| **Toplam** | **29** | **~1520** |

---

## ✅ Doğrulama

### Tüm Pattern'ler Mevcut

- ✅ **Strategy:** IMovementStrategy interface'i ve 3 concrete implementation
- ✅ **Observer:** IExplosionObserver interface'i ve GameMap notifier
- ✅ **State:** IPlayerState interface'i ve AlivePlayerState/DeadPlayerState
- ✅ **Abstract Factory:** IWallFactory ve 3 tema factory'si
- ✅ **Builder:** IMapBuilder ve ClassicMapBuilder
- ✅ **Factory Method:** Wall abstract class ve 3 concrete wall
- ✅ **Decorator:** PlayerDecorator ve 3 power-up decorator
- ✅ **Adapter:** SignalRGameClient adapts HubConnection
- ✅ **Repository:** IUserRepository ve SqliteUserRepository

### Proje Derlenebilir

```bash
cd /Users/sudedincer/Desktop/bomberman\ 2/Bomberman
dotnet build
```

Başarılı: Build succeeded. 0 Warning(s). 0 Error(s).

### Multiplayer Çalışıyor

- ✅ Server başlıyor
- ✅ İki client bağlanabiliyor
- ✅ Tema senkronize ediliyor
- ✅ Harita seed ile aynı oluşuyor
- ✅ Player movement senkronize
- ✅ Bomb placement senkronize
- ✅ Stats kaydediliyor

### Hard Wall Özelliği

- ✅ HardWall sınıfı mevcut
- ✅ HitsRemaining = 2
- ✅ İlk TakeHit(): HitsRemaining = 1
- ✅ İkinci TakeHit(): IsDestroyed = true

---

## 🎓 Öğrenme Kaynakları

Yarınki sunumunuz için hazırladığım dokümanlara bakın:

1. **[study_guide.md](file:///Users/sudedincer/.gemini/antigravity/brain/85bab514-e47d-4bae-95eb-3567200a9c5e/study_guide.md)** - Kapsamlı öğrenme rehberi (her pattern detaylı)
2. **[cheat_sheet.md](file:///Users/sudedincer/.gemini/antigravity/brain/85bab514-e47d-4bae-95eb-3567200a9c5e/cheat_sheet.md)** - Son dakika kopya kağıdı
3. **[DESIGN_DOCUMENT.md](file:///Users/sudedincer/Desktop/bomberman%202/Bomberman/DESIGN_DOCUMENT.md)** - Resmi tasarım dokümanı

---

## 🎯 Sonuç

Bomberman Multiplayer projeniz **9 farklı tasarım deseni** içeren, gerçek zamanlı çok oyunculu bir oyun. Her pattern organik bir ihtiyaçtan doğmuş ve profesyonel bir şekilde uygulanmış.

**Başarılar! 🚀**
