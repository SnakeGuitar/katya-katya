# Katya Katya - Game Systems & Mechanics

## Overview

**Katya Katya** is a dating simulator with idle clicker mechanics. Players engage in memory card games to earn currency, which they spend on dates and gifts to increase affinity with Katya. As affinity increases, new dialogues and story scenes unlock, revealing Katya's character and deepening the romantic storyline.

**Core Loop**:
1. Play memory games → Earn coins
2. Spend coins on dates/gifts → Gain affinity
3. Affinity unlocks dialogues and story scenes
4. Progress toward romance and relationship milestones

---

## Systems

### 1. Love Points System (Affinity)

**What are Love Points?**
- Measure of Katya's affection toward the player
- Range: 0-100 levels
- Higher levels unlock romantic scenes and dialogue

**How Love Points are Earned**:
- **Dates**: Spend coins to take Katya on a date → +10-30 Love Points
- **Gifts**: Send gifts to Katya → +5-20 Love Points (varies by gift type)
- **Dialogue choices**: Choose romantic dialogue options → +5-10 Love Points
- **Milestones**: Unlock special scenes at level thresholds

**Love Point Tiers** (100 total levels):
- Each 10-level milestone unlocks new dialogues/scenes
- Examples:
  - Levels 0-10: Introduction, first date
  - Levels 11-20: Getting to know you
  - Levels 51-60: First kiss / romantic confession
  - Levels 91-100: Endgame / relationship conclusion

**Visual Feedback**:
- Progress bar: Current Love Points / Points needed for next level
- Text: "Love Level X / Next level at Y points"
- Notifications on milestone unlocks

### 2. Coins System (Currency)

**What are Coins?**
- Conventional in-game currency
- Used to purchase dates, gifts, and other activities
- Earned by playing memory games

**How Coins are Generated** (MVP):
- **Memory games**: Win games to earn coins
  - Formula: `coins = difficulty * pairs_matched + time_bonus`
  - Easy: ~10-20 coins per game
  - Medium: ~20-40 coins per game
  - Hard: ~40-60 coins per game

**Future Methods** (Phase 2-3):
- Manual clicker (tap button for coins)
- Idle generation (passive income over time)

**Coin Spending**:
- **Date with Katya**: 50-200 coins → +10-30 Love Points
- **Gifts**: 25-100 coins per gift → +5-20 Love Points + gift dialogue
- **Future**: Cosmetics, special items, shop purchases

### 3. Gift System

**Send Gifts to Katya**:
- Player spends coins to send a gift
- Katya receives gift and responds (dialogue)
- Affinity boost: 5-15 points depending on gift type
- Character response varies by gift type and current affinity level

**Gift Types** (Examples):
- Flower (25 coins, 5 affinity)
- Chocolate (50 coins, 10 affinity)
- Letter/Love Note (75 coins, 15 affinity)
- [Other gifts as defined]

**Gift Log**:
- View all gifts sent to Katya
- Katya's reactions and dialogue tied to gifts
- Milestone: "First gift sent", "10 gifts sent", etc.

### 4. Game Integration

**SinglePlayer → Dating Loop**:
1. Play memory games → Earn Coins
2. Spend Coins on Dates/Gifts → Gain Love Points
3. Love Points unlock Story & Dialogue → Progress relationship

**Progress Persistence**:
- Server stores: User Coins, Love Points, affinity level, unlocked dialogues
- Client caches: Coins balance, Love Points progress (syncs on refresh)
- Continues across sessions

**Offline Handling**:
- Games can be played offline (no server required during gameplay)
- Coins/Love Points changes sync when connection resumes

### 5. Progression Model (MVP)

**Phase 1 (MVP)**: Memory game rewards
- **Coins**: Earned from SinglePlayer memory games
- **Love Points**: Earned by spending Coins on dates/gifts with Katya
- **Story**: Unlocks at Love Point milestones (every 10 levels)

