// v2 environment: background, rangoli, bunting, conveyor belt pieces and fx sprites.
// Every svg starts with kitDefs() and uses id() for its own gradient/pattern ids.
import { kitDefs, grad, ball, gloss, glint, contact, line, lighten, darken, mix, svg, id } from '../style.mjs';

const GOLD = '#F2B53A', CREAM = '#FFF4E0';
const BRASS = '#D9A43C';

// seeded rng so the art is stable between renders
const rng = s => () => (s = (s * 16807) % 2147483647) / 2147483647;
const f1 = v => Math.round(v * 10) / 10;

// ============================================================ background
// viewBox 1024x2048 covers 12.4 x 24.8 world units centred on world (0,0).
const U = 82.58;
const wx = x => (x + 6.2) * U;
const wy = y => (12.4 - y) * U;

function dome(cx, base, r, h = 1.45) {
  // onion dome with a finial
  return `M${cx - r},${base} C${cx - r * 1.08},${base - r * 0.75} ${cx - r * 0.55},${base - r * 1.05} ${cx - r * 0.12},${base - r * h * 0.95}
    C${cx - r * 0.05},${base - r * h * 1.02} ${cx},${base - r * h * 1.08} ${cx},${base - r * h * 1.12}
    C${cx},${base - r * h * 1.08} ${cx + r * 0.05},${base - r * h * 1.02} ${cx + r * 0.12},${base - r * h * 0.95}
    C${cx + r * 0.55},${base - r * 1.05} ${cx + r * 1.08},${base - r * 0.75} ${cx + r},${base} Z`;
}

/** Far skyline: pale, hazy domes, minarets and chhatris. */
function farSkyline(base, col, lit) {
  const g = id('far'), parts = [];
  const add = d => parts.push(d);
  // big mosque left of centre
  const mosque = (cx, s) => {
    add(`<rect x="${cx - 120 * s}" y="${base - 70 * s}" width="${240 * s}" height="${70 * s + 40}"/>`);
    add(`<rect x="${cx - 60 * s}" y="${base - 95 * s}" width="${120 * s}" height="${30 * s}"/>`);
    add(`<path d="${dome(cx, base - 95 * s, 58 * s)}"/>`);
    add(`<rect x="${cx - 1.6 * s}" y="${base - 95 * s - 58 * s * 1.7 - 14 * s}" width="${3.2 * s}" height="${16 * s}"/>`);
    for (const dx of [-95, 95]) add(`<path d="${dome(cx + dx * s, base - 70 * s, 24 * s)}"/>`);
    for (const dx of [-140, 140]) {
      const x = cx + dx * s, top = base - 215 * s;
      add(`<path d="M${x - 10 * s},${base} L${x - 7 * s},${top} L${x + 7 * s},${top} L${x + 10 * s},${base} Z"/>`);
      for (const t of [0.35, 0.65]) add(`<rect x="${x - 12 * s}" y="${base - 215 * s * t - 0}" width="${24 * s}" height="${5 * s}" rx="${2 * s}"/>`);
      add(`<rect x="${x - 11 * s}" y="${top - 6 * s}" width="${22 * s}" height="${6 * s}"/>`);
      add(`<path d="M${x - 9 * s},${top - 6 * s} C${x - 9 * s},${top - 20 * s} ${x},${top - 24 * s} ${x},${top - 30 * s} C${x},${top - 24 * s} ${x + 9 * s},${top - 20 * s} ${x + 9 * s},${top - 6 * s} Z"/>`);
    }
  };
  const fort = (x0, w, h) => {
    add(`<rect x="${x0}" y="${base - h}" width="${w}" height="${h + 40}"/>`);
    for (let x = x0; x < x0 + w - 6; x += 16) add(`<rect x="${x}" y="${base - h - 9}" width="9" height="10"/>`);
  };
  const chhatri = (cx, b, w) => {
    add(`<rect x="${cx - w / 2}" y="${b - w * 0.55}" width="${w}" height="${w * 0.55}"/>`);
    add(`<rect x="${cx - w * 0.6}" y="${b - w * 0.62}" width="${w * 1.2}" height="${w * 0.1}"/>`);
    add(`<path d="${dome(cx, b - w * 0.6, w * 0.48, 1.3)}"/>`);
  };
  fort(-20, 190, 52);
  chhatri(60, base - 52, 34);
  mosque(330, 1.0);
  fort(520, 160, 40);
  chhatri(600, base - 40, 40);
  // palace with tower on the right, behind the sun
  fort(720, 330, 66);
  add(`<rect x="860" y="${base - 150}" width="70" height="90"/>`);
  for (let x = 860; x < 925; x += 14) add(`<rect x="${x}" y="${base - 158}" width="8" height="9"/>`);
  chhatri(895, base - 158, 46);
  chhatri(760, base - 66, 30);
  chhatri(1000, base - 66, 30);
  return `<defs><linearGradient id="${g}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${lit}"/><stop offset="1" stop-color="${col}"/></linearGradient></defs>
    <g fill="url(#${g})">${parts.join('')}</g>`;
}

/** Mid layer: havelis with jharokhas, chhatris and cornices, softly hazed. */
function havelis(base, r) {
  const out = [], defs = [];
  const tones = ['#E9A882', '#F0BE8A', '#DE9A86', '#EBB27C', '#E6A08E', '#D9B08A'];
  const haze = '#F7D3B0';
  let x = -30, k = 0;
  while (x < 1050) {
    const w = 120 + r() * 70, top = base - 72 - r() * 26, col = mix(tones[k % tones.length], haze, 0.3);
    const gb = id('hv'); defs.push(`<linearGradient id="${gb}" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="${darken(col, 0.08)}"/><stop offset=".7" stop-color="${col}"/><stop offset="1" stop-color="${lighten(col, 0.18)}"/></linearGradient>`);
    const edge = darken(col, 0.22), deep = mix(darken(col, 0.55), haze, 0.15);
    out.push(`<rect x="${f1(x)}" y="${f1(top)}" width="${f1(w)}" height="${f1(base - top + 20)}" fill="url(#${gb})"/>`);
    // sunlit right edge and shaded left edge
    out.push(`<rect x="${f1(x + w - 4)}" y="${f1(top)}" width="4" height="${f1(base - top)}" fill="#FFF1D8" opacity=".45"/>`);
    out.push(`<rect x="${f1(x)}" y="${f1(top)}" width="5" height="${f1(base - top)}" fill="${edge}" opacity=".35"/>`);
    // cornice + parapet merlons
    out.push(`<rect x="${f1(x - 5)}" y="${f1(top - 3)}" width="${f1(w + 10)}" height="9" rx="2" fill="${lighten(col, 0.25)}"/><rect x="${f1(x - 5)}" y="${f1(top + 6)}" width="${f1(w + 10)}" height="4" fill="${edge}" opacity=".55"/>`);
    for (let mx = x + 4; mx < x + w - 8; mx += 15) out.push(`<path d="M${f1(mx)},${f1(top - 3)} v-6 a4,4 0 0 1 8,0 v6 z" fill="${lighten(col, 0.12)}"/>`);
    // band of carved trim
    out.push(`<rect x="${f1(x)}" y="${f1(top + 70)}" width="${f1(w)}" height="5" fill="${edge}" opacity=".35"/>`);
    // jharokhas (balconied windows) on the upper floor
    const n = w > 160 ? 3 : 2;
    for (let i = 0; i < n; i++) {
      const cx = x + w * (i + 0.5) / n, wy0 = top + 22, ww = 26;
      out.push(`<path d="M${f1(cx - ww / 2 - 5)},${f1(wy0 + 4)} C${f1(cx - ww / 2 - 5)},${f1(wy0 - 12)} ${f1(cx)},${f1(wy0 - 16)} ${f1(cx)},${f1(wy0 - 20)} C${f1(cx)},${f1(wy0 - 16)} ${f1(cx + ww / 2 + 5)},${f1(wy0 - 12)} ${f1(cx + ww / 2 + 5)},${f1(wy0 + 4)} Z" fill="${lighten(col, 0.2)}"/>`);
      out.push(`<path d="M${f1(cx - ww / 2)},${f1(wy0 + 32)} V${f1(wy0 + 8)} a${ww / 2},${ww / 2 - 3} 0 0 1 ${ww},0 V${f1(wy0 + 32)} Z" fill="${deep}"/>`);
      out.push(`<path d="M${f1(cx - ww / 2 + 3)},${f1(wy0 + 32)} V${f1(wy0 + 10)} a${ww / 2 - 3},${ww / 2 - 6} 0 0 1 ${ww - 6},0" fill="none" stroke="#FFD9A0" stroke-width="1.4" opacity=".55"/>`);
      out.push(`<rect x="${f1(cx - ww / 2 - 6)}" y="${f1(wy0 + 30)}" width="${ww + 12}" height="9" rx="2" fill="${lighten(col, 0.16)}"/>`);
      out.push(`<path d="M${f1(cx - ww / 2 - 2)},${f1(wy0 + 39)} l6,9 h${ww - 8} l6,-9 z" fill="${edge}" opacity=".55"/>`);
      for (let j = 1; j < 4; j++) out.push(`<rect x="${f1(cx - ww / 2 - 4 + j * (ww + 8) / 4)}" y="${f1(wy0 + 30)}" width="1.6" height="9" fill="${edge}" opacity=".5"/>`);
    }
    // roof chhatri on some houses
    if (r() < 0.55) {
      const cx = x + w * (0.3 + r() * 0.4), cw = 30;
      out.push(`<rect x="${f1(cx - cw / 2)}" y="${f1(top - 30)}" width="${cw}" height="22" fill="${col}"/><rect x="${f1(cx - cw / 2 + 5)}" y="${f1(top - 25)}" width="${cw - 10}" height="17" fill="${deep}" opacity=".7"/>`);
      out.push(`<rect x="${f1(cx - cw / 2 - 4)}" y="${f1(top - 34)}" width="${cw + 8}" height="6" rx="2" fill="${lighten(col, 0.22)}"/>`);
      out.push(`<path d="${dome(cx, top - 34, 15, 1.35)}" fill="${lighten(col, 0.1)}"/><path d="${dome(cx + 3, top - 34, 9, 1.4)}" fill="#FFF3DA" opacity=".35"/>`);
    }
    x += w + 2 + r() * 8; k++;
  }
  return { defs: defs.join(''), body: out.join('') };
}

