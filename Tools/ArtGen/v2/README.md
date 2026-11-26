# Art v2 team contract

Style kit: ../style.mjs (soft volume gradients, coloured outlines = darken(fill), inner rim light/shadow via filter vol/volS,
gloss highlights, sticker rim on goods, warm dark never pure black). Benchmark: goods.mjs body.chai().
Every svg must start with `<defs>${kitDefs()}</defs>` (filters vol, volS, drop, sticker, blur1/2/4) and use id() for
gradient ids. Draw in a 0..100 viewBox unless noted. No text anywhere.

Each module exports `jobs()` returning [[name, svgString], ...]. Names and pivots must match the game (Scripts/Visuals).

| module | sprites (render px) | contract |
| --- | --- | --- |
| goods.mjs | item_{chai,choori,aam,patang,lattoo,diya} 256; bubble_{same} 256; tray 256; crate_{same} 512; crate_hidden 512 | item: goods centred, sticker rim. bubble: round order badge, cream ring + goods-colour disc + faint goods silhouette; a white number with dark outline is drawn on its centre, keep centre calm. tray: brass thaali seen from above, round. crate: wooden crate body x5..95, y22..96, goods peeking above it (y 2..30), a goods-colour plaque centred at (50,60) about 52x38 for a white number. crate_hidden: same crate under a tied jute sack, dark round patch at (50,60) for a "?" |
| stalls.mjs | stall_0..5 512; stall_closed 512; stall_lock 512; gate 512 | stall frame fills x4..96, y2..96; awning top y2..34, counter y72..96; an order badge (radius ~26) covers the centre (50,52), so decoration lives at the sides, awning and counter. 6 themes: 0 chai, 1 choori (bangles), 2 fruit (aam), 3 kites, 4 toys (lattoo), 5 diyas. stall_closed: overlay on the same frame: metal shutter rolled down over x11..89 y30..94, marigold garland, green tick badge top-right. stall_lock: overlay: dimmed stall, two crossing chains, padlock at centre (number drawn on it). gate: godown arch doorway, x8..92, y2..96, dark interior centred near (50,58) for a white number. |
| env.mjs | background 2048x4096; rangoli 1024; bunting 2048x256; belt_h 256; belt_v 256; belt_corner 256; belt_hub 256; fx_sparkle 128, fx_glow 256, fx_ring 256, fx_puff 128, fx_petal 128, fx_confetti 64, fx_coin 256, fx_kite 256, fx_shadow 256x128 | see env section in agent brief |
