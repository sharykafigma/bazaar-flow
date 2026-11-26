// v2 stalls: stall_0..5 (themed bazaar stalls), stall_closed / stall_lock overlays, gate (godown arch). All 512 px.
// Layout (0..100 viewBox): frame x4..96 y2..96, awning y2..34, counter y72..96. An order badge (r~26) covers (50,52),
// so all the charm lives in the awning, posts, side shelves/decor and the counter cloth.
import { kitDefs, grad, ball, gloss, glint, contact, line, lighten, darken, mix, svg, id } from '../style.mjs';

let COLORS = { red: '#E8423D', blue: '#387DEB', yellow: '#FACC2E', green: '#45BF5C', purple: '#9E5CDB', orange: '#FA8C29' };
try { ({ COLORS } = await import('./goods.mjs')); } catch { /* goods.mjs mid-edit: keep fallback palette */ }

const GOLD = '#F2B53A', CREAM = '#FFF4E0', WOOD = '#B86F35', WOOD_L = '#D9944F', CLAY = '#C8602E';

// ---------------------------------------------------------------- small helpers (defs are collected per sprite)
let D = [];
const lg = (c, o) => { const g = id('sg'); D.push(grad(g, c, o)); return `url(#${g})`; };
const rg = (c, o) => { const g = id('sb'); D.push(ball(g, c, o)); return `url(#${g})`; };
const raw = def => { const g = id('sx'); D.push(def(g)); return `url(#${g})`; };
const rr = (x, y, w, h, r) => `M${x + r},${y} H${x + w - r} Q${x + w},${y} ${x + w},${y + r} V${y + h - r} Q${x + w},${y + h} ${x + w - r},${y + h} H${x + r} Q${x},${y + h} ${x},${y + h - r} V${y + r} Q${x},${y} ${x + r},${y} Z`;
/** Shape with soft volume: gradient + inner rim light/shadow + coloured outline. */
const vol = (d, c, { w = 1, big = false, o, fill } = {}) =>
  `<path d="${d}" fill="${fill || lg(c, o)}" filter="url(#${big ? 'vol' : 'volS'})"/><path d="${d}" fill="none" stroke="${line(c)}" stroke-width="${w}" stroke-linejoin="round"/>`;
const flat = (d, fill, stroke, w = 0.8, extra = '') => `<path d="${d}" fill="${fill}" stroke="${stroke}" stroke-width="${w}" stroke-linejoin="round" stroke-linecap="round" ${extra}/>`;
const glow = (cx, cy, r, c, a = 0.8) => {
  const u = raw(g => `<radialGradient id="${g}"><stop offset="0" stop-color="${c}" stop-opacity="${a}"/><stop offset=".45" stop-color="${c}" stop-opacity="${a * 0.35}"/><stop offset="1" stop-color="${c}" stop-opacity="0"/></radialGradient>`);
  return `<circle cx="${cx}" cy="${cy}" r="${r}" fill="${u}"/>`;
};
const T = (x, y, s = 1, r = 0) => `transform="translate(${x},${y}) rotate(${r}) scale(${s})"`;