/** Near layer: shopfront row with colourful striped awnings right above the board. */
function shopRow(base) {
  const out = [], defs = [];
  const awn = ['#E8423D', '#2FA39A', '#FA8C29', '#387DEB', '#E2569A', '#45BF5C', '#9E5CDB', '#F2B53A'];
  const wallG = id('wall');
  defs.push(`<linearGradient id="${wallG}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#E3A27A"/><stop offset="1" stop-color="#C9835E"/></linearGradient>`);
  out.push(`<rect x="0" y="${base - 82}" width="1024" height="90" fill="url(#${wallG})"/>`);
  let x = -12, k = 0;
  const r = rng(91);
  while (x < 1040) {
    const w = 104 + r() * 30, c = mix(awn[k % awn.length], '#FBE3C8', 0.12);
    const ag = id('awn');
    defs.push(grad(ag, c, { top: 0.25, bottom: 0.25 }));
    // dark shop opening with hints of goods
    const op = id('op');
    defs.push(`<linearGradient id="${op}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#3B1B10"/><stop offset="1" stop-color="#6A3420"/></linearGradient>`);
    out.push(`<rect x="${f1(x + 8)}" y="${base - 46}" width="${f1(w - 16)}" height="46" fill="url(#${op})"/>`);
    for (let gx = x + 18; gx < x + w - 14; gx += 13) {
      const gc = awn[(k + Math.floor(gx / 13)) % awn.length];
      out.push(`<circle cx="${f1(gx)}" cy="${base - 12 - (gx % 3) * 3}" r="5" fill="${mix(gc, '#6A3420', 0.25)}"/><circle cx="${f1(gx - 1.5)}" cy="${base - 14 - (gx % 3) * 3}" r="1.6" fill="#FFE3B8" opacity=".6"/>`);
    }
    out.push(`<rect x="${f1(x + 4)}" y="${base - 48}" width="6" height="48" fill="#B9714C"/><rect x="${f1(x + w - 10)}" y="${base - 48}" width="6" height="48" fill="#B9714C"/>`);
    // awning: sloped canopy with stripes + scalloped hem
    const t = base - 78, b = base - 46, L = x + 2, R = x + w - 2;
    const clip = id('ac');
    defs.push(`<clipPath id="${clip}"><path d="M${f1(L + 6)},${t} H${f1(R - 6)} L${f1(R + 4)},${b} H${f1(L - 4)} Z"/></clipPath>`);
    out.push(`<path d="M${f1(L + 6)},${t} H${f1(R - 6)} L${f1(R + 4)},${b} H${f1(L - 4)} Z" fill="url(#${ag})"/>`);
    const sw = (R - L) / 7;
    const stripes = [];
    for (let i = 0; i < 8; i += 2) stripes.push(`<path d="M${f1(L + 6 + i * (R - L - 12) / 7)},${t} h${f1((R - L - 12) / 7)} L${f1(L - 4 + (i + 1) * (R - L + 8) / 7)},${b} h${f1(-(R - L + 8) / 7)} Z" fill="#FFF6E6" opacity=".88"/>`);
    out.push(`<g clip-path="url(#${clip})">${stripes.join('')}<rect x="${L - 6}" y="${t}" width="${R - L + 12}" height="8" fill="#fff" opacity=".22"/><rect x="${L - 6}" y="${b - 6}" width="${R - L + 12}" height="6" fill="#2a0f08" opacity=".12"/></g>`);
    let hem = `M${f1(L - 4)},${b}`;
    for (let i = 0; i < 7; i++) hem += ` a${f1(sw * 0.57)},${f1(sw * 0.5)} 0 0 0 ${f1((R - L + 8) / 7)},0`;
    out.push(`<path d="${hem} Z" fill="${c}"/><path d="${hem}" fill="none" stroke="${line(c)}" stroke-width="1.4" opacity=".55"/>`);
    out.push(`<path d="M${f1(L + 6)},${t} H${f1(R - 6)}" stroke="${darken(c, 0.3)}" stroke-width="3" stroke-linecap="round"/>`);
    x += w + 2; k++;
  }
  return { defs: defs.join(''), body: out.join('') };
}

function sack(cx, cy, s, tone, weave, open = null) {
  const g = id('sk'), gs = id('sk');
  const body = `M-46,44 C-60,6 -50,-28 -26,-40 C-22,-46 -24,-52 -16,-56 L16,-56 C24,-52 22,-46 26,-40 C50,-28 60,6 46,44 C30,54 -30,54 -46,44 Z`;
  let top = '';
  if (open) {
    top = `<ellipse cx="0" cy="-50" rx="24" ry="9" fill="${darken(tone, 0.3)}"/>
      <ellipse cx="0" cy="-51" rx="20" ry="7" fill="url(#${gs})"/>
      ${Array.from({ length: 9 }, (_, i) => `<ellipse cx="${-12 + (i % 5) * 6}" cy="${-53 + Math.floor(i / 5) * 4}" rx="3.2" ry="1.8" fill="${lighten(open, 0.35)}" opacity=".8"/>`).join('')}
      <path d="M-26,-44 C-30,-54 -20,-62 -14,-58 M26,-44 C30,-54 20,-62 14,-58" stroke="${darken(tone, 0.15)}" stroke-width="5" fill="none" stroke-linecap="round"/>`;
  } else {
    top = `<path d="M-14,-56 C-20,-66 -10,-74 0,-70 C10,-74 20,-66 14,-56 Z" fill="url(#${g})" stroke="${line(tone)}" stroke-width="2" stroke-opacity=".6"/>
      <path d="M-18,-54 C-6,-50 6,-50 18,-54" stroke="#8A5A2B" stroke-width="5" fill="none" stroke-linecap="round"/>
      <path d="M-18,-54 C-6,-50 6,-50 18,-54" stroke="#D2A868" stroke-width="1.6" fill="none" stroke-linecap="round" stroke-dasharray="3 3"/>`;
  }
  return `<g transform="translate(${cx},${cy}) scale(${s})">
    <defs>${ball(g, tone, { hi: 0.32, lo: 0.42, cx: '62%', cy: '28%' })}${ball(gs, open || tone, { hi: 0.3, lo: 0.3 })}</defs>
    <ellipse cx="0" cy="46" rx="58" ry="14" fill="#1E0B05" opacity=".45" filter="url(#blur4)"/>
    <path d="${body}" fill="url(#${g})"/>
    <path d="${body}" fill="url(#${weave})" opacity=".55"/>
    <path d="${body}" fill="none" stroke="${line(tone)}" stroke-width="2.2" stroke-opacity=".7"/>
    <path d="M-34,-10 C-14,0 14,0 34,-10" stroke="#7A2E1C" stroke-width="5" fill="none" opacity=".5"/>
    <path d="M-36,4 C-14,14 14,14 36,4" stroke="#2F6B3A" stroke-width="3" fill="none" opacity=".4"/>
    <path d="M26,-30 C42,-18 46,8 38,30" stroke="#FFF0CC" stroke-width="6" fill="none" opacity=".25" stroke-linecap="round"/>
    ${top}</g>`;
}

