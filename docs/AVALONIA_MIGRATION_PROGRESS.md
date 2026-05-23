# Avalonia 12 Migration Progress

## Completed (✅)

### Core Infrastructure
- App.xaml.cs → App.axaml.cs (DI setup with Avalonia containers)
- Localization framework (4 locales: es-MX, ja-JP, zh-CN, ko-KR)

### Navigation & Event Bindings
- ViewLocator for ViewModel→View resolution
- INavigationService implementation for Avalonia

---

## In Progress / Deferred (🚧)

### Views & Styling (Pending Re-implementation)
> [!IMPORTANT]
> All UI views, styles, themes, and client-side animation resources have been removed from the KatyaKatya project to ensure the core remains clean / separated. These will need to be re-implemented.

- **Shell & Navigation**
  - [ ] MainWindow shell with navigation ContentControl
- **Theme & Styling**
  - [ ] Theme system (`BaseTheme.axaml` with colors, brushes, gradients, shadows)
  - [ ] Font loading via `avares://` URIs with explicit file paths
  - [ ] `CommonStyles.axaml` (button, text, input styles)
- **Views - Session Flow**
  - [ ] `SplashScreenView` (entrance animation with ScaleTransform)
  - [ ] `TitleScreenView` (login, register, guest button)
  - [ ] `LoginView` (with fixed StringConverters)
  - [ ] `RegisterView` (with fixed StringConverters)
  - [ ] `GuestLoginView` (new flow for guest login)
  - [ ] `VerifyEmailView` (email PIN entry post-registration)
- **Views - Main App**
  - [ ] `MainMenuView`
  - [ ] `MoreMenuView`
  - [ ] `SettingsView`
  - [ ] `ProfileView` and `EditProfileView`
  - [ ] `FriendsView`
  - [ ] `GalleryView` (card gallery)
  - [ ] `LobbyMenuView`
- **Views - Game**
  - [ ] `GameBoardView` (multiplayer board with players, cards, chat)
    - [ ] Players panel (left column)
    - [ ] Card grid (center, UniformGrid with Viewbox)
    - [ ] Chat panel (right column)
    - [ ] Game Over overlay with animations
- **Styling & Animations**
  - [ ] `GameAnimations.axaml`
    - [ ] Animated card hover/press (scale transitions)
    - [ ] Trophy wiggle animation (rotation keyframes on game over)
    - [ ] Board entrance (fade + slide up)
    - [ ] Game Over overlay fade + card slide up with bounce
  - [ ] `BaseTheme.axaml` updates:
    - [ ] Font paths: explicit .ttf files instead of directory URIs
    - [ ] CardBackground: increased opacity (85-67% range) for visible glass effect
    - [ ] Shadow.GlassCard: dual shadow (dark base + rose tint) matching WPF DropShadowEffect
    - [ ] InputBackground: increased opacity for readable text fields
- **MVVM Bindings**
  - [ ] Proper MVVM command bindings using `x:CompileBindings="True"`
  - [ ] Event-to-command via RelativeSource and `$parent` binding syntax

### Particle System & Advanced Animations
**Status**: Architecture planned, implementation deferred

The WPF `GameAnimationService` provides:
- Physics-based particle system (Gravity, Drag, Velocity)
- Three particle types: Hearts, Stars, Sparkles
- Shockwave rings on match
- Floating combo text
- Motion blur trails
- Combo multiplier detection (1x, 2x, 3x+)

