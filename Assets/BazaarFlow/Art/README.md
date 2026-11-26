# Bazaar Flow art

All art is original and generated from code, so it can be changed and re-rendered at any size.

- **Edit:** `Tools/ArtGen/art.mjs` (game sprites) and `Tools/ArtGen/store.mjs` (icon, feature graphic).
- **Render:** `node Tools/ArtGen/render.mjs` (needs Node + Playwright with Chromium). `node Tools/ArtGen/store.mjs` for store art.
- **Preview a level screen:** `node Tools/ArtGen/preview.mjs out.png` (same world positions as the game).

## How it gets into the game

`Scripts/Visuals` starts by itself on Play (no scene setup). `BazaarVisuals` watches the board and puts a view on each
gameplay object (`BeltView`, `ShopView`, `CrateView`, `ItemView`, `GateView`). Views only read public gameplay state and
hide the placeholder sprites, so gameplay scripts need no changes. If `Resources/BazaarArt` is missing, the placeholder
look stays.

`Scripts/Visuals/Editor/BazaarArtImporter.cs` sets import settings for everything in this folder (sprite, no mipmaps,
ASTC on Android).

## Style

- Warm desi bazaar, chunky flat vector, one dark-brown outline (`#3b2417`), light from top-left.
- Goods have a cream "sticker" rim so they read on any background.
- One goods type per colour, so shape also tells colours apart (colour-blind friendly):
  Red = chai, Blue = choori (bangles), Yellow = aam (mango), Green = patang (kite), Purple = lattoo (top), Orange = diya.
- No text inside game art. Any text (store graphics) is English.

## Juice (Scripts/Visuals)

| Moment | Effect |
| --- | --- |
| Level start | stalls pop in one by one, crates drop in, small camera settle |
| Front crate | soft golden glow and breathing, crates behind are dimmed |
| Crate tap | crate squashes and flies into the godown arch, dust puff, arch bumps |
| Hidden crate revealed | sack poof, sparkles, crate pops |
| Goods on belt | bob and wobble with a shadow; brass hubs turn with the belt |
| Goods into shop | spin + sparkle trail, then sparkle burst, coloured ring, coin hop; order bubble pops |
| New order | bubble flips to the new goods |
| Shop done | shutter rolls down with marigold garland and tick, petal burst |
| Shop unlocked | padlock and chains fall away, sparkles |
| Level won | marigold petal and confetti rain, camera punch |
| Belt Jam | camera shake and red flash; white flash when continued |
| Ambient | rangoli slowly turns, bunting sways, kites drift, warm glints float |

## Store art (`Store/`)

- `icon_512.png` for the Play listing (32-bit PNG), `icon_1024.png` master.
- `icon_adaptive_fg_432.png` + `icon_adaptive_bg_432.png`: Android adaptive icon layers (xxxhdpi 108dp). Set them in
  Player Settings > Android > Icon > Adaptive.
- `feature_graphic_1024x500.png` for the Play listing.
