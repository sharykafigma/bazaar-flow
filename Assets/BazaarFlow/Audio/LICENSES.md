# Bazaar Flow audio: sources and licenses

Every audio file in `Assets/BazaarFlow/Audio` was **synthesized from scratch** by
`Tools/Audio/generate_audio.py` (numpy math: sine waves, filtered noise,
Karplus-Strong plucked strings). No samples, loops, presets or recordings from
anyone else were used, so there is nothing to attribute and no third-party license
applies. The files are original work of this project and are dedicated to the
public domain under **CC0 1.0** (https://creativecommons.org/publicdomain/zero/1.0/),
so they are safe to ship in a commercial Play Store game.

The music is written in the style of Raag Khamaj with a Keherwa tabla pattern.
Raag scales and taals are traditional and not copyrightable; the melody itself is
original.

| File | What it is | Length | Made by |
| --- | --- | --- | --- |
| `Resources/BazaarFlowAudio/sfx_tap.wav` | Wooden crate knock when you tap a crate | 0.12 s | `sfx_tap()` |
| `Resources/BazaarFlowAudio/sfx_belt_load.wav` | Soft conveyor clack when goods go on the belt | 0.09 s | `sfx_belt_load()` |
| `Resources/BazaarFlowAudio/sfx_pickup.wav` | "Ting" when a shop takes goods (pitch climbs on combos) | 0.35 s | `sfx_pickup()` |
| `Resources/BazaarFlowAudio/sfx_shop_done.wav` | Santoor arpeggio when a shop order is complete | 0.9 s | `sfx_shop_done()` |
| `Resources/BazaarFlowAudio/sfx_win.wav` | Santoor + tabla celebration on level win | 2.2 s | `sfx_win()` |
| `Resources/BazaarFlowAudio/sfx_jam.wav` | Motor grinding down + sad two-note fall on Belt Jam | 1.2 s | `sfx_jam()` |
| `Resources/BazaarFlowAudio/sfx_ui_click.wav` | Button click | 0.06 s | `sfx_ui_click()` |
| `Resources/BazaarFlowAudio/sfx_unlock.wav` | Lock opening when a locked shop opens | 0.5 s | `sfx_unlock()` |
| `Resources/BazaarFlowAudio/sfx_ui_popup.wav` | Airy swoosh when a panel opens | 0.25 s | `sfx_ui_popup()` |
| `Resources/BazaarFlowAudio/sfx_star.wav` | Bell for each star on the win screen | 0.6 s | `sfx_star()` |
| `Resources/BazaarFlowAudio/sfx_coin.wav` | "Ka-ching" when coins are counted | 0.3 s | `sfx_coin()` |
| `Resources/BazaarFlowAudio/sfx_purchase.wav` | Coin cascade + santoor chord after a purchase | 1.2 s | `sfx_purchase()` |
| `Resources/BazaarFlowAudio/sfx_locked.wav` | Low double buzz for locked / not allowed | 0.3 s | `sfx_locked()` |
| `Resources/BazaarFlowAudio/music_bazaar_loop.ogg` | Light bazaar loop: tanpura drone, tabla, santoor, ghungroo (96 BPM, seamless) | 20 s | `music_loop()` |

To change a sound, edit its function in the script and run
`python3 Tools/Audio/generate_audio.py` (needs numpy and ffmpeg with libvorbis).
The random seed is fixed, so the same code always gives the same files.

If you ever replace a sound with a downloaded one, add a row here with its source
URL, author and license (CC0 or a license that allows commercial use) before shipping.
