// Mock-up of a level screen using the rendered sprites at the same world positions the game uses.
// Run: node Tools/ArtGen/preview.mjs [out.png]   (after render.mjs)
import { chromium } from 'playwright';
import path from 'node:path';
import fs from 'node:fs';
import { fileURLToPath } from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const art = 'file://' + path.join(root, 'Assets/BazaarFlow/Art/Resources/BazaarArt') + '/';
const outFile = process.argv[2] || path.join(root, 'Tools/ArtGen/preview.png');

const W = 1080, H = 1920, size = Math.max(8.2, 4.7 / (W / H)), ppu = H / (2 * size);
const sx = x => W / 2 + x * ppu, sy = y => H / 2 - y * ppu;
const els = [];
// world-space sprite: centre (x,y), world size w,h, pivot (0..1 from left/top of image)
const spr = (img, x, y, w, h, extra = '', px = 0.5, py = 0.5) =>
  els.push(`<img src="${art}${img}.png" style="position:absolute;left:${sx(x) - w * ppu * px}px;top:${sy(y) - h * ppu * py}px;width:${w * ppu}px;height:${h * ppu}px;${extra}">`);
const label = (x, y, t, fs = 30) => els.push(`<div style="position:absolute;left:${sx(x) - 100}px;top:${sy(y) - fs * 0.7}px;width:200px;text-align:center;font:800 ${fs}px 'Inter',sans-serif;color:#fff;text-shadow:0 2px 0 #3b2417,0 0 6px #3b2417">${t}</div>`);

const goods = ['chai', 'choori', 'aam', 'patang', 'lattoo', 'diya'];
spr('background', 0, 0, 12.4, 24.8);
// belt
const BC = { x: 0, y: 1.6 }, BW = 7.2, BH = 7.0, T = 0.9;
spr('rangoli', 0, 1.6, 3.4, 3.4, 'opacity:.9');
const tile = (img, x, y, w, h) => els.push(`<div style="position:absolute;left:${sx(x - w / 2)}px;top:${sy(y + h / 2)}px;width:${w * ppu}px;height:${h * ppu}px;background:url(${art}${img}.png);background-size:${T * ppu}px ${T * ppu}px;background-position:center"></div>`);
tile('belt_h', 0, BC.y + BH / 2, BW, T); tile('belt_h', 0, BC.y - BH / 2, BW, T);
tile('belt_v', -BW / 2, BC.y, T, BH); tile('belt_v', BW / 2, BC.y, T, BH);
for (const [cx, cy] of [[-1, 1], [1, 1], [1, -1], [-1, -1]]) {
  spr('belt_corner', BC.x + cx * BW / 2, BC.y + cy * BH / 2, T * 1.08, T * 1.08);
  spr('belt_hub', BC.x + cx * BW / 2, BC.y + cy * BH / 2, T * 0.9, T * 0.9, 'transform:rotate(17deg)');
}
spr('bunting', 0, BC.y + BH / 2 + 1.15, 10.5, 1.3);
// belt path
const L = 2 * (BW + BH), slots = 20;
const pointAt = s => { s = ((s % L) + L) % L; const hw = BW / 2, hh = BH / 2;
  if (s < BW) return [-hw + s, BC.y + hh]; s -= BW; if (s < BH) return [hw, BC.y + hh - s]; s -= BH;
  if (s < BW) return [hw - s, BC.y - hh]; s -= BW; return [-hw, BC.y - hh + s]; };
const onBelt = [0, 1, 2, 4, 5, 7, 11, 12, 15];
for (let i = 0; i < slots; i++) {
  const [x, y] = pointAt(i * L / slots + 0.4);
  spr('tray', x, y, 0.69, 0.69);
  if (onBelt.includes(i)) spr('item_' + goods[[0, 1, 0, 2, 1, 0, 2, 1, 0][onBelt.indexOf(i)]], x, y + 0.04, 0.73, 0.73);
}
// gate
const gate = pointAt(BW + BH + BW / 2);
spr('gate', gate[0], gate[1] - 0.75, 1.35, 1.35);
label(gate[0], gate[1] - 0.75, '3');
// shops
const spots = [[0, .35], [0, .65], [1, .35], [3, .65], [1, .75], [3, .25]];
const edgeS = (e, t) => [BW * t, BW + BH * t, BW + BH + BW * t, 2 * BW + BH + BH * t][e];
const inward = s => { s = ((s % L) + L) % L; return s < BW ? [0, -1] : s < BW + BH ? [-1, 0] : s < 2 * BW + BH ? [0, 1] : [1, 0]; };
const shopState = [['chai', '4', 0], ['choori', '3', 1], ['aam', '5', 2], [null, 'Band', 3], ['diya', 'Tala', 5], ['lattoo', '2', 4]];
for (let i = 0; i < 6; i++) {
  const s = edgeS(...spots[i]), p = pointAt(s), n = inward(s), x = p[0] + n[0] * 1.05, y = p[1] + n[1] * 1.05;
  const [g, txt, st] = shopState[i];
  spr('fx_shadow', x, y - 0.68, 1.4, 0.35);
  spr('stall_' + st, x, y, 1.45, 1.45, '', 0.5, 0.52);
  if (g && txt !== 'Tala') spr('bubble_' + g, x, y, 0.74, 0.74);
  if (txt === 'Band') spr('stall_closed', x, y, 1.45, 1.45, '', 0.5, 0.52);
  if (txt === 'Tala') { spr('bubble_' + g, x, y, 0.74, 0.74); spr('stall_lock', x, y, 1.45, 1.45, '', 0.5, 0.52); }
  label(x, y - 0.05, txt, txt.length > 2 ? 26 : 34);
}
// godown
const cols = [['chai', 'choori', 'hidden', 'aam'], ['choori', 'hidden', 'chai'], ['aam', 'chai', 'hidden', 'choori'], ['diya', 'lattoo']];
const counts = [[4, 3, '?', 5], [3, '?', 4], [5, 2, '?', 3], [4, 3]];
const n = cols.length, step = Math.min(1.25, 6.5 / (n - 1)), topY = BC.y - BH / 2 - 1.75;
for (let c = 0; c < n; c++) for (let r = 0; r < cols[c].length; r++) {
  const x = -step * (n - 1) / 2 + c * step, y = topY - r * 1.0, k = cols[c][r];
  spr(k === 'hidden' || r > 0 && false ? 'crate_hidden' : 'crate_' + k, x, y, 1.12, 1.12, r === 0 ? 'filter:drop-shadow(0 0 10px #fff6c0)' : 'filter:brightness(.88)', 0.5, 0.6);
  label(x, y, r === 0 || k !== 'hidden' ? String(counts[c][r]) : '?');
}
// fake hud
els.push(`<div style="position:absolute;left:30px;top:40px;font:800 46px Inter;color:#3b2417">Level 3</div><div style="position:absolute;right:30px;top:40px;font:800 46px Inter;color:#3b2417">Coins 120</div>`);
// a sparkle burst near shop 0 to show fx
const html = `<html><body style="margin:0;width:${W}px;height:${H}px;overflow:hidden;position:relative;background:#ffe6c4">${els.join('')}</body></html>`;
const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: W, height: H } });
const tmp = path.join(path.dirname(outFile), '_preview.html');
fs.writeFileSync(tmp, html);
await page.goto('file://' + tmp, { waitUntil: 'load' });
await page.screenshot({ path: outFile });
await browser.close();
console.log('preview ->', outFile);
