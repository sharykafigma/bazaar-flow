// Bazaar Flow art: every sprite is hand-written SVG in this file, rendered to PNG by render.mjs.
// Style: warm desi bazaar, chunky flat vector, one dark-brown outline, soft top-left light,
// white "sticker" rim on goods so they read on any background.
// All art is original to this project (no third-party assets). License: see Assets/BazaarFlow/Art/ART_LICENSES.md

export const OL = '#3b2417';           // outline
export const C = {                       // goods colours, same hues as GameColors.cs
  red: '#E8423D', blue: '#387DEB', yellow: '#FACC2E', green: '#45BF5C', purple: '#9E5CDB', orange: '#FA8C29',
};
export const GOODS = [
  { key: 'chai', color: 'red' },
  { key: 'choori', color: 'blue' },
  { key: 'aam', color: 'yellow' },
  { key: 'patang', color: 'green' },
  { key: 'lattoo', color: 'purple' },
  { key: 'diya', color: 'orange' },
];

export function shade(hex, f) {
  const n = parseInt(hex.slice(1), 16);
  let r = (n >> 16) & 255, g = (n >> 8) & 255, b = n & 255;
  const t = f < 0 ? 0 : 255, p = Math.abs(f);
  r = Math.round((t - r) * p + r); g = Math.round((t - g) * p + g); b = Math.round((t - b) * p + b);
  return '#' + ((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1);
}

const svg = (w, h, body, vb = `0 0 ${w} ${h}`) =>
  `<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="${vb}">${body}</svg>`;

// Sticker filter: white rim + soft drop shadow. r = rim radius in viewBox units.
const stickerDefs = (id, r = 3.2, sh = 2.2) => `
<filter id="${id}" x="-25%" y="-25%" width="150%" height="150%">
  <feMorphology in="SourceAlpha" operator="dilate" radius="${r}" result="d"/>
  <feFlood flood-color="#fffaf0"/><feComposite in2="d" operator="in" result="rim"/>
  <feMorphology in="SourceAlpha" operator="dilate" radius="${r}" result="d2"/>
  <feGaussianBlur in="d2" stdDeviation="${sh}"/><feOffset dy="${sh}" result="b"/>
  <feFlood flood-color="#3b2417" flood-opacity="0.35"/><feComposite in2="b" operator="in" result="shadow"/>
  <feMerge><feMergeNode in="shadow"/><feMergeNode in="rim"/><feMergeNode in="SourceGraphic"/></feMerge>
</filter>`;

const sw = 'stroke="' + OL + '" stroke-width="2.6" stroke-linejoin="round" stroke-linecap="round"';

// ---------------------------------------------------------------- goods (viewBox 100)
export const goodsBody = {
  chai: () => { const c = C.red, d = shade(c, -0.3);
    return `
    <path d="M42,30 C36,22 48,18 42,10" fill="none" stroke="#fff" stroke-width="3.5" stroke-linecap="round" opacity=".85"/>
    <path d="M58,30 C52,22 64,18 58,10" fill="none" stroke="#fff" stroke-width="3.5" stroke-linecap="round" opacity=".85"/>
    <ellipse cx="50" cy="82" rx="36" ry="9" fill="#f6efe3" ${sw}/>
    <ellipse cx="50" cy="80" rx="22" ry="4.5" fill="#e3d6c1"/>
    <path d="M74,46 C93,42 93,70 70,67" fill="none" stroke="${OL}" stroke-width="10" stroke-linecap="round"/>
    <path d="M74,46 C93,42 93,70 70,67" fill="none" stroke="${c}" stroke-width="5" stroke-linecap="round"/>
    <path d="M20,38 L80,38 L73,74 Q50,86 27,74 Z" fill="${c}" ${sw}/>
    <path d="M24,56 Q50,64 76,56 L75,62 Q50,70 25,62 Z" fill="#fff3d6" opacity=".95"/>
    <path d="M25,59 Q50,67 75,59" fill="none" stroke="${C.yellow}" stroke-width="1.6" stroke-dasharray="2 3"/>
    <path d="M27,74 Q50,86 73,74 L74,68 Q50,80 26,68 Z" fill="${d}" opacity=".55"/>
    <ellipse cx="50" cy="38" rx="30" ry="8" fill="#fff6e8" ${sw}/>
    <ellipse cx="50" cy="39" rx="24" ry="5.2" fill="#9a5a2c"/>
    <ellipse cx="44" cy="38" rx="8" ry="1.6" fill="#c98a52" opacity=".8"/>
    <path d="M30,44 Q29,56 33,68" fill="none" stroke="#fff" stroke-width="3.5" stroke-linecap="round" opacity=".55"/>`; },

  choori: () => { const c = C.blue, l = shade(c, 0.45), d = shade(c, -0.35);
    const ring = (cy) => `
      <ellipse cx="50" cy="${cy}" rx="33" ry="12" fill="none" stroke="${OL}" stroke-width="12.5"/>
      <ellipse cx="50" cy="${cy}" rx="33" ry="12" fill="none" stroke="${c}" stroke-width="7.5"/>
      <path d="M19,${cy - 2} Q50,${cy - 16} 81,${cy - 2}" fill="none" stroke="${l}" stroke-width="2.4" stroke-linecap="round"/>
      <path d="M22,${cy + 5} Q50,${cy + 17} 78,${cy + 5}" fill="none" stroke="${d}" stroke-width="2.4" stroke-linecap="round"/>
      ${[0, 1, 2, 3, 4, 5, 6].map(i => { const a = Math.PI * (0.12 + i * 0.127); return `<circle cx="${(50 - 33 * Math.cos(a)).toFixed(1)}" cy="${(cy + 12 * Math.sin(a)).toFixed(1)}" r="1.9" fill="#ffd84a"/>`; }).join('')}`;
    return ring(34) + ring(50) + ring(66); },

  aam: () => { const c = C.yellow;
    return `
    <defs><radialGradient id="mg" cx="35%" cy="30%" r="80%"><stop offset="0" stop-color="#fff07a"/><stop offset=".55" stop-color="${c}"/><stop offset="1" stop-color="#f08a24"/></radialGradient></defs>
    <path d="M50,22 C78,18 90,46 82,66 C74,86 44,92 28,78 C14,64 20,40 34,28 C39,24 44,22 50,22 Z" fill="url(#mg)" ${sw}/>
    <path d="M70,74 C64,82 52,86 42,84" fill="none" stroke="#e8662a" stroke-width="4" stroke-linecap="round" opacity=".5"/>
    <path d="M51,23 C52,16 54,12 57,9" fill="none" stroke="${OL}" stroke-width="6" stroke-linecap="round"/>
    <path d="M51,23 C52,16 54,12 57,9" fill="none" stroke="#7a4a24" stroke-width="2.6" stroke-linecap="round"/>
    <path d="M56,14 C66,4 84,6 88,14 C78,22 64,22 56,14 Z" fill="${C.green}" ${sw}/>
    <path d="M58,14 C68,12 78,12 86,14" fill="none" stroke="${shade(C.green, -0.3)}" stroke-width="1.6"/>
    <ellipse cx="38" cy="38" rx="7" ry="10" transform="rotate(35 38 38)" fill="#fff" opacity=".55"/>`; },

  patang: () => { const c = C.green, d = shade(c, -0.3);
    return `
    <path d="M50,76 C46,82 58,84 54,90 C50,96 62,96 60,98" fill="none" stroke="${OL}" stroke-width="2.4" stroke-linecap="round"/>
    <path d="M47,84 l-9,-4 l1,9 z M58,92 l9,-5 l1,9 z" fill="${C.red}" ${sw}/>
    <path d="M50,8 L82,42 L50,78 L18,42 Z" fill="${c}" ${sw}/>
    <path d="M50,8 L82,42 L50,42 Z" fill="${shade(c, 0.25)}"/>
    <path d="M50,42 L50,78 L18,42 Z" fill="${d}"/>
    <path d="M50,24 L62,42 L50,58 L38,42 Z" fill="#fff6d8"/>
    <path d="M50,8 L82,42 L50,78 L18,42 Z" fill="none" ${sw}/>
    <path d="M50,10 L50,76 M20,42 L80,42" stroke="${OL}" stroke-width="2" opacity=".7"/>
    <circle cx="50" cy="42" r="3" fill="${C.red}" stroke="${OL}" stroke-width="1.5"/>`; },

  lattoo: () => { const c = C.purple, l = shade(c, 0.35), d = shade(c, -0.3);
    return `
    <defs><clipPath id="lc"><path d="M50,90 L26,54 C20,42 32,30 50,30 C68,30 80,42 74,54 Z"/></clipPath></defs>
    <path d="M44,90 L50,98 L56,90 Z" fill="#b8bcc6" ${sw}/>
    <path d="M50,90 L26,54 C20,42 32,30 50,30 C68,30 80,42 74,54 Z" fill="${c}"/>
    <g clip-path="url(#lc)">
      <path d="M10,50 Q50,62 90,50 L90,58 Q50,70 10,58 Z" fill="#ffd84a"/>
      <path d="M10,64 Q50,76 90,64 L90,70 Q50,82 10,70 Z" fill="#ff8fb7"/>
      <path d="M10,36 Q50,48 90,36 L90,42 Q50,54 10,42 Z" fill="${l}"/>
      <path d="M50,30 L50,92 L90,92 L90,30 Z" fill="${d}" opacity=".35"/>
    </g>
    <path d="M50,90 L26,54 C20,42 32,30 50,30 C68,30 80,42 74,54 Z" fill="none" ${sw}/>
    <rect x="43" y="14" width="14" height="18" rx="4" fill="#c98a52" ${sw}/>
    <ellipse cx="50" cy="15" rx="7" ry="3" fill="#e0a86a" ${sw}/>
    <path d="M57,22 C70,18 80,24 86,16" fill="none" stroke="#f3e2c0" stroke-width="2.6" stroke-linecap="round"/>
    <ellipse cx="36" cy="44" rx="4" ry="8" transform="rotate(30 36 44)" fill="#fff" opacity=".5"/>`; },

  diya: () => { const c = C.orange, d = shade(c, -0.35);
    return `
    <defs><radialGradient id="fg" cx="50%" cy="60%" r="50%"><stop offset="0" stop-color="#fff6b0" stop-opacity=".95"/><stop offset="1" stop-color="#ffcc33" stop-opacity="0"/></radialGradient></defs>
    <circle cx="74" cy="34" r="22" fill="url(#fg)"/>
    <path d="M74,48 C64,40 68,26 74,14 C80,26 84,40 74,48 Z" fill="#ffcf3a" ${sw}/>
    <path d="M74,45 C70,40 71,32 74,26 C77,32 78,40 74,45 Z" fill="#fffbe0"/>
    <path d="M12,54 C16,84 78,88 90,52 L74,54 C58,60 32,60 12,54 Z" fill="${c}" ${sw}/>
    <path d="M12,54 C30,62 60,62 76,54 L90,52 C76,50 70,46 62,46 C44,44 24,48 12,54 Z" fill="${d}" ${sw}/>
    <path d="M20,58 C40,64 62,64 82,56" fill="none" stroke="#fff3d6" stroke-width="2.2"/>
    ${[22, 34, 46, 58, 70].map((x, i) => `<circle cx="${x}" cy="${68 + (i % 2 ? 4 : 0) - Math.abs(x - 46) * 0.12}" r="2.6" fill="#fff3d6"/>`).join('')}
    <path d="M24,72 Q46,82 72,70" fill="none" stroke="${d}" stroke-width="2" opacity=".6"/>`; },
};

export function goodsIcon(key, size = 128, sticker = true) {
  return svg(size, size, `<defs>${stickerDefs('st')}</defs><g transform="translate(8,8) scale(.84)"><g ${sticker ? 'filter="url(#st)"' : ''}>${goodsBody[key]()}</g></g>`, '0 0 100 100');
}

// Order bubble: coloured disc, ring, faint goods silhouette behind the HUD number.
export function bubble(key, colorName, size = 128) {
  const c = C[colorName], d = shade(c, -0.25), l = shade(c, 0.35);
  return svg(size, size, `
  <defs>
    <radialGradient id="bg" cx="40%" cy="30%" r="75%"><stop offset="0" stop-color="${l}"/><stop offset=".6" stop-color="${c}"/><stop offset="1" stop-color="${d}"/></radialGradient>
    <filter id="sil"><feFlood flood-color="${shade(c, -0.45)}" flood-opacity=".45"/><feComposite in2="SourceAlpha" operator="in"/></filter>
    <filter id="sh" x="-20%" y="-20%" width="140%" height="140%"><feGaussianBlur in="SourceAlpha" stdDeviation="2"/><feOffset dy="2.5"/><feComponentTransfer><feFuncA type="linear" slope=".35"/></feComponentTransfer><feMerge><feMergeNode/><feMergeNode in="SourceGraphic"/></feMerge></filter>
  </defs>
  <g filter="url(#sh)">
    <circle cx="50" cy="50" r="44" fill="#fffaf0" stroke="${OL}" stroke-width="3"/>
    <circle cx="50" cy="50" r="36.5" fill="url(#bg)" stroke="${OL}" stroke-width="2"/>
  </g>
  <g transform="translate(22,22) scale(.56)" filter="url(#sil)">${goodsBody[key]()}</g>
  <path d="M17,47 A33,33 0 0 1 83,47 Q50,36 17,47 Z" fill="#fff" opacity=".28"/>
  <path d="M26,36 A27,27 0 0 1 44,23" fill="none" stroke="#fff" stroke-width="4.5" stroke-linecap="round" opacity=".8"/>
  <circle cx="24" cy="44" r="2.4" fill="#fff" opacity=".8"/>
  ${[0, 1, 2, 3, 4, 5, 6, 7].map(i => { const a = i * Math.PI / 4; return `<circle cx="${(50 + 40.3 * Math.cos(a)).toFixed(1)}" cy="${(50 + 40.3 * Math.sin(a)).toFixed(1)}" r="1.7" fill="${c}"/>`; }).join('')}`, '0 0 100 100');
}

// ---------------------------------------------------------------- crates (viewBox 100, plaque centre at y=60)
const wood = '#c98a4b', woodD = '#8f5528', woodL = '#e2ad6e';
function crateBox() {
  return `
  <defs><linearGradient id="wd" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${woodL}"/><stop offset=".25" stop-color="${wood}"/><stop offset="1" stop-color="#a86a34"/></linearGradient></defs>
  <rect x="5" y="24" width="90" height="72" rx="7" fill="url(#wd)" ${sw}/>
  <path d="M9,47 H91 M9,71 H91" stroke="${woodD}" stroke-width="2"/>
  <path d="M9,26 H91" stroke="${woodL}" stroke-width="3" opacity=".8"/>
  ${[[15, 35], [60, 40], [30, 59], [78, 84], [20, 86]].map(([x, y]) => `<path d="M${x},${y} q6,-2 10,0" stroke="${woodD}" stroke-width="1.4" fill="none" opacity=".7"/>`).join('')}
  <rect x="5" y="24" width="90" height="72" rx="7" fill="none" stroke="${woodD}" stroke-width="0"/>
  <path d="M5,31 V89 Q5,96 12,96 H20 V24 H12 Q5,24 5,31 Z M95,31 V89 Q95,96 88,96 H80 V24 H88 Q95,24 95,31 Z" fill="${woodD}" opacity=".55"/>
  ${[[12.5, 31], [87.5, 31], [12.5, 89], [87.5, 89]].map(([x, y]) => `<circle cx="${x}" cy="${y}" r="2.3" fill="#f0d39a" stroke="${OL}" stroke-width="1.2"/>`).join('')}
  <path d="M12,27 H88" stroke="#fff" stroke-width="2.4" stroke-linecap="round" opacity=".55"/>
  <rect x="5" y="88" width="90" height="8" rx="4" fill="#000" opacity=".14"/>
  <rect x="5" y="24" width="90" height="72" rx="7" fill="none" ${sw}/>`;
}
export function crate(key, colorName, size = 256) {
  const c = C[colorName], d = shade(c, -0.22);
  // goods peeking out of the top
  const peek = [[-22, 0, -14], [0, -6, 6], [22, 0, 16]].map(([dx, dy, rot]) =>
    `<g transform="translate(${50 + dx},${20 + dy}) rotate(${rot}) scale(.3) translate(-50,-50)">${goodsBody[key]()}</g>`).join('');
  return svg(size, size, `<defs>${stickerDefs('st', 2.4, 1.6)}
    <linearGradient id="pl" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${shade(c, 0.12)}"/><stop offset="1" stop-color="${d}"/></linearGradient></defs>
    <g filter="url(#st)">${peek}</g>
    <g filter="url(#st)">${crateBox()}
    <rect x="24" y="42.5" width="52" height="38" rx="9" fill="${shade(c, -0.5)}"/>
    <rect x="24" y="41" width="52" height="38" rx="9" fill="url(#pl)" stroke="${OL}" stroke-width="2.4"/>
    <path d="M28,58 V50 Q28,44.5 34,44.5 H66 Q72,44.5 72,50 V58 Q50,52 28,58 Z" fill="#fff" opacity=".32"/>
    <rect x="27" y="73" width="46" height="3.5" rx="1.75" fill="${shade(c, -0.4)}" opacity=".5"/>
    <circle cx="20" cy="60" r="0" fill="none"/></g>`, '0 0 100 100');
}
export function crateHidden(size = 256) {
  return svg(size, size, `<defs>${stickerDefs('st', 2.4, 1.6)}
    <pattern id="weave" width="5" height="5" patternUnits="userSpaceOnUse"><rect width="5" height="5" fill="#c7a266"/><path d="M0,2.5 H5 M2.5,0 V5" stroke="#a9844c" stroke-width="1.1"/></pattern></defs>
    <g filter="url(#st)">${crateBox()}
    <path d="M3,30 C20,16 80,16 97,30 L95,78 C80,86 70,74 58,84 C48,90 36,78 24,86 C14,90 8,82 4,80 Z" fill="url(#weave)" ${sw}/>
    <path d="M3,30 C20,16 80,16 97,30" fill="none" stroke="#e7cc93" stroke-width="2.5"/>
    <path d="M16,24 C22,50 20,70 22,86 M84,24 C78,50 80,70 78,84" fill="none" stroke="#7a4f22" stroke-width="3"/>
    <circle cx="50" cy="60" r="17" fill="#5a4a5e" stroke="${OL}" stroke-width="2.4"/>
    <circle cx="50" cy="60" r="13" fill="none" stroke="#8b7a90" stroke-width="1.6" stroke-dasharray="3 3"/></g>`, '0 0 100 100');
}

// ---------------------------------------------------------------- belt
export function tray(size = 128) {
  return svg(size, size, `<defs>
    <radialGradient id="br" cx="38%" cy="32%" r="75%"><stop offset="0" stop-color="#fff0a8"/><stop offset=".45" stop-color="#f0c35a"/><stop offset="1" stop-color="#b9822a"/></radialGradient>
    <filter id="sh" x="-20%" y="-20%" width="140%" height="140%"><feGaussianBlur in="SourceAlpha" stdDeviation="2.5"/><feOffset dy="3"/><feComponentTransfer><feFuncA type="linear" slope=".45"/></feComponentTransfer><feMerge><feMergeNode/><feMergeNode in="SourceGraphic"/></feMerge></filter></defs>
    <g filter="url(#sh)"><circle cx="50" cy="50" r="44" fill="url(#br)" stroke="${OL}" stroke-width="3"/></g>
    <circle cx="50" cy="50" r="33" fill="none" stroke="#a8721f" stroke-width="2.2"/>
    <circle cx="50" cy="50" r="31" fill="#e9b94f" opacity=".5"/>
    ${Array.from({ length: 16 }, (_, i) => { const a = i * Math.PI / 8; return `<circle cx="${(50 + 38.5 * Math.cos(a)).toFixed(1)}" cy="${(50 + 38.5 * Math.sin(a)).toFixed(1)}" r="1.6" fill="#a8721f"/>`; }).join('')}
    <path d="M22,36 A32,32 0 0 1 44,19" fill="none" stroke="#fff" stroke-width="4" stroke-linecap="round" opacity=".7"/>`, '0 0 100 100');
}

// Tileable belt strip, horizontal: 1 tile = belt thickness square.
function beltStrip() {
  return `
  <rect x="0" y="0" width="100" height="100" fill="#5b3a2c"/>
  <rect x="0" y="16" width="100" height="68" fill="#6e4636"/>
  ${[12.5, 37.5, 62.5, 87.5].map(x => `<rect x="${x - 1.5}" y="16" width="3" height="68" fill="#4a2e23"/><rect x="${x + 1.5}" y="16" width="1.5" height="68" fill="#8a5a45"/>`).join('')}
  <rect x="0" y="16" width="100" height="5" fill="#000" opacity=".18"/>
  <rect x="0" y="0" width="100" height="16" fill="#d79a4a"/><rect x="0" y="0" width="100" height="5" fill="#f2c27a"/><rect x="0" y="13" width="100" height="3" fill="${OL}"/>
  <rect x="0" y="84" width="100" height="16" fill="#d79a4a"/><rect x="0" y="84" width="100" height="3" fill="${OL}"/><rect x="0" y="95" width="100" height="5" fill="#a96b2c"/>
  ${[25, 75].map(x => `<circle cx="${x}" cy="8" r="2.6" fill="#fff0b8" stroke="${OL}" stroke-width="1"/><circle cx="${x}" cy="92" r="2.6" fill="#fff0b8" stroke="${OL}" stroke-width="1"/>`).join('')}`;
}
export const beltH = (size = 128) => svg(size, size, beltStrip(), '0 0 100 100');
export const beltV = (size = 128) => svg(size, size, `<g transform="rotate(90 50 50)">${beltStrip()}</g>`, '0 0 100 100');
export function beltCorner(size = 128) {
  return svg(size, size, `
  <rect x="0" y="0" width="100" height="100" rx="22" fill="#d79a4a" stroke="${OL}" stroke-width="5"/>
  <rect x="6" y="6" width="88" height="88" rx="18" fill="none" stroke="#f2c27a" stroke-width="3"/>
  <rect x="14" y="14" width="72" height="72" rx="14" fill="#5b3a2c" stroke="${OL}" stroke-width="3"/>`, '0 0 100 100');
}
export function beltHub(size = 128) {
  return svg(size, size, `<defs><radialGradient id="h" cx="40%" cy="35%" r="70%"><stop offset="0" stop-color="#fff0a8"/><stop offset="1" stop-color="#c08a30"/></radialGradient></defs>
  <circle cx="50" cy="50" r="34" fill="url(#h)" stroke="${OL}" stroke-width="4"/>
  ${[0, 1, 2, 3, 4, 5].map(i => `<rect x="47" y="20" width="6" height="30" rx="3" fill="#a8721f" transform="rotate(${i * 60} 50 50)"/>`).join('')}
  <circle cx="50" cy="50" r="10" fill="#e8b24a" stroke="${OL}" stroke-width="3"/>
  <circle cx="50" cy="50" r="3.5" fill="${OL}"/>`, '0 0 100 100');
}

// ---------------------------------------------------------------- gate (godown arch), viewBox 100x80
export function gate(w = 256, h = 256) {
  return svg(w, h, `<defs>
    <linearGradient id="in" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#1e100b"/><stop offset="1" stop-color="#4a2a1c"/></linearGradient>
    <linearGradient id="st" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#f0a865"/><stop offset="1" stop-color="#c9713a"/></linearGradient></defs>
  <path d="M8,96 V44 C8,30 20,22 30,18 C40,14 46,8 50,2 C54,8 60,14 70,18 C80,22 92,30 92,44 V96 Z" fill="url(#st)" ${sw}/>
  <path d="M14,96 V46 C14,34 24,28 32,24 C42,20 47,15 50,10 C53,15 58,20 68,24 C76,28 86,34 86,46 V96" fill="none" stroke="#fde2b4" stroke-width="2" stroke-dasharray="3 3"/>
  <path d="M22,96 V52 C22,40 32,34 40,30 C45,28 48,24 50,20 C52,24 55,28 60,30 C68,34 78,40 78,52 V96 Z" fill="url(#in)" ${sw}/>
  <path d="M22,96 V56 L34,62 V96 Z" fill="#2f8f83" ${sw}/><path d="M78,96 V56 L66,62 V96 Z" fill="#2f8f83" ${sw}/>
  ${[66, 74, 82, 90].map(y => `<circle cx="28" cy="${y}" r="1.6" fill="#ffd84a"/><circle cx="72" cy="${y}" r="1.6" fill="#ffd84a"/>`).join('')}
  <circle cx="50" cy="40" r="4" fill="#ffd84a" stroke="${OL}" stroke-width="1.5"/>
  <path d="M4,96 H96" stroke="${OL}" stroke-width="4" stroke-linecap="round"/>
  ${Array.from({ length: 9 }, (_, i) => `<path d="M${14 + i * 9},4 l0,0" />`).join('')}`, '0 0 100 100');
}

// ---------------------------------------------------------------- stalls (viewBox 100, bubble centre at (50,52))
const STALLS = [
  { name: 'chai', a: '#E8423D', b: '#fff3dc', cloth: '#f2c94c' },
  { name: 'choori', a: '#e2569a', b: '#ffd84a', cloth: '#387DEB' },
  { name: 'phal', a: '#3aa357', b: '#ffe27a', cloth: '#f08a24' },
  { name: 'patang', a: '#1f9e9a', b: '#fff3dc', cloth: '#E8423D' },
  { name: 'khilona', a: '#8a4fd0', b: '#ff9ec4', cloth: '#45BF5C' },
  { name: 'diya', a: '#e8762a', b: '#7a1f2b', cloth: '#ffd84a' },
];
export const STALL_COUNT = STALLS.length;

function stallDecor(i) {
  const mini = (key, x, y, s = 0.2, r = 0) => `<g transform="translate(${x},${y}) rotate(${r}) scale(${s * 1.3}) translate(-50,-50)">${goodsBody[key]()}</g>`;
  switch (i) {
    case 0: return `
      <path d="M12,70 h14 l-2,-12 h-10 z" fill="#c0c6cc" ${sw}/><path d="M26,62 q6,-2 6,-8" fill="none" stroke="${OL}" stroke-width="2.4"/>
      <path d="M15,58 q4,-6 8,0" fill="#c0c6cc" ${sw}/>
      ${mini('chai', 80, 66, 0.17)}${mini('chai', 90, 67, 0.15)}
      <path d="M17,52 c-3,-5 3,-7 0,-12" fill="none" stroke="#fff" stroke-width="2" opacity=".8"/>`;
    case 1: return `
      <path d="M18,72 V44" stroke="${OL}" stroke-width="3"/><path d="M82,72 V44" stroke="${OL}" stroke-width="3"/>
      ${[46, 52, 58, 64].map((y, k) => `<ellipse cx="18" cy="${y}" rx="6" ry="2.2" fill="none" stroke="${['#387DEB', '#E8423D', '#ffd84a', '#45BF5C'][k]}" stroke-width="2.4"/><ellipse cx="82" cy="${y}" rx="6" ry="2.2" fill="none" stroke="${['#9E5CDB', '#ffd84a', '#387DEB', '#e2569a'][k]}" stroke-width="2.4"/>`).join('')}`;
    case 2: return `
      <path d="M8,72 q8,-12 18,0 z" fill="#b5793a" ${sw}/>${mini('aam', 13, 64, 0.13)}${mini('aam', 20, 63, 0.13, 20)}
      <path d="M74,72 q8,-12 18,0 z" fill="#b5793a" ${sw}/>
      <circle cx="80" cy="65" r="4" fill="#E8423D" ${sw}/><circle cx="87" cy="65" r="4" fill="#E8423D" ${sw}/>`;
    case 3: return `${mini('patang', 17, 52, 0.2, -15)}${mini('patang', 83, 52, 0.2, 15)}
      <circle cx="18" cy="68" r="5" fill="#f3e2c0" ${sw}/><circle cx="18" cy="68" r="2" fill="#c98a52"/>`;
    case 4: return `${mini('lattoo', 16, 63, 0.17)}${mini('lattoo', 84, 63, 0.17)}
      <circle cx="84" cy="48" r="4" fill="#ffd84a" ${sw}/><circle cx="16" cy="48" r="4" fill="#45BF5C" ${sw}/>`;
    default: return `${mini('diya', 16, 66, 0.15)}${mini('diya', 84, 66, 0.15)}${mini('diya', 22, 56, 0.12)}
      <path d="M8,40 Q50,52 92,40" fill="none" stroke="${OL}" stroke-width="1.2"/>
      ${[16, 30, 44, 58, 72, 86].map(x => `<circle cx="${x}" cy="${40 + 12 * (1 - Math.pow((x - 50) / 42, 2)) * 0.8}" r="2.2" fill="#ffe27a"/>`).join('')}`;
  }
}

export function stall(i, size = 256) {
  const s = STALLS[i];
  const scallops = Array.from({ length: 8 }, (_, k) => {
    const x0 = 6 + k * 11;
    return `<path d="M${x0},30 q5.5,9 11,0 z" fill="${k % 2 ? s.b : s.a}" ${sw}/>`;
  }).join('');
  const stripes = Array.from({ length: 8 }, (_, k) => `<path d="M${6 + k * 11},30 L${10 + k * 10},10 L${20 + k * 10},10 L${17 + k * 11},30 Z" fill="${k % 2 ? s.b : s.a}"/>`).join('');
  return svg(size, size, `<defs>${stickerDefs('st', 2, 1.8)}
    <linearGradient id="wall" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#5a3022"/><stop offset="1" stop-color="#7a4430"/></linearGradient></defs>
    <g filter="url(#st)">
    <rect x="9" y="22" width="82" height="52" fill="url(#wall)"/>
    <path d="M12,34 H88" stroke="#8a5238" stroke-width="2"/>
    <rect x="7" y="20" width="6" height="76" rx="2" fill="#a86a36" ${sw}/><rect x="87" y="20" width="6" height="76" rx="2" fill="#a86a36" ${sw}/>
    <linearGradient id="aw" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#fff" stop-opacity=".35"/><stop offset=".45" stop-color="#fff" stop-opacity="0"/><stop offset="1" stop-color="#000" stop-opacity=".22"/></linearGradient>
    <path d="M6,30 L10,10 L90,10 L94,30 Z" fill="${s.a}"/>${stripes}
    <path d="M6,30 L10,10 L90,10 L94,30 Z" fill="url(#aw)"/>
    <path d="M6,30 L10,10 L90,10 L94,30 Z" fill="none" ${sw}/>
    <path d="M10,10 L90,10" stroke="#fff" stroke-width="2" opacity=".5"/>
    ${scallops}
    <rect x="4" y="3" width="92" height="9" rx="3" fill="#8f5528" ${sw}/>
    <path d="M30,3 q20,-6 40,0" fill="#ffd84a" ${sw}/>
    ${stallDecor(i)}
    <linearGradient id="ct" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#d9965a"/><stop offset="1" stop-color="#9c5e2c"/></linearGradient>
    <rect x="5" y="72" width="90" height="24" rx="4" fill="url(#ct)" ${sw}/>
    <path d="M5,76 H95" stroke="#e2ad6e" stroke-width="2.5"/>
    <path d="M9,79 H91 V90 Q70,96 50,90 Q30,96 9,90 Z" fill="${s.cloth}" stroke="${OL}" stroke-width="1.8"/>
    ${[18, 34, 50, 66, 82].map(x => `<circle cx="${x}" cy="85" r="2" fill="#fff6e0" opacity=".9"/>`).join('')}
    </g>`, '0 0 100 100');
}

export function stallClosed(size = 256) {
  return svg(size, size, `<defs>${stickerDefs('st', 2, 1.6)}
    <linearGradient id="sh" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#8fa0ab"/><stop offset="1" stop-color="#5f6e78"/></linearGradient></defs>
    <rect x="11" y="30" width="78" height="64" rx="2" fill="url(#sh)" stroke="${OL}" stroke-width="2.4"/>
    ${Array.from({ length: 10 }, (_, k) => `<path d="M12,${36 + k * 6} H88" stroke="#4d5a63" stroke-width="1.6"/><path d="M12,${37.5 + k * 6} H88" stroke="#b9c6ce" stroke-width=".9"/>`).join('')}
    <rect x="42" y="86" width="16" height="5" rx="2" fill="#3d454c"/>
    <path d="M8,30 Q24,42 34,30 Q50,44 66,30 Q76,42 92,30" fill="none" stroke="#2e7d32" stroke-width="2"/>
    ${[12, 20, 28, 40, 50, 60, 72, 80, 88].map((x, k) => `<circle cx="${x}" cy="${32 + [3, 6, 3, 6, 9, 6, 3, 6, 3][k]}" r="3.6" fill="${k % 2 ? '#ffb020' : '#ff7a1a'}" stroke="#b34d00" stroke-width="1"/>`).join('')}
    <g filter="url(#st)"><circle cx="80" cy="20" r="12" fill="#3fb950" stroke="${OL}" stroke-width="2.4"/>
    <path d="M74,20 l4.5,4.5 l8,-9" fill="none" stroke="#fff" stroke-width="3.6" stroke-linecap="round" stroke-linejoin="round"/></g>`, '0 0 100 100');
}

export function stallLock(size = 256) {
  const chain = (x1, y1, x2, y2) => {
    const n = 9, out = [];
    for (let k = 0; k <= n; k++) {
      const t = k / n, x = x1 + (x2 - x1) * t, y = y1 + (y2 - y1) * t, a = Math.atan2(y2 - y1, x2 - x1) * 180 / Math.PI + (k % 2 ? 90 : 0);
      out.push(`<ellipse cx="${x}" cy="${y}" rx="5.2" ry="3.2" transform="rotate(${a} ${x} ${y})" fill="none" stroke="${OL}" stroke-width="4.4"/><ellipse cx="${x}" cy="${y}" rx="5.2" ry="3.2" transform="rotate(${a} ${x} ${y})" fill="none" stroke="#b7bcc4" stroke-width="2"/>`);
    }
    return out.join('');
  };
  return svg(size, size, `<defs>${stickerDefs('st', 2, 1.6)}</defs>
    <rect x="6" y="8" width="88" height="88" rx="6" fill="#1d1414" opacity=".45"/>
    ${chain(10, 22, 90, 88)}${chain(90, 22, 10, 88)}
    <g filter="url(#st)">
    <path d="M38,50 V40 a12,12 0 0 1 24,0 V50" fill="none" stroke="${OL}" stroke-width="8"/>
    <path d="M38,50 V40 a12,12 0 0 1 24,0 V50" fill="none" stroke="#f0c35a" stroke-width="4"/>
    <rect x="30" y="48" width="40" height="32" rx="6" fill="#3d3a40" stroke="${OL}" stroke-width="2.6"/>
    <rect x="33" y="51" width="34" height="5" rx="2.5" fill="#6b6670"/></g>`, '0 0 100 100');
}

// ---------------------------------------------------------------- background (1024x2048 = 12.4 x 24.8 world units, centre = world 0,0)
export const BG = { w: 1024, h: 2048, worldW: 12.4, worldH: 24.8 };
const wy = y => (BG.worldH / 2 - y) * (BG.h / BG.worldH);   // world y -> px
const wx = x => (x + BG.worldW / 2) * (BG.w / BG.worldW);

export function background() {
  const skyBase = wy(5.75), floorBottom = wy(-2.4);
  // seeded rng
  let seed = 7; const rnd = () => (seed = (seed * 16807) % 2147483647) / 2147483647;
  const far = [], near = [];
  // far skyline: domes and minarets
  for (let x = -20; x < 1044; x += 70 + rnd() * 60) {
    const h = 60 + rnd() * 70, w = 50 + rnd() * 50;
    far.push(`<rect x="${x}" y="${skyBase - 40 - h}" width="${w}" height="${h + 40}" fill="#f3c9a2"/>`);
    if (rnd() < 0.5) far.push(`<path d="M${x - 4},${skyBase - 40 - h} a${w / 2 + 4},${w / 2 + 4} 0 0 1 ${w + 8},0 z" fill="#f3c9a2"/><path d="M${x + w / 2},${skyBase - 40 - h - w / 2 - 4} v-14" stroke="#f3c9a2" stroke-width="3"/>`);
    else if (rnd() < 0.6) far.push(`<rect x="${x + w / 2 - 7}" y="${skyBase - 40 - h - 70}" width="14" height="70" fill="#f3c9a2"/><path d="M${x + w / 2 - 11},${skyBase - 40 - h - 70} a11,11 0 0 1 22,0 z" fill="#f3c9a2"/>`);
  }
  // near: bazaar houses with jharokha windows and awnings
  const awn = ['#E8423D', '#387DEB', '#45BF5C', '#FA8C29', '#9E5CDB', '#e2569a', '#1f9e9a'];
  for (let x = -10, k = 0; x < 1044; k++) {
    const w = 110 + rnd() * 70, h = 90 + rnd() * 70, top = skyBase - h, col = ['#e9a77a', '#dd9467', '#e8b183', '#d98a6a'][k % 4];
    near.push(`<rect x="${x}" y="${top}" width="${w}" height="${h + 10}" fill="${col}" stroke="${OL}" stroke-opacity=".25" stroke-width="2"/>`);
    near.push(`<rect x="${x - 4}" y="${top - 8}" width="${w + 8}" height="10" fill="${shade(col, -0.15)}"/>`);
    const win = w * 0.28;
    for (const fx of [0.22, 0.64]) {
      const wxp = x + w * fx;
      near.push(`<path d="M${wxp},${top + 58} v-26 a${win / 2},${win / 2} 0 0 1 ${win},0 v26 z" fill="#7a3f2a" opacity=".75"/><rect x="${wxp - 4}" y="${top + 56}" width="${win + 8}" height="6" fill="${shade(col, -0.2)}"/>`);
    }
    const a = awn[k % awn.length];
    near.push(`<path d="M${x + 6},${skyBase - 26} h${w - 12} l8,18 h${-(w + 4)} z" fill="${a}" stroke="${OL}" stroke-opacity=".4" stroke-width="2"/>`);
    for (let sx = x + 10; sx < x + w - 10; sx += 22) near.push(`<path d="M${sx},${skyBase - 26} h10 l3,18 h-10 z" fill="#fff6e0" opacity=".85"/>`);
    x += w + 4;
  }
  // floor tiles
  const tiles = [];
  for (let y = skyBase; y < floorBottom + 60; y += 64)
    for (let x = 0; x < 1024; x += 64) {
      const odd = ((x + y) / 64) % 2 === 0;
      tiles.push(`<rect x="${x}" y="${y}" width="64" height="64" fill="${odd ? '#f4dfb8' : '#efd5a8'}"/>`);
      tiles.push(`<path d="M${x + 32},${y + 20} l12,12 l-12,12 l-12,-12 z" fill="#e6c48e" opacity=".55"/>`);
    }
  // godown planks
  const planks = [];
  for (let x = 0; x < 1024; x += 56) {
    const c = ['#8a5634', '#7e4e2f', '#93603b'][Math.floor(x / 56) % 3];
    planks.push(`<rect x="${x}" y="${floorBottom}" width="56" height="${2048 - floorBottom}" fill="${c}"/><path d="M${x},${floorBottom} V2048" stroke="#5a341e" stroke-width="4"/>`);
    for (let y = floorBottom + 60 + (x % 3) * 50; y < 2048; y += 180 + (x % 5) * 20) planks.push(`<path d="M${x + 14},${y} q14,-6 28,0" stroke="#5a341e" stroke-width="2.5" fill="none" opacity=".6"/>`);
  }
  // jute sacks along godown sides
  const sack = (cx, cy, s, c) => `<g transform="translate(${cx},${cy}) scale(${s})"><path d="M-40,40 C-50,0 -36,-30 -20,-38 L-12,-50 L12,-50 L20,-38 C36,-30 50,0 40,40 Z" fill="${c}" stroke="${OL}" stroke-width="4"/><path d="M-14,-40 H14" stroke="#7a4f22" stroke-width="6"/><path d="M-28,0 q28,10 56,0" stroke="#a9844c" stroke-width="3" fill="none"/></g>`;
  return svg(BG.w, BG.h, `<defs>
    <linearGradient id="sky" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#ffcf9e"/><stop offset=".6" stop-color="#ffe6c4"/><stop offset="1" stop-color="#fff0d8"/></linearGradient>
    <radialGradient id="sun" cx="50%" cy="50%" r="50%"><stop offset="0" stop-color="#fff8e0"/><stop offset="1" stop-color="#fff8e0" stop-opacity="0"/></radialGradient>
    <radialGradient id="vig" cx="50%" cy="35%" r="70%"><stop offset=".65" stop-color="#000" stop-opacity="0"/><stop offset="1" stop-color="#2a1208" stop-opacity=".45"/></radialGradient>
    <linearGradient id="gd" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#2a1208" stop-opacity=".55"/><stop offset=".12" stop-color="#2a1208" stop-opacity="0"/></linearGradient>
  </defs>
  <rect width="1024" height="${skyBase}" fill="url(#sky)"/>
  <circle cx="760" cy="${skyBase - 260}" r="160" fill="url(#sun)"/>
  <circle cx="760" cy="${skyBase - 260}" r="46" fill="#fff4cf" opacity=".9"/>
  ${far.join('')}${near.join('')}
  ${tiles.join('')}
  <rect x="0" y="${skyBase - 4}" width="1024" height="14" fill="#c9875a"/><rect x="0" y="${skyBase + 10}" width="1024" height="8" fill="#000" opacity=".08"/>
  ${planks.join('')}
  <rect x="0" y="${floorBottom}" width="1024" height="${2048 - floorBottom}" fill="url(#gd)"/>
  <rect x="0" y="${floorBottom - 18}" width="1024" height="26" fill="#c46a3a" stroke="${OL}" stroke-width="3"/>
  ${Array.from({ length: 32 }, (_, k) => `<path d="M${k * 32 + 16},${floorBottom - 14} l7,9 l-7,9 l-7,-9 z" fill="#ffd27a" opacity=".9"/>`).join('')}
  ${sack(wx(-4.75), wy(-4.2), 0.9, '#c7a266')}${sack(wx(-4.95), wy(-5.6), 1.0, '#b8925a')}${sack(wx(4.8), wy(-4.5), 0.95, '#c7a266')}${sack(wx(4.9), wy(-6.1), 1.05, '#b8925a')}
  ${sack(wx(-4.85), wy(-7.4), 0.95, '#c7a266')}${sack(wx(4.85), wy(-7.8), 0.9, '#c7a266')}
  <rect width="1024" height="2048" fill="url(#vig)"/>`);
}

// Rangoli for the middle of the loop (512, transparent)
export function rangoli(size = 512) {
  const petals = (n, r, len, w, col, rot = 0) => Array.from({ length: n }, (_, k) =>
    `<ellipse cx="50" cy="${50 - r}" rx="${w}" ry="${len}" fill="${col}" transform="rotate(${rot + k * 360 / n} 50 50)"/>`).join('');
  const dots = (n, r, rr, col) => Array.from({ length: n }, (_, k) => { const a = k * 2 * Math.PI / n; return `<circle cx="${50 + r * Math.cos(a)}" cy="${50 + r * Math.sin(a)}" r="${rr}" fill="${col}"/>`; }).join('');
  return svg(size, size, `<g opacity=".9">
    <circle cx="50" cy="50" r="48" fill="#fff1d6" opacity=".55"/>
    ${dots(48, 47, 0.9, '#e8762a')}
    ${petals(16, 34, 10, 4.5, '#e2569a')}${petals(16, 34, 6, 2.2, '#ffd0e4')}
    ${petals(12, 22, 9, 5, '#ffb020', 15)}${petals(12, 22, 5, 2.2, '#fff3c2', 15)}
    ${petals(8, 12, 7, 4, '#387DEB', 22.5)}
    <circle cx="50" cy="50" r="6.5" fill="#45BF5C"/><circle cx="50" cy="50" r="3" fill="#ffd84a"/>
    ${dots(24, 41, 1.2, '#45BF5C')}${dots(12, 28.5, 1.1, '#9E5CDB')}
  </g>`, '0 0 100 100');
}

// Bunting strip (1024x128), top of the board
export function bunting(w = 1024, h = 128) {
  const cols = ['#E8423D', '#FACC2E', '#45BF5C', '#387DEB', '#FA8C29', '#e2569a', '#9E5CDB'];
  const sag = x => 22 + 40 * Math.sin(Math.PI * x / w);
  const flags = [];
  for (let k = 0, x = 20; x < w - 20; k++, x += 52) {
    const y = sag(x), y2 = sag(x + 36);
    flags.push(`<path d="M${x},${y} L${x + 36},${y2} L${x + 18},${(y + y2) / 2 + 46} Z" fill="${cols[k % cols.length]}" stroke="${OL}" stroke-width="3" stroke-linejoin="round"/>`);
    flags.push(`<path d="M${x + 6},${y + 6} L${x + 30},${y2 + 6}" stroke="#fff" stroke-width="3" opacity=".5"/>`);
  }
  let d = `M0,${sag(0)}`; for (let x = 8; x <= w; x += 8) d += ` L${x},${sag(x)}`;
  return svg(w, h, `<path d="${d}" fill="none" stroke="${OL}" stroke-width="4"/>${flags.join('')}`);
}

// ---------------------------------------------------------------- fx
export const fx = {
  sparkle: () => svg(64, 64, `<path d="M50,4 C54,40 60,46 96,50 C60,54 54,60 50,96 C46,60 40,54 4,50 C40,46 46,40 50,4 Z" fill="#fff"/>`, '0 0 100 100'),
  glow: () => svg(128, 128, `<defs><radialGradient id="g"><stop offset="0" stop-color="#fff" stop-opacity="1"/><stop offset=".4" stop-color="#fff" stop-opacity=".45"/><stop offset="1" stop-color="#fff" stop-opacity="0"/></radialGradient></defs><circle cx="50" cy="50" r="50" fill="url(#g)"/>`, '0 0 100 100'),
  ring: () => svg(128, 128, `<circle cx="50" cy="50" r="42" fill="none" stroke="#fff" stroke-width="7"/>`, '0 0 100 100'),
  puff: () => svg(64, 64, `<defs><radialGradient id="g"><stop offset="0" stop-color="#fff" stop-opacity=".9"/><stop offset="1" stop-color="#fff" stop-opacity="0"/></radialGradient></defs><circle cx="50" cy="50" r="50" fill="url(#g)"/>`, '0 0 100 100'),
  petal: () => svg(64, 64, `<path d="M50,6 C80,20 84,70 50,94 C16,70 20,20 50,6 Z" fill="#fff"/><path d="M50,14 V86" stroke="#000" stroke-opacity=".18" stroke-width="4"/>`, '0 0 100 100'),
  confetti: () => svg(32, 32, `<rect x="10" y="25" width="80" height="50" rx="8" fill="#fff"/>`, '0 0 100 100'),
  coin: () => svg(128, 128, `<defs><radialGradient id="c" cx="38%" cy="32%" r="75%"><stop offset="0" stop-color="#fff3a8"/><stop offset=".5" stop-color="#ffc83a"/><stop offset="1" stop-color="#d48a10"/></radialGradient></defs>
    <circle cx="50" cy="52" r="42" fill="#b36b00" stroke="${OL}" stroke-width="3.5"/><circle cx="50" cy="48" r="42" fill="url(#c)" stroke="${OL}" stroke-width="3.5"/>
    <circle cx="50" cy="48" r="31" fill="none" stroke="#d48a10" stroke-width="3"/>
    <path d="M50,26 l6.5,13.5 l15,2 l-11,10.5 l2.7,15 l-13.2,-7.2 l-13.2,7.2 l2.7,-15 l-11,-10.5 l15,-2 z" fill="#fff6c8" stroke="#d48a10" stroke-width="2" stroke-linejoin="round"/>`, '0 0 100 100'),
  kite: () => svg(128, 128, `<path d="M50,6 L80,40 L50,76 L20,40 Z" fill="#fff" stroke="#000" stroke-opacity=".25" stroke-width="2.5"/><path d="M50,6 V76 M20,40 H80" stroke="#000" stroke-opacity=".2" stroke-width="2"/><path d="M50,76 C44,84 58,88 50,96" fill="none" stroke="#fff" stroke-width="2.5"/>`, '0 0 100 100'),
  shadow: () => svg(128, 64, `<defs><radialGradient id="g"><stop offset="0" stop-color="#000" stop-opacity=".45"/><stop offset="1" stop-color="#000" stop-opacity="0"/></radialGradient></defs><ellipse cx="64" cy="32" rx="64" ry="32" fill="url(#g)"/>`),
};

export { STALLS };