// ---------------------------------------------------------------- mini goods (drawn around their base/centre at 0,0)
function mango(x, y, s = 1, r = 0) {
  const body = 'M-4.6,0.6 C-5.2,-3.6 -1.6,-5.4 1.6,-4.8 C5,-4.2 5.6,-0.6 4.4,2 C3,4.8 -2.2,5.2 -3.8,3.2 C-4.4,2.4 -4.5,1.6 -4.6,0.6 Z';
  const blush = raw(g => `<radialGradient id="${g}" cx="75%" cy="70%" r="60%"><stop offset="0" stop-color="#F0512E" stop-opacity=".85"/><stop offset="1" stop-color="#F0512E" stop-opacity="0"/></radialGradient>`);
  return `<g ${T(x, y, s, r)}>
    <path d="${body}" fill="${rg('#FFC328', { hi: 0.55, lo: 0.3 })}"/><path d="${body}" fill="${blush}"/>
    <path d="${body}" fill="none" stroke="${line('#F39A1C')}" stroke-width=".55"/>
    <path d="M0.6,-4.9 C2.6,-7.6 5.6,-7.2 6.6,-6.2 C5,-4.4 2.6,-4.2 0.6,-4.9 Z" fill="${lg('#4CB85A', { top: 0.3 })}" stroke="${line('#3E9E4A')}" stroke-width=".45"/>
    <ellipse cx="-1.9" cy="-2.2" rx="1.5" ry=".9" transform="rotate(-30 -1.9 -2.2)" fill="#fff" opacity=".75"/></g>`;
}
function chaiGlass(x, y, s = 1) {
  return `<g ${T(x, y, s)}>
    ${flat('M-2.8,-7 L2.8,-7 L2.2,0 L-2.2,0 Z', lg('#B8683A', { top: 0.15, bottom: 0.35 }), line('#B8683A'), 0.5)}
    <path d="M-2.8,-7 L2.8,-7 L2.7,-5.6 L-2.7,-5.6 Z" fill="${CREAM}" opacity=".9"/>
    <path d="M-1.9,-5 L-1.5,-0.8" stroke="#fff" stroke-width=".6" opacity=".7" stroke-linecap="round"/></g>`;
}
function cup(x, y, s = 1, c = COLORS.red) {
  return `<g ${T(x, y, s)}>
    <ellipse cx="0" cy=".3" rx="4.4" ry="1.2" fill="${CREAM}" stroke="${line(CREAM)}" stroke-width=".4"/>
    ${flat('M3,-4.2 C5.4,-4.4 5.6,-1 2.6,-1', 'none', line(c), 0.9)}
    ${vol('M-3.4,-5 L3.4,-5 C3.2,-2 2.6,0 0,0 C-2.6,0 -3.2,-2 -3.4,-5 Z', c, { w: 0.5 })}
    <path d="M-3.3,-3.3 C-1,-2.6 1,-2.6 3.3,-3.3" stroke="${CREAM}" stroke-width=".8" fill="none"/>
    <ellipse cx="0" cy="-5" rx="3.4" ry=".9" fill="#9A5326" stroke="${line(c)}" stroke-width=".4"/>
    <ellipse cx="-1.8" cy="-2" rx=".5" ry="1.3" fill="#fff" opacity=".6"/></g>`;
}
function kettle(x, y, s = 1) {
  return `<g ${T(x, y, s)}>
    ${flat('M-5.5,-3.5 C-9,-4 -10,-8 -11.5,-10.5 L-10.2,-11.2 C-8.4,-9 -7.6,-7.2 -5,-6.6 Z', lg(GOLD), line(GOLD), 0.6)}
    ${flat('M-4.2,-9.5 C-4.2,-15.5 4.2,-15.5 4.2,-9.5', 'none', line(GOLD), 1.4)}
    ${flat('M-4.2,-9.5 C-4.2,-15.5 4.2,-15.5 4.2,-9.5', 'none', '#F7CF6A', 0.6)}
    ${vol('M-7,0 C-8,-4 -6,-9 0,-9.4 C6,-9 8,-4 7,0 Z', GOLD, { w: 0.7, o: { top: 0.45, bottom: 0.35 } })}
    ${vol('M-3.4,-9 C-3,-11 3,-11 3.4,-9 Z', GOLD, { w: 0.5 })}
    <circle cx="0" cy="-11" r="1.1" fill="${lg(GOLD)}" stroke="${line(GOLD)}" stroke-width=".4"/>
    <path d="M-6.6,-2.6 H6.6" stroke="${darken(GOLD, 0.25)}" stroke-width=".7"/>
    ${gloss(-3.2, -5.6, 1.6, 2.8, 20, 0.9)}${glint(-3.6, -6.2, 0.6)}</g>`;
}
function bangleStack(x, top, bottom, cols) {
  const n = cols.length, step = (bottom - top) / (n - 1);
  let back = '', front = '';
  cols.forEach((c, k) => {
    const y = top + k * step;
    back += `<path d="M${x - 6},${y} A6,1.9 0 0 1 ${x + 6},${y}" fill="none" stroke="${darken(c, 0.35)}" stroke-width="1.7"/>`;
    front += `<path d="M${x - 6},${y} A6,1.9 0 0 0 ${x + 6},${y}" fill="none" stroke="${line(c)}" stroke-width="2.3"/>
      <path d="M${x - 6},${y} A6,1.9 0 0 0 ${x + 6},${y}" fill="none" stroke="${c}" stroke-width="1.4"/>
      <path d="M${x - 4.6},${y + 1} A6,1.9 0 0 0 ${x - 1},${y + 1.85}" fill="none" stroke="#fff" stroke-width=".55" opacity=".85" stroke-linecap="round"/>`;
  });
  const rod = `${flat(rr(x - 0.9, top - 4, 1.8, bottom - top + 10, 0.9), lg(GOLD, { angle: 'h' }), line(GOLD), 0.5)}
    <circle cx="${x}" cy="${top - 4.6}" r="1.9" fill="${rg(GOLD)}" stroke="${line(GOLD)}" stroke-width=".5"/>`;
  const base = vol(rr(x - 5, bottom + 4.6, 10, 3, 1.2), WOOD, { w: 0.6 });
  return back + rod + front + base;
}
function kite(x, y, s, c1, c2, r = 0, tail = true) {
  return `<g ${T(x, y, s, r)}>
    ${tail ? `<path d="M0,10 C-2,13 2,15 0,18 C-2,21 1,23 0,25" fill="none" stroke="${line(c2)}" stroke-width=".55"/>
    <path d="M0,15 l-2.2,-1.2 v2.4 z M0,15 l2.2,-1.2 v2.4 z M0,21.4 l-2,-1.1 v2.2 z M0,21.4 l2,-1.1 v2.2 z" fill="${c2}" stroke="${line(c2)}" stroke-width=".35"/>` : ''}
    ${vol('M0,-9 L7,0 L0,10 L-7,0 Z', c1, { w: 0.8 })}
    <path d="M-7,0 L0,10 L7,0 C3,1.8 -3,1.8 -7,0 Z" fill="${lg(c2, { top: 0.2 })}" opacity=".95"/>
    <path d="M0,-9 L-7,0 L0,-2.5 Z" fill="#fff" opacity=".22"/>
    <path d="M0,-9 L0,10 M-7,0 C-3,-2 3,-2 7,0" fill="none" stroke="${line(c1)}" stroke-width=".5" opacity=".8"/>
    <path d="M0,-9 L7,0 L0,10 L-7,0 Z" fill="none" stroke="${line(c1)}" stroke-width=".8" stroke-linejoin="round"/>
    <circle cx="0" cy="1.2" r="1.3" fill="${GOLD}" stroke="${line(GOLD)}" stroke-width=".35"/></g>`;
}
function lattoo(x, y, s, c, c2 = GOLD) {
  // y = tip (bottom); body spins on a metal spike
  const d = 'M0,-11 C5.5,-11 7,-8 6.6,-6 C6,-3 2.4,-1.6 0.9,0 L-0.9,0 C-2.4,-1.6 -6,-3 -6.6,-6 C-7,-8 -5.5,-11 0,-11 Z';
  return `<g ${T(x, y, s)}>
    <path d="M-0.5,0 L0,2.6 L0.5,0 Z" fill="#9AA4AD" stroke="#4A5058" stroke-width=".35"/>
    ${vol(d, c, { w: 0.7 })}
    <path d="M-6.4,-6.6 C-2,-5.2 2,-5.2 6.4,-6.6 L6.1,-5.2 C2,-3.6 -2,-3.6 -6.1,-5.2 Z" fill="${c2}" opacity=".95"/>
    <path d="M-4.6,-9.6 C-1.6,-8.8 1.6,-8.8 4.6,-9.6" stroke="${CREAM}" stroke-width=".8" fill="none"/>
    ${vol(rr(-1.3, -14, 2.6, 3.4, 1), WOOD_L, { w: 0.45 })}
    <ellipse cx="-3" cy="-8" rx="1.2" ry="2" transform="rotate(25 -3 -8)" fill="#fff" opacity=".7"/></g>`;
}
function pinwheel(x, y, s, r = 0) {
  const cs = [COLORS.red, COLORS.yellow, COLORS.blue, COLORS.green];
  return `<g ${T(x, y, s, r)}>${cs.map((c, k) => `<g transform="rotate(${k * 90})">${vol('M0,0 L0,-7 C4,-7 6,-4 6,-1.5 Z', c, { w: 0.55 })}</g>`).join('')}
    <circle r="1.3" fill="${rg(GOLD)}" stroke="${line(GOLD)}" stroke-width=".4"/></g>`;
}
function diya(x, y, s = 1, lit = true) {
  return `<g ${T(x, y, s)}>
    ${lit ? glow(0, -4, 9, '#FFC44D', 0.75) : ''}
    ${vol('M-6,-2.6 C-5.6,1.2 5.6,1.2 6,-2.6 L7.6,-4 C4,-2.4 -4,-2.4 -6,-2.6 Z', CLAY, { w: 0.6 })}
    <path d="M-6,-2.6 C-4,-3.5 4.4,-3.6 7.6,-4" stroke="${GOLD}" stroke-width=".7" fill="none"/>
    <circle cx="-2.4" cy="-.6" r=".55" fill="${GOLD}"/><circle cx="0" cy="-.2" r=".55" fill="${GOLD}"/><circle cx="2.4" cy="-.6" r=".55" fill="${GOLD}"/>
    ${lit ? `<path d="M5.4,-4 C3.6,-6 4.4,-8.6 5.8,-10.4 C7.2,-8.6 8,-6 6.2,-4 Z" fill="#FFB52E"/>
    <path d="M5.6,-4.4 C4.8,-5.8 5.2,-7.2 5.85,-8.2 C6.5,-7.2 6.9,-5.8 6.1,-4.4 Z" fill="#FFF6C8"/>` : ''}</g>`;
}