**Phase 2**: Manual clicker
- **Coins**: Tap button to earn coins (supplement game rewards)
- **Love Points**: Unchanged

**Phase 3**: Idle generation
- **Coins**: Automatic generation when app is closed
- **Multiplier**: Tied to highest Love Point level achieved
- **Love Points**: Unchanged

**Phase 4+**: Shop, cosmetics, events, etc.

### 6. UI Navigation

**Dating Hub**:
- Shows Katya (portrait + name)
- **Resource bars** (top):
  - Coins balance: 💰 [amount]
  - Love Points: ❤️ [current] / [next level threshold]
- Love Points progress bar (visual)
- Action buttons: "Send Gift", "Go on Date"
- Button to play memory games (earn coins)

**Character Detail**:
- Full portrait of Katya
- Name, Love Point level, level description
- Love Points progress: [current] / 100
- Unlock history: List of unlocked dialogues at each level
- Gift history: Recent gifts sent
- Relationship milestones achieved

**Game Completion Modal**:
- Displayed after each memory game
- Shows: 💰 Coins earned: [amount]
- Coins total: [new balance]
- Quick stats and celebration

---

## 7. Data Model (Code-First EF Core)

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

// Character metadata
public class Character
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string AssetBaseId { get; set; }  // e.g., "katya-1"
    public ICollection<CharacterAffinity> Affinities { get; set; } = new List<CharacterAffinity>();
    public ICollection<Gift> GiftsReceived { get; set; } = new List<Gift>();
}

// User-Character relationship with progression
public class CharacterAffinity
{
    public int UserId { get; set; }
    public int CharacterId { get; set; }
    public int LovePoints { get; set; } = 0;       // Current Love Points
    public int Level { get; set; } = 0;             // Current level (0-100, never decreases)
    public string Mood { get; set; } = "Happy";     // "Happy" or "Upset"
    public int PenaltyCount { get; set; } = 0;      // Penalties in current 7-day window
    public DateTime? LastInteraction { get; set; }   // For inactivity tracking
    
    public User User { get; set; }
    public Character Character { get; set; }
}

// Gift log
public class Gift
{
    public int Id { get; set; }
    public int SenderId { get; set; }  // User sending the gift
    public int CharacterId { get; set; }  // Character receiving
    public string GiftType { get; set; }  // "flower", "chocolate", "letter"
    public int AffinityBoost { get; set; }
    public DateTime SentAt { get; set; }
    
