# Bazaar Flow awaaz aur music

Kuch wire nahi karna. Play dabate hi `AudioManager` khud ban jata hai, music chalta hai,
aur `GameAudioDirector` board dekh kar sahi waqt pe awaaz bajata hai:

| Kab | Awaaz |
| --- | --- |
| Crate tap | `sfx_tap` |
| Maal belt pe chadha | `sfx_belt_load` |
| Dukaan ne maal liya | `sfx_pickup` (jaldi jaldi liya to sur upar chadhta hai) |
| Dukaan ka order poora | `sfx_shop_done` |
| Tala wali dukaan khuli | `sfx_unlock` |
| Jeet | `sfx_win` (music thodi der dheema) |
| Belt Jam | `sfx_jam` (music thodi der dheema) |
| Music / Awaaz button | `sfx_ui_click` |
| UI: panel khulna, star, coin, purchase, tala | `sfx_ui_popup`, `sfx_star`, `sfx_coin`, `sfx_purchase`, `sfx_locked` |

Upar daayen kone mein **Music: ON/OFF** aur **Awaaz: ON/OFF** buttons hain (prototype ke liye).
Pasand `PlayerPrefs` mein save hoti hai: `bf_music`, `bf_sfx` (wohi keys jo `UI.UISettings` use karta hai),
is liye UI ke Sound / Music switch bhi khud kaam karte hain.

Asal UI (UI branch) ke saath jodne ke liye startup pe yeh do lines:

```csharp
BazaarFlow.UI.UIServices.PlaySfx = BazaarFlow.AudioManager.PlayUi;
BazaarFlow.AudioManager.ShowPrototypeButtons = false;
```

`PlayUi` UI ke ids samajhta hai: ui_tap, ui_back, ui_close, ui_toggle, ui_popup, ui_star, ui_coin,
ui_purchase, ui_locked. `ui_win` aur `ui_jam` jaan boojh kar khamosh hain, kyunke jeet / Jam ki awaaz
`GameAudioDirector` pehle hi bajata hai.

Code se awaaz: `AudioManager.Play(Sfx.Tap)`, `AudioManager.PlayPickup()`, `AudioManager.PlayStinger(Sfx.Win)`.

Import settings (`Scripts/Audio/Editor/BazaarFlowAudioImporter.cs`) khud lagti hain:
SFX mono + ADPCM, music mono + streaming Vorbis, taake phone pe memory kam lage.

Saari files hamari apni banayi hui hain (CC0), detail `LICENSES.md` mein.
