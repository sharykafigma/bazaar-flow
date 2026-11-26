// Google Play store graphics for Bazaar Flow.  Run from Tools/ArtGen:  node store.mjs
// Output: Assets/BazaarFlow/Art/Store/
//   icon_1024.png, icon_512.png            hi-res app icon (full bleed, opaque, no text)
//   icon_adaptive_fg_432.png / _bg_432.png Android adaptive icon layers (108dp @ xxxhdpi)
//   feature_graphic_1024x500.png           Play feature graphic (opaque)
// Plus a contact sheet in the scratch dir (argv[2] or ./store_preview.png).
// Everything is vector SVG built from art.mjs, rendered by Playwright Chromium.
// Title font: Lilita One (SIL OFL 1.1), Assets/BazaarFlow/Art/Fonts/LilitaOne-Regular.ttf
import { chromium } from 'playwright';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import zlib from 'node:zlib';
import * as A from './art.mjs';

const { OL, C, shade, goodsBody } = A;
const here = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(here, '../..');
const out = path.join(root, 'Assets/BazaarFlow/Art/Store');
const fontFile = path.join(root, 'Assets/BazaarFlow/Art/Fonts/LilitaOne-Regular.ttf');
const previewFile = process.argv[2] || path.join(process.cwd(), 'store_preview.png');
fs.mkdirSync(out, { recursive: true });

