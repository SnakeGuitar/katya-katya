# WPF → Avalonia UI Migration Plan

AI AGENT DIRECTIVE: DO NOT attempt a 1:1 syntax translation. Read the UI_SPEC.md for functional requirements and build the XAML strictly using Avalonia 11/12 native paradigms. BlurEffect, Effect, and code-behind UI logic are explicitly banned.

## Executive Summary

**Goal**: Migrate `MemoryGame.Client` from WPF to Avalonia UI for cross-platform support (Windows, Linux, macOS, Android, iOS/Web future).

**Estimated effort**: 6-8 weeks (single developer)  
**Risk level**: Medium — ViewModels/Services are 70-80% reusable, Views need full rewrite.

---

## 1. Current State Analysis

### Project Stats

| Metric | Count |
|--------|-------|
| XAML Views | 25 |
| ViewModels | 18 |
| Services | 22 |
| NuGet packages | 5 |
| Localization locales | 4 |

### NuGet Compatibility

| Package                                  | WPF        | Avalonia Equivalent      | Action  |
| ------------------------------------------| ------------| --------------------------| ---------|
| CommunityToolkit.Mvvm                    | ✅          | ✅ Same package           | None    |
| Microsoft.AspNetCore.SignalR.Client      | ✅          | ✅ Same package           | None    |
| Microsoft.Extensions.DependencyInjection | ✅          | ✅ Same package           | None    |
| Microsoft.Extensions.Http                | ✅          | ✅ Same package           | None    |
| SkiaSharp.Views.WPF                      | ❌ WPF-only | SkiaSharp.Views.Avalonia | Replace |

### WPF-Specific Concerns (Must Rewrite)

CONCEPTUAL MAPPING ONLY: This table represents conceptual goals, not syntactical drop-ins. DO NOT use this as a search-and-replace dictionary. Views must be rebuilt structurally.

| Concern            | WPF                                        | Avalonia Replacement                                  |
| --------------------| --------------------------------------------| -------------------------------------------------------|
| Window chrome      | `WindowChrome`                             | `ExtendClientAreaToDecorationsHint`                   |
| Audio playback     | `System.Windows.Media.MediaPlayer`         | `LibVLCSharp` or `NAudio` (Windows) / platform audio  |
| Particle rendering | `SkiaSharp.Views.WPF.SKElement`            | `SkiaSharp.Views.Avalonia.SKCanvasView`               |
| Animation loop     | `CompositionTarget.Rendering`              | `DispatcherTimer` or `TopLevel.RequestAnimationFrame` |
| Resource URIs      | `pack://application:,,,/`                  | `avares://AssemblyName/`                              |
| Visibility         | `Visibility.Visible/Hidden/Collapsed`      | `bool IsVisible`                                      |
| Triggers           | `DataTrigger`, `EventTrigger`              | Avalonia `Styles` with `Selector` or `Animations`     |
| Storyboards        | WPF `Storyboard` in `EventTrigger`         | Avalonia `Animation` with `Transitions` or keyframes  |
| DependencyProperty | `DependencyProperty.Register`              | `StyledProperty<T>` or `DirectProperty<T>`            |
| Converters         | `BooleanToVisibilityConverter`             | `IsVisible` binding directly (bool)                   |
| Popup              | `System.Windows.Controls.Primitives.Popup` | `Avalonia.Controls.Primitives.Popup`                  |
| Drop shadows       | `DropShadowEffect`                         | `BoxShadow` property on `Border`                      |
| ImageBrush         | `ImageBrush` with pack URI                 | `ImageBrush` with avares URI                          |
| UniformGrid        | `UniformGrid`                              | `Avalonia.Controls.UniformGrid` (Labs or built-in)    |

---

## 2. Migration Strategy

### Approach: **New Project, Shared Core**

Strict Design Adherence: For any View being rewritten, you MUST extract all colors, fonts, opacities, and spacing strictly from UI_SPEC.md or BaseTheme.axaml. Do not hallucinate or guess hex codes. If a visual state (like hover) is not specified, use Avalonia's default Fluent theme behavior or ask the user.

Create a new `KatyaKatya` project alongside the WPF client (parallel existence during transition). Copy ViewModels/Services into the new project, then rewrite Views. All new code uses the `KatyaKatya` namespace.