function basket(cx, cy, s, fill, weave) {
  const g = id('bk'), gf = id('bk');
  const items = [];
  const r = rng(Math.round(cx * 7 + cy));
  for (let i = 0; i < 22; i++) {
    const a = r() * Math.PI * 2, d = Math.sqrt(r()) * 34;
    items.push(`<circle cx="${f1(Math.cos(a) * d)}" cy="${f1(Math.sin(a) * d * 0.45 - 20)}" r="${f1(7 + r() * 3)}" fill="url(#${gf})"/>`);
  }
  return `<g transform="translate(${cx},${cy}) scale(${s})">
    <defs>${grad(g, '#C08A48', { top: 0.3, bottom: 0.4 })}${ball(gf, fill, { hi: 0.45, lo: 0.3 })}</defs>
    <ellipse cx="0" cy="34" rx="62" ry="13" fill="#1E0B05" opacity=".45" filter="url(#blur4)"/>
    <path d="M-52,-18 L-40,30 C-20,40 20,40 40,30 L52,-18 Z" fill="url(#${g})"/>
    <path d="M-52,-18 L-40,30 C-20,40 20,40 40,30 L52,-18 Z" fill="url(#${weave})" opacity=".5"/>
    <ellipse cx="0" cy="-18" rx="52" ry="16" fill="#6E431F"/>
    ${items.join('')}
    <ellipse cx="0" cy="-18" rx="52" ry="16" fill="none" stroke="#D9A866" stroke-width="6"/>
    <ellipse cx="0" cy="-18" rx="52" ry="16" fill="none" stroke="#8A5A2B" stroke-width="2" stroke-dasharray="4 3"/>
    <path d="M-50,-8 C-20,4 20,4 50,-8 M-46,10 C-20,22 20,22 46,10" stroke="#8A5A2B" stroke-width="2.4" fill="none" opacity=".6"/>
  </g>`;
}

