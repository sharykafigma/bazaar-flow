# Belt Jam: Conveyor Sort UI

All menus and screens, built from code with uGUI (no prefabs, no scene setup). Press Play in any scene and the UI builds itself next to the gameplay prototype.

## Screens

| Screen | File | What it does |
| --- | --- | --- |
| Main Menu | `Screens/MainMenuScreen.cs` | Awning, bunting, logo over rays, market stall, PLAY, bottom tab bar (Shop, Map, Home, No Ads, Settings) |
| Level Map | `Screens/LevelMapScreen.cs` | Scrolling road through bazaar districts, 20 levels each, current level bounces with a pin |
| HUD | `Screens/HudScreen.cs` | Pause, level ribbon, coin counter, optional belt-fill meter |
| Pause | `Screens/PauseScreen.cs` | Sound / Music / Vibration switches, Resume, Restart, Home |
| Settings | `Screens/PauseScreen.cs` | Switches, Restore Purchases, Privacy Policy, version |
| Level Complete | `Screens/WinScreen.cs` | Stars, confetti, coin reward that flies into the counter, Next Level, 2x Coins (ad) |
| Jammed! (lose) | `Screens/JamScreen.cs` | Continue (ad), Continue for coins, Try Again |
| Shop | `Screens/ShopScreen.cs` | Starter pack, coin packs, No Ads, Booster Bundle, daily free booster |

`UIRoot.cs` runs the flow. Android back button: closes popups, pauses the game, quits from the menu.

## How other systems plug in

Everything outside the UI goes through `Core/UIServices.cs` (ads, purchases, prices, wallet, boosters, sounds, belt meter). Each member has an editor-safe default; other modules assign their own at startup, e.g.

```csharp
UIServices.ShowRewarded = AdsService.ShowRewarded;
UIServices.Purchase = IapService.Purchase;
UIServices.PlaySfx = id => AudioManager.Play(id);
```

Sound ids the UI asks for: `ui_tap, ui_back, ui_popup, ui_close, ui_toggle, ui_win, ui_star, ui_coin, ui_jam, ui_purchase, ui_locked`.
Settings are in `UISettings` (`bf_sfx`, `bf_music`, `bf_haptics` PlayerPrefs) with a `Changed` event.

If `GameManager` has `public static bool DrawPrototypeHud`, the UI sets it to false so the old OnGUI panels hide. Without it, the UI pauses the GameManager while its own popups are open.

## Look

- Colours, button palettes, sizes: `Core/UITheme.cs`.
- Font: `Resources/Fonts/BazaarFont.ttf` = Lilita One (SIL Open Font License, licence file next to it).
- Sprites are drawn at runtime from shapes (`Core/UISprites.cs`). To use real art, put a sprite in `Resources/UI/<name>` (e.g. `Resources/UI/rounded`, `Resources/UI/icon_Play`) and it replaces the generated one.
- Juice: `Widgets/JuicyButton.cs` (press squash, springy release, haptic tick, shine sweep), `Core/Tween.cs`, `UIFx.cs` (confetti, coin fly, toasts). All animation runs on unscaled time.
- Safe areas (notches, gesture bar): `Core/SafeArea.cs`.
