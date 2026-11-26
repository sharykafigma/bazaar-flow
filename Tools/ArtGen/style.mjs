// Bazaar Flow art v2 style kit. Every v2 sprite builds on these helpers so the whole game shares one look:
//  - soft volume: each shape gets a top-light / bottom-dark gradient plus inner rim light and inner shadow
//  - coloured outlines: outline = the shape's own colour, much darker (no single brown outline everywhere)
//  - gloss: one soft specular highlight per shape, top-left
//  - soft warm drop shadow under the whole sprite
// Draw in a 0..100 viewBox. Render size is chosen by the renderer (256 for small sprites, 512 for big ones).

export function mix(a, b, t) {
  const pa = hex(a), pb = hex(b);
  return rgb(pa.map((v, i) => Math.round(v + (pb[i] - v) * t)));
}
const hex = h => { const n = parseInt(h.slice(1), 16); return [(n >> 16) & 255, (n >> 8) & 255, n & 255]; };
const rgb = a => '#' + a.map(v => Math.max(0, Math.min(255, v)).toString(16).padStart(2, '0')).join('');
export const lighten = (c, t) => mix(c, '#ffffff', t);
export const darken = (c, t) => mix(c, '#2a0f08', t);   // warm dark, never pure black
export const line = c => darken(c, 0.62);               // outline colour for a fill colour

let uid = 0;
export const id = p => `${p}${++uid}`;

/** Defs every v2 sprite needs. Call once at the top of the svg. */
export function kitDefs() {
  return `
  <filter id="vol" x="-10%" y="-10%" width="120%" height="120%">
    <feFlood flood-color="#2a0f08" flood-opacity=".38"/><feComposite in2="SourceAlpha" operator="out"/>
    <feOffset dy="-3.2"/><feGaussianBlur stdDeviation="2.2"/><feComposite in2="SourceAlpha" operator="in" result="dk"/>
    <feFlood flood-color="#fff" flood-opacity=".55"/><feComposite in2="SourceAlpha" operator="out"/>
    <feOffset dx="1" dy="2.2"/><feGaussianBlur stdDeviation="1.2"/><feComposite in2="SourceAlpha" operator="in" result="lt"/>
    <feMerge><feMergeNode in="SourceGraphic"/><feMergeNode in="dk"/><feMergeNode in="lt"/></feMerge>
  </filter>
  <filter id="volS" x="-10%" y="-10%" width="120%" height="120%">
    <feFlood flood-color="#2a0f08" flood-opacity=".3"/><feComposite in2="SourceAlpha" operator="out"/>
    <feOffset dy="-1.6"/><feGaussianBlur stdDeviation="1.1"/><feComposite in2="SourceAlpha" operator="in" result="dk"/>
    <feFlood flood-color="#fff" flood-opacity=".5"/><feComposite in2="SourceAlpha" operator="out"/>
    <feOffset dy="1.2"/><feGaussianBlur stdDeviation=".7"/><feComposite in2="SourceAlpha" operator="in" result="lt"/>
    <feMerge><feMergeNode in="SourceGraphic"/><feMergeNode in="dk"/><feMergeNode in="lt"/></feMerge>
  </filter>
  <filter id="drop" x="-25%" y="-25%" width="150%" height="150%">
    <feGaussianBlur in="SourceAlpha" stdDeviation="2.4"/><feOffset dy="3"/>
    <feComponentTransfer><feFuncA type="linear" slope=".38"/></feComponentTransfer>
    <feColorMatrix type="matrix" values="0 0 0 0 .16  0 0 0 0 .06  0 0 0 0 .03  0 0 0 1 0"/>
    <feMerge><feMergeNode/><feMergeNode in="SourceGraphic"/></feMerge>
  </filter>
  <filter id="sticker" x="-25%" y="-25%" width="150%" height="150%">
    <feMorphology in="SourceAlpha" operator="dilate" radius="2.6" result="d"/>
    <feGaussianBlur in="d" stdDeviation="2.4"/><feOffset dy="3"/>
    <feComponentTransfer><feFuncA type="linear" slope=".4"/></feComponentTransfer>
    <feColorMatrix type="matrix" values="0 0 0 0 .16  0 0 0 0 .06  0 0 0 0 .03  0 0 0 1 0" result="sh"/>
    <feFlood flood-color="#fffaf0"/><feComposite in2="d" operator="in" result="rim"/>
    <feMerge><feMergeNode in="sh"/><feMergeNode in="rim"/><feMergeNode in="SourceGraphic"/></feMerge>
  </filter>
  <filter id="blur1"><feGaussianBlur stdDeviation="1"/></filter>
  <filter id="blur2"><feGaussianBlur stdDeviation="2"/></filter>
  <filter id="blur4"><feGaussianBlur stdDeviation="4"/></filter>`;
}

/** Vertical volume gradient for a fill colour (light top, rich middle, deep bottom). */
export function grad(gid, c, { top = 0.38, bottom = 0.28, angle = 'v' } = {}) {
  const [x2, y2] = angle === 'v' ? [0, 1] : angle === 'h' ? [1, 0] : [0.7, 1];
  return `<linearGradient id="${gid}" x1="0" y1="0" x2="${x2}" y2="${y2}">
    <stop offset="0" stop-color="${lighten(c, top)}"/><stop offset=".45" stop-color="${c}"/><stop offset="1" stop-color="${darken(c, bottom)}"/></linearGradient>`;
}

/** Radial "ball" gradient (highlight up-left). */
export function ball(gid, c, { hi = 0.5, lo = 0.32, cx = '36%', cy = '30%' } = {}) {
  return `<radialGradient id="${gid}" cx="${cx}" cy="${cy}" r="78%">
    <stop offset="0" stop-color="${lighten(c, hi)}"/><stop offset=".5" stop-color="${c}"/><stop offset="1" stop-color="${darken(c, lo)}"/></radialGradient>`;
}

/** A shape with volume: gradient fill, coloured outline, inner light/shadow. */
export function shape(d, c, gid, { w = 1.8, small = false, extra = '' } = {}) {
  return `<path d="${d}" fill="url(#${gid})" filter="url(#${small ? 'volS' : 'vol'})" ${extra}/>
  <path d="${d}" fill="none" stroke="${line(c)}" stroke-width="${w}" stroke-linejoin="round"/>`;
}

/** Soft specular highlight (an ellipse that fades out). */
export function gloss(cx, cy, rx, ry, rot = -25, a = 0.75) {
  const g = id('gl');
  return `<radialGradient id="${g}"><stop offset="0" stop-color="#fff" stop-opacity="${a}"/><stop offset=".55" stop-color="#fff" stop-opacity="${a * 0.45}"/><stop offset="1" stop-color="#fff" stop-opacity="0"/></radialGradient>
  <ellipse cx="${cx}" cy="${cy}" rx="${rx}" ry="${ry}" transform="rotate(${rot} ${cx} ${cy})" fill="url(#${g})"/>`;
}

/** Small sharp sparkle highlight dot. */
export const glint = (x, y, r = 1.6) => `<circle cx="${x}" cy="${y}" r="${r}" fill="#fff" opacity=".95"/>`;

/** Soft contact shadow ellipse. */
export const contact = (cx, cy, rx, ry, a = 0.3) => `<ellipse cx="${cx}" cy="${cy}" rx="${rx}" ry="${ry}" fill="#2a0f08" opacity="${a}" filter="url(#blur2)"/>`;

export const svg = (w, h, body, vb = '0 0 100 100') =>
  `<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="${vb}">${body}</svg>`;