export function background() {
  const HZ = wy(5.75), FB = wy(-2.4);           // 549, 1222
  const r = rng(11);
  const sky = id('sky'), sun = id('sun'), sunC = id('sunc'), cloud = id('cl'), floorP = id('flp'), floorL = id('fll'), floorS = id('fls');
  const beam = id('bm'), plank = id('pl'), gdTop = id('gdt'), gdVig = id('gdv'), shaft = id('sh'), weave = id('wv'), border = id('bd'), borderP = id('bdp'), grain = id('gr');
  const T = U;                                     // floor tile = 1 world unit
  const ox = wx(0) % (4 * T), oy = HZ % (4 * T);
  const hv = havelis(HZ - 66, r), shops = shopRow(HZ - 6);

  // one floor tile (sandstone) with a soft bevel and a quiet star inlay
  const tile = (x, y, c) => `
    <rect x="${x}" y="${y}" width="${T}" height="${T}" fill="${c}"/>
    <rect x="${x}" y="${y}" width="${T}" height="${T}" fill="none" stroke="#C9976A" stroke-width="1.6" opacity=".35"/>
    <path d="M${x + 2},${y + T - 2} V${y + 2} H${x + T - 2}" stroke="#FFF7E6" stroke-width="2" fill="none" opacity=".45"/>
    <path d="M${x + T / 2},${y + T / 2 - 15} L${x + T / 2 + 5},${y + T / 2 - 5} L${x + T / 2 + 15},${y + T / 2} L${x + T / 2 + 5},${y + T / 2 + 5} L${x + T / 2},${y + T / 2 + 15} L${x + T / 2 - 5},${y + T / 2 + 5} L${x + T / 2 - 15},${y + T / 2} L${x + T / 2 - 5},${y + T / 2 - 5} Z" fill="#E2B985" opacity=".3"/>`;
  const corner = (x, y) => `<path d="M${x},${y - 9} L${x + 9},${y} L${x},${y + 9} L${x - 9},${y} Z" fill="#D99468" opacity=".55"/><path d="M${x},${y - 4} L${x + 4},${y} L${x},${y + 4} L${x - 4},${y} Z" fill="#F6CF9A" opacity=".7"/>`;

  const tr = rng(5), tones = ['#F0D0A2', '#EECC9C', '#F1D3A6', '#ECC898'];
  let floorTiles = '';
  for (let j = 0; j < 4; j++) for (let i = 0; i < 4; i++) floorTiles += tile(i * T, j * T, tones[Math.floor(tr() * tones.length)]);
  for (let j = 0; j <= 4; j++) for (let i = 0; i <= 4; i++) floorTiles += corner(i * T, j * T);

  // godown wood: 3 planks of 64 with grain, knots and staggered end joints
  const PW = 64, PH = 640;
  const planks = [];
  const pTone = ['#8C5532', '#7E4A2B', '#955E38'];
  for (let i = 0; i < 3; i++) {
    const x = i * PW, c = pTone[i], pg = id('pg');
    planks.push(`<defs><linearGradient id="${pg}" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="${darken(c, 0.12)}"/><stop offset=".25" stop-color="${lighten(c, 0.06)}"/><stop offset=".8" stop-color="${c}"/><stop offset="1" stop-color="${darken(c, 0.18)}"/></linearGradient></defs>
      <rect x="${x}" y="0" width="${PW}" height="${PH}" fill="url(#${pg})"/>`);
    for (let gI = 0; gI < 5; gI++) {
      const gx = x + 8 + gI * 11 + (i * 3) % 5;
      let d = `M${gx},0`;
      for (let y = 40; y <= PH; y += 40) d += ` Q${gx + ((y / 40 + gI + i) % 2 ? 3 : -3)},${y - 20} ${gx},${y}`;
      planks.push(`<path d="${d}" stroke="${darken(c, 0.35)}" stroke-width="1.2" fill="none" opacity=".35"/>`);
    }
    const ky = 120 + i * 190;
    planks.push(`<ellipse cx="${x + 30 + i * 4}" cy="${ky}" rx="6" ry="13" fill="${darken(c, 0.4)}" opacity=".5"/><ellipse cx="${x + 30 + i * 4}" cy="${ky}" rx="11" ry="22" fill="none" stroke="${darken(c, 0.3)}" stroke-width="1.2" opacity=".4"/>`);
    const jy = [210, 470, 330][i];
    planks.push(`<rect x="${x}" y="${jy}" width="${PW}" height="3" fill="#3A1C0E" opacity=".7"/><rect x="${x}" y="${jy + 3}" width="${PW}" height="2" fill="${lighten(c, 0.25)}" opacity=".45"/>`);
    for (const ny of [jy - 12, jy + 14]) for (const nx of [x + 10, x + PW - 10]) planks.push(`<circle cx="${nx}" cy="${ny}" r="2.6" fill="#4A2615"/><circle cx="${nx - 0.7}" cy="${ny - 0.7}" r="1" fill="#D9A06A" opacity=".6"/>`);
    planks.push(`<rect x="${x}" y="0" width="3" height="${PH}" fill="#2E150A" opacity=".85"/><rect x="${x + 3}" y="0" width="2" height="${PH}" fill="${lighten(c, 0.3)}" opacity=".3"/>`);
  }

  // carved border band between board and godown
  const bTop = FB - 20, bH = 40;
  const sacks = [
    sack(wx(-5.45), wy(-3.3), 1.0, '#BF9660', weave),
    sack(wx(-4.75), wy(-3.85), 0.9, '#CDA86C', weave, '#E9E2CF'),
    basket(wx(-5.1), wy(-5.05), 0.88, '#FA8C29', weave),
    sack(wx(-5.25), wy(-8.0), 1.08, '#C29A62', weave, '#D8432F'),
    basket(wx(-4.75), wy(-9.05), 0.82, '#45BF5C', weave),
    sack(wx(-5.4), wy(-10.3), 1.0, '#CDA86C', weave),
    sack(wx(5.45), wy(-4.5), 1.05, '#BF9660', weave),
    sack(wx(4.8), wy(-5.15), 0.92, '#CDA86C', weave, '#F2C14A'),
    basket(wx(5.3), wy(-6.45), 0.88, '#E8423D', weave),
    sack(wx(5.3), wy(-9.2), 1.08, '#C29A62', weave, '#E9E2CF'),
    basket(wx(4.8), wy(-10.1), 0.85, '#FACC2E', weave),
  ];

  // soft clouds
  const clouds = [[180, 250, 1.1], [690, 150, 0.9], [430, 380, 0.7], [960, 330, 0.8], [90, 60, 0.8]]
    .map(([cx, cy, s]) => `<g transform="translate(${cx},${cy}) scale(${s})" opacity=".85">
      <ellipse cx="0" cy="0" rx="130" ry="26" fill="url(#${cloud})"/><ellipse cx="-30" cy="-12" rx="60" ry="26" fill="url(#${cloud})"/><ellipse cx="30" cy="-8" rx="70" ry="22" fill="url(#${cloud})"/></g>`).join('');

  // kites far off in the sky
  const kite = (x, y, s, c, rot) => `<g transform="translate(${x},${y}) rotate(${rot}) scale(${s})" opacity=".55"><path d="M0,-14 L11,0 L0,16 L-11,0 Z" fill="${c}"/><path d="M0,-14 V16 M-11,0 C-4,-5 4,-5 11,0" stroke="#fff" stroke-width="1" fill="none" opacity=".6"/><path d="M0,16 C-5,24 6,30 -2,40" stroke="${c}" stroke-width="1.2" fill="none"/></g>`;

  const body = `<defs>${kitDefs()}
    <linearGradient id="${sky}" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#FFF7EA"/><stop offset=".45" stop-color="#FFF0DA"/><stop offset=".75" stop-color="#FFDDB6"/><stop offset="1" stop-color="#FCC497"/></linearGradient>
    <radialGradient id="${sun}" cx="50%" cy="50%" r="50%"><stop offset="0" stop-color="#FFF6D8" stop-opacity=".95"/><stop offset=".3" stop-color="#FFE7B0" stop-opacity=".55"/><stop offset="1" stop-color="#FFD39A" stop-opacity="0"/></radialGradient>
    <radialGradient id="${sunC}" cx="45%" cy="40%" r="55%"><stop offset="0" stop-color="#FFFDF2"/><stop offset="1" stop-color="#FFE9B5"/></radialGradient>
    <radialGradient id="${cloud}" cx="50%" cy="40%" r="50%"><stop offset="0" stop-color="#FFFFFF" stop-opacity=".9"/><stop offset=".6" stop-color="#FFF6EA" stop-opacity=".5"/><stop offset="1" stop-color="#FFF3E2" stop-opacity="0"/></radialGradient>
    <pattern id="${floorP}" patternUnits="userSpaceOnUse" x="${ox}" y="${oy}" width="${4 * T}" height="${4 * T}">
      ${floorTiles}
    </pattern>
    <radialGradient id="${floorL}" cx="512" cy="${(HZ + FB) / 2 - 40}" r="620" gradientUnits="userSpaceOnUse" gradientTransform="translate(512 ${(HZ + FB) / 2 - 40}) scale(1 .78) translate(-512 ${-((HZ + FB) / 2 - 40)})">
      <stop offset="0" stop-color="#FFF8E8" stop-opacity=".35"/><stop offset=".45" stop-color="#FFF4E0" stop-opacity="0"/><stop offset=".8" stop-color="#9A5430" stop-opacity=".14"/><stop offset="1" stop-color="#7A3A1A" stop-opacity=".34"/></radialGradient>
    <linearGradient id="${floorS}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#5A2A12" stop-opacity=".35"/><stop offset="1" stop-color="#5A2A12" stop-opacity="0"/></linearGradient>
    <pattern id="${plank}" patternUnits="userSpaceOnUse" x="${wx(0) % (3 * PW) - PW / 2}" y="${FB}" width="${3 * PW}" height="${PH}">${planks.join('')}</pattern>
    <linearGradient id="${gdTop}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#1E0B04" stop-opacity=".7"/><stop offset=".12" stop-color="#1E0B04" stop-opacity=".2"/><stop offset=".3" stop-color="#1E0B04" stop-opacity="0"/><stop offset="1" stop-color="#1E0B04" stop-opacity=".35"/></linearGradient>
    <radialGradient id="${gdVig}" cx="512" cy="${FB + 260}" r="700" gradientUnits="userSpaceOnUse" gradientTransform="translate(512 ${FB + 260}) scale(.9 1.25) translate(-512 ${-(FB + 260)})">
      <stop offset="0" stop-color="#FFC27A" stop-opacity=".22"/><stop offset=".45" stop-color="#FFB060" stop-opacity="0"/><stop offset=".85" stop-color="#1A0802" stop-opacity=".45"/><stop offset="1" stop-color="#1A0802" stop-opacity=".7"/></radialGradient>
    <linearGradient id="${shaft}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#FFD9A0" stop-opacity=".22"/><stop offset="1" stop-color="#FFD9A0" stop-opacity="0"/></linearGradient>
    <pattern id="${weave}" patternUnits="userSpaceOnUse" width="8" height="8"><rect width="8" height="8" fill="none"/><path d="M0,2 H8 M0,6 H8" stroke="#5A3A18" stroke-width="1" opacity=".35"/><path d="M2,0 V4 M6,4 V8" stroke="#FFF0CC" stroke-width="1" opacity=".3"/><path d="M6,0 V4 M2,4 V8" stroke="#5A3A18" stroke-width="1" opacity=".25"/></pattern>
    <linearGradient id="${border}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#F2B98A"/><stop offset=".5" stop-color="#D98C5A"/><stop offset="1" stop-color="#A9582F"/></linearGradient>
    <pattern id="${borderP}" patternUnits="userSpaceOnUse" x="${wx(0) % 48}" y="${bTop}" width="48" height="${bH}">
      <path d="M4,${bH - 8} V18 a20,12 0 0 1 40,0 V${bH - 8}" fill="#B9643A" opacity=".55"/>
      <path d="M8,${bH - 8} V19 a16,9 0 0 1 32,0 V${bH - 8} Z" fill="#8E4424" opacity=".55"/>
      <path d="M24,13 l4,6 l-4,6 l-4,-6 z" fill="#FFD27A"/>
      <path d="M8,19 a16,9 0 0 1 32,0" stroke="#FFE1B0" stroke-width="1.5" fill="none" opacity=".7"/>
    </pattern>
    <filter id="${grain}" x="0" y="0" width="100%" height="100%"><feTurbulence type="fractalNoise" baseFrequency=".9" numOctaves="2" seed="4"/><feColorMatrix values="0 0 0 0 .35  0 0 0 0 .18  0 0 0 0 .08  0 0 0 -1.4 1.05"/></filter>
    <filter id="${beam}" x="-10%" y="-10%" width="120%" height="120%"><feGaussianBlur stdDeviation="28"/></filter>
    ${hv.defs}${shops.defs}
  </defs>
  <rect width="1024" height="${HZ}" fill="url(#${sky})"/>
  <circle cx="800" cy="${HZ - 230}" r="300" fill="url(#${sun})"/>
  <circle cx="800" cy="${HZ - 230}" r="54" fill="url(#${sunC})" opacity=".95"/>
  ${clouds}
  ${kite(160, 380, 0.9, '#E2569A', -12)}${kite(905, 245, 0.7, '#387DEB', 10)}${kite(560, 300, 0.55, '#FA8C29', -6)}
  ${farSkyline(HZ - 120, '#F0C7A8', '#FBE5CF')}
  <rect x="0" y="${HZ - 200}" width="1024" height="100" fill="#FDE6CC" opacity=".3"/>
  ${hv.body}
  
  ${shops.body}
  <rect x="0" y="${HZ}" width="1024" height="${FB - HZ}" fill="url(#${floorP})"/>
  <rect x="0" y="${HZ}" width="1024" height="${FB - HZ}" fill="url(#${floorL})"/>
  <path d="M780,${HZ} L1060,${HZ} L1060,${HZ + 300} L460,${FB + 40} L60,${FB + 40} Z" fill="#FFF3D6" opacity=".24" filter="url(#${beam})"/>
  <rect x="0" y="${HZ}" width="1024" height="${FB - HZ}" filter="url(#${grain})" opacity=".16"/>
  <rect x="0" y="${HZ + 12}" width="1024" height="60" fill="url(#${floorS})"/>
  <rect x="0" y="${HZ - 8}" width="1024" height="14" fill="#E9B286"/><rect x="0" y="${HZ - 8}" width="1024" height="3" fill="#FFE2BC"/><rect x="0" y="${HZ + 6}" width="1024" height="6" fill="#A8603A"/>
  <rect x="0" y="${bTop}" width="1024" height="${2048 - bTop}" fill="url(#${plank})"/>
  <rect x="0" y="${bTop}" width="1024" height="${2048 - bTop}" filter="url(#${grain})" opacity=".22"/>
  <g filter="url(#${beam})"><path d="M300,${FB + 20} L440,${FB + 20} L250,2048 L30,2048 Z" fill="url(#${shaft})"/>
  <path d="M560,${FB + 20} L650,${FB + 20} L530,2048 L370,2048 Z" fill="url(#${shaft})" opacity=".8"/></g>
  <rect x="0" y="${FB}" width="1024" height="${2048 - FB}" fill="url(#${gdTop})"/>
  <rect x="0" y="${FB}" width="1024" height="${2048 - FB}" fill="url(#${gdVig})"/>
  ${sacks.join('')}
  <rect x="0" y="${bTop + bH}" width="1024" height="26" fill="url(#${floorS})" opacity="1.6"/>
  <rect x="0" y="${bTop}" width="1024" height="${bH}" fill="url(#${border})"/>
  <rect x="0" y="${bTop}" width="1024" height="${bH}" fill="url(#${borderP})"/>
  <rect x="0" y="${bTop}" width="1024" height="4" fill="#FFE6C2"/><rect x="0" y="${bTop + 4}" width="1024" height="2" fill="#8E4424" opacity=".5"/>
  <rect x="0" y="${bTop + bH - 6}" width="1024" height="6" fill="#7A3418"/><rect x="0" y="${bTop + bH}" width="1024" height="2" fill="#2A0F08" opacity=".6"/>`;
  return svg(2048, 4096, body, '0 0 1024 2048');
}