// ---------------------------------------------------------------- stall frame
// Shrink a hair about the badge centre so the soft drop shadow is not cut by the canvas edge (overlays use the same fit).
const FIT = 'transform="translate(50,52) scale(.965) translate(-50,-52)"';
const THEMES = [
  { a: COLORS.red, b: CREAM, cloth: GOLD, trim: COLORS.red },                     // 0 chai
  { a: '#E0457B', b: COLORS.yellow, cloth: COLORS.blue, trim: '#E0457B' },        // 1 choori
  { a: COLORS.green, b: '#FFEA9A', cloth: COLORS.orange, trim: COLORS.green },    // 2 fruit
  { a: '#1FA3A0', b: CREAM, cloth: COLORS.red, trim: '#1FA3A0' },                 // 3 kites
  { a: COLORS.purple, b: '#FF9CC6', cloth: COLORS.green, trim: COLORS.purple },   // 4 toys
  { a: COLORS.orange, b: '#A3243B', cloth: COLORS.yellow, trim: '#A3243B' },      // 5 diyas
];

function interior(t, warm = 0.6) {
  const wall = raw(g => `<linearGradient id="${g}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#6E3A1E"/><stop offset=".5" stop-color="#91502A"/><stop offset="1" stop-color="#5E2E17"/></linearGradient>`);
  const side = raw(g => `<linearGradient id="${g}" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#2a0f08" stop-opacity=".55"/><stop offset=".22" stop-color="#2a0f08" stop-opacity="0"/><stop offset=".78" stop-color="#2a0f08" stop-opacity="0"/><stop offset="1" stop-color="#2a0f08" stop-opacity=".55"/></linearGradient>`);
  const lamp = raw(g => `<radialGradient id="${g}" cx="50%" cy="10%" r="75%"><stop offset="0" stop-color="#FFD27A" stop-opacity="${warm}"/><stop offset=".55" stop-color="#F0954A" stop-opacity="${warm * 0.35}"/><stop offset="1" stop-color="#F0954A" stop-opacity="0"/></radialGradient>`);
  const tint = mix('#91502A', t.a, 0.3);
  return `<rect x="8" y="18" width="84" height="58" fill="${wall}"/>
    <rect x="8" y="18" width="84" height="58" fill="${tint}" opacity=".3"/>
    ${[17, 26, 35, 44, 53, 62, 71, 80].map(x => `<path d="M${x},20 V76" stroke="#2a0f08" stroke-width=".5" opacity=".28"/><path d="M${x + 0.6},20 V76" stroke="#E8A36A" stroke-width=".35" opacity=".14"/>`).join('')}
    <rect x="8" y="18" width="84" height="58" fill="${lamp}"/>
    <rect x="8" y="18" width="84" height="58" fill="${side}"/>
    <rect x="8" y="28" width="84" height="8" fill="#2a0f08" opacity=".4" filter="url(#blur2)"/>`;
}
function shelf(x0, x1, y) {
  return `<rect x="${x0}" y="${y + 2}" width="${x1 - x0}" height="1.6" fill="#2a0f08" opacity=".35" filter="url(#blur1)"/>
    ${vol(rr(x0, y, x1 - x0, 2.4, 0.8), WOOD_L, { w: 0.55 })}
    <path d="M${x0 + 2},${y + 2.4} l0,2.4 l2.4,-2.4 Z M${x1 - 2},${y + 2.4} l0,2.4 l-2.4,-2.4 Z" fill="${WOOD}" stroke="${line(WOOD)}" stroke-width=".4"/>`;
}
function posts() {
  const p = x => `${vol(rr(x, 16, 8, 80, 2.2), WOOD, { w: 0.9, o: { angle: 'h', top: 0.32, bottom: 0.36 } })}
    <path d="M${x + 2.2},36 V70" stroke="#fff" stroke-width=".9" opacity=".22" stroke-linecap="round"/>
    ${[40, 60].map(y => `${vol(rr(x - 0.5, y, 9, 3, 1.2), GOLD, { w: 0.55 })}<path d="M${x + 1},${y + 0.8} H${x + 4}" stroke="#fff" stroke-width=".6" opacity=".8" stroke-linecap="round"/>`).join('')}`;
  return p(4.5) + p(87.5);
}
function awning(t) {
  const n = 7, yT = 12, yB = 28.5, yS = 36;
  const fold = raw(g => `<linearGradient id="${g}" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#fff" stop-opacity=".28"/><stop offset=".35" stop-color="#fff" stop-opacity=".05"/><stop offset=".7" stop-color="#2a0f08" stop-opacity=".06"/><stop offset="1" stop-color="#2a0f08" stop-opacity=".26"/></linearGradient>`);
  const vshade = raw(g => `<linearGradient id="${g}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#2a0f08" stop-opacity=".28"/><stop offset=".25" stop-color="#fff" stop-opacity=".12"/><stop offset=".6" stop-color="#fff" stop-opacity="0"/><stop offset="1" stop-color="#2a0f08" stop-opacity=".12"/></linearGradient>`);
  const ga = lg(t.a, { top: 0.25, bottom: 0.22 }), gb = lg(t.b, { top: 0.25, bottom: 0.2 });
  const sa = lg(t.a, { top: 0.1, bottom: 0.38 }), sb = lg(t.b, { top: 0.05, bottom: 0.3 });
  let stripes = '', scal = '', beads = '';
  for (let k = 0; k < n; k++) {
    const tx0 = 9 + (82 * k) / n, tx1 = 9 + (82 * (k + 1)) / n, bx0 = 4 + (92 * k) / n, bx1 = 4 + (92 * (k + 1)) / n;
    const c = k % 2 ? t.b : t.a;
    // cloth bellies outwards a little between ribs
    const d = `M${tx0},${yT} L${tx1},${yT} C${tx1 + 0.4},${yT + 6} ${bx1 + 0.2},${yB - 5} ${bx1},${yB} C${(bx0 + bx1) / 2},${yB + 1.2} ${(bx0 + bx1) / 2},${yB + 1.2} ${bx0},${yB} C${bx0 + 0.2},${yB - 5} ${tx0 + 0.4},${yT + 6} ${tx0},${yT} Z`;
    stripes += `<path d="${d}" fill="${k % 2 ? gb : ga}"/><path d="${d}" fill="${fold}"/><path d="M${tx0 + 1.2},${yT + 1} C${tx0 + 1.4},${yT + 7} ${bx0 + 1.6},${yB - 5} ${bx0 + 1.6},${yB - 1}" stroke="#fff" stroke-width=".7" opacity=".3" fill="none" stroke-linecap="round"/>`;
    const m = (bx0 + bx1) / 2;
    const sd = `M${bx0},${yB} C${bx0 + 0.2},${yS + 1} ${bx1 - 0.2},${yS + 1} ${bx1},${yB} C${m + 2},${yB + 1.1} ${m - 2},${yB + 1.1} ${bx0},${yB} Z`;
    scal += `<path d="${sd}" fill="${k % 2 ? sb : sa}" filter="url(#volS)"/><path d="${sd}" fill="none" stroke="${line(c)}" stroke-width=".8" stroke-linejoin="round"/>
      <path d="M${bx0 + 2},${yB + 2.2} C${bx0 + 3},${yS - 1.5} ${bx1 - 3},${yS - 1.5} ${bx1 - 2},${yB + 2.2}" fill="none" stroke="${k % 2 ? t.a : GOLD}" stroke-width=".7" opacity=".85"/>`;
    if (k > 0) beads += `<circle cx="${bx0}" cy="${yB + 1.3}" r="1.25" fill="${rg(GOLD)}" stroke="${line(GOLD)}" stroke-width=".4"/>`;
  }
  const outline = `M9,${yT} L91,${yT} C91.4,${yT + 6} 96.2,${yB - 5} 96,${yB} L4,${yB} C3.8,${yB - 5} 8.6,${yT + 6} 9,${yT} Z`;
  return `<path d="M6,${yB} H94 L92,${yS + 4} H8 Z" fill="#2a0f08" opacity=".35" filter="url(#blur2)"/>
    ${stripes}<path d="${outline}" fill="${vshade}"/>
    <path d="${outline}" fill="none" stroke="${line(t.a)}" stroke-width=".9" stroke-linejoin="round"/>
    ${scal}${beads}
    <path d="M4,${yB} C30,${yB + 0.8} 70,${yB + 0.8} 96,${yB}" stroke="${GOLD}" stroke-width="1.1" fill="none"/>
    ${gloss(24, 17, 12, 2.4, -6, 0.55)}`;
}
function roof(t) {
  const crest = 'M36,6 C40,6 43,2.4 50,2.4 C57,2.4 60,6 64,6 Z';
  return `${vol(rr(4, 5, 92, 9, 2.4), '#9E5A2B', { w: 0.9, big: true, o: { top: 0.3, bottom: 0.3 } })}
    <path d="M7,7.2 H93" stroke="#fff" stroke-width=".7" opacity=".3" stroke-linecap="round"/>
    <path d="M6,12.4 H94" stroke="${GOLD}" stroke-width=".9" opacity=".9"/>
    ${[12, 24, 76, 88].map(x => `<circle cx="${x}" cy="9.4" r="1.1" fill="${rg(GOLD)}" stroke="${line(GOLD)}" stroke-width=".35"/>`).join('')}
    ${[30, 38, 62, 70].map(x => `<path d="M${x - 2},9.4 L${x},7.6 L${x + 2},9.4 L${x},11.2 Z" fill="${t.trim}" stroke="${line(t.trim)}" stroke-width=".35" opacity=".9"/>`).join('')}
    ${vol(crest, GOLD, { w: 0.7, o: { top: 0.45, bottom: 0.3 } })}
    <circle cx="50" cy="8.6" r="2.4" fill="${rg(t.trim)}" stroke="${line(GOLD)}" stroke-width=".6"/>
    ${glint(49.2, 7.8, 0.6)}
    <circle cx="50" cy="3.4" r="1.4" fill="${rg(GOLD, { hi: 0.7 })}" stroke="${line(GOLD)}" stroke-width=".4"/>
    ${[4.6, 95.4].map(x => `<circle cx="${x}" cy="9.5" r="2.2" fill="${rg(GOLD)}" stroke="${line(GOLD)}" stroke-width=".5"/>`).join('')}`;
}
function counter(t) {
  const c = t.cloth;
  const drape = 'M6,77.5 H94 V87.5 C88,94.5 71,94.5 64.7,87.5 C58,94.5 42,94.5 35.3,87.5 C29,94.5 12,94.5 6,87.5 Z';
  const tassel = x => `<path d="M${x},87.5 V90.6" stroke="${darken(GOLD, 0.3)}" stroke-width=".5"/>
    <path d="M${x - 1.2},90.4 h2.4 l.6,3.6 c-1,.8 -2.6,.8 -3.6,0 Z" fill="${lg(t.trim)}" stroke="${line(t.trim)}" stroke-width=".35"/>
    <circle cx="${x}" cy="90" r="1" fill="${rg(GOLD)}" stroke="${line(GOLD)}" stroke-width=".3"/>`;
  return `
    ${vol(rr(5, 74, 90, 22, 3), '#8E5126', { w: 0.9, big: true })}
    ${[20, 35, 50, 65, 80].map(x => `<path d="M${x},92 V95.6" stroke="#2a0f08" stroke-width=".5" opacity=".35"/>`).join('')}
    ${vol(drape, c, { w: 0.8, big: true, o: { top: 0.3, bottom: 0.25 } })}
    ${[20.6, 50, 79.4].map(x => `<path d="M${x - 8},88 C${x - 4},91.6 ${x + 4},91.6 ${x + 8},88" stroke="${darken(c, 0.4)}" stroke-width=".8" fill="none" opacity=".45"/>`).join('')}
    <path d="M6,80.2 H94" stroke="${t.trim}" stroke-width="2.2"/>
    <path d="M6,79.1 H94 M6,81.3 H94" stroke="${GOLD}" stroke-width=".5"/>
    ${Array.from({ length: 15 }, (_, k) => { const x = 9 + k * 5.85; return `<path d="M${x - 1.5},84.4 L${x},82.9 L${x + 1.5},84.4 L${x},85.9 Z" fill="${CREAM}" opacity=".92"/><circle cx="${x + 2.9}" cy="84.4" r=".55" fill="${t.trim}" opacity=".9"/>`; }).join('')}
    ${[6, 35.3, 64.7, 94].map(tassel).join('')}
    ${vol(rr(2.5, 70, 95, 7.6, 2.4), WOOD_L, { w: 0.9, big: true, o: { top: 0.35, bottom: 0.3 } })}
    <path d="M5,71.5 H95" stroke="#fff" stroke-width=".8" opacity=".45" stroke-linecap="round"/>
    ${gloss(18, 72, 10, 1.2, 0, 0.6)}`;
}

