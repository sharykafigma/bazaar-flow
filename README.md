# Bazaar Flow (prototype)

Bazaar ke theme wali flow/conveyor puzzle game. Design doc: https://claude.ai/artifact/KNAT1bkgv1fmNVc2iSPeiy

## Unity mein kaise chalayein

1. Unity Hub se naya project banayein: **Unity 6 (6000.x)**, template **Universal 2D** (ya 2D Core).
2. Is folder ka `Assets/BazaarFlow` apne project ke `Assets` mein copy kar dein.
3. Koi bhi scene kholein (naya khaali scene bhi theek hai) aur **Play** dabayein. Game khud ban jati hai, kuch drag nahi karna.
4. Game view ko portrait rakhein, maslan 1080x1920.

Phone pe: File > Build Profiles > Android > Switch Platform, phir Build And Run.

## Kaise khelte hain

- Neeche godown mein crates hain. Har qatar ka saamne wala crate tap karo.
- Crate ka maal gate se belt pe chadhta hai. Dukaan ka gol nishaan batata hai woh kis rang ka maal maang rahi hai, aur number batata hai kitna.
- Saari dukaanein band = jeet. Belt bhar jaye aur koi dukaan belt ka maal na le = Belt Jam (haar).
- Level 11 se chhupe crate ("?"), level 16 se tala wali dukaan.

## Code ka naqsha

| File | Kaam |
| --- | --- |
| `Scripts/Core/Bootstrap.cs` | Play dabane pe camera aur GameManager banata hai |
| `Scripts/Core/GameColors.cs` | 6 rang aur palette |
| `Scripts/Core/SpriteFactory.cs` | Art ke baghair placeholder sprites |
| `Scripts/Levels/LevelData.cs` | Level ka data (dukaanein, orders, crates) |
| `Scripts/Levels/LevelLibrary.cs` | Level 1-3 haath se, 4+ seeded generator |
| `Scripts/Gameplay/Belt.cs` | Loop belt, khane, raasta |
| `Scripts/Gameplay/Shop.cs` | Dukaan aur us ke orders |
| `Scripts/Gameplay/Godown.cs` | Crates ki qataarein |
| `Scripts/Gameplay/GameManager.cs` | Level chalana, tap, jeet/haar, prototype HUD |

Progress `PlayerPrefs` mein save hota hai (`bf_level`, aur coins/boosters `bf_save` mein). Level 1 se dobara shuru karne ke liye Edit > Clear All PlayerPrefs.

## Ads aur purchases

Rewarded/interstitial ads (LevelPlay), Google Play purchases (Unity IAP 5), coins/boosters ka save aur Dukaan screen `Scripts/Monetization` mein hain. Accounts, IDs aur Play Console products ki list: [Docs/MONETIZATION_SETUP.md](Docs/MONETIZATION_SETUP.md).

## Abhi kya nahi hai

Asal art, awaaz, Apna Bazaar meta, lives. Yeh design doc ke milestones 2-3 mein hain.