// ============================================================ rangoli
export function rangoli(size = 1024) {
  const pw = id('pw'), out = [], defs = [];
  // powder palette: softer, chalky versions of the game colours
  const P = { pink: '#E2779B', saffron: '#F0983C', yellow: '#F4C95A', teal: '#3FA898', blue: '#5F8FDB', violet: '#9C7AD2', green: '#6CBB70', white: '#FFF8EC', red: '#D8644F', maroon: '#B9505A' };
  const pg = (c) => { const g = id('rg'); defs.push(`<radialGradient id="${g}" cx="50%" cy="60%" r="65%"><stop offset="0" stop-color="${lighten(c, 0.28)}"/><stop offset=".7" stop-color="${c}"/><stop offset="1" stop-color="${darken(c, 0.12)}"/></radialGradient>`); return g; };
  const petal = (r, len, w, rot, c, outline = P.white, tip = 1) => {
    const g = pg(c);
    const d = `M50,${50 - r} C${50 - w},${50 - r - len * 0.3} ${50 - w * 0.7},${50 - r - len * 0.8} 50,${50 - r - len * tip} C${50 + w * 0.7},${50 - r - len * 0.8} ${50 + w},${50 - r - len * 0.3} 50,${50 - r} Z`;
    return `<path d="${d}" transform="rotate(${rot} 50 50)" fill="url(#${g})" stroke="${outline}" stroke-width="1" stroke-linejoin="round"/>`;
  };
  const ringOf = (n, fn) => Array.from({ length: n }, (_, k) => fn(k * 360 / n, k)).join('');
  const dots = (n, r, rr, c, off = 0) => ringOf(n, a => { const t = (a + off) * Math.PI / 180; return `<circle cx="${(50 + r * Math.sin(t)).toFixed(2)}" cy="${(50 - r * Math.cos(t)).toFixed(2)}" r="${rr}" fill="${c}"/>`; });

  out.push(`<circle cx="50" cy="50" r="47.5" fill="${P.white}" opacity=".4"/>`);
  out.push(`<circle cx="50" cy="50" r="38" fill="none" stroke="${P.maroon}" stroke-width="15" opacity=".38"/>`);
  out.push(`<circle cx="50" cy="50" r="19" fill="${P.saffron}" opacity=".25"/>`);
  out.push(`<circle cx="50" cy="50" r="46" fill="none" stroke="${P.saffron}" stroke-width=".9" stroke-dasharray=".1 2.1" stroke-linecap="round"/>`);
  out.push(dots(40, 44, 1.05, P.pink));
  out.push(dots(40, 44, 0.45, P.white));
  // outer scalloped lotus band
  out.push(ringOf(16, a => petal(26, 16, 7.5, a, P.pink)));
  out.push(ringOf(16, a => petal(29, 10, 3.6, a, mix(P.pink, P.white, 0.55), 'none')));
  out.push(ringOf(16, a => petal(30, 12, 3.2, a + 11.25, P.teal, P.white, 1)));
  out.push(dots(16, 40.5, 0.9, P.yellow, 0));
  // saffron mid ring
  out.push(`<circle cx="50" cy="50" r="26" fill="${P.white}" opacity=".5"/>`);
  out.push(ringOf(12, a => petal(14, 12.5, 6.5, a + 15, P.saffron)));
  out.push(ringOf(12, a => petal(16, 7, 2.6, a + 15, P.yellow, 'none')));
  out.push(dots(24, 24.5, 0.8, P.violet, 7.5));
  // inner blue/violet flower
  out.push(ringOf(8, a => petal(5, 10, 5, a + 22.5, P.blue)));
  out.push(ringOf(8, a => petal(6, 7, 3.5, a, P.violet)));
  out.push(`<circle cx="50" cy="50" r="6" fill="url(#${pg(P.green)})" stroke="${P.white}" stroke-width=".7"/>`);
  out.push(`<circle cx="50" cy="50" r="3" fill="url(#${pg(P.yellow)})"/>`);
  out.push(dots(8, 4.6, 0.55, P.white, 22.5));
  out.push(`<circle cx="50" cy="50" r="1.1" fill="${P.red}"/>`);

  defs.push(`<filter id="${pw}" x="-5%" y="-5%" width="110%" height="110%">
    <feTurbulence type="fractalNoise" baseFrequency="2.2" numOctaves="2" seed="3" result="n"/>
    <feDisplacementMap in="SourceGraphic" in2="n" scale=".9" xChannelSelector="R" yChannelSelector="G" result="d"/>
    <feTurbulence type="fractalNoise" baseFrequency="4.5" numOctaves="1" seed="8" result="g"/>
    <feColorMatrix in="g" type="matrix" values="0 0 0 0 1  0 0 0 0 1  0 0 0 0 1  0 0 0 -1.3 1.25" result="ga"/>
    <feComposite in="d" in2="ga" operator="in" result="speck"/>
    <feGaussianBlur in="speck" stdDeviation=".12" result="soft"/>
    <feMerge><feMergeNode in="soft"/></feMerge>
  </filter>`);
  return svg(size, size, `<defs>${kitDefs()}${defs.join('')}</defs><g filter="url(#${pw})">${out.join('')}</g>`);
}

