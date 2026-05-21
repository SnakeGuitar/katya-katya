# UI Specification - Katya Katya

## Views

### Dating Hub
**Path**: `MemoryGame.Client/Views/Dating/DatingHubView.xaml`
**Navigation**: From MainMenu → "Dating" button
**ViewModel**: `MemoryGame.Client/ViewModels/Dating/DatingHubViewModel.cs`

**Content**:
- Header: "Katya's Heart" or similar
- User coins display (top-right)
- Character grid (2-3 columns, scrollable)
- Each character card shows:
  - Character portrait (from AssetBaseId)
  - Character name
  - Affinity level (0-100 progress bar)
  - Level description (e.g., "Acquainted")
  - Click → CharacterDetailView

**State**:
- Loads from `GetUserAffinitiesQuery`
- Caches locally via `CurrencyService`
- Refresh on navigation from SinglePlayerGameViewModel

### Character Detail
**Path**: `MemoryGame.Client/Views/Dating/CharacterDetailView.xaml`
**Navigation**: From DatingHub → character card
**ViewModel**: `MemoryGame.Client/ViewModels/Dating/CharacterDetailViewModel.cs`

**Content**:
- Back button
- Large character portrait
- Name + level description
- Affinity meter (visual + numeric)
- "Next Level at X points" indicator
- List of unlocked dialogues (if any)
- [Future] Gift button, send flower

**State**:
- Loads from `GetCharacterDetailsQuery`
- Subscribes to affinity updates from `CompleteGameCommand`
- Updates UI when affinity changes

### Game Completion Modal
**Enhancement**: Existing `SinglePlayerGameViewModel` → on finish, show modal:

```xaml
<!-- Overlay modal -->
<ContentControl
  Background="#80000000"
  HorizontalAlignment="Stretch"
  VerticalAlignment="Stretch">
  
  <!-- Card center -->
  <StackPanel
    VerticalAlignment="Center"
    HorizontalAlignment="Center"
    Spacing="16">
    
    <TextBlock Text="Great job!" FontSize="28" FontWeight="Bold" />
    <TextBlock Text="+80 Coins" Foreground="#E7726A" FontSize="18" />
    <TextBlock Text="+5 Affinity with Katya" FontSize="16" />
    
    <!-- Animated progress bars -->
    <ProgressBar Value="25" Maximum="100" Height="8" />
    
    <Button Content="Close" Command="{Binding CloseGameOverCommand}" />
  </StackPanel>
</ContentControl>
```

**Behavior**:
- Animate coin counter from 0 to earned amount
- Animate affinity bar to new value
- Play unlock sound if new level reached
- Auto-close after 3 seconds or on button click
- Navigate back to MainMenu or DatingHub on close

## Navigation Flow

```
MainMenu
  ↓
  ├─ Single Player → SinglePlayer Game → [Modal] → MainMenu
  ↓
  └─ Dating → DatingHubView
       ↓
       └─ Character Card → CharacterDetailView
             ↓
             └─ Back → DatingHubView
```

## Design System Usage

- **Primary Color** (#E7726A): Affinity progress bars, character highlights, important buttons
- **Secondary Color** (#6C63FF): Interactive elements, unlock notifications, gift icons
- **Neutral Colors**: Text, backgrounds, UI structure
- **Typography**: Segoe UI, per DESIGN.md specs
- **Spacing**: 4px grid, 8px/16px padding typical
- **Rounded**: 8px–12px for cards/buttons

## Localization
All UI strings must be added to `MemoryGame.Client/Localization/` resource dictionaries:
- `Strings.es-MX.xaml` (Spanish - Mexico)
- `Strings.ja-JP.xaml` (Japanese)
- `Strings.zh-CN.xaml` (Chinese Simplified)
- `Strings.ko-KR.xaml` (Korean)

Key strings:
- `DATING_HUB_TITLE` = "Katya's Heart" (or locale equivalent)
- `AFFINITY_LEVEL_{1..N}` = Level descriptions
- `COIN_REWARD` = "+{amount} Coins"
- `AFFINITY_REWARD` = "+{amount} Affinity with {character}"

## Responsive Design
- **Desktop (1920x1080)**: Full character grid, side-by-side layouts
- **Tablet (768x1024)**: 2-column grid
- **Mobile-like**: Adjust spacing and font sizes down proportionally

No mobile native app required; responsive WPF Grid/StackPanel layouts handle scaling.
