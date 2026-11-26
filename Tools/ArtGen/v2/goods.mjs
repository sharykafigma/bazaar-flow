// v2 goods. Each body() draws in a 0..100 viewBox using the style kit; icon() wraps it with the sticker rim.
import { mix, kitDefs, grad, ball, shape, gloss, glint, contact, line, lighten, darken, svg, id } from '../style.mjs';

export const COLORS = { red: '#E8423D', blue: '#387DEB', yellow: '#FACC2E', green: '#45BF5C', purple: '#9E5CDB', orange: '#FA8C29' };
export const GOLD = '#F2B53A', CREAM = '#FFF4E0';

export const body = {
  chai() {
    const c = COLORS.red, g1 = id('g'), g2 = id('g'), g3 = id('g'), g4 = id('g'), g5 = id('g');
    const cup = 'M19,40 C19,40 20,62 26,72 C31,80 69,80 74,72 C80,62 81,40 81,40 Z';
    const handle = 'M77,47 C93,44 95,68 72,70 L73,63 C86,62 85,52 78,54 Z';
    const saucer = 'M8,80 C8,73 92,73 92,80 C92,88 8,88 8,80 Z';
    return `<defs>${grad(g1, c, { top: 0.32, bottom: 0.35 })}${grad(g2, CREAM, { top: 0.4, bottom: 0.14 })}${grad(g3, c, { top: 0.2, bottom: 0.4 })}
      <radialGradient id="${g4}" cx="45%" cy="40%" r="60%"><stop offset="0" stop-color="#C9844C"/><stop offset=".7" stop-color="#9A5326"/><stop offset="1" stop-color="#6E3418"/></radialGradient>
      ${grad(g5, GOLD, { top: 0.4, bottom: 0.25 })}</defs>
    <g opacity=".9" filter="url(#blur1)">
      <path d="M40,32 C33,24 46,19 39,9" fill="none" stroke="#fff" stroke-width="4.5" stroke-linecap="round" opacity=".75"/>
      <path d="M57,31 C50,22 64,17 57,6" fill="none" stroke="#fff" stroke-width="4" stroke-linecap="round" opacity=".6"/>
    </g>
    ${contact(50, 84, 40, 5, 0.25)}
    ${shape(saucer, CREAM, g2, { w: 1.6 })}
    <path d="M14,79.5 C14,75 86,75 86,79.5" fill="none" stroke="${GOLD}" stroke-width="1.6" opacity=".9"/>
    <ellipse cx="50" cy="78" rx="24" ry="4" fill="${darken(CREAM, 0.25)}" opacity=".55"/>
    ${shape(handle, c, g3, { w: 1.6, small: true })}
    ${shape(cup, c, g1)}
    <path d="M21,52 C30,57 70,57 79,52 L78.3,60 C69,65 31,65 21.7,60 Z" fill="${CREAM}" opacity=".97"/>
    <path d="M21,52 C30,57 70,57 79,52" fill="none" stroke="${GOLD}" stroke-width="1.4"/>
    <path d="M21.7,60 C31,65 69,65 78.3,60" fill="none" stroke="${GOLD}" stroke-width="1.4"/>
    ${[28, 39, 50, 61, 72].map((x, i) => `<g transform="translate(${x},${58.5 + (i === 0 || i === 4 ? -0.8 : 0.6)})"><circle r="2.1" fill="${c}"/><circle r="0.9" fill="${GOLD}"/></g>`).join('')}
    ${gloss(31, 50, 5, 13, 12, 0.6)}
    <path d="M74,48 C75,58 73,66 70,71" fill="none" stroke="${darken(c, 0.5)}" stroke-width="2.5" opacity=".35" stroke-linecap="round"/>
    <ellipse cx="50" cy="40" rx="31.5" ry="8.5" fill="url(#${g5})" stroke="${line(GOLD)}" stroke-width="1.5"/>
    <ellipse cx="50" cy="40.6" rx="27" ry="6.2" fill="url(#${g4})"/>
    <ellipse cx="44" cy="39" rx="10" ry="1.8" fill="#E8B07A" opacity=".75"/>
    ${glint(63, 39.5, 1.2)}`;
  },
  choori() {
    const c = COLORS.blue, gG = id('g');
    // one glass bangle = thick elliptical torus: coloured dark rim, body gradient, inner shade, gold bead line, glass streaks
    const ring = (cx, cy, rx, ry, rot, k, part = '') => {
      const g = id('g'), tint = [c, lighten(c, 0.1), darken(c, 0.06)][k];
      const e = (w, col, extra = '', dx = 0, dy = 0, drx = 0) => `<ellipse cx="${cx + dx}" cy="${cy + dy}" rx="${rx + drx}" ry="${ry + drx}" fill="none" stroke="${col}" stroke-width="${w}" ${extra}/>`;
      return `<defs><linearGradient id="${g}" x1="0" y1="0" x2=".3" y2="1"><stop offset="0" stop-color="${lighten(tint, 0.55)}"/><stop offset=".4" stop-color="${tint}"/><stop offset="1" stop-color="${darken(tint, 0.42)}"/></linearGradient></defs>
      <g ${part}><g transform="rotate(${rot} ${cx} ${cy})">${e(11, line(c))}${e(8.4, `url(#${g})`)}
      ${e(2, darken(c, 0.5), 'opacity=".3"', 0, 0, -3)}
      ${e(1.4, `url(#${gG})`, '', 0, 0, 2)}
      ${e(2.6, '#fff', 'stroke-linecap="round" opacity=".9" pathLength="100" stroke-dasharray="0 54 14 32"', 0, 0, -1.6)}
      ${e(1.4, '#fff', 'stroke-linecap="round" opacity=".6" pathLength="100" stroke-dasharray="0 4 6 90"', 0, 0, 1.8)}
      ${e(2, lighten(GOLD, 0.5), 'stroke-linecap="round" pathLength="100" stroke-dasharray="0 12.5"', 0, 0, 2)}</g></g>`;
    };
    const cpA = id('cp');
    return `<defs>${grad(gG, GOLD, { top: 0.45, bottom: 0.3 })}<clipPath id="${cpA}"><rect x="43" y="0" width="19" height="46"/></clipPath></defs>
    ${contact(50, 86, 38, 4.5, 0.28)}
    ${ring(50, 73, 35, 11.5, 0, 2)}
    ${ring(37, 47, 19, 27, -16, 0)}
    ${ring(63, 47, 19, 27, 16, 1)}
    ${ring(37, 47, 19, 27, -16, 0, `clip-path="url(#${cpA})"`)}
    <g transform="translate(86,14)"><path d="M0,-10 C1,-3 3,-1 10,0 C3,1 1,3 0,10 C-1,3 -3,1 -10,0 C-3,-1 -1,-3 0,-10 Z" fill="#fff" stroke="${lighten(c, 0.3)}" stroke-width=".7"/><circle r="1.8" fill="#fff"/></g>
    <g transform="translate(12,20) scale(.5)"><path d="M0,-10 C1,-3 3,-1 10,0 C3,1 1,3 0,10 C-1,3 -3,1 -10,0 C-3,-1 -1,-3 0,-10 Z" fill="#fff" stroke="${lighten(c, 0.3)}" stroke-width="1"/></g>`;
  },

  aam() {
    const c = COLORS.yellow, g1 = id('g'), g2 = id('g'), g3 = id('g'), gL = id('g');
    const fruit = 'M52,22 C70,19 86,32 86,52 C86,74 70,90 50,90 C32,90 15,79 15,61 C15,49 22,44 27,38 C33,30 40,24 52,22 Z';
    const leaf = 'M53,22 C56,12 68,4 86,6 C82,18 70,26 53,22 Z';
    return `<defs>${ball(g1, c, { hi: 0.55, lo: 0.25, cx: '34%', cy: '28%' })}
      <radialGradient id="${g2}" cx="80%" cy="70%" r="55%"><stop offset="0" stop-color="#F0602E" stop-opacity=".9"/><stop offset=".45" stop-color="${COLORS.orange}" stop-opacity=".55"/><stop offset="1" stop-color="${COLORS.orange}" stop-opacity="0"/></radialGradient>
      ${grad(gL, COLORS.green, { top: 0.35, bottom: 0.35, angle: 'd' })}
      <clipPath id="${g3}"><path d="${fruit}"/></clipPath></defs>
    ${contact(50, 88, 33, 4.5, 0.28)}
    <path d="${fruit}" fill="url(#${g1})" filter="url(#vol)"/>
    <g clip-path="url(#${g3})"><path d="${fruit}" fill="url(#${g2})"/>
      <path d="M86,40 C80,72 62,88 34,90 L60,98 L96,70 Z" fill="${darken(c, 0.35)}" opacity=".22" filter="url(#blur2)"/></g>
    <path d="${fruit}" fill="none" stroke="${line(c)}" stroke-width="1.8" stroke-linejoin="round"/>
    <path d="M50,26 C46,40 44,52 48,66" fill="none" stroke="${darken(c, 0.2)}" stroke-width="1.6" opacity=".35" stroke-linecap="round"/>
    ${gloss(35, 42, 9, 14, 30, 0.8)}
    ${glint(29, 37, 1.8)}${glint(74, 46, 1)}${glint(68, 66, 0.8)}
    <path d="M52,24 C51,19 52,15 55,12" fill="none" stroke="#6B3F1C" stroke-width="3" stroke-linecap="round"/>
    ${shape(leaf, COLORS.green, gL, { w: 1.5, small: true })}
    <path d="M55,20 C64,15 73,11 83,8" fill="none" stroke="${darken(COLORS.green, 0.4)}" stroke-width="1.1" stroke-linecap="round" opacity=".7"/>
    <path d="M60,11 C66,8 72,7 78,8" fill="none" stroke="#fff" stroke-width="1.3" stroke-linecap="round" opacity=".55"/>`;
  },

  patang() {
    const c = COLORS.green, g1 = id('g'), g2 = id('g'), g3 = id('g'), cp = id('cp');
    const kite = 'M48,4 L84,38 L48,74 L12,38 Z';
    const bow = (x, y, r, col) => `<g transform="translate(${x},${y}) rotate(${r})">
      <path d="M0,0 C-4,-6 -10,-5 -9,0 C-10,5 -4,6 0,0 C4,-6 10,-5 9,0 C10,5 4,6 0,0 Z" fill="${col}" stroke="${line(col)}" stroke-width="1"/>
      <path d="M-7,-1.5 C-6,-3 -4,-3 -3,-2" fill="none" stroke="#fff" stroke-width="1" opacity=".7"/><circle r="1.8" fill="${darken(col, 0.2)}"/></g>`;
    return `<defs>${grad(g1, c, { top: 0.35, bottom: 0.3, angle: 'd' })}${grad(g2, lighten(c, 0.55), { top: 0.3, bottom: 0.1 })}${grad(g3, GOLD, { top: 0.4, bottom: 0.3 })}
      <clipPath id="${cp}"><path d="${kite}"/></clipPath></defs>
    ${contact(54, 94, 26, 3, 0.18)}
    <g transform="rotate(-8 48 40)">
      <path d="M48,74 C44,82 54,86 60,90 C66,94 74,92 80,96" fill="none" stroke="${line(c)}" stroke-width="1.4" stroke-linecap="round"/>
      ${shape(kite, c, g1, { w: 2 })}
      <g clip-path="url(#${cp})">
        <path d="M48,4 L84,38 L48,38 Z" fill="${darken(c, 0.25)}" opacity=".35"/>
        <path d="M48,38 L12,38 L48,74 Z" fill="${lighten(c, 0.15)}" opacity=".3"/>
        <path d="M48,56 L66,74 L48,92 L30,74 Z" fill="${darken(c, 0.42)}"/>
        <path d="M48,56 L66,74 L48,92 L30,74 Z" fill="none" stroke="${GOLD}" stroke-width="1.6"/>
        <path d="M48,4 L59,14.5 L48,25 L37,14.5 Z" fill="url(#${g2})" opacity=".85"/>
        <path d="M48,4 L59,14.5 L48,25 L37,14.5 Z" fill="none" stroke="${GOLD}" stroke-width="1.2"/>
      </g>
      <path d="M48,5 L48,73" stroke="${darken(c, 0.55)}" stroke-width="1.6"/>
      <path d="M13,38 C26,24 70,24 83,38" fill="none" stroke="${darken(c, 0.55)}" stroke-width="1.8" stroke-linecap="round"/>
      <path d="M16,37 C28,27 46,25 52,26" fill="none" stroke="#fff" stroke-width="1" opacity=".5"/>
      <path d="M48,8 L78,37" stroke="#fff" stroke-width="1.6" opacity=".35" stroke-linecap="round"/>
      ${gloss(34, 30, 6, 11, 45, 0.65)}
      <circle cx="48" cy="38" r="5.2" fill="url(#${g3})" stroke="${line(GOLD)}" stroke-width="1.2"/>
      <circle cx="48" cy="38" r="2" fill="${COLORS.red}"/>${glint(46.3, 36.3, 0.9)}
      <path d="M48,73 L41,82 L55,82 Z" fill="${GOLD}" stroke="${line(GOLD)}" stroke-width="1.2" stroke-linejoin="round"/>
      ${bow(60, 90, -20, COLORS.red)}${bow(80, 96, 10, GOLD)}
    </g>`;
  },

  lattoo() {
    const c = COLORS.purple, g1 = id('g'), gW = id('g'), gM = id('g'), cp = id('cp'), gS = id('g');
    const top = 'M50,24 C70,24 85,33 85,45 C85,57 66,72 54,84 C52,86 48,86 46,84 C34,72 15,57 15,45 C15,33 30,24 50,24 Z';
    const head = 'M43,14 C43,11 57,11 57,14 L57,26 C57,29 43,29 43,26 Z';
    const tip = 'M45.5,82 L54.5,82 L50,95 Z';
    return `<defs>${ball(g1, c, { hi: 0.5, lo: 0.4, cx: '32%', cy: '22%' })}${grad(gW, '#C8874A', { top: 0.35, bottom: 0.35, angle: 'h' })}
      <linearGradient id="${gM}" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#7d8592"/><stop offset=".35" stop-color="#f4f6fa"/><stop offset="1" stop-color="#5d6470"/></linearGradient>
      ${grad(gS, CREAM, { top: 0.3, bottom: 0.2 })}
      <clipPath id="${cp}"><path d="${top}"/></clipPath></defs>
    ${contact(50, 94, 18, 3, 0.3)}
    <path d="${tip}" fill="url(#${gM})" stroke="#40454f" stroke-width="1.2" stroke-linejoin="round"/>
    <path d="${top}" fill="url(#${g1})" filter="url(#vol)"/>
    <g clip-path="url(#${cp})">
      <path d="M10,40 C30,50 70,50 90,40 L90,47 C70,57 30,57 10,47 Z" fill="${lighten(c, 0.55)}" opacity=".9"/>
      <path d="M10,49 C30,59 70,59 90,49" fill="none" stroke="${GOLD}" stroke-width="2"/>
      <path d="M10,58 C30,68 70,68 90,58 L90,63 C70,73 30,73 10,63 Z" fill="#E8509A" opacity=".85"/>
      <path d="M10,32 C30,40 70,40 90,32" fill="none" stroke="${GOLD}" stroke-width="1.6"/>
      <path d="M14,68 C30,78 70,78 86,68 L86,90 L14,90 Z" fill="${darken(c, 0.3)}" opacity=".4"/>
      <path d="M62,24 C86,30 88,52 62,80" fill="none" stroke="${darken(c, 0.5)}" stroke-width="9" opacity=".22" filter="url(#blur2)"/>
    </g>
    <path d="M24,67 C38,74 62,74 76,64 M30,74 C42,79 58,79 70,71" fill="none" stroke="${darken(CREAM, 0.45)}" stroke-width="3.2" stroke-linecap="round"/>
    <path d="M24,67 C38,74 62,74 76,64 M30,74 C42,79 58,79 70,71" fill="none" stroke="url(#${gS})" stroke-width="1.9" stroke-linecap="round"/>
    <path d="${top}" fill="none" stroke="${line(c)}" stroke-width="1.9" stroke-linejoin="round"/>
    <path d="M76,64 C84,62 90,66 88,73 C86,79 80,76 82,72" fill="none" stroke="${darken(CREAM, 0.45)}" stroke-width="3.2" stroke-linecap="round"/>
    <path d="M76,64 C84,62 90,66 88,73 C86,79 80,76 82,72" fill="none" stroke="${CREAM}" stroke-width="1.9" stroke-linecap="round"/>
    ${gloss(33, 37, 8, 12, 50, 0.7)}
    <ellipse cx="50" cy="27" rx="13" ry="4" fill="${darken(c, 0.4)}" opacity=".45"/>
    ${shape(head, '#C8874A', gW, { w: 1.5, small: true })}
    <ellipse cx="50" cy="13.5" rx="7" ry="2.4" fill="${lighten('#C8874A', 0.35)}" stroke="${line('#C8874A')}" stroke-width="1.1"/>
    <path d="M45,17 L45,25" stroke="#fff" stroke-width="1.3" opacity=".5" stroke-linecap="round"/>
    ${glint(27, 36, 1.7)}`;
  },

  diya() {
    const c = COLORS.orange, g1 = id('g'), g2 = id('g'), gF = id('g'), gO = id('g'), cp = id('cp'), gH = id('g');
    const bowl = 'M10,56 C10,50 30,46 50,46 C66,46 80,47 92,50 C88,56 82,58 80,60 C76,74 64,82 48,82 C30,82 13,72 10,56 Z';
    const rim = 'M10,56 C10,50 30,46 50,46 C66,46 80,47 92,50 C84,55 78,60 64,62 C54,64 42,64 30,63 C18,62 10,60 10,56 Z';
    const oil = 'M17,55.5 C18,52 32,50 50,50 C64,50 76,50.6 85,51.5 C78,55 72,57.6 62,58.6 C52,59.6 40,59.6 30,59 C21,58.4 16.5,57.5 17,55.5 Z';
    const flame = 'M78,52 C70,48 70,38 74,30 C77,24 78,18 77,10 C84,16 90,26 90,36 C90,46 85,52 78,52 Z';
    const core = 'M79,50 C75,47 75,40 78,35 C80,31 81,28 81,24 C85,30 87,36 86,42 C85,47 82,50 79,50 Z';
    return `<defs>${grad(g1, c, { top: 0.25, bottom: 0.4 })}
      <linearGradient id="${g2}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#5a2a10"/><stop offset="1" stop-color="#9a5a22"/></linearGradient>
      <linearGradient id="${gF}" x1="0" y1="1" x2="0" y2="0"><stop offset="0" stop-color="#ff7a1a"/><stop offset=".45" stop-color="#ffb02a"/><stop offset="1" stop-color="#ffe066"/></linearGradient>
      <linearGradient id="${gO}" x1="0" y1="1" x2="0" y2="0"><stop offset="0" stop-color="#fff2b0"/><stop offset="1" stop-color="#ffffff"/></linearGradient>
      <radialGradient id="${gH}"><stop offset="0" stop-color="#ffd23a" stop-opacity=".85"/><stop offset=".45" stop-color="#ffb02a" stop-opacity=".35"/><stop offset="1" stop-color="#ff9a2a" stop-opacity="0"/></radialGradient>
      <clipPath id="${cp}"><path d="${bowl}"/></clipPath></defs>
    <ellipse cx="81" cy="33" rx="15" ry="19" fill="url(#${gH})"/>
    ${contact(48, 82, 36, 4.5, 0.3)}
    <path d="M36,80 C36,86 60,86 60,80 Z" fill="${darken(c, 0.35)}" stroke="${line(c)}" stroke-width="1.4"/>
    <path d="${bowl}" fill="url(#${g1})" filter="url(#vol)"/>
    <g clip-path="url(#${cp})">
      <path d="M12,64 C26,72 66,74 82,62" fill="none" stroke="${CREAM}" stroke-width="1.6" opacity=".95"/>
      <path d="M14,69 C28,77 64,79 78,68" fill="none" stroke="${GOLD}" stroke-width="1.4" opacity=".95"/>
      ${[[22, 68.2], [31, 71.4], [40, 73], [49, 73.4], [58, 72.6], [67, 70.6], [75, 67.4]].map(([x, y], i) =>
        `<circle cx="${x}" cy="${y}" r="${i % 2 ? 1.6 : 2.1}" fill="${i % 2 ? GOLD : CREAM}" stroke="${darken(c, 0.4)}" stroke-width=".4"/>`).join('')}
      ${[[30, 77.5], [44, 79], [58, 78]].map(([x, y]) => `<path d="M${x - 3},${y} l3,-2.4 l3,2.4 l-3,2.4 Z" fill="${COLORS.red}" opacity=".85"/>`).join('')}
    </g>
    <path d="${bowl}" fill="none" stroke="${line(c)}" stroke-width="1.8" stroke-linejoin="round"/>
    <path d="${rim}" fill="${lighten(c, 0.2)}" stroke="${line(c)}" stroke-width="1.5" stroke-linejoin="round"/>
    <path d="${oil}" fill="url(#${g2})"/>
    <ellipse cx="40" cy="53.5" rx="12" ry="1.6" fill="#E8B07A" opacity=".55"/>
    <path d="M16,55 C18,50 34,48 50,48" fill="none" stroke="#fff" stroke-width="1.3" opacity=".6" stroke-linecap="round"/>
    ${gloss(24, 66, 5, 8, 50, 0.55)}
    <path d="M74,53 C77,50 79,50 81,51" fill="none" stroke="#3a1a0a" stroke-width="2.4" stroke-linecap="round"/>
    <path d="${flame}" fill="url(#${gF})" stroke="#e8661a" stroke-width="1.2" stroke-linejoin="round"/>
    <path d="${core}" fill="url(#${gO})"/>
    <circle cx="80" cy="33" r="10" fill="#fff" opacity=".18" filter="url(#blur4)"/>`;
  },
};

/** Sticker-rimmed goods icon. */
export function icon(key, size = 256) {
  return svg(size, size, `<defs>${kitDefs()}</defs><g transform="translate(7,7) scale(.86)"><g filter="url(#sticker)">${body[key]()}</g></g>`);
}

/** Tighter drop shadow than the kit's, so badge/tray/crate shadows stay inside the 0..100 canvas. */
const gdrop = (fid, blur = 1.6, dy = 2) => `<filter id="${fid}" x="-25%" y="-25%" width="150%" height="150%">
    <feGaussianBlur in="SourceAlpha" stdDeviation="${blur}"/><feOffset dy="${dy}"/>
    <feComponentTransfer><feFuncA type="linear" slope=".4"/></feComponentTransfer>
    <feColorMatrix type="matrix" values="0 0 0 0 .16  0 0 0 0 .06  0 0 0 0 .03  0 0 0 1 0"/>
    <feMerge><feMergeNode/><feMergeNode in="SourceGraphic"/></feMerge></filter>`;
const fit = (k, inner) => `<g transform="translate(50,50) scale(${k}) translate(-50,-50)">${inner}</g>`;

export const KEYS = ['chai', 'choori', 'aam', 'patang', 'lattoo', 'diya'];
export const KEY_COLOR = { chai: COLORS.red, choori: COLORS.blue, aam: COLORS.yellow, patang: COLORS.green, lattoo: COLORS.purple, diya: COLORS.orange };

/** Round order badge: cream ring, goods-colour disc, faint goods silhouette. Centre stays calm for a white number. */
export function bubble(key, size = 256) {
  const c = KEY_COLOR[key], fD = id('f'), gD = id('g'), gR = id('g'), gS = id('g'), fS = id('f');
  return svg(size, size, `<defs>${kitDefs()}
    <radialGradient id="${gD}" cx="40%" cy="30%" r="75%"><stop offset="0" stop-color="${lighten(c, 0.3)}"/><stop offset=".6" stop-color="${c}"/><stop offset="1" stop-color="${darken(c, 0.3)}"/></radialGradient>
    ${grad(gR, CREAM, { top: 0.5, bottom: 0.2 })}${grad(gS, GOLD, { top: 0.4, bottom: 0.3 })}
    <filter id="${fS}"><feFlood flood-color="${darken(c, 0.45)}" flood-opacity=".3"/><feComposite in2="SourceAlpha" operator="in"/></filter>${gdrop(fD)}</defs>
  ${fit(0.93, `<g filter="url(#${fD})">
    <circle cx="50" cy="50" r="45" fill="url(#${gS})" stroke="${line(GOLD)}" stroke-width="1.6"/>
    <circle cx="50" cy="50" r="41.5" fill="url(#${gR})" filter="url(#volS)"/>
  </g>
  ${Array.from({ length: 12 }, (_, i) => { const a = i * Math.PI / 6; return `<circle cx="${(50 + 39.2 * Math.cos(a)).toFixed(2)}" cy="${(50 + 39.2 * Math.sin(a)).toFixed(2)}" r="1.3" fill="${GOLD}"/>`; }).join('')}
  <circle cx="50" cy="50" r="35.5" fill="url(#${gD})" stroke="${line(c)}" stroke-width="1.6"/>
  <circle cx="50" cy="50" r="35.5" fill="none" stroke="${darken(c, 0.5)}" stroke-width="3" opacity=".25" filter="url(#blur1)"/>
  <g transform="translate(23,23) scale(.54)" filter="url(#${fS})">${body[key]()}</g>
  <path d="M18,47 A32,32 0 0 1 82,47 C62,38 38,38 18,47 Z" fill="#fff" opacity=".22"/>
  <path d="M24,38 A28,28 0 0 1 44,22.5" fill="none" stroke="#fff" stroke-width="3.6" stroke-linecap="round" opacity=".85"/>
  ${glint(21.5, 45, 1.8)}`)}`);
}

/** Brass thaali seen from above. */
export function tray(size = 256) {
  const g1 = id('g'), g2 = id('g'), g3 = id('g'), fD = id('f');
  const B = '#E0A93A';
  return svg(size, size, `<defs>${kitDefs()}
    <radialGradient id="${g1}" cx="36%" cy="30%" r="80%"><stop offset="0" stop-color="#FFF3B8"/><stop offset=".4" stop-color="#F2C556"/><stop offset=".85" stop-color="#C98A26"/><stop offset="1" stop-color="#A66C1C"/></radialGradient>
    <radialGradient id="${g2}" cx="62%" cy="68%" r="75%"><stop offset="0" stop-color="#F8D570"/><stop offset=".6" stop-color="#E0A93A"/><stop offset="1" stop-color="#B57A22"/></radialGradient>
    <radialGradient id="${g3}" cx="40%" cy="34%" r="70%"><stop offset="0" stop-color="#FFE9A0"/><stop offset="1" stop-color="#D49A34"/></radialGradient>${gdrop(fD)}</defs>
  ${fit(0.93, `<g filter="url(#${fD})"><circle cx="50" cy="50" r="45" fill="url(#${g1})" stroke="${line(B)}" stroke-width="1.8"/></g>
  <circle cx="50" cy="50" r="36" fill="url(#${g2})" stroke="${darken(B, 0.35)}" stroke-width="1.3"/>
  <circle cx="50" cy="50" r="36" fill="none" stroke="#fff6cc" stroke-width="1" transform="translate(.6,.9)" opacity=".7"/>
  <circle cx="50" cy="50" r="33" fill="none" stroke="${darken(B, 0.2)}" stroke-width="1.6" opacity=".45" filter="url(#blur1)"/>
  ${Array.from({ length: 20 }, (_, i) => { const a = i * Math.PI / 10; return `<circle cx="${(50 + 40.5 * Math.cos(a)).toFixed(2)}" cy="${(50 + 40.5 * Math.sin(a)).toFixed(2)}" r="1.25" fill="#fff2b6" stroke="${darken(B, 0.35)}" stroke-width=".5"/>`; }).join('')}
  <g opacity=".42" fill="none" stroke="${darken(B, 0.35)}" stroke-width=".9">
    ${Array.from({ length: 8 }, (_, i) => `<path d="M50,50 C46,40 46,30 50,24 C54,30 54,40 50,50 Z" transform="rotate(${i * 45} 50 50)"/>`).join('')}
    <circle cx="50" cy="50" r="27"/>
  </g>
  <circle cx="50" cy="50" r="7" fill="url(#${g3})" stroke="${darken(B, 0.3)}" stroke-width=".9"/>
  <path d="M20,37 A33,33 0 0 1 40,17" fill="none" stroke="#fff" stroke-width="3.5" stroke-linecap="round" opacity=".75"/>
  <path d="M70,80 A33,33 0 0 0 82,68" fill="none" stroke="#fff" stroke-width="1.6" stroke-linecap="round" opacity=".4"/>
  ${gloss(34, 30, 10, 5, -35, 0.45)}${glint(23, 30, 1.6)}`)}`);
}

// ---------------------------------------------------------------- crates (body x5..95 y22..96, plaque centre (50,60) ~52x38)
const WOOD = '#C98A4B', IRON = '#5E6A78';
function crateBox() {
  const gF = id('g'), gP = id('g'), gI = id('g'), gIn = id('g');
  const plank = (y, h) => `<rect x="15" y="${y}" width="70" height="${h}" fill="url(#${gP})"/>
    <path d="M15,${y + 0.6} H85" stroke="#fff" stroke-width=".8" opacity=".3"/><path d="M15,${y + h - 0.5} H85" stroke="${darken(WOOD, 0.5)}" stroke-width="1.1" opacity=".75"/>`;
  const grain = [[19, 38, 14], [56, 41, 18], [20, 52, 10], [70, 55, 12], [18, 70, 12], [66, 74, 16], [30, 84, 18]].map(([x, y, w]) =>
    `<path d="M${x},${y} c${w * 0.3},-1.6 ${w * 0.7},1.6 ${w},0" fill="none" stroke="${darken(WOOD, 0.35)}" stroke-width=".8" opacity=".6"/>`).join('');
  const knot = (x, y) => `<ellipse cx="${x}" cy="${y}" rx="2.2" ry="1.3" fill="none" stroke="${darken(WOOD, 0.4)}" stroke-width=".8" opacity=".6"/>`;
  const post = (x) => `<rect x="${x}" y="22" width="12" height="74" rx="${x < 50 ? 4 : 4}" fill="url(#${gF})" filter="url(#volS)"/>
    <path d="M${x + 3},26 V92" stroke="${darken(WOOD, 0.3)}" stroke-width=".7" opacity=".5"/><path d="M${x + 8},30 C${x + 9},50 ${x + 7.5},70 ${x + 8.5},90" stroke="${darken(WOOD, 0.3)}" stroke-width=".7" fill="none" opacity=".5"/>`;
  const nail = (x, y) => `<circle cx="${x}" cy="${y}" r="1.55" fill="#dfe4ea" stroke="#2f3640" stroke-width=".7"/><circle cx="${x - 0.45}" cy="${y - 0.45}" r=".55" fill="#fff"/>`;
  const bracket = (x, y, sx, sy) => `<g transform="translate(${x},${y}) scale(${sx},${sy})">
    <path d="M0,3 Q0,0 3,0 H16 Q17.5,0 17.5,1.5 V5 Q17.5,6.5 16,6.5 H6.5 V16 Q6.5,17.5 5,17.5 H1.5 Q0,17.5 0,16 Z" fill="url(#${gI})" stroke="#2a3038" stroke-width=".9"/>
    <path d="M1.5,14 V3 Q1.5,1.5 3,1.5 H15" fill="none" stroke="#fff" stroke-width=".8" opacity="${sy > 0 ? 0.55 : 0.2}"/></g>
    ${nail(x + sx * 3.3, y + sy * 3.3)}${nail(x + sx * 12.5, y + sy * 3.3)}${nail(x + sx * 3.3, y + sy * 12.5)}`;
  return `<defs>${grad(gF, lighten(WOOD, 0.08), { top: 0.3, bottom: 0.3, angle: 'h' })}
    <linearGradient id="${gP}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${darken(WOOD, 0.06)}"/><stop offset="1" stop-color="${darken(WOOD, 0.24)}"/></linearGradient>
    <linearGradient id="${gI}" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="${lighten(IRON, 0.45)}"/><stop offset=".5" stop-color="${IRON}"/><stop offset="1" stop-color="${darken(IRON, 0.3)}"/></linearGradient>
    <linearGradient id="${gIn}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${darken(WOOD, 0.55)}"/><stop offset="1" stop-color="${darken(WOOD, 0.35)}"/></linearGradient></defs>
  <rect x="5" y="22" width="90" height="74" rx="6" fill="${darken(WOOD, 0.45)}"/>
  ${plank(33, 18.5)}${plank(51.5, 18.5)}${plank(70, 18)}
  ${grain}${knot(76, 46)}${knot(24, 79)}
  <rect x="15" y="33" width="70" height="3" fill="${darken(WOOD, 0.6)}" opacity=".35"/>
  ${post(5)}${post(83)}
  <rect x="5" y="22" width="90" height="12" rx="4" fill="url(#${gF})" filter="url(#volS)"/>
  <path d="M10,25 H90" stroke="#fff" stroke-width="1.4" opacity=".45" stroke-linecap="round"/>
  <path d="M22,29 c8,-1.5 16,1.5 24,0 M58,28.5 c6,-1.2 12,1.2 18,0" fill="none" stroke="${darken(WOOD, 0.3)}" stroke-width=".8" opacity=".6"/>
  <rect x="5" y="87" width="90" height="9" rx="3.5" fill="url(#${gF})" filter="url(#volS)"/>
  <rect x="5" y="87" width="90" height="9" rx="3.5" fill="${darken(WOOD, 0.2)}" opacity=".35"/>
  <path d="M15,${87.5} H85" stroke="#fff" stroke-width=".8" opacity=".35"/>
  <rect x="5" y="22" width="90" height="74" rx="6" fill="none" stroke="${line(WOOD)}" stroke-width="1.8"/>
  <path d="M5,34 H95 M5,87 H95 M17,34 V87 M83,34 V87" stroke="${line(WOOD)}" stroke-width="1.1" opacity=".8"/>
  ${bracket(5, 22, 1, 1)}${bracket(95, 22, -1, 1)}${bracket(5, 96, 1, -1)}${bracket(95, 96, -1, -1)}`;
}

/** Wooden crate with goods peeking out of the top and a glossy goods-colour plaque for a white number. */
export function crate(key, size = 512) {
  const c = KEY_COLOR[key], pc = key === 'aam' ? mix(c, '#D9670A', 0.35) : darken(c, 0.18), gP = id('g'), cp = id('cp'), gIn = id('g');
  const item = (dx, cy, rot, s) => `<g transform="translate(${50 + dx},${cy}) rotate(${rot}) scale(${s}) translate(-50,-50)">${body[key]()}</g>`;
  const peek = item(-25, 21, -18, 0.37) + item(25, 21, 16, 0.37);
  const front = item(0, 21, 0, 0.42);
  return svg(size, size, `<defs>${kitDefs()}
    <linearGradient id="${gP}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${lighten(pc, 0.18)}"/><stop offset=".5" stop-color="${pc}"/><stop offset="1" stop-color="${darken(pc, 0.3)}"/></linearGradient>
    <linearGradient id="${gIn}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="${darken(WOOD, 0.7)}"/><stop offset="1" stop-color="${darken(WOOD, 0.5)}"/></linearGradient>
    ${gdrop(cp + 'd', 1.1, 1.4)}</defs>
  <g filter="url(#${cp}d)">
    <path d="M8,26 C8,20 12,19 16,19 H84 C88,19 92,20 92,26 Z" fill="url(#${gIn})"/>
    <g clip-path="url(#${cp})">${peek}</g>
    ${crateBox()}
    <ellipse cx="50" cy="32.5" rx="17" ry="3.2" fill="#2a0f08" opacity=".35" filter="url(#blur1)"/>
    ${front}
    <rect x="24" y="42.5" width="52" height="38" rx="8" fill="${darken(c, 0.55)}" opacity=".6" filter="url(#blur1)"/>
    <rect x="24" y="41" width="52" height="38" rx="8" fill="url(#${gP})" filter="url(#vol)"/>
    <rect x="24" y="41" width="52" height="38" rx="8" fill="none" stroke="${line(c)}" stroke-width="1.8"/>
    <rect x="27" y="44" width="46" height="32" rx="6" fill="none" stroke="${lighten(c, 0.35)}" stroke-width=".9" opacity=".6"/>
    <path d="M28,57 V50 Q28,45 33,45 H67 Q72,45 72,50 V55 Q50,50 28,57 Z" fill="#fff" opacity=".28"/>
    ${glint(30, 48, 1.3)}
  </g>`);
}

/** Hidden crate: the same crate under a tied jute sack, dark round patch at (50,60) for a "?". */
export function crateHidden(size = 512) {
  const J = '#C9A266', gJ = id('g'), pt = id('p'), gD = id('g'), cp = id('cp'), gR = id('g');
  const sack = 'M4,34 C3,26 7,20 13,18 C28,12 72,12 87,18 C93,20 97,26 96,34 L94.5,80 C88,86 80,79 70,84 C60,90 52,82 42,88 C32,92 24,83 14,87 C8,88 5,84 5,80 Z';
  return svg(size, size, `<defs>${kitDefs()}
    ${grad(gJ, J, { top: 0.3, bottom: 0.35 })}
    <pattern id="${pt}" width="3" height="3" patternUnits="userSpaceOnUse"><path d="M0,1.5 H3" stroke="${darken(J, 0.35)}" stroke-width=".7" opacity=".38"/><path d="M1.5,0 V3" stroke="${lighten(J, 0.3)}" stroke-width=".6" opacity=".3"/></pattern>
    <radialGradient id="${gD}" cx="45%" cy="38%" r="65%"><stop offset="0" stop-color="#6b5470"/><stop offset=".7" stop-color="#3e2c44"/><stop offset="1" stop-color="#2a1c30"/></radialGradient>
    ${grad(gR, '#B07A3A', { top: 0.35, bottom: 0.3 })}
    <clipPath id="${cp}"><path d="${sack}"/></clipPath>${gdrop(cp + 'd', 1.1, 1.4)}</defs>
  <g filter="url(#${cp}d)">
    ${crateBox()}
    <path d="${sack}" fill="url(#${gJ})" filter="url(#vol)"/>
    <g clip-path="url(#${cp})">
      <rect x="0" y="0" width="100" height="100" fill="url(#${pt})"/>
      <path d="M22,18 C26,40 22,64 26,90 M50,14 C52,30 49,50 51,70 M78,18 C74,40 78,64 74,88" fill="none" stroke="${darken(J, 0.4)}" stroke-width="2.2" opacity=".35" filter="url(#blur1)"/>
      <path d="M3,38 C30,44 70,44 97,38 L97,47 C70,52 30,52 3,47 Z" fill="${darken(J, 0.35)}" opacity=".35" filter="url(#blur1)"/>
    </g>
    <path d="${sack}" fill="none" stroke="${line(J)}" stroke-width="1.8" stroke-linejoin="round"/>
    <path d="M9,30 C10,23 16,20 30,18" fill="none" stroke="#fff" stroke-width="1.8" opacity=".5" stroke-linecap="round"/>
    <path d="M8,27 C26,21 74,21 92,27" fill="none" stroke="${darken(J, 0.45)}" stroke-width=".9" stroke-dasharray="2 1.6" opacity=".7"/>
    <path d="M4,37 C30,43 70,43 96,37" fill="none" stroke="${line('#B07A3A')}" stroke-width="4.6" stroke-linecap="round"/>
    <path d="M4,37 C30,43 70,43 96,37" fill="none" stroke="url(#${gR})" stroke-width="3.2" stroke-linecap="round"/>
    <path d="M4,37 C30,43 70,43 96,37" fill="none" stroke="${line('#B07A3A')}" stroke-width=".6" stroke-dasharray="1.6 1.6" opacity=".6"/>
    <g stroke-linejoin="round">
      <path d="M49,39 C44,46 40,50 36,54 M51,39 C56,46 60,50 64,53" fill="none" stroke="${line('#B07A3A')}" stroke-width="4.2" stroke-linecap="round"/>
      <path d="M49,39 C44,46 40,50 36,54 M51,39 C56,46 60,50 64,53" fill="none" stroke="url(#${gR})" stroke-width="2.8" stroke-linecap="round"/>
      <path d="M48,39 C40,32 30,30 31,37 C32,43 42,42 48,40 Z M52,39 C60,32 70,30 69,37 C68,43 58,42 52,40 Z" fill="url(#${gR})" stroke="${line('#B07A3A')}" stroke-width="1.2"/>
      <path d="M46,38.5 C40,35 35,34 35,37 M54,38.5 C60,35 65,34 65,37" fill="none" stroke="${darken('#B07A3A', 0.45)}" stroke-width="1.2" opacity=".6"/>
      <ellipse cx="50" cy="39.5" rx="4" ry="3.4" fill="url(#${gR})" stroke="${line('#B07A3A')}" stroke-width="1.1"/>
      <path d="M33,33.5 C36,32 39,32 42,34" fill="none" stroke="#fff" stroke-width=".9" opacity=".55" stroke-linecap="round"/>
    </g>
    <circle cx="50" cy="60" r="17.5" fill="${darken(J, 0.5)}" opacity=".4" filter="url(#blur1)" transform="translate(0,1.2)"/>
    <circle cx="50" cy="60" r="17" fill="url(#${gD})" stroke="#24162a" stroke-width="1.6"/>
    <circle cx="50" cy="60" r="13.6" fill="none" stroke="#9a86a0" stroke-width="1" stroke-dasharray="2.4 2.2" opacity=".8"/>
    <path d="M38,54 A13,13 0 0 1 50,46.5" fill="none" stroke="#fff" stroke-width="1.8" stroke-linecap="round" opacity=".35"/>
  </g>`);
}

export function jobs() {
  return [
    ...KEYS.map(k => ['item_' + k, icon(k)]),
    ...KEYS.map(k => ['bubble_' + k, bubble(k)]),
    ['tray', tray()],
    ...KEYS.map(k => ['crate_' + k, crate(k)]),
    ['crate_hidden', crateHidden()],
  ];
}