// ---------------------------------------------------------------- themed decoration
const DECOR = [
  // 0 chai: kettle on a clay stove, glass jars, chai glasses, steam
  {
    back: () => `${shelf(10, 27, 44)}${shelf(73, 90, 44)}
      ${[17, 23].map((x, k) => `<g>${flat(rr(x - 2.6, 34.4, 5.2, 9.6, 1.2), '#F6E6C8', '#8A5A3A', 0.45, 'opacity=".55"')}
        <rect x="${x - 2.1}" y="${38 + k}" width="4.2" height="${5.6 - k}" rx=".8" fill="${k ? '#3E7A2E' : '#6B3416'}"/>
        ${flat(rr(x - 2.8, 33.4, 5.6, 1.8, 0.6), lg(COLORS.red), line(COLORS.red), 0.4)}
        <path d="M${x - 1.6},36 V42.6" stroke="#fff" stroke-width=".6" opacity=".7" stroke-linecap="round"/></g>`).join('')}
      ${cup(78, 44, 0.85, COLORS.red)}${cup(85, 44, 0.85, COLORS.blue)}`,
    front: () => `
      ${vol(rr(9.5, 62, 18, 9, 2.2), CLAY, { w: 0.7 })}
      <ellipse cx="18.5" cy="67.6" rx="4.6" ry="2.3" fill="#3A1408"/>${glow(18.5, 67.6, 6, '#FF9A2E', 0.95)}
      <ellipse cx="18.5" cy="68" rx="3" ry="1.2" fill="#FFB43C"/>
      ${kettle(18.5, 62.4, 1.2)}
      <g filter="url(#blur1)" opacity=".85"><path d="M5.4,47.5 C3.4,44 7.6,42 5.6,37.5" stroke="#fff" stroke-width="1.6" fill="none" stroke-linecap="round" opacity=".8"/>
      <path d="M8.4,46 C6.8,43 9.8,41 8.4,38" stroke="#fff" stroke-width="1.1" fill="none" stroke-linecap="round" opacity=".55"/></g>
      ${vol('M74.5,71 C74.5,69.2 94,69.2 94,71 C94,72.2 74.5,72.2 74.5,71 Z', GOLD, { w: 0.5 })}
      ${cup(84.5, 66.6, 1.0, COLORS.red)}${cup(79.6, 70.2, 1.15, COLORS.red)}${cup(89.4, 70.2, 1.15, COLORS.red)}`,
  },
  // 1 choori: two brass bangle poles stacked with glossy bangles
  {
    back: t => `${[[18, -1], [82, 1]].map(([x, f]) => `<path d="M${x - 9 * f},20 C${x - 4 * f},30 ${x + 3 * f},34 ${x + 7 * f},28" fill="none" stroke="${lighten(t.a, 0.2)}" stroke-width="3.2" opacity=".55"/>`).join('')}
      ${shelf(10, 26, 37)}${shelf(74, 90, 37)}
      ${[[13, COLORS.green], [18, COLORS.purple], [23, GOLD], [77, COLORS.red], [82, COLORS.blue], [87, '#E0457B']].map(([x, c]) =>
        `<ellipse cx="${x}" cy="34.6" rx="2.2" ry="2.6" fill="none" stroke="${line(c)}" stroke-width="1.4"/><ellipse cx="${x}" cy="34.6" rx="2.2" ry="2.6" fill="none" stroke="${c}" stroke-width=".8"/>`).join('')}`,
    front: () => `${bangleStack(16, 47, 64, [COLORS.red, GOLD, COLORS.blue, COLORS.green, '#E0457B', COLORS.purple, COLORS.orange])}
      ${bangleStack(84, 47, 64, [COLORS.blue, '#E0457B', GOLD, COLORS.purple, COLORS.green, COLORS.red, COLORS.yellow])}`,
  },
  // 2 fruit: woven baskets heaped with mangoes, mango-leaf toran, crates on shelves
  {
    back: () => `${shelf(10, 27, 46)}${shelf(73, 90, 46)}
      ${[[18.5], [81.5]].map(([x]) => `${mango(x - 4, 42.4, 0.62, -10)}${mango(x + 3.6, 42.6, 0.62, 15)}${mango(x, 41.2, 0.62, 0)}
        ${vol(rr(x - 7.5, 42.5, 15, 3.8, 0.8), WOOD_L, { w: 0.5 })}<path d="M${x - 7},44.4 H${x + 7}" stroke="${line(WOOD_L)}" stroke-width=".35"/>`).join('')}`,
    front: () => {
      const basket = (x, f) => {
        const d = `M${x - 9},64 L${x + 9},64 C${x + 8.6},68.4 ${x + 7},71 ${x + 5},71 L${x - 5},71 C${x - 7},71 ${x - 8.6},68.4 ${x - 9},64 Z`;
        return `${mango(x - 5, 61.4, 0.9, -20)}${mango(x + 5, 61.6, 0.9, 18)}${mango(x, 59, 0.95, 4 * f)}${mango(x - 1.8, 63, 0.9, -5)}${mango(x + 4, 63.4, 0.85, 30)}
          ${vol(d, '#C68A45', { w: 0.7 })}
          ${[66, 68.4].map(y => `<path d="M${x - 8.6},${y} H${x + 8.6}" stroke="${line('#C68A45')}" stroke-width=".35" stroke-dasharray="1.4 1" opacity=".8"/>`).join('')}
          ${vol(rr(x - 9.8, 63, 19.6, 2.4, 1.2), '#D9A05B', { w: 0.5 })}`;
      };
      const leaves = [17.1, 24, 30.3, 69.7, 76, 82.9].map((x, k) => `<path d="M${x},29.4 C${x - 1.8},32.4 ${x - 1.4},36.4 ${x},38.6 C${x + 1.4},36.4 ${x + 1.8},32.4 ${x},29.4 Z" fill="${lg(k % 2 ? '#3FA34D' : '#5CC163', { top: 0.3 })}" stroke="${line('#3FA34D')}" stroke-width=".4"/><path d="M${x},30 V37.6" stroke="${darken('#3FA34D', 0.3)}" stroke-width=".3"/>`).join('');
      return basket(15.5, 1) + basket(84.5, -1) + leaves;
    },
  },
  // 3 kites: kites hanging from the awning, charkhi spool, kite stack
  {
    back: t => `${kite(19, 52, 0.52, COLORS.yellow, COLORS.green, 12, false)}${kite(81, 52, 0.52, COLORS.purple, COLORS.yellow, -12, false)}`,
    front: t => `
      <path d="M15,30 V36" stroke="#5A3A20" stroke-width=".5"/><path d="M85,30 V36" stroke="#5A3A20" stroke-width=".5"/>
      ${kite(15, 43, 0.9, COLORS.red, COLORS.yellow, -8)}${kite(85, 43, 0.9, COLORS.blue, CREAM, 8)}
      <g>${flat('M11,64.5 L8.6,71 M23,64.5 L25.4,71', 'none', line(WOOD_L), 1.6)}${flat('M11,64.5 L8.6,71 M23,64.5 L25.4,71', 'none', WOOD_L, 0.8)}
        ${vol(rr(11, 60, 12, 9, 3.6), '#E8423D', { w: 0.6 })}
        ${[61.8, 63.6, 65.4, 67.2].map(y => `<path d="M12,${y} H22" stroke="${lighten('#E8423D', 0.35)}" stroke-width=".45" opacity=".9"/>`).join('')}
        ${[11, 23].map(x => `${vol(`M${x - 1.6},64.5 a1.6,6.4 0 1 0 3.2,0 a1.6,6.4 0 1 0 -3.2,0 Z`, WOOD_L, { w: 0.5 })}<circle cx="${x}" cy="64.5" r=".7" fill="${GOLD}"/>`).join('')}
        ${gloss(15, 62, 2.6, 1.1, 0, 0.7)}
        <path d="M17,60 C17,52 15.5,50 15,52" stroke="${darken('#E8423D', 0.1)}" stroke-width=".4" fill="none" opacity=".7"/></g>
      ${kite(84, 63.5, 0.58, COLORS.green, CREAM, -10, false)}${kite(78.5, 65.2, 0.5, '#E0457B', COLORS.yellow, 14, false)}`,
  },
  // 4 toys: lattoo spinning tops on shelves and counter, pinwheel
  {
    back: () => `${shelf(10, 27, 45)}${shelf(73, 90, 45)}
      ${lattoo(14.4, 44.6, 0.52, COLORS.blue)}${lattoo(21.6, 44.6, 0.52, COLORS.yellow, COLORS.red)}
      ${lattoo(78.4, 44.6, 0.52, COLORS.green)}<circle cx="85.4" cy="42" r="2.6" fill="${rg(COLORS.red)}" stroke="${line(COLORS.red)}" stroke-width=".45"/>
      <path d="M83.6,40.6 C84.6,41.6 86.6,41.6 87.4,40.6" stroke="#fff" stroke-width=".5" fill="none"/>`,
    front: () => `${lattoo(13.2, 68.6, 1.2, COLORS.purple)}${lattoo(23, 69.4, 0.86, COLORS.red, COLORS.yellow)}
      ${vol(rr(76.5, 63.4, 10, 7.6, 1.8), CLAY, { w: 0.55 })}<path d="M77,65.6 H86" stroke="${GOLD}" stroke-width=".7"/>
      ${flat('M82,63.6 L83.4,46', 'none', line(WOOD_L), 1.4)}${flat('M82,63.6 L83.4,46', 'none', WOOD_L, 0.7)}
      ${pinwheel(83.4, 46, 1.3, 12)}
      ${vol('M89.6,70.8 a3.2,3.2 0 1 1 .01,0 Z', COLORS.green, { w: 0.45 })}<circle cx="89.6" cy="67.6" r="1.4" fill="none" stroke="${COLORS.yellow}" stroke-width=".7"/>`,
  },
  // 5 diyas: lit lamps on shelves and counter, fairy lights under the awning
  {
    back: () => `${shelf(10, 27, 46)}${shelf(73, 90, 46)}${diya(14.4, 46, 0.72)}${diya(22.4, 46, 0.72)}${diya(77.6, 46, 0.72)}${diya(85.6, 46, 0.72)}`,
    front: () => {
      const bulbs = [];
      const sag = x => 37 + 5.2 * (1 - Math.pow((x - 50) / 46, 2));
      const wire = `M4,37 Q50,47.4 96,37`;
      for (let k = 0; k < 13; k++) {
        const x = 7 + k * 7.17, y = sag(x) + 0.8, c = ['#FFD24A', '#FF6B6B', '#7FE0FF', '#9CFF8A'][k % 4];
        bulbs.push(`${glow(x, y + 1, 3.4, c, 0.85)}<ellipse cx="${x}" cy="${y + 1}" rx="1" ry="1.35" fill="${lighten(c, 0.35)}" stroke="${darken(c, 0.45)}" stroke-width=".3"/>`);
      }
      return `<path d="${wire}" fill="none" stroke="#3A2A1E" stroke-width=".55"/>${bulbs.join('')}
        ${diya(12, 70.4, 1.15)}${diya(23, 70.6, 0.95)}${diya(17.6, 63.2, 0.9)}
        ${diya(78, 70.6, 0.95)}${diya(88.6, 70.4, 1.15)}${diya(83.4, 63.2, 0.9)}`;
    },
  },
];

