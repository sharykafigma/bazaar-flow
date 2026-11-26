// Renders every sprite in art.mjs to PNG.  Run:  node Tools/ArtGen/render.mjs   (needs Playwright + Chromium)
// Output: Assets/BazaarFlow/Art/Resources/BazaarArt/*.png  (loaded at runtime by BazaarArt.cs)
import { chromium } from 'playwright';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import * as A from './art.mjs';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const out = path.join(root, 'Assets/BazaarFlow/Art/Resources/BazaarArt');
fs.mkdirSync(out, { recursive: true });

const jobs = [];
for (const g of A.GOODS) {
  jobs.push([`item_${g.key}`, A.goodsIcon(g.key)]);
  jobs.push([`bubble_${g.key}`, A.bubble(g.key, g.color)]);
  jobs.push([`crate_${g.key}`, A.crate(g.key, g.color)]);
}
jobs.push(['crate_hidden', A.crateHidden()]);
jobs.push(['tray', A.tray()], ['belt_h', A.beltH()], ['belt_v', A.beltV()], ['belt_corner', A.beltCorner()], ['belt_hub', A.beltHub()]);
jobs.push(['gate', A.gate()]);
for (let i = 0; i < A.STALL_COUNT; i++) jobs.push([`stall_${i}`, A.stall(i)]);
jobs.push(['stall_closed', A.stallClosed()], ['stall_lock', A.stallLock()]);
jobs.push(['background', A.background()], ['rangoli', A.rangoli()], ['bunting', A.bunting()]);
for (const [k, f] of Object.entries(A.fx)) jobs.push([`fx_${k}`, f()]);

export async function renderSvg(page, svgText, file) {
  await page.setContent(`<html><body style="margin:0;background:transparent">${svgText}</body></html>`);
  const el = await page.$('svg');
  await el.screenshot({ path: file, omitBackground: true });
}

const only = process.argv[2];
const browser = await chromium.launch();
const page = await browser.newPage({ deviceScaleFactor: 1 });
for (const [name, s] of jobs) {
  if (only && !name.includes(only)) continue;
  await renderSvg(page, s, path.join(out, name + '.png'));
}
await browser.close();
console.log(`rendered ${jobs.length} sprites -> ${path.relative(root, out)}`);
