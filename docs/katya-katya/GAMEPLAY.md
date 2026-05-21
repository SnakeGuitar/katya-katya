# Katya Katya - Game Systems & Mechanics

## Overview
[Game loop, core mechanics, progression model]

## Systems

### 1. Affinity System
- **How affinity is earned**: [Rewards from games, activities]
- **Affinity tiers**: [Level 0-20, 21-50, etc. and what unlocks]
- **Visual feedback**: [Progress bars, unlock notifications]

### 2. Currency System
- **Currency name**: [What players earn - coins, hearts, etc.]
- **Generation methods**: [Game rewards, idle, clicks (phase 2-3)]
- **Spending**: [What currency buys: gifts, cosmetics, etc.]
- **Earning formula**: [coins = difficulty * pairsMatched + time bonuses?]

### 3. Gift System
- **Send Gifts to Character**: Player spends coins to gift items → affinity boost
- **Receive Gifts from Character**: Character sends gifts back on level-up milestones → rewards/coins
- **Gift Types**: Flowers, chocolate, letters, etc. (cosmetic with different affinity multipliers)
- **Gift Log**: Visual history of exchanges with each character

### 4. Game Integration
- **SinglePlayer → Dating**: [How a game completion triggers affinity/coins]
- **Progress Persistence**: [Server storage, client caching]
- **Offline Handling**: [What happens if no connection?]

### 5. Progression Model (MVP)
- **Phase 1 (MVP)**: Game rewards only (coins + affinity from SinglePlayer)
- **Phase 2**: Manual clicker (tap button for coins)
- **Phase 3**: Idle generation (passive income over time)
- **Phase 4+**: Gift shop, gift interactions, dialogue tiers, cosmetics

### 6. UI Navigation
- **Dating Hub**: Character gallery, affinity overview
- **Character Detail**: Specific character progress, unlock notifications
- **Gift Shop** (future): Spend coins on gifts/cosmetics
- **Dialogue** (future): Story unlocks tied to affinity

### 7. Data Model (Code-First EF Core)

Entity definitions generate the database schema via EF Core migrations.

```csharp
// Enhance existing User entity
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    // ... existing fields
    public int Coins { get; set; } = 0;
    public ICollection<CharacterAffinity> Affinities { get; set; } = new List<CharacterAffinity>();
    public ICollection<Gift> GiftsSent { get; set; } = new List<Gift>();
}

// New: Character metadata
public class Character
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string AssetBaseId { get; set; }
    public ICollection<CharacterAffinity> Affinities { get; set; } = new List<CharacterAffinity>();
    public ICollection<Gift> GiftsReceived { get; set; } = new List<Gift>();
}

// New: User-Character relationship with progression
public class CharacterAffinity
{
    public int UserId { get; set; }
    public int CharacterId { get; set; }
    public int AffinityPoints { get; set; } = 0;
    public int UnlockedLevel { get; set; } = 0;
    
    public User User { get; set; }
    public Character Character { get; set; }
}

// New: Gift log (user → character or character → user)
public class Gift
{
    public int Id { get; set; }
    public int? SenderId { get; set; }
    public int CharacterId { get; set; }
    public string GiftType { get; set; }
    public int AffinityBoost { get; set; } = 0;
    public DateTime SentAt { get; set; }
    
    public User? Sender { get; set; }
    public Character Character { get; set; }
}
```

**EF Core Configuration**:
- Composite key: `(UserId, CharacterId)` for `CharacterAffinity`
- Foreign keys with cascade delete on User/Character
- Index on `CharacterId` for efficient queries
- Snake_case naming convention (via `EFCore.NamingConventions`)

**Migration**: Run `dotnet ef migrations add AddDatingSystem`

### 8. API Endpoints

**Core endpoints**:
- `POST /api/dating/complete-game` — Award coins/affinity from SinglePlayer completion
- `GET /api/dating/user-affinities` — Fetch all character progress
- `GET /api/dating/character/:characterId` — Character metadata and unlock thresholds
- `POST /api/dating/send-gift` — Send gift to character (costs coins, boosts affinity)
- `GET /api/dating/character/:characterId/gift-log` — View gift history
