# API Specification - Dating System

## Endpoints

### POST /api/dating/complete-game
Triggered on SinglePlayer game completion. Awards coins and affinity.

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
  "affinityUpdates": [
    {
      "characterId": 1,
      "characterName": "Katya",
      "affinityGained": 5,
      "newAffinityPoints": 25,
      "newLevel": 1,
      "leveledUp": false,
      "nextLevelThreshold": 50
    }
  ]
}
```

### GET /api/dating/user-affinities
Fetch all characters with current affinity progress.

**Response** (200):
```json
{
  "coins": 150,
  "characters": [
    {
      "characterId": 1,
      "characterName": "Katya",
      "assetBaseId": "katya-1",
      "affinityPoints": 25,
      "level": 1,
      "nextLevelThreshold": 50,
      "levelDescription": "Acquainted",
      "unlockedDialogue": ["intro", "morning-greeting"]
    }
  ]
}
```

### GET /api/dating/character/:characterId
Character metadata and unlock thresholds.

**Response** (200):
```json
{
  "characterId": 1,
  "name": "Katya",
  "assetBaseId": "katya-1",
  "levelThresholds": [
    { "level": 0, "minPoints": 0, "description": "Stranger", "dialogues": [] },
    { "level": 1, "minPoints": 50, "description": "Acquainted", "dialogues": ["intro"] },
    { "level": 2, "minPoints": 150, "description": "Close Friend", "dialogues": ["intro", "date-1"] }
  ]
}
```

### POST /api/dating/send-gift
Send a gift to a character (costs coins, boosts affinity).

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
  "coinsSpent": 50,
  "affinityGained": 10,
  "newAffinityPoints": 35,
  "newCoins": 100,
  "characterResponse": "Oh, flowers! How sweet... *blushes*"
}
```

### GET /api/dating/character/:characterId/gift-log
View all gifts exchanged with a character.

**Response** (200):
```json
{
  "gifts": [
    {
      "giftId": 1,
      "giftType": "flower",
      "fromCharacter": false,
      "affinityBoost": 10,
      "sentAt": "2026-05-21T10:30:00Z"
    },
    {
      "giftId": 2,
      "giftType": "love-letter",
      "fromCharacter": true,
      "affinityBoost": 15,
      "sentAt": "2026-05-21T15:00:00Z",
      "message": "I hope this reaches your heart..."
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
