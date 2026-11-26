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

Upar daayen kone mein **Music: ON/OFF** aur **Awaaz: ON/OFF** buttons hain (prototype ke liye).
Pasand `PlayerPrefs` mein save hoti hai: `bf_music_on`, `bf_sfx_on`.
Asal UI aane pe `AudioManager.ShowPrototypeButtons = false` karein aur apne buttons se
`AudioManager.Instance.SetMusic(bool)` / `SetSfx(bool)` bulayein.

Code se awaaz: `AudioManager.Play(Sfx.Tap)`, `AudioManager.PlayPickup()`, `AudioManager.PlayStinger(Sfx.Win)`.

Import settings (`Scripts/Audio/Editor/BazaarFlowAudioImporter.cs`) khud lagti hain:
SFX mono + ADPCM, music mono + streaming Vorbis, taake phone pe memory kam lage.

Saari files hamari apni banayi hui hain (CC0), detail `LICENSES.md` mein.