// ============================================================ bunting
export function bunting(w = 2048, h = 256) {
  const cols = ['#E8423D', '#FACC2E', '#2FA39A', '#387DEB', '#FA8C29', '#E2569A', '#45BF5C', '#9E5CDB'];
  const sag = x => 26 + 74 * Math.sin(Math.PI * x / w);
  const slope = x => 74 * Math.PI / w * Math.cos(Math.PI * x / w);
  const defs = [], flags = [];
  const step = 92, fw = 72, fl = 112;
  let k = 0;
  for (let x = 30; x < w - 40; x += step, k++) {
    const c = cols[k % cols.length], cx = x + fw / 2, y = sag(cx), ang = Math.atan(slope(cx)) * 180 / Math.PI;
    const sway = ((k * 37) % 7 - 3) * 1.2;
    const g = id('fl'), sh = id('fs');
    defs.push(`<linearGradient id="${g}" x1="0" y1="0" x2=".35" y2="1"><stop offset="0" stop-color="${lighten(c, 0.3)}"/><stop offset=".45" stop-color="${c}"/><stop offset="1" stop-color="${darken(c, 0.3)}"/></linearGradient>`);
    defs.push(`<linearGradient id="${sh}" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#fff" stop-opacity=".18"/><stop offset=".45" stop-color="#fff" stop-opacity="0"/><stop offset=".55" stop-color="#2a0f08" stop-opacity=".0"/><stop offset="1" stop-color="#2a0f08" stop-opacity=".22"/></linearGradient>`);
    const hw = fw / 2;
    const tri = `M${-hw},4 C${-hw * 0.6},${fl * 0.4} ${-hw * 0.25},${fl * 0.75} 0,${fl} C${hw * 0.25},${fl * 0.75} ${hw * 0.6},${fl * 0.4} ${hw},4 Z`;
    const inset = `M${-hw + 8},11 C${-hw * 0.5},${fl * 0.42} ${-hw * 0.2},${fl * 0.72} 0,${fl - 12} C${hw * 0.2},${fl * 0.72} ${hw * 0.5},${fl * 0.42} ${hw - 8},11`;
    // block-print motif alternates between dots and a small flower
    const motif = k % 2
      ? [[0, 40], [-12, 26], [12, 26], [0, 62]].map(([mx, my]) => `<circle cx="${mx}" cy="${my}" r="3.4" fill="${CREAM}" opacity=".8"/><circle cx="${mx}" cy="${my}" r="1.3" fill="${darken(c, 0.2)}"/>`).join('')
      : `<g transform="translate(0,40)">${[0, 72, 144, 216, 288].map(a => `<ellipse cx="0" cy="-6" rx="3.4" ry="6" fill="${CREAM}" opacity=".82" transform="rotate(${a})"/>`).join('')}<circle r="3" fill="${GOLD}"/></g>`;
    flags.push(`<g transform="translate(${f1(cx)},${f1(y - 3)}) rotate(${f1(ang + sway)})">
      <g filter="url(#drop)">
        <path d="${tri}" fill="url(#${g})"/>
        <path d="${tri}" fill="url(#${sh})"/>
        <path d="M${-hw * 0.1},10 C${hw * 0.05},${fl * 0.45} ${hw * 0.0},${fl * 0.7} 0,${fl - 4}" stroke="${darken(c, 0.4)}" stroke-width="4" fill="none" opacity=".18" stroke-linecap="round"/>
        ${motif}
        <path d="${inset}" fill="none" stroke="${CREAM}" stroke-width="1.8" stroke-dasharray="5 3.5" stroke-linecap="round" opacity=".9"/>
        <path d="${tri}" fill="none" stroke="${line(c)}" stroke-width="2.2" stroke-linejoin="round"/>
        <rect x="${-hw - 2}" y="-3" width="${fw + 4}" height="12" rx="4" fill="${darken(c, 0.18)}"/>
        <rect x="${-hw - 2}" y="-3" width="${fw + 4}" height="4.5" rx="2" fill="${lighten(c, 0.35)}" opacity=".7"/>
        <rect x="${-hw - 2}" y="-3" width="${fw + 4}" height="12" rx="4" fill="none" stroke="${line(c)}" stroke-width="1.6"/>
      </g></g>`);
  }
  let d = `M0,${f1(sag(0))}`; for (let x = 8; x <= w; x += 8) d += ` L${x},${f1(sag(x))}`;
  return svg(w, h, `<defs>${kitDefs()}${defs.join('')}</defs>
    <path d="${d}" fill="none" stroke="#2a0f08" stroke-width="7" opacity=".18" transform="translate(0,5)" filter="url(#blur2)"/>
    <path d="${d}" fill="none" stroke="#6B3A1E" stroke-width="5"/>
    <path d="${d}" fill="none" stroke="#C9925A" stroke-width="2" stroke-dasharray="6 5"/>
    ${flags.join('')}`, `0 0 ${w} ${h}`);
}

// ============================================================ conveyor belt
// Horizontal strip in a 0..100 box; every feature repeats with period 25 along x so the tile is seamless.
function beltStrip() {
  const railG = id('rail'), railG2 = id('rail'), beltG = id('belt'), cleat = id('clt'), riv = id('riv'), tex = id('tex');
  const rivet = (x, y) => `<circle cx="${x}" cy="${y + 0.6}" r="2.6" fill="#5A3210" opacity=".45"/><circle cx="${x}" cy="${y}" r="2.3" fill="url(#${riv})"/><circle cx="${x - 0.7}" cy="${y - 0.8}" r=".7" fill="#FFF8D8"/>`;
  const xs = [12.5, 37.5, 62.5, 87.5], cx = [25, 75];
  return `<defs>
    <linearGradient id="${railG}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#FFE7A6"/><stop offset=".25" stop-color="#F0BE5C"/><stop offset=".7" stop-color="#C98A2E"/><stop offset="1" stop-color="#8A541A"/></linearGradient>
    <linearGradient id="${railG2}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#F6CF7A"/><stop offset=".35" stop-color="#E2A848"/><stop offset=".8" stop-color="#B57724"/><stop offset="1" stop-color="#7C4614"/></linearGradient>
    <linearGradient id="${beltG}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#3E2218"/><stop offset=".12" stop-color="#5E3A2A"/><stop offset=".5" stop-color="#6E4532"/><stop offset=".88" stop-color="#5A3626"/><stop offset="1" stop-color="#3A1F15"/></linearGradient>
    <linearGradient id="${cleat}" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#2E170E"/><stop offset=".35" stop-color="#8A5A42"/><stop offset=".55" stop-color="#A87458"/><stop offset="1" stop-color="#4A2A1C"/></linearGradient>
    <radialGradient id="${riv}" cx="35%" cy="30%" r="75%"><stop offset="0" stop-color="#FFF1BC"/><stop offset=".5" stop-color="#D9A43C"/><stop offset="1" stop-color="#7C4A14"/></radialGradient>
    <pattern id="${tex}" patternUnits="userSpaceOnUse" width="5" height="5"><path d="M0,5 L5,0" stroke="#000" stroke-width=".5" opacity=".12"/></pattern>
  </defs>
  <rect x="0" y="17" width="100" height="66" fill="url(#${beltG})"/>
  <rect x="0" y="17" width="100" height="66" fill="url(#${tex})"/>
  <rect x="0" y="44" width="100" height="12" fill="#B07A5C" opacity=".12"/>
  ${cx.map(x => `<rect x="${x - 4.5}" y="20" width="9" height="60" rx="2" fill="#1E0B05" opacity=".3" transform="translate(1.2,0)"/><rect x="${x - 4.5}" y="20" width="9" height="60" rx="2" fill="url(#${cleat})"/><rect x="${x - 2.2}" y="21" width="1.6" height="58" rx=".8" fill="#E0AA84" opacity=".5"/>`).join('')}
  ${[0, 50, 100].map(x => `<rect x="${x - .6}" y="20" width="1.2" height="60" fill="#2A140C" opacity=".45"/>`).join('')}
  <rect x="0" y="17" width="100" height="7" fill="#1E0B05" opacity=".35"/>
  <rect x="0" y="79" width="100" height="4" fill="#1E0B05" opacity=".2"/>
  <rect x="0" y="0" width="100" height="17" fill="url(#${railG})"/>
  <rect x="0" y="13.5" width="100" height="3.5" fill="#6B3A12"/>
  <rect x="0" y="1.2" width="100" height="1.4" fill="#FFF6D6" opacity=".7"/>
  <rect x="0" y="83" width="100" height="17" fill="url(#${railG2})"/>
  <rect x="0" y="83" width="100" height="2.2" fill="#FFE7A6" opacity=".8"/>
  <rect x="0" y="98" width="100" height="2" fill="#5A3210"/>
  ${xs.map(x => rivet(x, 7.5) + rivet(x, 91.5)).join('')}`;
}
export const beltH = () => svg(256, 256, `<defs>${kitDefs()}</defs>${beltStrip()}`);
export const beltV = () => svg(256, 256, `<defs>${kitDefs()}</defs><g transform="rotate(-90 50 50)">${beltStrip()}</g>`);