**Avalonia Challenge**: No native `SKCanvasView` equivalent in Avalonia 12
- Avalonia uses SkiaSharp internally but doesn't expose `SKCanvasView`
- Options explored:
  1. Custom ICustomDrawOperation implementation ✗ (internal API)
  2. Canvas with DrawingContext operations ✗ (no SkiaSharp access)
  3. Third-party SkiaSharp.Views.Avalonia ✗ (doesn't exist)
  4. Custom control with bitmap rendering (TBD)

**Planned approach**: 
- Implement ParticleCanvas as custom Avalonia control
- Render to offscreen bitmap via SkiaSharp
- Update canvas with bitmap each frame via DispatcherTimer (60fps)

**Deferred to v2**: Particle system will be added after stable Avalonia baseline is achieved.

### Features Without Implementation Yet
- Background bokeh canvas (dynamic animated background)
- Particle effects on card match
- Score pulse animations
- Card flip animations (IsFlipped binding trigger)
- Match bounce and sparkle ring (IsMatched binding trigger)
- Vote-to-kick right-click menu

### Known Differences from WPF
| Feature | WPF | Avalonia Status |
|---------|-----|--------|
| CacheMode="BitmapCache" | ✅ Performance hint | ❌ Removed (Avalonia optimizes automatically) |
| DropShadowEffect | ✅ Via <Effect> | ✅ BoxShadow property |
| Visibility enum | ✅ Visible/Hidden/Collapsed | ✅ IsVisible bool (collapsed handled by layout) |
| DataTrigger/EventTrigger | ✅ In XAML | ❌ Use Styles with selectors + Animations |
| Storyboard | ✅ EventTrigger-driven | ✅ Keyframe animations in Styles |
| CompositionTarget.Rendering | ✅ vsync loop | ✅ DispatcherTimer (60fps target) |
| pack://application:,,,/ | ✅ WPF URI scheme | ✅ avares://AssemblyName/ for Avalonia |

---

## Test Status

### Build
- ❌ Desktop (.NET 9.0) compilation is broken due to missing view references in App.axaml.cs (needs cleanup or views restored)
- ⏳ Android project requires workload installation (deferred)

### Runtime (Not yet tested)
- [ ] Full navigation flow (splash → title → login → lobby → game)
- [ ] View transitions and animations
- [ ] Chat message scrolling
- [ ] Game Over overlay display and animation
- [ ] Multiplayer board state sync (via existing HubService)
- [ ] Theme switching
- [ ] Localization (4 language switching)

---

## Architecture Notes

### Avalonia 12 Key Patterns

**Compiled Bindings**
```xml
<Window x:CompileBindings="True" x:DataType="vm:MainWindowViewModel">
  <!-- Bindings checked at compile time, better performance -->
</Window>
```

**Animations via Styles** (replaces WPF EventTrigger+Storyboard)
```xml
<Style Selector="Button.trophy-wiggle">
  <Style.Animations>
    <Animation Duration="0:0:0.6" IterationCount="3">
      <KeyFrame Cue="0%">
        <Setter Property="RotateTransform.Angle" Value="0"/>
      </KeyFrame>
      <KeyFrame Cue="50%">
        <Setter Property="RotateTransform.Angle" Value="-15"/>
      </KeyFrame>
    </Animation>
  </Style.Animations>
</Style>
```

**Binding Syntax Changes**
- RelativeSource with type: `{Binding DataContext, RelativeSource={RelativeSource AncestorType=UserControl}}`
- Negation operator: `IsVisible="{Binding !IsCurrentUser}"`
- Parent access: `{Binding $parent[UserControl].((vm:GameBoardViewModel)DataContext).VoteToKickCommand}`

**Removing Code-Behind Logic**
- WPF: EventTrigger in XAML calls code-behind animations
- Avalonia: Move all animation definitions to Styles, keep code-behind minimal

---

## Next Steps

### v1 (Baseline - Current)
1. 🚧 Core navigation and views (Pending Re-implementation)
2. 🚧 Game board layout and styling (Pending Re-implementation)
3. 🚧 Game Over animations (Pending Re-implementation)
4. 🚧 **Particle system** (waiting on custom Canvas approach)
5. 🚧 **Performance testing** on 1920x1080 fullscreen

### v2 (Polish)
1. Particle effects (card match, floating score)
2. Card flip animation (3D effect via scale/opacity)
3. Background effects (bokeh, parallax)
4. Sound effects integration (LibVLCSharp)
5. Android deployment (touch input, responsive layouts)

### Known Issues to Investigate
- [ ] SettingsView slow to load (music service optimization needed)
- [ ] Card gallery visual parity with WPF
- [ ] Font embedding working correctly across platforms
- [ ] High-res fullscreen (1920x1080) performance metrics

---

## References

- Avalonia 12 Documentation: https://docs.avaloniaui.net/
- XAML → AXAML Cheat Sheet: See inline comments in views
- WPF → Avalonia Port Guide: In code comments where conversions differ significantly