// ---------------------------------------------------------------- helpers
let uid = 0;
// Place an art.mjs sprite (a standalone <svg>) inside another SVG, with ids made unique.
function place(svgText, x, y, w, h, extra = '') {
  const p = `u${uid++}_`;
  const s = svgText
    .replace(/id="([^"]+)"/g, `id="${p}$1"`)
    .replace(/url\(#([^)]+)\)/g, `url(#${p}$1)`)
    .replace(/<svg ([^>]*?)width="[^"]*" height="[^"]*"/, `<svg $1x="${x}" y="${y}" width="${w}" height="${h}" ${extra}`);
  return s;
}
// Sticker filter (white rim + soft shadow) in the 0..100 goods space.
const stickerFilter = (id, r = 3.4, sh = 3, op = 0.4) => `
<filter id="${id}" x="-30%" y="-30%" width="160%" height="160%">
  <feMorphology in="SourceAlpha" operator="dilate" radius="${r}" result="d"/>
  <feFlood flood-color="#fffaf0"/><feComposite in2="d" operator="in" result="rim"/>
  <feMorphology in="SourceAlpha" operator="dilate" radius="${r + 1.6}" result="d2"/>
  <feFlood flood-color="${OL}"/><feComposite in2="d2" operator="in" result="ol"/>
  <feGaussianBlur in="d2" stdDeviation="${sh}"/><feOffset dy="${sh * 1.3}" result="b"/>
  <feFlood flood-color="#3b1020" flood-opacity="${op}"/><feComposite in2="b" operator="in" result="shadow"/>
  <feMerge><feMergeNode in="shadow"/><feMergeNode in="ol"/><feMergeNode in="rim"/><feMergeNode in="SourceGraphic"/></feMerge>
</filter>`;
// A good centred at (cx,cy) drawn `size` px wide.
const good = (key, cx, cy, size, rot = 0, filter = 'stk') =>
  `<g transform="translate(${cx},${cy}) rotate(${rot}) scale(${size / 100}) translate(-50,-50)"><g filter="url(#${filter})">${goodsBody[key]()}</g></g>`;
const sparkle = (x, y, s, op = 1, col = '#fff') =>
  `<path transform="translate(${x},${y}) scale(${s / 100}) translate(-50,-50)" d="M50,4 C54,40 60,46 96,50 C60,54 54,60 50,96 C46,60 40,54 4,50 C40,46 46,40 50,4 Z" fill="${col}" opacity="${op}"/>`;
const polar = (cx, cy, r, a) => [cx + r * Math.cos(a), cy + r * Math.sin(a)];
const f1 = n => n.toFixed(1);

// Mughal arch path, centred on cx, springing at y0, apex at yTop, base at yb.
function archPath(cx, w, yTop, ySpring, yb) {
  const l = cx - w / 2, r = cx + w / 2;
  return `M${l},${yb} V${ySpring} C${l},${ySpring - (ySpring - yTop) * 0.55} ${cx - w * 0.18},${yTop + (ySpring - yTop) * 0.25} ${cx},${yTop}
          C${cx + w * 0.18},${yTop + (ySpring - yTop) * 0.25} ${r},${ySpring - (ySpring - yTop) * 0.55} ${r},${ySpring} V${yb} Z`;
}

// Striped bazaar awning across [x0,x1], top y0, stripe height h, scallop depth sd.
function awning(x0, x1, y0, h, sd, colA, colB, n) {
  const w = (x1 - x0) / n, parts = [];
  for (let k = 0; k < n; k++) {
    const x = x0 + k * w, c = k % 2 ? colB : colA;
    parts.push(`<rect x="${f1(x)}" y="${y0}" width="${f1(w + 0.6)}" height="${h}" fill="${c}"/>`);
    parts.push(`<path d="M${f1(x)},${y0 + h} q${f1(w / 2)},${sd * 2} ${f1(w)},0 z" fill="${c}" stroke="${OL}" stroke-width="${sd * 0.28}" stroke-linejoin="round"/>`);
  }
  // folds / shading
  for (let k = 0; k < n; k++) {
    const x = x0 + k * w;
    parts.push(`<rect x="${f1(x + w * 0.62)}" y="${y0}" width="${f1(w * 0.38)}" height="${h}" fill="#000" opacity=".08"/>`);
  }
  parts.push(`<rect x="${x0}" y="${y0 + h - sd * 0.4}" width="${x1 - x0}" height="${sd * 0.4}" fill="#000" opacity=".12"/>`);
  parts.push(`<path d="M${x0},${y0 + h} H${x1}" stroke="${OL}" stroke-width="${sd * 0.28}"/>`);
  return parts.join('');
}

// ---------------------------------------------------------------- APP ICON (1024 design space)
const ICON = { cx: 512, cy: 618, r: 318, band: 74 };

function iconDefs() {
  return `
  <radialGradient id="bgG" gradientUnits="userSpaceOnUse" cx="512" cy="470" r="760">
    <stop offset="0" stop-color="#ffd45c"/><stop offset=".38" stop-color="#ff9a3c"/>
    <stop offset=".72" stop-color="#f2536f"/><stop offset="1" stop-color="#b8205c"/></radialGradient>
  <radialGradient id="brass" gradientUnits="userSpaceOnUse" cx="430" cy="470" r="460">
    <stop offset="0" stop-color="#fff3b0"/><stop offset=".45" stop-color="#f6c24a"/><stop offset="1" stop-color="#c47f18"/></radialGradient>
  <radialGradient id="thaali" gradientUnits="userSpaceOnUse" cx="470" cy="540" r="300">
    <stop offset="0" stop-color="#fff0c0"/><stop offset=".6" stop-color="#f3c55a"/><stop offset="1" stop-color="#d8952c"/></radialGradient>
  <linearGradient id="awnRod" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#c98a4b"/><stop offset="1" stop-color="#7a4422"/></linearGradient>
  <filter id="dropL" x="-20%" y="-20%" width="140%" height="150%"><feGaussianBlur in="SourceAlpha" stdDeviation="14"/><feOffset dy="18"/>
    <feComponentTransfer><feFuncA type="linear" slope=".45"/></feComponentTransfer><feMerge><feMergeNode/><feMergeNode in="SourceGraphic"/></feMerge></filter>
  <filter id="dropS" x="-20%" y="-20%" width="140%" height="160%"><feGaussianBlur in="SourceAlpha" stdDeviation="8"/><feOffset dy="10"/>
    <feComponentTransfer><feFuncA type="linear" slope=".4"/></feComponentTransfer><feMerge><feMergeNode/><feMergeNode in="SourceGraphic"/></feMerge></filter>
  <radialGradient id="vig" gradientUnits="userSpaceOnUse" cx="512" cy="540" r="720">
    <stop offset=".62" stop-color="#5a0a2e" stop-opacity="0"/><stop offset="1" stop-color="#5a0a2e" stop-opacity=".38"/></radialGradient>
  ${stickerFilter('stk', 3.2, 2.6, 0.45)}`;
}

// Background layer: gradient, sun rays, rangoli dots, faint arch.
function iconBackground() {
  const rays = Array.from({ length: 20 }, (_, k) => {
    const a0 = (k / 20) * Math.PI * 2, a1 = a0 + Math.PI / 20;
    const [x0, y0] = polar(512, 560, 2000, a0), [x1, y1] = polar(512, 560, 2000, a1);
    return `<path d="M512,560 L${f1(x0)},${f1(y0)} L${f1(x1)},${f1(y1)} Z" fill="#fff" opacity=".075"/>`;
  }).join('');
  const dots = (n, r, rr, col, op) => Array.from({ length: n }, (_, k) => {
    const [x, y] = polar(512, 612, r, (k / n) * Math.PI * 2);
    return `<circle cx="${f1(x)}" cy="${f1(y)}" r="${rr}" fill="${col}" opacity="${op}"/>`;
  }).join('');
  const petals = Array.from({ length: 24 }, (_, k) =>
    `<ellipse cx="512" cy="${612 - 445}" rx="16" ry="34" fill="#fff" opacity=".12" transform="rotate(${k * 15} 512 612)"/>`).join('');
  return `
  <rect x="-1200" y="-1200" width="3424" height="3424" fill="#b8205c"/>
  <rect x="-1200" y="-1200" width="3424" height="3424" fill="url(#bgG)"/>
  ${rays}
  ${petals}
  ${dots(48, 490, 6, '#fff', .22)}${dots(36, 520, 4, '#ffe27a', .3)}
  <path d="${archPath(512, 820, 120, 420, 1100)}" fill="#fff4d6" opacity=".16"/>
  <path d="${archPath(512, 740, 175, 450, 1100)}" fill="none" stroke="#fff4d6" stroke-width="10" stroke-dasharray="2 22" stroke-linecap="round" opacity=".55"/>
  <rect x="-1200" y="-1200" width="3424" height="3424" fill="url(#vig)"/>`;
}

// Looped belt / brass thaali ring.
function iconRing() {
  const { cx, cy, r, band } = ICON;
  const ro = r + band / 2, ri = r - band / 2;
  const bolts = Array.from({ length: 24 }, (_, k) => {
    const [x, y] = polar(cx, cy, ro + 17, (k / 24) * Math.PI * 2 + 0.13);
    return `<circle cx="${f1(x)}" cy="${f1(y)}" r="6.5" fill="#fff0b8" stroke="${OL}" stroke-width="3"/>`;
  }).join('');
  // flow chevrons on the belt (clockwise)
  const chev = Array.from({ length: 8 }, (_, k) => {
    const a = (k / 8) * Math.PI * 2 + Math.PI / 8, deg = a * 180 / Math.PI + 90;
    const [x, y] = polar(cx, cy, r, a);
    return `<path transform="translate(${f1(x)},${f1(y)}) rotate(${f1(deg)})" d="M-14,-18 L8,0 L-14,18" fill="none" stroke="#ffd36b" stroke-width="11" stroke-linecap="round" stroke-linejoin="round" opacity=".9"/>`;
  }).join('');
  const circ = 2 * Math.PI * r;
  return `
  <g filter="url(#dropL)">
    <circle cx="${cx}" cy="${cy}" r="${ro + 34}" fill="url(#brass)" stroke="${OL}" stroke-width="12"/>
  </g>
  <circle cx="${cx}" cy="${cy}" r="${ro + 27}" fill="none" stroke="#fff6c8" stroke-width="5" opacity=".75" stroke-dasharray="${f1(circ * 0.3)} ${f1(circ)}" transform="rotate(195 ${cx} ${cy})"/>
  ${bolts}
  <circle cx="${cx}" cy="${cy}" r="${r}" fill="none" stroke="#5b3a2c" stroke-width="${band}"/>
  <circle cx="${cx}" cy="${cy}" r="${r}" fill="none" stroke="#6e4636" stroke-width="${band - 22}"/>
  <circle cx="${cx}" cy="${cy}" r="${r}" fill="none" stroke="#3f261c" stroke-width="${band - 22}" stroke-dasharray="7 ${f1(circ / 36 - 7)}"/>
  <circle cx="${cx}" cy="${cy}" r="${ro}" fill="none" stroke="${OL}" stroke-width="9"/>
  <circle cx="${cx}" cy="${cy}" r="${ri}" fill="url(#thaali)" stroke="${OL}" stroke-width="10"/>
  <circle cx="${cx}" cy="${cy}" r="${ri - 26}" fill="none" stroke="#b87a22" stroke-width="5" stroke-dasharray="3 14" stroke-linecap="round"/>
  <path d="M${cx - ri + 40},${cy - 60} A${ri - 30},${ri - 30} 0 0 1 ${cx - 70},${cy - ri + 38}" fill="none" stroke="#fff" stroke-width="12" stroke-linecap="round" opacity=".55"/>`;
}

function iconAwning() {
  return `
  <g filter="url(#dropS)">
    ${awning(-409, 1433, 70, 112, 38, '#E8423D', '#fff3dc', 15)}
    <rect x="-409" y="34" width="1842" height="46" rx="14" fill="url(#awnRod)" stroke="${OL}" stroke-width="10"/>
    <rect x="-409" y="42" width="1842" height="10" fill="#f2c27a" opacity=".6"/>
  </g>`;
}

// Foreground: awning + ring + goods + sparkles (everything except the background).
function iconForeground() {
  const { cx, cy } = ICON;
  return `
  ${iconAwning()}
  ${iconRing()}
  <ellipse cx="${cx}" cy="${cy + 150}" rx="220" ry="40" fill="#7a3a10" opacity=".22"/>
  ${good('aam', cx - 190, cy - 70, 380, -14)}
  ${good('choori', cx + 192, cy - 66, 380, 12)}
  ${good('chai', cx + 4, cy + 60, 470, 0)}
  ${sparkle(cx - 330, cy - 290, 96)}${sparkle(cx + 345, cy + 255, 72, .9)}${sparkle(cx - 360, cy + 220, 50, .8)}`;
}

const iconSvg = (w, h, vb, body) =>
  `<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="${vb}"><defs>${iconDefs()}</defs>${body}</svg>`;

// Full icon at any size (vector, so 512 is rendered natively, not downscaled).
const icon = size => iconSvg(size, size, '0 0 1024 1024', iconBackground() + iconForeground());
// Adaptive layers: 432px = 108dp. Visible viewport is the middle 72dp (288px), safe circle 66dp (264px).
// Map the 1024 design square onto the central ADAPT px so the ring + goods sit inside the safe circle.
const ADAPT = 300;
const adaptVb = (() => { const s = 1024 / ADAPT * 432, o = (1024 - s) / 2; return `${f1(o)} ${f1(o)} ${f1(s)} ${f1(s)}`; })();
const adaptiveFg = () => iconSvg(432, 432, adaptVb, iconForeground());
const adaptiveBg = () => iconSvg(432, 432, adaptVb, iconBackground());

// ---------------------------------------------------------------- FEATURE GRAPHIC 1024x500
function feature(fontUrl) {
  const W = 1024, H = 500;
  let seed = 11; const rnd = () => (seed = (seed * 16807) % 2147483647) / 2147483647;
  // skyline
  const far = [], near = [];
  for (let x = -30; x < W + 30; x += 60 + rnd() * 70) {
    const w = 46 + rnd() * 60, h = 60 + rnd() * 90, base = 330;
    far.push(`<rect x="${f1(x)}" y="${f1(base - h)}" width="${f1(w)}" height="${f1(h + 40)}" fill="#f7b98a"/>`);
    const t = rnd();
    if (t < 0.45) far.push(`<path d="M${f1(x - 4)},${f1(base - h)} a${f1(w / 2 + 4)},${f1(w / 2 + 8)} 0 0 1 ${f1(w + 8)},0 z" fill="#f7b98a"/><path d="M${f1(x + w / 2)},${f1(base - h - w / 2 - 8)} v-16" stroke="#f7b98a" stroke-width="4"/>`);
    else if (t < 0.75) far.push(`<rect x="${f1(x + w / 2 - 8)}" y="${f1(base - h - 90)}" width="16" height="90" fill="#f7b98a"/><path d="M${f1(x + w / 2 - 13)},${f1(base - h - 90)} a13,15 0 0 1 26,0 z" fill="#f7b98a"/><rect x="${f1(x + w / 2 - 13)}" y="${f1(base - h - 50)}" width="26" height="6" fill="#f7b98a"/>`);
  }
  const cols = ['#ee9a6c', '#e48a60', '#f0a878', '#e2835e'];
  for (let x = -20, k = 0; x < W + 20; k++) {
    const w = 110 + rnd() * 60, h = 70 + rnd() * 60, top = 345 - h, col = cols[k % 4];
    near.push(`<rect x="${f1(x)}" y="${f1(top)}" width="${f1(w)}" height="${f1(h + 60)}" fill="${col}" stroke="${OL}" stroke-opacity=".2" stroke-width="2"/>`);
    near.push(`<rect x="${f1(x - 4)}" y="${f1(top - 8)}" width="${f1(w + 8)}" height="10" fill="${shade(col, -0.15)}"/>`);
    for (const fx of [0.2, 0.62]) {
      const wx = x + w * fx, ww = w * 0.24;
      near.push(`<path d="${archPath(wx + ww / 2, ww, top + 18, top + 34, top + 62)}" fill="#8a4630" opacity=".55"/>`);
    }
    x += w + 6;
  }
  // belt (horizontal), y 392..462, built from the game's belt tile
  const beltY = 388, beltH = 74;
  const beltTile = A.beltH(100);
  const trays = [], goodsRow = ['chai', 'aam', 'choori', 'diya', 'patang', 'lattoo', 'chai', 'aam', 'choori'];
  for (let k = 0; k < 9; k++) {
    const x = 62 + k * 112;
    trays.push(place(A.tray(), x - 40, beltY - 3, 80, 80));
    trays.push(good(goodsRow[k], x, beltY + 30, 66, (k % 2 ? 6 : -6)));
  }
  const title = (txt, x, y, size) => `
    <text x="${x}" y="${y + 14}" font-size="${size}" text-anchor="middle" fill="#6a1631" stroke="#6a1631" stroke-width="${size * 0.22}" stroke-linejoin="round" opacity=".55" filter="url(#blur3)">${txt}</text>
    <text x="${x}" y="${y}" font-size="${size}" text-anchor="middle" fill="${OL}" stroke="${OL}" stroke-width="${size * 0.2}" stroke-linejoin="round">${txt}</text>
    <text x="${x}" y="${y}" font-size="${size}" text-anchor="middle" fill="#fff6dc" stroke="#fffbe8" stroke-width="${size * 0.11}" stroke-linejoin="round">${txt}</text>
    <text x="${x}" y="${y}" font-size="${size}" text-anchor="middle" fill="url(#tGrad)" stroke="#b33a12" stroke-width="${size * 0.03}" stroke-linejoin="round">${txt}</text>
    <text x="${x}" y="${y}" font-size="${size}" text-anchor="middle" fill="url(#tShine)" clip-path="url(#shine${y})">${txt}</text>`;
  return `<svg xmlns="http://www.w3.org/2000/svg" width="${W}" height="${H}" viewBox="0 0 ${W} ${H}">
  <defs>
    <style>@font-face{font-family:'Lilita One';src:url('${fontUrl}') format('truetype');} text{font-family:'Lilita One',sans-serif;}</style>
    <linearGradient id="sky" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#ff7aa0"/><stop offset=".45" stop-color="#ffab5c"/><stop offset="1" stop-color="#ffd98a"/></linearGradient>
    <radialGradient id="sun" cx="512" cy="230" r="420" gradientUnits="userSpaceOnUse"><stop offset="0" stop-color="#fff3c4" stop-opacity=".95"/><stop offset=".5" stop-color="#ffe08a" stop-opacity=".35"/><stop offset="1" stop-color="#ffd27a" stop-opacity="0"/></radialGradient>
    <linearGradient id="tGrad" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#ffe94a"/><stop offset=".5" stop-color="#ffb21a"/><stop offset="1" stop-color="#f2551d"/></linearGradient>
    <linearGradient id="tShine" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#fff" stop-opacity=".55"/><stop offset="1" stop-color="#fff" stop-opacity=".15"/></linearGradient>
    <clipPath id="shine196"><rect x="0" y="0" width="1024" height="${196 - 132 * 0.42}"/></clipPath>
    <clipPath id="shine322"><rect x="0" y="0" width="1024" height="${322 - 132 * 0.42}"/></clipPath>
    <linearGradient id="floor" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#a8683a"/><stop offset="1" stop-color="#6e3f22"/></linearGradient>
    <pattern id="belt" patternUnits="userSpaceOnUse" x="0" y="${beltY}" width="${beltH}" height="${beltH}">${place(beltTile, 0, 0, beltH, beltH)}</pattern>
    <filter id="blur3" x="-10%" y="-20%" width="120%" height="150%"><feGaussianBlur stdDeviation="4"/></filter>
    <filter id="drop" x="-20%" y="-20%" width="140%" height="150%"><feGaussianBlur in="SourceAlpha" stdDeviation="6"/><feOffset dy="8"/>
      <feComponentTransfer><feFuncA type="linear" slope=".4"/></feComponentTransfer><feMerge><feMergeNode/><feMergeNode in="SourceGraphic"/></feMerge></filter>
    <radialGradient id="fvig" gradientUnits="userSpaceOnUse" cx="512" cy="250" r="640" gradientTransform="translate(512 250) scale(1 .6) translate(-512 -250)">
      <stop offset=".7" stop-color="#4a0d26" stop-opacity="0"/><stop offset="1" stop-color="#4a0d26" stop-opacity=".3"/></radialGradient>
    <radialGradient id="tGlow" cx="512" cy="240" r="300" gradientUnits="userSpaceOnUse"><stop offset="0" stop-color="#fff8d0" stop-opacity=".7"/><stop offset="1" stop-color="#fff8d0" stop-opacity="0"/></radialGradient>
    ${stickerFilter('stk', 3.4, 2.4, 0.4)}
  </defs>
  <rect width="${W}" height="${H}" fill="url(#sky)"/>
  ${Array.from({ length: 18 }, (_, k) => { const a0 = Math.PI + (k / 18) * Math.PI, a1 = a0 + Math.PI / 36;
    const [x0, y0] = polar(512, 250, 1200, a0), [x1, y1] = polar(512, 250, 1200, a1);
    return `<path d="M512,250 L${f1(x0)},${f1(y0)} L${f1(x1)},${f1(y1)} Z" fill="#fff" opacity=".09"/>`; }).join('')}
  <rect width="${W}" height="${H}" fill="url(#sun)"/>
  <g opacity=".9">${far.join('')}</g>
  ${near.join('')}
  ${place(A.bunting(1024, 128), -10, -18, 1044, 130)}
  ${place(A.bunting(1024, 128), 60, 40, 904, 92, 'opacity=".55" preserveAspectRatio="none"')}
  <!-- kites -->
  <g transform="translate(250,128) rotate(-18)"><g transform="scale(.62) translate(-50,-50)"><g filter="url(#stk)">${goodsBody.patang()}</g></g></g>
  <g transform="translate(800,120) rotate(16)"><g transform="scale(.5) translate(-50,-50)"><g filter="url(#stk)">${goodsBody.patang().replaceAll(C.green, '#e2569a')}</g></g></g>
  <!-- stalls -->
  <g filter="url(#drop)">
    ${place(A.stall(0), 22, 168, 236, 236)}
    ${place(A.stall(2), 766, 168, 236, 236)}
  </g>
  ${good('chai', 140, 168 + 236 * 0.5, 96, -6)}
  ${good('aam', 884, 168 + 236 * 0.5, 96, 8)}
  <!-- floor + belt -->
  <rect x="0" y="${beltY + beltH - 6}" width="${W}" height="${H - beltY}" fill="url(#floor)"/>
  ${Array.from({ length: 12 }, (_, k) => `<path d="M${k * 92 + 20},${beltY + beltH + 6} v40" stroke="#5a3019" stroke-width="3" opacity=".5"/>`).join('')}
  <g filter="url(#drop)"><rect x="-10" y="${beltY}" width="${W + 20}" height="${beltH}" fill="url(#belt)"/>
  <rect x="-10" y="${beltY}" width="${W + 20}" height="${beltH}" fill="none" stroke="${OL}" stroke-width="4"/></g>
  ${trays.join('')}
  <ellipse cx="512" cy="250" rx="330" ry="150" fill="url(#tGlow)"/>
  <!-- title -->
  <g transform="translate(512,0) rotate(-4) translate(-512,0)">
    ${title('Bazaar', 512, 196, 132)}
    ${title('Flow', 512, 322, 132)}
  </g>
  ${sparkle(250, 180, 46)}${sparkle(790, 150, 38, .95)}${sparkle(700, 322, 30, .9)}${sparkle(330, 330, 26, .85)}
  <rect width="${W}" height="${H}" fill="url(#fvig)"/>
</svg>`;
}


// Chromium writes fully-opaque screenshots as 24-bit RGB; Play asks for a 32-bit PNG icon.
// Re-encode an 8-bit RGB PNG as RGBA (alpha 255) with plain zlib.
function pngToRGBA(file) {
  const buf = fs.readFileSync(file);
  let off = 8, w, h, depth, ctype; const idat = [];
  while (off < buf.length) {
    const len = buf.readUInt32BE(off), type = buf.toString('ascii', off + 4, off + 8), data = buf.subarray(off + 8, off + 8 + len);
    if (type === 'IHDR') { w = data.readUInt32BE(0); h = data.readUInt32BE(4); depth = data[8]; ctype = data[9]; }
    else if (type === 'IDAT') idat.push(data);
    off += 12 + len;
  }
  if (ctype === 6) return;                        // already RGBA
  if (ctype !== 2 || depth !== 8 || buf[8 + 8 + 12] /* interlace */) throw new Error(`unexpected PNG format in ${file}`);
  const raw = zlib.inflateSync(Buffer.concat(idat)), bpp = 3, stride = w * bpp;
  const cur = Buffer.alloc(stride), prev = Buffer.alloc(stride), outRaw = Buffer.alloc(h * (1 + w * 4));
  for (let y = 0; y < h; y++) {
    const ft = raw[y * (stride + 1)], line = raw.subarray(y * (stride + 1) + 1, (y + 1) * (stride + 1));
    for (let i = 0; i < stride; i++) {
      const a = i >= bpp ? cur[i - bpp] : 0, b = prev[i], c = i >= bpp ? prev[i - bpp] : 0;
      let v = line[i];
      if (ft === 1) v += a; else if (ft === 2) v += b; else if (ft === 3) v += (a + b) >> 1;
      else if (ft === 4) { const p = a + b - c, pa = Math.abs(p - a), pb = Math.abs(p - b), pc = Math.abs(p - c); v += pa <= pb && pa <= pc ? a : pb <= pc ? b : c; }
      cur[i] = v & 255;
    }
    const o = y * (1 + w * 4); outRaw[o] = 0;
    for (let x = 0; x < w; x++) { outRaw[o + 1 + x * 4] = cur[x * 3]; outRaw[o + 2 + x * 4] = cur[x * 3 + 1]; outRaw[o + 3 + x * 4] = cur[x * 3 + 2]; outRaw[o + 4 + x * 4] = 255; }
    cur.copy(prev);
  }
  const crcT = Array.from({ length: 256 }, (_, n) => { let c = n; for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1; return c >>> 0; });
  const crc = b => { let c = 0xffffffff; for (const x of b) c = crcT[(c ^ x) & 255] ^ (c >>> 8); return (c ^ 0xffffffff) >>> 0; };
  const chunk = (type, data) => { const t = Buffer.from(type, 'ascii'), l = Buffer.alloc(4), cbuf = Buffer.alloc(4);
    l.writeUInt32BE(data.length); cbuf.writeUInt32BE(crc(Buffer.concat([t, data]))); return Buffer.concat([l, t, data, cbuf]); };
  const ihdr = Buffer.alloc(13); ihdr.writeUInt32BE(w, 0); ihdr.writeUInt32BE(h, 4); ihdr[8] = 8; ihdr[9] = 6;
  fs.writeFileSync(file, Buffer.concat([buf.subarray(0, 8), chunk('IHDR', ihdr), chunk('IDAT', zlib.deflateSync(outRaw, { level: 9 })), chunk('IEND', Buffer.alloc(0))]));
}

// ---------------------------------------------------------------- render
const browser = await chromium.launch();
const page = await browser.newPage({ deviceScaleFactor: 1 });
const tmpHtml = path.join(out, '.render.html');

async function render(svgText, file, { transparent = false, jpeg = false } = {}) {
  fs.writeFileSync(tmpHtml, `<!doctype html><html><head><meta charset="utf-8"></head><body style="margin:0;background:${transparent ? 'transparent' : '#fff'}">${svgText}</body></html>`);
  await page.goto(pathToFileURL(tmpHtml).href);
  await page.evaluate(() => document.fonts.ready);
  const el = await page.$('svg');
  await el.screenshot({ path: file, omitBackground: transparent, type: jpeg ? 'jpeg' : 'png' });
}

const fontUrl = pathToFileURL(fontFile).href;
await page.setViewportSize({ width: 1200, height: 1200 });
await render(icon(1024), path.join(out, 'icon_1024.png'), { transparent: true });   // opaque art, RGBA = 32-bit PNG
await render(icon(512), path.join(out, 'icon_512.png'), { transparent: true });
await render(adaptiveFg(), path.join(out, 'icon_adaptive_fg_432.png'), { transparent: true });
await render(adaptiveBg(), path.join(out, 'icon_adaptive_bg_432.png'), { transparent: true });
await render(feature(fontUrl), path.join(out, 'feature_graphic_1024x500.png'));
for (const f of ['icon_1024.png', 'icon_512.png', 'icon_adaptive_bg_432.png']) pngToRGBA(path.join(out, f));

// ---------------------------------------------------------------- contact sheet
const u = f => pathToFileURL(path.join(out, f)).href;
const iconImg = (s, r) => `<img src="${u('icon_512.png')}" width="${s}" height="${s}" style="border-radius:${r};display:block;box-shadow:0 2px 6px rgba(0,0,0,.25)">`;
const adaptive = (s, r) => `<div style="position:relative;width:${s}px;height:${s}px;border-radius:${r};overflow:hidden;box-shadow:0 2px 6px rgba(0,0,0,.25)">
  <img src="${u('icon_adaptive_bg_432.png')}" style="position:absolute;left:${-s / 4}px;top:${-s / 4}px;width:${s * 1.5}px;height:${s * 1.5}px">
  <img src="${u('icon_adaptive_fg_432.png')}" style="position:absolute;left:${-s / 4}px;top:${-s / 4}px;width:${s * 1.5}px;height:${s * 1.5}px"></div>`;
const cell = (label, inner) => `<div style="display:flex;flex-direction:column;align-items:center;gap:6px">${inner}<div class="l">${label}</div></div>`;
const row = sizes => sizes.map(([s, lab]) => cell(`${lab} square`, iconImg(s, '0')) + cell(`${lab} squircle`, iconImg(s, '30%')) + cell(`${lab} circle`, iconImg(s, '50%'))).join('');
const sheet = `<!doctype html><html><head><meta charset="utf-8"><style>
  body{margin:0;background:#2b2230;font:13px Inter,sans-serif;color:#eee}
  .wrap{padding:24px;display:flex;flex-direction:column;gap:22px;width:1100px}
  .r{display:flex;gap:22px;align-items:flex-end;flex-wrap:wrap}.l{opacity:.75}
  h2{margin:0;font-size:16px;font-weight:600}
  .light{background:#f1f1f4;color:#333;padding:16px;border-radius:12px}
</style></head><body><div class="wrap" id="sheet">
  <h2>Bazaar Flow - Play store art</h2>
  <div class="r">${cell('512 (Play hi-res icon, Play masks to ~30% radius)', iconImg(512, '30%'))}
    <div style="display:flex;flex-direction:column;gap:18px">
      <div class="r">${row([[96, '96']])}</div>
      <div class="r">${row([[48, '48']])}</div>
      <div class="r light">${row([[48, '48']])}</div>
      <div class="r">${cell('adaptive 96 circle', adaptive(96, '50%'))}${cell('adaptive 96 squircle', adaptive(96, '30%'))}${cell('adaptive 48', adaptive(48, '50%'))}${cell('adaptive 48 sq', adaptive(48, '22%'))}</div>
    </div></div>
  <div>${cell('feature graphic 1024x500', `<img src="${u('feature_graphic_1024x500.png')}" width="1024" height="500" style="display:block">`)}</div>
</div></body></html>`;
fs.writeFileSync(tmpHtml, sheet);
await page.setViewportSize({ width: 1150, height: 1400 });
await page.goto(pathToFileURL(tmpHtml).href);
await (await page.$('#sheet')).screenshot({ path: previewFile });
fs.rmSync(tmpHtml);
await browser.close();
console.log('store art ->', path.relative(root, out), '| preview ->', previewFile);