export function beltCorner() {
  const g = id('bc'), g2 = id('bc'), pit = id('bc'), riv = id('riv');
  const rivet = (x, y) => `<circle cx="${x}" cy="${y + 0.6}" r="2.8" fill="#5A3210" opacity=".45"/><circle cx="${x}" cy="${y}" r="2.5" fill="url(#${riv})"/><circle cx="${x - 0.8}" cy="${y - 0.9}" r=".8" fill="#FFF8D8"/>`;
  return svg(256, 256, `<defs>${kitDefs()}
    <linearGradient id="${g}" x1="0" y1="0" x2=".6" y2="1"><stop offset="0" stop-color="#FFE7A6"/><stop offset=".35" stop-color="#EDB656"/><stop offset=".8" stop-color="#C08030"/><stop offset="1" stop-color="#8A541A"/></linearGradient>
    <linearGradient id="${g2}" x1="0" y1="0" x2=".6" y2="1"><stop offset="0" stop-color="#8A541A"/><stop offset="1" stop-color="#F0BE5C"/></linearGradient>
    <radialGradient id="${pit}" cx="50%" cy="45%" r="60%"><stop offset="0" stop-color="#5E3A2A"/><stop offset=".75" stop-color="#3A1F15"/><stop offset="1" stop-color="#1E0B05"/></radialGradient>
    <radialGradient id="${riv}" cx="35%" cy="30%" r="75%"><stop offset="0" stop-color="#FFF1BC"/><stop offset=".5" stop-color="#D9A43C"/><stop offset="1" stop-color="#7C4A14"/></radialGradient>
  </defs>
  <rect x="1" y="1" width="98" height="98" rx="16" fill="url(#${g})"/>
  <rect x="1" y="1" width="98" height="98" rx="16" fill="none" stroke="#6B3A12" stroke-width="2"/>
  <rect x="4" y="4" width="92" height="92" rx="13" fill="none" stroke="#FFF2C8" stroke-width="1.4" opacity=".7"/>
  <rect x="15" y="15" width="70" height="70" rx="10" fill="url(#${g2})"/>
  <rect x="17" y="17" width="66" height="66" rx="9" fill="url(#${pit})"/>
  <rect x="17" y="17" width="66" height="66" rx="9" fill="none" stroke="#1E0B05" stroke-width="1.5" opacity=".6"/>
  ${[[9, 9], [91, 9], [9, 91], [91, 91], [50, 8.5], [50, 91.5], [8.5, 50], [91.5, 50]].map(([x, y]) => rivet(x, y)).join('')}`);
}

export function beltHub() {
  const rim = id('hb'), sp = id('hb'), cap = id('hb'), dk = id('hb');
  const spokes = Array.from({ length: 8 }, (_, i) => `<g transform="rotate(${i * 45} 50 50)"><path d="M47,20 L53,20 L54.5,44 L45.5,44 Z" fill="url(#${sp})"/><path d="M47,20 L48.5,44" stroke="#FFF2C0" stroke-width=".8" opacity=".6"/></g>`).join('');
  const bolts = Array.from({ length: 12 }, (_, i) => { const a = i * Math.PI / 6; return `<circle cx="${f1(50 + 35.5 * Math.cos(a))}" cy="${f1(50 + 35.5 * Math.sin(a))}" r="1.5" fill="#7C4A14"/><circle cx="${f1(50 + 35.5 * Math.cos(a) - 0.4)}" cy="${f1(50 + 35.5 * Math.sin(a) - 0.4)}" r=".6" fill="#FFF1BC"/>`; }).join('');
  return svg(256, 256, `<defs>${kitDefs()}
    <radialGradient id="${rim}" cx="50%" cy="50%" r="50%"><stop offset=".78" stop-color="#B57724"/><stop offset=".86" stop-color="#FFE29A"/><stop offset=".94" stop-color="#D9A43C"/><stop offset="1" stop-color="#7C4614"/></radialGradient>
    <linearGradient id="${sp}" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#F6CF7A"/><stop offset=".5" stop-color="#D9A43C"/><stop offset="1" stop-color="#8A541A"/></linearGradient>
    <radialGradient id="${cap}" cx="50%" cy="50%" r="50%"><stop offset="0" stop-color="#FFF3C4"/><stop offset=".55" stop-color="#E8B24A"/><stop offset=".85" stop-color="#B57724"/><stop offset="1" stop-color="#6B3A12"/></radialGradient>
    <radialGradient id="${dk}" cx="50%" cy="50%" r="50%"><stop offset=".7" stop-color="#2E170E"/><stop offset="1" stop-color="#4A2A1C"/></radialGradient>
  </defs>
  <circle cx="50" cy="51.5" r="44" fill="#1E0B05" opacity=".35" filter="url(#blur2)"/>
  <circle cx="50" cy="50" r="43" fill="url(#${rim})"/>
  <circle cx="50" cy="50" r="43" fill="none" stroke="#5A3210" stroke-width="1.6"/>
  <circle cx="50" cy="50" r="33" fill="url(#${dk})"/>
  ${spokes}
  ${bolts}
  <circle cx="50" cy="50" r="13" fill="url(#${cap})" stroke="#6B3A12" stroke-width="1.4"/>
  <circle cx="50" cy="50" r="8" fill="none" stroke="#FFF1BC" stroke-width="1" opacity=".7"/>
  <circle cx="50" cy="50" r="3.2" fill="#5A3210"/><circle cx="49" cy="49" r="1.1" fill="#FFF8D8"/>`);
}

