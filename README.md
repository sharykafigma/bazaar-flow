# Belt Jam: Conveyor Sort

Conveyor sort puzzle for Android (Unity 6). Package: `com.sharystudio.beltjam`. Design doc: https://claude.ai/artifact/KNAT1bkgv1fmNVc2iSPeiy

## Unity mein kaise chalayein

1. Unity Hub se naya project banayein: **Unity 6 (6000.x)**, template **Universal 2D** (ya 2D Core).
2. Is repo ka `Assets` folder project mein copy karein (ya repo ko hi project folder bana lein).
3. Koi bhi scene kholein (khaali scene bhi theek hai) aur **Play** dabayein. Menu, art, awaaz aur game khud ban jate hain.
4. Game view portrait rakhein, maslan 1080x1920.

Phone pe: File > Build Profiles > Android > Switch Platform, phir Build And Run. Release build: [docs/README.md](docs/README.md).

## Game (English in-game text)

- Tap the front crate of a column. Its goods ride through the gate onto the belt.
- Each stall shows the goods it wants and how many. Stalls grab matching goods as they pass.
- All stalls done = win (1-3 stars). Belt full and no stall wants anything = **Jammed!**
- Twists: hidden crates (11+), locked stall (16+), twin orders (21+), rush hour (31+), monsoon shutters (41+), all mixed (61+). Every 5th level is hard, every 10th very hard (more coins).
- Boosters: Extra Belt (+4 slots), Sweep (one colour back to the godown), Reveal (hidden crates), Reverse (belt runs back one lap).
- 200 levels on the map; the generator keeps going after that.

## Code ka naqsha

| Folder | Kaam |
| --- | --- |
| `Scripts/Core` | Bootstrap (camera + game on Play), colours, placeholder sprites |
| `Scripts/Levels` | Level data, level 1-3 haath se, 4+ seeded generator (twists, difficulty) |
| `Scripts/Gameplay` | Belt, stalls, godown, GameManager (events + boosters), world number labels |
| `Scripts/Progression` | Progress + stars + win streak, lives (5, 30 min), daily reward (7 din) |
| `Scripts/Integration/GameWiring.cs` | UI ko ads, IAP, wallet, sound aur progress se jorta hai |
| `Scripts/UI` | Saari screens (menu, map, HUD + boosters, win, Jammed!, shop, settings, daily reward, out of lives) |
| `Scripts/Visuals` + `Art` | Bazaar art aur juice (effects) |
| `Scripts/Audio` + `Audio` | Sounds aur music |
| `Scripts/Monetization` | Ads (LevelPlay), IAP (Unity IAP), wallet: [Docs/MONETIZATION_SETUP.md](Docs/MONETIZATION_SETUP.md) |

### Gameplay events (doosre systems inko sunte hain)

`GameManager.LevelStarted, LevelWon(level, reward, stars), LevelJammed, LevelContinued, CrateTapped, GoodDelivered, ShopCompleted, BoosterUsed, RushChanged` aur `Progress.StreakRewarded`.

### Save (PlayerPrefs)

`bf_level` (unlocked level), `bf_stars`, `bf_streak`, `bf_lives*`, `bf_daily_*`, `bf_save` (coins, boosters, no ads). Level 1 se shuru karne ke liye Edit > Clear All PlayerPrefs.