    public User Sender { get; set; }
    public Character Character { get; set; }
}
```

**EF Core Configuration**:
- Composite key: `(UserId, CharacterId)` for `CharacterAffinity`
- Foreign keys with cascade delete
- Index on `CharacterId` for efficient queries
- Snake_case naming convention

**Migration**: `dotnet ef migrations add AddDatingSystem`

---

## 8. API Endpoints

### Core Endpoints

**POST /api/dating/complete-game**
- Award coins from SinglePlayer game completion
- Input: `{ difficulty, pairsMatched, elapsedSeconds }`
- Output: `{ coinsEarned, newCoinBalance }`

**GET /api/dating/user-affinities**
- Fetch all characters with current Love Points and Coins
- Output: `{ coins, characters: [{ characterId, name, lovePoints, level, nextLevelThreshold }] }`

**POST /api/dating/go-on-date**
- Spend coins to go on a date, gain Love Points
- Input: `{ characterId, coinsCost }`
- Output: `{ success, lovePointsGained, newLovePoints, newCoins, dialogueUnlocked }`

**GET /api/dating/character/:characterId**
- Character metadata and Love Point thresholds
- Output: Character details, level thresholds, unlocked dialogues

**POST /api/dating/send-gift**
- Send gift to character (costs coins, boosts Love Points)
- Input: `{ characterId, giftType }`
- Output: `{ success, coinsSpent, lovePointsGained, newCoins, newLovePoints, characterResponse }`

**GET /api/dating/character/:characterId/gift-log**
- View all gifts sent to a character
- Output: `{ gifts: [...] }`

---

## 9. Penalty & Mood System

### Core Rule
**Levels are permanent.** Once you unlock Love Level 10, you never lose it. However, Love Points *within* the current level can decrease, slowing progress to the next level.

### Penalty Triggers

| Trigger | Love Points Lost | Katya's Reaction |
|---------|-----------------|------------------|
| **Inactivity** (3+ days without playing) | -5 LP/day (caps at current level floor) | "I missed you... did you forget about me?" |
| **Bad dialogue choice** | -10 LP | Visible disappointment, sad expression |
| **Wrong gift** (gift she dislikes) | -5 LP | "Oh... thanks, I guess." (cold response) |
| **Abandoning a memory game** (quit mid-game) | -2 LP | "You gave up? That's not like you..." |
| **Ignoring a milestone event** (special date available but not taken within time window) | -15 LP | "I thought today was going to be special..." |

### Mood States

Katya has a **mood** that affects gameplay:

**Happy (default)**:
- Normal Love Points gain from dates/gifts
- Cheerful dialogue and warm expressions
- Standard gameplay

**Upset** (triggered after accumulating penalties):
- Dates and gifts give **50% reduced** Love Points
- Dialogue is colder, shorter, distant
- UI shows a visual cue (muted colors, different expression)
- Persists until reconciliation

**How to trigger Upset**: Accumulate 3+ penalty events within a 7-day window

### Reconciliation

When Katya is Upset, a special **"Make it up to her"** option appears:

- **Reconciliation date**: Free (no coin cost), but requires completing a harder memory game
- **Apology gift**: Special gift type only available during Upset state (costs 150 coins)
- **Dialogue event**: A unique reconciliation dialogue plays — emotional, vulnerable, exclusive content the player only sees if they messed up

**After reconciliation**:
- Mood returns to Happy
- Love Points gain returns to normal
- Player unlocks a small "reconciliation memory" in their log (collectible)

### Design Intent

The penalty system creates **emotional stakes without mechanical punishment**:
- You never lose unlocked content (scenes, dialogues, levels)
- Katya's *reactions* are the real consequence — her sadness hits harder than a number dropping
- Reconciliation events are **bonus content** — players who never trigger them miss exclusive dialogue
- This creates a natural tension: do I want to see reconciliation content? Or keep her happy?

---

## 10. Love Points Unlock Thresholds (MVP)

**Katya Story/Dialogue Unlocks** (at 10-level intervals):
- **Love Level 0-10**: Introduction, first date dialogue
- **Love Level 11-20**: Getting to know you, shared interests
- **Love Level 21-30**: Laughter and comfort with each other
- **Love Level 31-40**: Emotional vulnerability and trust
- **Love Level 41-50**: "I like you" moment, romantic tension
- **Love Level 51-60**: First kiss or romantic confession
- **Love Level 61-70**: Deepening commitment, exclusivity
- **Love Level 71-80**: Planning a future together
- **Love Level 81-90**: Relationship milestones and deepening love
- **Love Level 91-100**: Endgame scene / relationship conclusion

---

## 11. Engagement Systems

### Layer 1: Dopamine Loop (In-Session Engagement)

#### Variable Rewards
- **Coin range**: Memory games reward coins in a range (not fixed). Example: Easy = 10-25, Medium = 20-45, Hard = 40-75
- **Critical Match**: Random chance (~10%) that a card pair triggers "Critical Match" → x2 coins for that pair. Visual: golden glow + special sound
- **Rare drops**: ~5% chance after completing a game to receive a free gift item (appears in inventory, can be gifted to Katya later)
- **Rotating greetings**: Katya has a pool of 50+ greetings that rotate each time the player opens the Dating Hub. Never the same twice in a row

#### Streaks
- **Daily login streak**: Consecutive days played earn escalating bonuses
  - Day 1: +10 coins
  - Day 3: +25 coins
  - Day 5: +50 coins
  - Day 7: +100 coins + 10 Love Points + special Katya dialogue
  - Day 14: +200 coins + rare gift item
  - Day 30: +500 coins + exclusive scene unlock
- **Win streak**: Consecutive games won without losing multiply coin rewards
  - 2 wins: x1.2 multiplier
  - 5 wins: x1.5 multiplier
  - 10 wins: x2.0 multiplier
  - Losing resets the streak; Katya encourages: "Don't worry, let's try again!"
- **Streak recovery**: Missing one day doesn't break the streak (grace period). Missing 2+ days resets it

#### Satisfying Feedback
- **Card match sound**: Satisfying "click + chime" on successful pair match
- **Coin rain animation**: Coins visually fall/cascade when earned
- **Love Points bar**: Fills with smooth animation + sparkle particles on gain
- **Katya's expression**: Changes in real-time based on game performance (smiles on matches, worried on mistakes, celebrates on win)
- **Level-up celebration**: Full-screen animation + special sound + Katya congratulates you
- **Screen shake**: Subtle shake on Critical Match

---

### Layer 2: Retention Loop (Daily Return Incentives)

#### Daily System
- **Daily Gift from Katya**: Every day the player logs in, Katya offers a small gift
  - Weekdays: 15-30 coins
  - Weekends: Special item or 50 coins
  - Presentation: Katya holds the gift with unique dialogue ("I got you something today!")
- **Daily Challenge**: One special memory game per day with bonus conditions
  - Examples: "Beat it under 60 seconds", "No mistakes allowed", "Use only 3 flips per pair"
  - Reward: 2x normal coins + 5 bonus Love Points
  - Completing 7 daily challenges in a row: exclusive Katya dialogue
- **Rotating Dialogue**: Katya has contextual dialogue based on:
  - Day of the week ("Happy Friday! Any plans?")
  - Time of day ("Good morning!" / "Still up this late?")
  - Season/month (seasonal greetings, holiday references)
  - Recent player actions ("Thanks again for the flowers yesterday")

#### Mystery & Anticipation
- **Scene previews**: Before each Love Level milestone, show a blurred/silhouetted preview of the next scene. Player can see the shape but not the content → curiosity drives grinding
- **Hidden gift catalog**: Gift log shows "??? Undiscovered" for gifts not yet sent. Display: "Discovered 4/12 gifts" → completionist urge
- **Katya's hints**: She drops subtle hints about her preferences without being direct
  - "I love sweet things..." → hint to buy chocolate
  - "I saw the prettiest flowers today..." → hint to buy flowers
  - "I've been reading a lot lately..." → hint for a letter/book gift
  - Hints rotate based on what gifts the player hasn't tried yet
- **Hidden achievements**: Some achievements have "???" titles until unlocked. Players must experiment to discover them
- **Locked diary entries**: Katya's diary (unlockable) has entries corresponding to each Love Level. Locked ones show dates but no text → anticipation

#### Progression Visibility
- **Scene gallery**: Collection of all unlocked scenes/dialogues (viewable anytime)
  - Shows completion percentage: "12/25 scenes unlocked"
  - Locked scenes show a silhouette + Love Level required
- **Gift catalog**: Visual grid of all giftable items
  - Discovered vs. undiscovered
  - Katya's reaction rating for each (after first gift of that type)
- **Achievement board**: 
  - Categories: Dating, Memory Games, Gifts, Secrets, Streaks
  - Progress bars per category
  - Total completion percentage
- **Relationship stats**:
  - "Time together": Total days since first interaction
  - "Total dates": Number of dates taken
  - "Games played together": Total memory games completed
  - "Gifts given": Total gifts sent
  - "Favorite gift": Most-sent gift type

---

### Layer 3: Emotional Investment (Long-Term Attachment)

#### Katya as a Living Character
- **Memory system**: Katya remembers and references past interactions
  - "Remember when you gave me flowers last week? I still have them on my desk"
  - "We've been talking for 30 days now... time flies with you"
  - "You always pick the hardest memory games. I admire that about you"
- **Preference discovery**: Katya's likes/dislikes are revealed gradually through gameplay
  - Favorite gift type (discovered by trying different gifts)
  - Favorite time of day to chat
  - Topics she enjoys discussing (unlocked at higher Love Levels)
- **Dynamic reactions**: Katya responds to patterns in player behavior
  - Plays every day → "I love that you always come back"
  - Plays late at night → "Night owl like me, huh?"
  - Sends lots of flowers → "You know me so well... flowers again?"
  - Win streak → "You're on fire! I love watching you play"
- **Mood variation**: Independent of penalty system, Katya has random mood shifts
  - Some days she's extra cheerful (bonus Love Points from interactions)
  - Some days she's thoughtful/quiet (different dialogue, same rewards)
  - Rare: "Katya had a bad day" → special comfort dialogue option → bonus Love Points for being supportive
- **Milestone celebrations**: Katya celebrates player milestones
  - "We've been together 7 days!"
  - "You've played 50 memory games... you're dedicated"
  - "100 Love Points with me... does that mean what I think it means?"

#### Player Investment Escalation
- **Early game (Levels 0-20)**: Fast progression, easy games, lots of rewards. Hook the player quickly
  - Coins flow freely, Love Points accumulate fast
  - Katya is friendly, warm, encouraging
  - Unlocks come every few games
- **Mid game (Levels 21-50)**: Progression slows, but rewards get better
  - Games are harder, requiring more skill
  - Dates cost more but Love Points per date also increase
  - Katya's dialogue becomes more personal and vulnerable
  - The player has invested enough time to care about continuing
- **Late game (Levels 51-80)**: Strategic spending matters
  - Coins are harder to earn; player must choose between gifts and dates
  - Katya's scenes become intimate, emotional, exclusive
  - Completing the gallery/catalog becomes a secondary goal
- **Endgame (Levels 81-100)**: The emotional payoff
  - Katya's most vulnerable, honest, romantic dialogues
  - Final scenes that reward the entire journey
  - Sense of completion and emotional closure

#### Secrets & Easter Eggs
- **Katya's birthday**: Play on a specific date (TBD) → exclusive birthday scene, unique gift option, special dialogue. Only available once per year
- **Midnight dialogue**: Open the Dating Hub between 00:00-01:00 → "Can't sleep either? ... I'm glad you're here"
- **Gift spam**: Send the same gift 10 times consecutively → unique reaction ("Another one?! You really love [gift], huh? ...actually, I kind of love that about you")
- **Perfect game**: Complete a memory game with zero mistakes → special animation (fireworks/confetti) + "Wow, you're incredible!" dialogue + 2x coins
- **Speed run**: Complete a Hard game under 30 seconds → secret achievement + "That was insane! How did you do that?!"
- **First gift ever**: The very first gift you send Katya triggers a unique, unrepeatable dialogue ("For me? Really? No one's ever... thank you.")
- **100 games milestone**: After exactly 100 memory games, Katya says something special about your dedication
- **Holiday events** (if applicable): Christmas, Valentine's Day, etc. → special temporary dialogue + themed gifts
- **Konami code** (or similar input): Hidden in settings or Dating Hub → unlocks a silly/funny Katya dialogue or cosmetic

---

## Notes

- All Love Points/Coin values are tunable
- Gift types and costs can be expanded
- Difficulty formula for coins can be adjusted for balance
- Penalty thresholds (inactivity days, upset trigger count) are configurable
- Reconciliation content is optional but adds emotional depth
- Engagement system values (streak bonuses, drop rates, multipliers) should be playtested and balanced
- Easter egg dates (birthday, holidays) need to be defined in LORE.md
- MVP focuses on core loop; full engagement systems can be phased in incrementally