// ============================================================ fx (white so the game can tint; coin is full colour)
const W = '#FFFFFF', W2 = '#EEF0F4';
export const fx = {
  sparkle() {
    const g = id('fx'), s = id('fx');
    const star = (r, w) => `M50,${50 - r} C${50 + w},${50 - w} ${50 + w},${50 - w} ${50 + r},50 C${50 + w},${50 + w} ${50 + w},${50 + w} 50,${50 + r} C${50 - w},${50 + w} ${50 - w},${50 + w} ${50 - r},50 C${50 - w},${50 - w} ${50 - w},${50 - w} 50,${50 - r} Z`;
    return svg(128, 128, `<defs>${kitDefs()}<radialGradient id="${g}"><stop offset="0" stop-color="${W}" stop-opacity=".85"/><stop offset=".3" stop-color="${W}" stop-opacity=".3"/><stop offset="1" stop-color="${W}" stop-opacity="0"/></radialGradient>
      <radialGradient id="${s}"><stop offset="0" stop-color="${W}"/><stop offset=".6" stop-color="${W}"/><stop offset="1" stop-color="${W2}"/></radialGradient></defs>
      <circle cx="50" cy="50" r="48" fill="url(#${g})"/>
      <path d="${star(46, 5)}" fill="url(#${s})"/>
      <path d="${star(26, 3)}" fill="${W}" transform="rotate(45 50 50)" opacity=".55"/>
      <circle cx="50" cy="50" r="6" fill="${W}"/>`);
  },
  glow() {
    const g = id('fx');
    return svg(256, 256, `<defs>${kitDefs()}<radialGradient id="${g}"><stop offset="0" stop-color="${W}"/><stop offset=".18" stop-color="${W}" stop-opacity=".75"/><stop offset=".45" stop-color="${W}" stop-opacity=".28"/><stop offset=".75" stop-color="${W}" stop-opacity=".07"/><stop offset="1" stop-color="${W}" stop-opacity="0"/></radialGradient></defs>
      <circle cx="50" cy="50" r="50" fill="url(#${g})"/>`);
  },
  ring() {
    const g = id('fx');
    return svg(256, 256, `<defs>${kitDefs()}<radialGradient id="${g}"><stop offset=".62" stop-color="${W}" stop-opacity="0"/><stop offset=".76" stop-color="${W}" stop-opacity=".55"/><stop offset=".82" stop-color="${W}" stop-opacity="1"/><stop offset=".88" stop-color="${W}" stop-opacity=".55"/><stop offset="1" stop-color="${W}" stop-opacity="0"/></radialGradient></defs>
      <circle cx="50" cy="50" r="50" fill="url(#${g})"/>`);
  },
  puff() {
    const g = id('fx'), sh = id('fx');
    const blobs = [[50, 56, 26], [32, 58, 18], [68, 58, 19], [42, 40, 20], [60, 38, 17], [50, 30, 12]];
    return svg(128, 128, `<defs>${kitDefs()}<radialGradient id="${g}" cx="40%" cy="35%" r="65%"><stop offset="0" stop-color="${W}"/><stop offset=".7" stop-color="${W}" stop-opacity=".95"/><stop offset="1" stop-color="#E4E7EE" stop-opacity=".85"/></radialGradient>
      <linearGradient id="${sh}" x1="0" y1="0" x2="0" y2="1"><stop offset=".5" stop-color="#C9CED8" stop-opacity="0"/><stop offset="1" stop-color="#C9CED8" stop-opacity=".55"/></linearGradient>
      <filter id="${g}b" x="-20%" y="-20%" width="140%" height="140%"><feGaussianBlur stdDeviation="2.2"/></filter></defs>
      <g filter="url(#${g}b)" opacity=".95">${blobs.map(([x, y, r]) => `<circle cx="${x}" cy="${y}" r="${r}" fill="url(#${g})"/>`).join('')}
      <path d="M14,62 C20,80 80,82 88,62 C80,74 22,74 14,62 Z" fill="url(#${sh})"/></g>`);
  },
  petal() {
    const g = id('fx');
    const d = 'M50,94 C30,88 18,64 20,40 C21,26 28,14 34,10 C38,14 42,8 46,6 C49,10 52,6 56,6 C59,10 63,8 66,11 C73,16 80,28 80,42 C81,66 70,88 50,94 Z';
    return svg(128, 128, `<defs>${kitDefs()}<linearGradient id="${g}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${W}"/><stop offset=".7" stop-color="#F6F6F8"/><stop offset="1" stop-color="#DADDE4"/></linearGradient></defs>
      <path d="${d}" fill="url(#${g})"/>
      <path d="M50,90 C49,70 48,40 50,12" stroke="#C4C8D2" stroke-width="2.2" fill="none" stroke-linecap="round"/>
      ${[[50, 70, 34, 52], [50, 70, 66, 52], [50, 52, 36, 32], [50, 52, 64, 32], [50, 36, 42, 18], [50, 36, 58, 18]].map(([x1, y1, x2, y2]) => `<path d="M${x1},${y1} Q${(x1 + x2) / 2},${y1 - 4} ${x2},${y2}" stroke="#CDD1DA" stroke-width="1.3" fill="none" stroke-linecap="round"/>`).join('')}
      <path d="M28,40 C28,56 34,72 44,84" stroke="${W}" stroke-width="3" fill="none" opacity=".9" stroke-linecap="round"/>
      <path d="${d}" fill="none" stroke="#D3D6DE" stroke-width="1.2"/>`);
  },
  confetti() {
    const g = id('fx');
    return svg(64, 64, `<defs>${kitDefs()}<linearGradient id="${g}" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#DCDFE6"/><stop offset=".3" stop-color="${W}"/><stop offset=".55" stop-color="${W}"/><stop offset="1" stop-color="#C9CDD6"/></linearGradient></defs>
      <path d="M14,30 C30,20 46,40 86,26 L88,58 C50,72 32,52 16,62 Z" fill="url(#${g})" stroke="#D0D4DC" stroke-width="1.5" stroke-linejoin="round"/>
      <path d="M18,40 C34,32 50,50 82,38" stroke="${W}" stroke-width="3" fill="none" opacity=".9" stroke-linecap="round"/>`);
  },
  coin() {
    const face = id('cn'), edge = id('cn'), star = id('cn'), inner = id('cn');
    const sp = (r, ri, n = 5) => Array.from({ length: n * 2 }, (_, i) => { const a = -Math.PI / 2 + i * Math.PI / n, rr = i % 2 ? ri : r; return `${i ? 'L' : 'M'}${f1(50 + rr * Math.cos(a))},${f1(48 + rr * Math.sin(a))}`; }).join(' ') + ' Z';
    const reed = Array.from({ length: 48 }, (_, i) => `<rect x="49.4" y="7.5" width="1.2" height="4" fill="#B87412" opacity=".55" transform="rotate(${i * 7.5} 50 48)"/>`).join('');
    return svg(256, 256, `<defs>${kitDefs()}
      <radialGradient id="${face}" cx="38%" cy="30%" r="80%"><stop offset="0" stop-color="#FFF6C4"/><stop offset=".35" stop-color="#FFD54A"/><stop offset=".75" stop-color="#F2A81E"/><stop offset="1" stop-color="#C9780C"/></radialGradient>
      <linearGradient id="${edge}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#E89A16"/><stop offset="1" stop-color="#9A5208"/></linearGradient>
      <linearGradient id="${star}" x1="0" y1="0" x2=".4" y2="1"><stop offset="0" stop-color="#FFFBE0"/><stop offset=".6" stop-color="#FFE17A"/><stop offset="1" stop-color="#F4B52C"/></linearGradient>
      <radialGradient id="${inner}" cx="60%" cy="70%" r="70%"><stop offset="0" stop-color="#FFD54A"/><stop offset="1" stop-color="#E39614"/></radialGradient>
    </defs>
    ${contact(50, 92, 32, 4, 0.3)}
    <circle cx="50" cy="53" r="41" fill="url(#${edge})"/>
    <circle cx="50" cy="53" r="41" fill="none" stroke="#6E3606" stroke-width="2"/>
    <circle cx="50" cy="48" r="41" fill="url(#${face})"/>
    ${reed}
    <circle cx="50" cy="48" r="41" fill="none" stroke="#8A4A08" stroke-width="2"/>
    <circle cx="50" cy="48" r="31" fill="url(#${inner})"/>
    <circle cx="50" cy="48" r="31" fill="none" stroke="#B87412" stroke-width="2"/>
    <circle cx="50" cy="48.8" r="30" fill="none" stroke="#FFF1B0" stroke-width="1.2" opacity=".6" stroke-dasharray="70 200" transform="rotate(20 50 48)"/>
    <path d="${sp(22, 9.5)}" fill="#A8600A" transform="translate(0.8,1.6)" opacity=".6" stroke-linejoin="round" stroke="#A8600A" stroke-width="2"/>
    <path d="${sp(22, 9.5)}" fill="url(#${star})" stroke="#C47E10" stroke-width="1.4" stroke-linejoin="round"/>
    <path d="M50,28 L53.5,40.5 L50,48 Z" fill="#fff" opacity=".45"/>
    ${gloss(36, 30, 12, 6, -35, 0.85)}
    ${glint(70, 30, 2)}${glint(30, 64, 1.1)}`);
  },
  kite() {
    const g = id('fx'), t = id('fx');
    return svg(256, 256, `<defs>${kitDefs()}<linearGradient id="${g}" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="${W}"/><stop offset=".55" stop-color="#F7F8FA"/><stop offset="1" stop-color="#DCDFE6"/></linearGradient>
      <linearGradient id="${t}" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#E4E7EE"/><stop offset="1" stop-color="${W}"/></linearGradient></defs>
      <path d="M50,58 C42,70 60,74 50,84 C42,92 54,96 50,99" fill="none" stroke="#D8DCE3" stroke-width="1.6"/>
      ${[[50, 68, -30], [51, 80, 25], [49, 91, -20]].map(([x, y, r]) => `<path d="M${x},${y} l-6,-3 l1,6 Z M${x},${y} l6,-3 l-1,6 Z" fill="url(#${t})" stroke="#C9CDD6" stroke-width=".6" transform="rotate(${r} ${x} ${y})"/>`).join('')}
      <path d="M50,4 L80,32 L50,62 L20,32 Z" fill="#000" opacity=".08" transform="translate(2,3)"/>
      <path d="M50,4 L80,32 L50,62 L20,32 Z" fill="url(#${g})" stroke="#C9CDD6" stroke-width="1.6" stroke-linejoin="round"/>
      <path d="M50,4 L80,32 L50,62 Z" fill="#E8EAEF" opacity=".55"/>
      <path d="M50,4 V62" stroke="#BFC4CE" stroke-width="1.8"/>
      <path d="M20,32 C32,20 68,20 80,32" stroke="#BFC4CE" stroke-width="1.6" fill="none"/>
      <path d="M50,48 L57,55 L50,62 L43,55 Z" fill="#E1E4EA"/>
      <path d="M27,31 L50,9" stroke="${W}" stroke-width="2.2" stroke-linecap="round" opacity=".9"/>`);
  },
  shadow() {
    const g = id('fx');
    return svg(256, 128, `<defs>${kitDefs()}<radialGradient id="${g}"><stop offset="0" stop-color="#000" stop-opacity=".55"/><stop offset=".45" stop-color="#000" stop-opacity=".32"/><stop offset=".8" stop-color="#000" stop-opacity=".08"/><stop offset="1" stop-color="#000" stop-opacity="0"/></radialGradient></defs>
      <ellipse cx="128" cy="64" rx="126" ry="62" fill="url(#${g})"/>`, '0 0 256 128');
  },
};

export function jobs() {
  return [
    ['background', background()],
    ['rangoli', rangoli()],
    ['bunting', bunting()],
    ['belt_h', beltH()],
    ['belt_v', beltV()],
    ['belt_corner', beltCorner()],
    ['belt_hub', beltHub()],
    ...Object.entries(fx).map(([k, f]) => [`fx_${k}`, f()]),
  ];
}