export function stall(i) {
  D = [];
  const t = THEMES[i], d = DECOR[i];
  const body = interior(t, i === 5 ? 0.95 : 0.8) + (d.back ? d.back(t) : '') + posts() + awning(t) + roof(t) + counter(t) + (d.front ? d.front(t) : '');
  return svg(512, 512, `<defs>${kitDefs()}${D.join('')}</defs><g ${FIT}><g filter="url(#drop)">${body}</g></g>`);
}

// ---------------------------------------------------------------- overlays
function marigold(x, y, r, c) {
  return `<circle cx="${x}" cy="${y}" r="${r}" fill="${rg(c, { hi: 0.35, lo: 0.35 })}" stroke="${line(c)}" stroke-width=".4"/>
    <circle cx="${x}" cy="${y}" r="${r * 0.55}" fill="none" stroke="${darken(c, 0.2)}" stroke-width=".35" stroke-dasharray=".7 .6"/>
    <circle cx="${x - r * 0.35}" cy="${y - r * 0.4}" r="${r * 0.28}" fill="#fff" opacity=".55"/>`;
}
export function stallClosed() {
  D = [];
  const steel = '#8FA2B2';
  const slats = Array.from({ length: 11 }, (_, k) => {
    const y = 34 + k * 5.2;
    return `<rect x="12" y="${y}" width="76" height="5.2" fill="${lg(steel, { top: 0.4, bottom: 0.3 })}"/>
      <path d="M12,${y + 5.1} H88" stroke="${darken(steel, 0.5)}" stroke-width=".55"/><path d="M12,${y + 0.5} H88" stroke="#fff" stroke-width=".45" opacity=".55"/>`;
  }).join('');
  const sag = (x0, x1, dy) => x => 36 + dy * Math.sin(Math.PI * (x - x0) / (x1 - x0));
  const flowers = [];
  [[10, 50, 6.5], [50, 90, 6.5]].forEach(([x0, x1, dy]) => {
    const f = sag(x0, x1, dy);
    for (let k = 0; k <= 9; k++) { const x = x0 + (k * (x1 - x0)) / 9; flowers.push([x, f(x)]); }
  });
  return svg(512, 512, `<defs>${kitDefs()}__DEFS__</defs><g ${FIT}>
    <g filter="url(#drop)">
    ${vol(rr(9.5, 30, 4, 65, 1.2), '#6E7E8A', { w: 0.6 })}${vol(rr(86.5, 30, 4, 65, 1.2), '#6E7E8A', { w: 0.6 })}
    <g>${slats}</g>
    <rect x="12" y="34" width="76" height="60" fill="${raw(g => `<linearGradient id="${g}" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#fff" stop-opacity=".22"/><stop offset=".4" stop-color="#fff" stop-opacity="0"/><stop offset="1" stop-color="#2a0f08" stop-opacity=".22"/></linearGradient>`)}"/>
    <path d="${rr(12, 34, 76, 60, 0.5)}" fill="none" stroke="${line(steel)}" stroke-width=".9"/>
    ${vol(rr(11, 89, 78, 5.4, 1.4), '#6E7E8A', { w: 0.7 })}
    ${vol(rr(43, 90.2, 14, 3, 1.4), '#4A555E', { w: 0.5 })}
    ${[24, 76].map(x => `${vol(rr(x - 2.6, 89.6, 5.2, 4.4, 1), GOLD, { w: 0.45 })}<circle cx="${x}" cy="91.8" r=".7" fill="${line(GOLD)}"/>`).join('')}
    ${vol(rr(8, 27, 84, 8, 4), '#7D8E9B', { w: 0.9, big: true, o: { top: 0.45, bottom: 0.35 } })}
    <path d="M11,29.2 H89" stroke="#fff" stroke-width=".9" opacity=".55" stroke-linecap="round"/>
    ${[[10, 50], [50, 90]].map(([a, b]) => `<path d="M${a},36 Q${(a + b) / 2},${36 + 13} ${b},36" fill="none" stroke="#2E7D32" stroke-width=".8"/>`).join('')}
    ${flowers.map(([x, y], k) => marigold(x, y, 2.5, k % 3 === 1 ? '#FFB020' : '#FF7A1A')).join('')}
    ${[10, 50, 90].map(x => `<path d="M${x},38 V44" stroke="#2E7D32" stroke-width=".6"/>${marigold(x, 44, 2, '#FF7A1A')}${marigold(x, 48, 1.8, '#FFB020')}
      <path d="M${x - 1.6},49.6 L${x},48.6 L${x + 1.6},49.6 L${x},53 Z" fill="${lg('#45BF5C')}" stroke="${line('#45BF5C')}" stroke-width=".3"/>`).join('')}
    </g>
    <g filter="url(#sticker)">
      <circle cx="82" cy="17" r="11" fill="${rg('#3FBF5A', { hi: 0.45, lo: 0.35 })}" filter="url(#vol)"/>
      <circle cx="82" cy="17" r="11" fill="none" stroke="${line('#3FBF5A')}" stroke-width="1.1"/>
      ${gloss(78, 12.6, 5, 2.6, -25, 0.7)}
      <path d="M76.4,17.4 l4,4 l7.4,-8.4" fill="none" stroke="${darken('#3FBF5A', 0.55)}" stroke-width="4.6" stroke-linecap="round" stroke-linejoin="round" opacity=".35" transform="translate(0,.8)"/>
      <path d="M76.4,17.4 l4,4 l7.4,-8.4" fill="none" stroke="#fff" stroke-width="3.6" stroke-linecap="round" stroke-linejoin="round"/>
    </g></g>`).replace('__DEFS__', D.join(''));
}
export function stallLock() {
  D = [];
  const steel = '#A9B4BF';
  const linkG = lg(steel, { top: 0.5, bottom: 0.4 });
  const chain = (x1, y1, x2, y2, n = 17) => {
    const out = [], a0 = Math.atan2(y2 - y1, x2 - x1) * 180 / Math.PI;
    for (let k = 0; k <= n; k++) {
      const t = k / n, x = x1 + (x2 - x1) * t, y = y1 + (y2 - y1) * t, flatLink = k % 2 === 0;
      out.push(flatLink
        ? `<ellipse cx="${x}" cy="${y}" rx="4.4" ry="2.6" transform="rotate(${a0} ${x} ${y})" fill="none" stroke="${line(steel)}" stroke-width="2.9"/>
           <ellipse cx="${x}" cy="${y}" rx="4.4" ry="2.6" transform="rotate(${a0} ${x} ${y})" fill="none" stroke="${linkG}" stroke-width="1.6"/>
           <path d="M${x - 3},${y - 1.6} q2,-1.4 4.4,-.6" transform="rotate(${a0} ${x} ${y})" stroke="#fff" stroke-width=".6" fill="none" opacity=".8"/>`
        : `<rect x="${x - 4.4}" y="${y - 1.2}" width="8.8" height="2.4" rx="1.2" transform="rotate(${a0} ${x} ${y})" fill="${linkG}" stroke="${line(steel)}" stroke-width=".7"/>`);
    }
    return out.join('');
  };
  const brass = '#E0A23A';
  return svg(512, 512, `<defs>${kitDefs()}__DEFS__</defs><g ${FIT}>
    <path d="${rr(4, 3, 92, 93, 4)}" fill="#1C0F0A" opacity=".5"/>
    <g filter="url(#drop)">${chain(9, 18, 91, 90)}${chain(91, 18, 9, 90)}</g>
    <g filter="url(#sticker)">
      <path d="M38,44 V33 C38,21 62,21 62,33 V44" fill="none" stroke="${line(steel)}" stroke-width="7.6" stroke-linecap="round"/>
      <path d="M38,44 V33 C38,21 62,21 62,33 V44" fill="none" stroke="${lg(steel, { top: 0.55, bottom: 0.3 })}" stroke-width="5" stroke-linecap="round"/>
      <path d="M39.4,36 V33.4 C39.4,25 46,23.6 50,23.6" fill="none" stroke="#fff" stroke-width="1.1" opacity=".75" stroke-linecap="round"/>
      ${vol(rr(30, 38, 40, 35, 8), brass, { w: 1.3, big: true, o: { top: 0.45, bottom: 0.35 } })}
      <path d="${rr(35.5, 43, 29, 19, 4.5)}" fill="${lg('#5A2E14', { top: 0.05, bottom: 0.3 })}" stroke="${darken(brass, 0.4)}" stroke-width=".9"/>
      <path d="M36.5,60.5 H63.5" stroke="#fff" stroke-width=".7" opacity=".35"/>
      ${gloss(37, 41, 6, 2, -10, 0.75)}
      <circle cx="50" cy="67" r="1.6" fill="#3A1A0A"/><path d="M49.3,67.4 h1.4 l.4,2.6 h-2.2 Z" fill="#3A1A0A"/>
      ${[33.5, 66.5].map(x => `<circle cx="${x}" cy="67.6" r=".9" fill="${lighten(brass, 0.4)}" stroke="${line(brass)}" stroke-width=".3"/>`).join('')}
    </g></g>`).replace('__DEFS__', D.join(''));
}

// ---------------------------------------------------------------- gate: carved sandstone Mughal arch, teal studded doors open
export function gate() {
  D = [];
  const stone = '#E0915A', marble = '#FFF1DC', teal = '#1F8C88';
  const archOuter = 'M21,96 V52 C21,37 36,29 50,20 C64,29 79,37 79,52 V96 Z';
  const archIn = 'M25,96 V53 C25,40 38,33 50,25 C62,33 75,40 75,53 V96 Z';
  const inside = raw(g => `<radialGradient id="${g}" cx="50%" cy="62%" r="62%"><stop offset="0" stop-color="#5A2A16"/><stop offset=".6" stop-color="#341509"/><stop offset="1" stop-color="#1E0B05"/></radialGradient>`);
  const inlay = [];
  for (let k = 0; k <= 16; k++) {
    // dots along the marble arch band
    const t = k / 16;
    let x, y;
    if (t < 0.25) { x = 23; y = 94 - (t / 0.25) * 40; } else if (t > 0.75) { x = 77; y = 54 + ((t - 0.75) / 0.25) * 40; } else {
      const u = (t - 0.25) / 0.5; const ang = Math.PI * (1 - u);
      x = 50 + 27 * Math.cos(ang); y = 52 - 28 * Math.sin(ang) * (u < 0.5 ? 1 : 1);
      // follow the pointed arch roughly
      y = 54 - (1 - Math.abs(u - 0.5) * 2) ** 0.8 * 31;
      x = 23 + 54 * u;
    }
    inlay.push(`<circle cx="${x.toFixed(2)}" cy="${y.toFixed(2)}" r=".95" fill="${k % 2 ? '#2E8B57' : '#C0392B'}"/>`);
  }
  const flower = (x, y, s, c) => `<g ${T(x, y, s)}>${[0, 72, 144, 216, 288].map(a => `<ellipse cx="0" cy="-2.2" rx="1.2" ry="2.2" transform="rotate(${a})" fill="${c}" stroke="${darken(c, 0.4)}" stroke-width=".25"/>`).join('')}<circle r="1" fill="${GOLD}"/></g>`;
  const leaf = (x, y, r, c = '#2E8B57') => `<ellipse cx="${x}" cy="${y}" rx=".9" ry="2" transform="rotate(${r} ${x} ${y})" fill="${c}"/>`;
  const studs = (x0, x1, ys) => ys.map(y => [x0, x1].map(x => `<circle cx="${x}" cy="${y}" r=".95" fill="${rg(GOLD, { hi: 0.7 })}" stroke="${line(GOLD)}" stroke-width=".25"/>`).join('')).join('');
  const leftDoor = 'M21,56 C19,52 15,50 11,52 L11,95 L21,95 Z';
  const rightDoor = 'M79,56 C81,52 85,50 89,52 L89,95 L79,95 Z';
  return svg(512, 512, `<defs>${kitDefs()}__DEFS__</defs><g transform="translate(50,58) scale(.96) translate(-50,-58)"><g filter="url(#drop)">
    ${vol('M10,14 H90 L92,18 V96 H8 V18 Z', stone, { w: 1.1, big: true, o: { top: 0.3, bottom: 0.3 } })}
    <path d="M12,20 H88 V94 H12 Z" fill="none" stroke="${marble}" stroke-width="1.8"/>
    <path d="M12,20 H88 V94 H12 Z" fill="none" stroke="${darken(stone, 0.35)}" stroke-width=".4" stroke-dasharray="1 1.6"/>
    ${[[18, 28], [82, 28]].map(([x, y]) => flower(x, y, 0.85, '#C0392B') + leaf(x - 3.2, y + 2.6, 50) + leaf(x + 3.2, y + 2.6, -50)).join('')}
    ${flower(35, 26.5, 0.6, '#2E6FB0')}${flower(65, 26.5, 0.6, '#2E6FB0')}
    ${vol(rr(6, 11, 88, 5, 1.6), lighten(stone, 0.12), { w: 0.8 })}
    ${Array.from({ length: 11 }, (_, k) => { const x = 13 + k * 7.4; return `<path d="M${x - 2},16 V14.2 C${x - 2},12.6 ${x + 2},12.6 ${x + 2},14.2 V16 Z" fill="${darken(stone, 0.3)}" opacity=".5"/>`; }).join('')}
    ${vol('M40,11 C40,4.6 45,2.8 50,2.8 C55,2.8 60,4.6 60,11 Z', marble, { w: 0.8, big: true, o: { top: 0.2, bottom: 0.18 } })}
    ${gloss(45, 6, 3, 1.6, -20, 0.8)}
    <circle cx="50" cy="2.6" r="1.3" fill="${rg(GOLD, { hi: 0.7 })}" stroke="${line(GOLD)}" stroke-width=".4"/>
    ${[9, 91].map(x => `${vol(`M${x - 3.4},14 C${x - 3.4},9 ${x + 3.4},9 ${x + 3.4},14 Z`, marble, { w: 0.6 })}<circle cx="${x}" cy="8.4" r="1" fill="${rg(GOLD)}" stroke="${line(GOLD)}" stroke-width=".3"/>`).join('')}
    ${vol(archOuter, marble, { w: 0.9, big: true, o: { top: 0.15, bottom: 0.2 } })}
    ${inlay.join('')}
    <path d="${archIn}" fill="${inside}"/>
    <path d="${archIn}" fill="none" stroke="${line(stone)}" stroke-width=".9"/>
    <path d="M27,96 V54 C27,42 39,35 50,28" fill="none" stroke="#2a0f08" stroke-width="2.6" opacity=".45" filter="url(#blur1)"/>
    ${[[31, 90], [69, 90]].map(([x, y]) => `<path d="M${x - 5},${y + 5} C${x - 6},${y - 1} ${x - 3},${y - 4} ${x},${y - 4} C${x + 3},${y - 4} ${x + 6},${y - 1} ${x + 5},${y + 5} Z" fill="#4A2412" opacity=".9"/><path d="M${x - 2},${y - 2.6} C${x},${y - 3.4} ${x + 1.5},${y - 3} ${x + 2.6},${y - 2}" stroke="#7A4428" stroke-width=".6" fill="none"/>`).join('')}
    <path d="M50,25 V33" stroke="#1E0B05" stroke-width=".5"/>
    ${glow(50, 37, 9, '#FFB54A', 0.6)}
    ${vol('M47.6,34 L52.4,34 L51.6,39.4 L48.4,39.4 Z', GOLD, { w: 0.4 })}<circle cx="50" cy="37" r="1.2" fill="#FFF3B0"/>
    <ellipse cx="50" cy="88" rx="23" ry="7" fill="#FF9A40" opacity=".22" filter="url(#blur4)"/>
    ${vol(leftDoor, teal, { w: 0.8, o: { angle: 'h', top: 0.12, bottom: 0.4 } })}
    ${vol(rightDoor, teal, { w: 0.8, o: { angle: 'h', top: 0.35, bottom: 0.12 } })}
    <path d="M13,58 L19,59 V92 H13 Z M87,58 L81,59 V92 H87 Z" fill="none" stroke="${darken(teal, 0.35)}" stroke-width=".55"/>
    ${studs(14.6, 85.4, [62, 69, 76, 83, 90])}${studs(17.6, 82.4, [62, 69, 76, 83, 90])}
    ${[[20, 72], [80, 72]].map(([x, y]) => `<circle cx="${x}" cy="${y}" r="1.6" fill="none" stroke="${GOLD}" stroke-width=".7"/>`).join('')}
    ${vol(rr(5, 92.4, 90, 4, 1.4), darken(stone, 0.12), { w: 0.8 })}
    ${gloss(22, 20, 10, 2.2, -8, 0.4)}
    </g></g>`).replace('__DEFS__', D.join(''));
}

export function jobs() {
  return [...[0, 1, 2, 3, 4, 5].map(i => [`stall_${i}`, stall(i)]), ['stall_closed', stallClosed()], ['stall_lock', stallLock()], ['gate', gate()]];
}
