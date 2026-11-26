// quick look: node v2/try.mjs <module> <export> <args...> -> scratch png
import { chromium } from 'playwright';
const [mod, fn, ...args] = process.argv.slice(2);
const m = await import('./' + mod + '.mjs');
const s = m[fn](...args.map(a => isNaN(a) ? a : Number(a)));
const b = await chromium.launch(); const p = await b.newPage();
await p.setContent(`<body style="margin:0;background:#e9d6b4">${s}</body>`);
await (await p.$('svg')).screenshot({ path: process.env.OUT || '/tmp/claude-0/-home-user/a0cce31f-a1fd-567c-a2a2-c73214268fa4/scratchpad/try.png' });
await b.close();
