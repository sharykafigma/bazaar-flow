// Art v2 renderer: node Tools/ArtGen/render2.mjs [filter]
// Renders every job from v2/goods.mjs, v2/stalls.mjs and v2/env.mjs into Assets/BazaarFlow/Art/Resources/BazaarArt.
import { chromium } from 'playwright';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const out = path.resolve(here, '../../Assets/BazaarFlow/Art/Resources/BazaarArt');
fs.mkdirSync(out, { recursive: true });
const only = process.argv[2];

const jobs = [];
for (const m of ['goods', 'stalls', 'env']) {
  const mod = await import(`./v2/${m}.mjs`);
  jobs.push(...mod.jobs());
}
const browser = await chromium.launch();
const page = await browser.newPage({ deviceScaleFactor: 1 });
let n = 0;
for (const [name, s] of jobs) {
  if (only && !name.includes(only)) continue;
  await page.setContent(`<html><body style="margin:0;background:transparent">${s}</body></html>`);
  await (await page.$('svg')).screenshot({ path: path.join(out, name + '.png'), omitBackground: true });
  n++;
}
await browser.close();
console.log(`rendered ${n} v2 sprites -> ${path.relative(path.resolve(here, '../..'), out)}`);