```
MemoryGame-Revival/
├── MemoryGame.Client/          ← WPF (keep as-is, freeze)
├── KatyaKatya/                 ← New Avalonia client
│   ├── KatyaKatya.csproj
│   ├── App.axaml / App.axaml.cs
│   ├── ViewModels/             ← Copy from WPF (rename namespace)
│   ├── Services/               ← Copy from WPF (platform abstraction)
│   ├── Views/                  ← Rewrite from scratch
│   ├── Resources/              ← Copy assets, update URIs
│   ├── Localization/           ← Copy, adapt to Avalonia resources
│   └── Platforms/
│       ├── Desktop/            ← Windows/Mac/Linux entry point
│       └── Android/            ← Android entry point
└── KatyaKatya.Server/          ← ASP.NET Core backend (inner projects still MemoryGame.*)
```

### Why Not In-Place Rewrite?

- WPF client still works and can serve as reference
- Parallel development: test Avalonia while WPF remains functional
- Rollback safety
- Can compare behavior side-by-side

---

## 3. Phase Breakdown

### Phase 1: Project Scaffolding (Days 1-2)

- [ ] Create Avalonia project with `dotnet new avalonia.app`
- [ ] Configure .csproj (target `net10.0` + `net10.0-android`)
- [ ] Add NuGet packages (CommunityToolkit.Mvvm, SignalR, DI, Http, SkiaSharp.Views.Avalonia)
- [ ] Set up project structure (ViewModels/, Services/, Views/, Resources/)
- [ ] Create `App.axaml` + `App.axaml.cs` with DI container (mirror WPF's `App.xaml.cs`)
- [ ] Create `MainWindow.axaml` shell with navigation ContentControl

### Phase 2: Core Infrastructure (Days 3-5)

- [ ] Copy all ViewModels (no changes expected — pure CommunityToolkit.Mvvm)
- [ ] Copy Services/Core/ (SessionService, ClientSettings)
- [ ] Copy Services/Network/ (ApiClient, HubService, LobbyService, etc.)
- [ ] Copy Services/Interfaces/ (all interfaces)
- [ ] Rewrite `NavigationService` for Avalonia (replace WPF-specific frame navigation)
- [ ] Rewrite `DialogService` for Avalonia (overlay dialogs instead of Window.ShowDialog)
- [ ] Rewrite `WindowService` for Avalonia (window state management)
- [ ] Rewrite `MusicService` — replace `MediaPlayer` with cross-platform audio
- [ ] Rewrite `ThemeService` — Avalonia uses `Styles` and `IResourceProvider`
- [ ] Set up Localization (Avalonia resource dictionaries or .resx files)

### Phase 3: Theme & Design System (Days 6-8)

- [ ] Build from scratch: BaseTheme.axaml. DO NOT port WPF styles. Re-engineer the design system using Avalonia ControlTheme. Use pure composition for visual depth (e.g., dual BoxShadows), completely ignoring how it was done in WPF.
- [ ] Port `SketchTheme.xaml` → `SketchTheme.axaml`
- [ ] Port `GameAnimations.xaml` → Avalonia animations
- [ ] Create shared button/textblock/border styles
- [ ] Implement `BoxShadow` replacements for `DropShadowEffect`
- [ ] Set up font loading (`avares://` URIs)
- [ ] Set up image resources (`avares://` URIs)
- [ ] Custom window chrome (traffic-light buttons, drag region)

### Phase 4: Views - Session Flow (Days 9-12)

- [ ] `SplashScreenView.axaml`
- [ ] `TitleScreenView.axaml`
- [ ] `LoginView.axaml`
- [ ] `RegisterView.axaml`
- [ ] `VerifyEmailView.axaml`
- [ ] `GuestLoginView.axaml`
- [ ] `SetupProfileView.axaml`

### Phase 5: Views - Main App (Days 13-17)

- [] `MainMenuView.axaml`
- [] `MoreMenuView.axaml`
- [] `SettingsView.axaml`
- [] `ProfileView.axaml`
- [] `EditProfileView.axaml`
- [] `FriendsView.axaml`
- [] `GalleryView.axaml`

### Phase 6: Views - Game (Days 18-24)

- [ ] `SinglePlayerMenuView.axaml`
- [ ] `SinglePlayerGameView.axaml` (most complex — card grid, animations, SkiaSharp particles)
- [ ] `LobbyMenuView.axaml`
- [ ] `HostLobbyView.axaml`
- [ ] `LobbyView.axaml`
- [ ] `GameBoardView.axaml`
- [ ] `DialogWindow.axaml` (convert to overlay panel)

### Phase 7: SkiaSharp & Animations (Days 25-28)

- [ ] Port particle system (`SKElement` → `SKCanvasView`)
- [ ] Replace `CompositionTarget.Rendering` with Avalonia animation timer
- [ ] Discard all WPF Storyboards and EventTriggers. Build interactions using Avalonia Transitions, Style Selectors (pseudo-classes like :pointerover), and keyframe Animations within styles.
- [ ] Port entrance/exit transitions
- [ ] Port score pulse animation
- [ ] Dynamic background (bokeh canvas)

### Phase 8: Android Target (Days 29-35)

- [ ] Add Android head project
- [ ] Configure Android manifest, permissions
- [ ] Test touch input (tap = click, swipe navigation)
- [ ] Responsive layouts for mobile screens
- [ ] Platform-specific audio implementation
- [ ] Back button handling
- [ ] Status bar / navigation bar integration

### Phase 9: Testing & Polish (Days 36-42)

- [ ] Functional testing (all views navigate correctly)
- [ ] Game logic testing (cards flip, match, score)
- [ ] SignalR testing (lobby, multiplayer)
- [ ] Performance testing (particle system, animations)
- [ ] Android device testing
- [ ] Fix platform-specific bugs
- [ ] Remove WPF project from active development

---

## 4. Key Technical Decisions

### Audio (MusicService replacement)

**Option A: LibVLCSharp** (Recommended)

- Cross-platform (Windows, Linux, macOS, Android, iOS)
- Supports MP3, OGG, FLAC
- NuGet: `LibVLCSharp` + `VideoLAN.LibVLC.{platform}`
- Heavier dependency (~30MB per platform)

**Option B: NAudio (Windows) + Platform-specific**

- NAudio for Windows, AVAudioPlayer for Android
- Lighter per-platform but more code to maintain

**Decisions**:

- ibVLCSharp for MVP simplicity, revisit if APK size is a concern.
- WPF Effects and Storyboards are explicitly banned. UI will be built bottom-up using Avalonia native composition.

### Animation Timer

```csharp
// Option 1: DispatcherTimer (simpler, slightly less precise)
var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
timer.Tick += OnFrame;

// Option 2: TopLevel.RequestAnimationFrame (vsync-locked, preferred)
TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnFrame);
```

**Decision**: Use `DispatcherTimer` at 60fps for particle system — simpler and sufficient.

### Window Chrome

```xml
<!-- Avalonia equivalent of WPF WindowChrome -->
<Window ExtendClientAreaToDecorationsHint="True"
        ExtendClientAreaChromeHints="NoChrome"
        ExtendClientAreaTitleBarHeightHint="46"
        SystemDecorations="None"
        TransparencyLevelHint="AcrylicBlur">
```

### Resource URIs

```
WPF:      pack://application:,,,/Resources/Images/logo.png
Avalonia: avares://MemoryGame.Avalonia/Resources/Images/logo.png
```

### Visibility Binding

```xml
<!-- WPF -->
<Border Visibility="{Binding IsVisible, Converter={StaticResource BoolToVisibilityConverter}}"/>

<!-- Avalonia -->
<Border IsVisible="{Binding IsVisible}"/>
```

### Animations

```xml
<!-- WPF: EventTrigger + Storyboard -->
<EventTrigger RoutedEvent="Loaded">
    <BeginStoryboard>
        <Storyboard>
            <DoubleAnimation To="1" Duration="0:0:0.5"/>
        </Storyboard>
    </BeginStoryboard>
</EventTrigger>

<!-- Avalonia: Transitions or Animation -->
<Border.Transitions>
    <Transitions>
        <DoubleTransition Property="Opacity" Duration="0:0:0.5"/>
    </Transitions>
</Border.Transitions>

<!-- Or explicit keyframe animation -->
<Border.Styles>
    <Style Selector="Border.fadeIn">
        <Style.Animations>
            <Animation Duration="0:0:0.5">
                <KeyFrame Cue="0%"><Setter Property="Opacity" Value="0"/></KeyFrame>
                <KeyFrame Cue="100%"><Setter Property="Opacity" Value="1"/></KeyFrame>
            </Animation>
        </Style.Animations>
    </Style>
</Border.Styles>
```

---

## 5. Files That Need NO Changes (Copy Directly)

These are platform-agnostic and work in both WPF and Avalonia:

- All ViewModels (`ViewModels/**/*.cs`) — CommunityToolkit.Mvvm is framework-agnostic
- `Services/Core/SessionService.cs`
- `Services/Core/ClientSettings.cs`
- `Services/Network/ApiClient.cs`
- `Services/Network/HubService.cs`
- `Services/Network/LobbyService.cs`
- `Services/Network/GameService.cs`
- `Services/Network/ChatService.cs`
- `Services/Network/ProfileService.cs`
- All interfaces (`Services/Interfaces/*.cs`)
- `Helpers/ProfileLoader.cs`
- Model/DTO classes

---

## 6. Files That Need Rewrite

| File | Reason |
|------|--------|
| `App.xaml.cs` | WPF `Application` → Avalonia `Application` |
| `MainWindow.xaml` | WindowChrome, WPF triggers, Popup |
| `MainWindow.xaml.cs` | WPF code-behind (window state, fade transitions) |
| All 25 View `.xaml` files | Namespace changes, trigger rewrites, style syntax |
| `Services/Media/MusicService.cs` | `System.Windows.Media.MediaPlayer` → LibVLCSharp |
| `Services/UI/NavigationService.cs` | WPF ContentControl navigation |
| `Services/UI/DialogService.cs` | WPF Window.ShowDialog |
| `Services/UI/WindowService.cs` | WPF Window state management |
| `Services/UI/ThemeService.cs` | WPF ResourceDictionary swap |
| `Resources/Themes/*.xaml` | WPF styles → Avalonia styles |
| `Resources/Styles/GameAnimations.xaml` | WPF Storyboard → Avalonia Animation |
| `Localization/LocalizationManager.cs` | WPF ResourceDictionary-based → Avalonia equivalent |

Zero Code-Behind Rule: The .axaml.cs files must remain completely empty except for InitializeComponent(). All UI logic, transitions, and states MUST be handled via MVVM Bindings or Avalonia Styles/Pseudo-classes.

---

## 7. Risk Matrix

| Risk                                     | Impact | Likelihood | Mitigation                                             |
| ------------------------------------------| --------| ------------| --------------------------------------------------------|
| Complex animations don't port cleanly    | High   | Medium     | Simplify animations, use SkiaSharp for complex effects |
| Audio library issues on Android          | Medium | Medium     | Test early, have fallback (platform-specific)          |
| Performance regression (particle system) | Medium | Low        | SkiaSharp performance is consistent cross-platform     |
| SignalR on Android network issues        | Low    | Low        | Same library, different transport negotiation          |
| Touch input needs different UX           | Medium | High       | Plan mobile-specific layouts from Phase 8              |

---

## 8. Success Criteria

- [ ] All 25 views render correctly on Windows
- [ ] Game loop functional (flip cards, match, score, timer)
- [ ] SignalR multiplayer works
- [ ] Music plays and cycles tracks
- [ ] Particle effects render via SkiaSharp
- [ ] Themes switch correctly
- [ ] Localization works (4 locales)
- [ ] Android APK builds and runs
- [ ] Touch input works for card flipping on Android
- [ ] Performance: 60fps during gameplay on mid-range Android

---

## 9. Dependencies to Install

```xml
<!-- KatyaKatya.csproj -->
<PackageReference Include="Avalonia" Version="11.*" />
<PackageReference Include="Avalonia.Desktop" Version="11.*" />
<PackageReference Include="Avalonia.Android" Version="11.*" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.*" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.2" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="10.0.5" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.5" />
<PackageReference Include="Microsoft.Extensions.Http" Version="10.0.5" />
<PackageReference Include="SkiaSharp.Views.Avalonia" Version="3.119.*" />
<PackageReference Include="LibVLCSharp" Version="3.*" />
```

---

## 10. Commands to Bootstrap

```bash
# From MemoryGame-Revival/
dotnet new avalonia.app -n KatyaKatya -o KatyaKatya
cd KatyaKatya

# Add packages
dotnet add package CommunityToolkit.Mvvm
dotnet add package Microsoft.AspNetCore.SignalR.Client
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Http
dotnet add package SkiaSharp.Views.Avalonia
dotnet add package LibVLCSharp

# Run
dotnet run
```
