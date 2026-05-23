# API Specification - Dating System

## Endpoints

### POST /api/dating/complete-game
Triggered on SinglePlayer game completion. Awards coins.

**Request**:
```json
{
  "difficulty": 1,
  "pairsMatched": 8,
  "elapsedSeconds": 120
}
```

**Response** (200):
```json
{
  "coinsEarned": 80,
  "newCoinBalance": 250
}
```

### GET /api/dating/user-affinities
Fetch all characters with current Love Points and Coins.

**Response** (200):
```json
{
  "coins": 250,
  "characters": [
    {
      "characterId": 1,
      "characterName": "Katya",
      "assetBaseId": "katya-1",
      "lovePoints": 25,
      "level": 1,
      "nextLevelThreshold": 50,
      "levelDescription": "Acquainted",
      "unlockedDialogues": ["intro", "morning-greeting"]
    }
  ]
}
```

### POST /api/dating/go-on-date
Spend coins to go on a date, gain Love Points.

**Request**:
```json
{
  "characterId": 1,
  "coinsCost": 100
}
```

**Response** (200):
```json
{
  "success": true,
  "lovePointsGained": 20,
  "newLovePoints": 45,
  "newCoins": 150,
  "dialogueUnlocked": "date-dialogue-1",
  "characterDialogue": "I had a wonderful time with you..."
}
```

### GET /api/dating/character/:characterId
Character metadata and Love Point unlock thresholds.

**Response** (200):
```json
{
  "characterId": 1,
  "name": "Katya",
  "assetBaseId": "katya-1",
  "loveLevelThresholds": [
    { "level": 0, "minLovePoints": 0, "description": "Stranger", "dialogues": [] },
    { "level": 1, "minLovePoints": 50, "description": "Acquainted", "dialogues": ["intro", "first-date"] },
    { "level": 2, "minLovePoints": 100, "description": "Friend", "dialogues": ["intro", "first-date", "second-date"] },
    { "level": 5, "minLovePoints": 250, "description": "Romantic Interest", "dialogues": [...] }
  ]
}
```

### POST /api/dating/send-gift
Send a gift to a character (costs coins, boosts Love Points).

**Request**:
```json
{
  "characterId": 1,
  "giftType": "flower"
}
```

**Response** (200):
```json
{
  "success": true,
  "coinsSpent": 25,
  "lovePointsGained": 5,
  "newLovePoints": 50,
  "newCoins": 225,
  "characterResponse": "Oh, flowers! How sweet... *blushes*",
  "leveledUp": false
}
```

### GET /api/dating/character/:characterId/gift-log
View all gifts sent to a character.

**Response** (200):
```json
{
  "gifts": [
    {
      "giftId": 1,
      "giftType": "flower",
      "lovePointsBoost": 5,
      "sentAt": "2026-05-21T10:30:00Z",
      "characterResponse": "Oh, flowers!..."
    },
    {
      "giftId": 2,
      "giftType": "chocolate",
      "lovePointsBoost": 10,
      "sentAt": "2026-05-21T15:00:00Z",
      "characterResponse": "My favorite..."
    }
  ]
}
```

## Error Handling

All errors follow the standard error contract from `ExceptionHandlingMiddleware`:
- **422 Unprocessable Entity**: Validation errors
  ```json
  {
    "errors": {
      "characterId": ["Character not found"],
      "giftType": ["Invalid gift type"]
    }
  }
  ```
- **400 Bad Request**: Domain errors
  ```json
  {
    "errorCode": "INSUFFICIENT_COINS",
    "message": "You don't have enough coins to send this gift"
  }
  ```
- **500 Internal Server Error**: Unhandled exceptions

## Authentication
All endpoints require JWT Bearer authentication via Authorization header or query param `access_token` for WebSocket upgrades.
