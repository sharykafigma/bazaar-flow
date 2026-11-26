using System.Collections.Generic;
using System.Linq;

namespace BazaarFlow
{
    /// <summary>
    /// Levels 1-3 are hand made (tutorial). Levels 4+ come from a seeded generator, so level N is always the same.
    /// Later this moves to ScriptableObjects made in a level editor.
    /// </summary>
    public static class LevelLibrary
    {
        public const int PrototypeLevelCount = 20;

        public static LevelData Get(int number)
        {
            LevelData level;
            switch (number)
            {
                case 1: level = Level1(); break;
                case 2: level = Level2(); break;
                case 3: level = Level3(); break;
                default: level = Generate(number); break;
            }
            level.number = number;
            return level;
        }

        // ---------- Hand made tutorial levels ----------

        static LevelData Level1()
        {
            var l = new LevelData { beltSlots = 16, beltSpeed = 2.4f };
            l.shops.Add(Shop(new OrderDef(ItemColor.Red, 4)));
            l.shops.Add(Shop(new OrderDef(ItemColor.Blue, 4)));
            l.columns.Add(Column(new CrateDef(ItemColor.Red, 4)));
            l.columns.Add(Column(new CrateDef(ItemColor.Blue, 4)));
            return l;
        }

        static LevelData Level2()
        {
            var l = new LevelData { beltSlots = 18, beltSpeed = 2.5f };
            l.shops.Add(Shop(new OrderDef(ItemColor.Red, 3), new OrderDef(ItemColor.Blue, 3)));
            l.shops.Add(Shop(new OrderDef(ItemColor.Blue, 4)));
            l.columns.Add(Column(new CrateDef(ItemColor.Red, 3), new CrateDef(ItemColor.Blue, 3)));
            l.columns.Add(Column(new CrateDef(ItemColor.Blue, 4)));
            return l;
        }

        static LevelData Level3()
        {
            var l = new LevelData { beltSlots = 20, beltSpeed = 2.6f };
            l.shops.Add(Shop(new OrderDef(ItemColor.Red, 4), new OrderDef(ItemColor.Red, 3)));
            l.shops.Add(Shop(new OrderDef(ItemColor.Blue, 5)));
            l.shops.Add(Shop(new OrderDef(ItemColor.Blue, 3), new OrderDef(ItemColor.Red, 3)));
            l.columns.Add(Column(new CrateDef(ItemColor.Red, 4), new CrateDef(ItemColor.Blue, 3), new CrateDef(ItemColor.Red, 3)));
            l.columns.Add(Column(new CrateDef(ItemColor.Blue, 5), new CrateDef(ItemColor.Red, 3)));
            return l;
        }

        static ShopDef Shop(params OrderDef[] orders)
        {
            var s = new ShopDef();
            s.orders.AddRange(orders);
            return s;
        }

        static CrateColumnDef Column(params CrateDef[] crates)
        {
            var c = new CrateColumnDef();
            c.crates.AddRange(crates);
            return c;
        }

        // ---------- Generator ----------

        /// <summary>Levels shown on the map. The generator keeps going past this for endless play.</summary>
        public const int TotalLevels = 200;

        static string IntroFor(int n)
        {
            switch (n)
            {
                case 11: return "Hidden crates! Their colour shows when they reach the front.";
                case 16: return "Locked stall: it opens when its neighbour is done.";
                case 21: return "Twin orders: some stalls want two colours at once.";
                case 31: return "Rush hour! The belt speeds up now and then.";
                case 41: return "Monsoon! Stalls pull their shutters down for a moment.";
                case 61: return "Every twist at once. Good luck!";
                default: return null;
            }
        }

        /// <summary>
        /// Builds a level that is always balanced. Crates are sorted roughly in the order the shops need them,
        /// then shuffled a little: more shuffle = harder. Every 5th level is hard, every 10th very hard.
        /// Twists come in by level: hidden crates 11+, locked stall 16+, twin orders 21+, rush hour 31+,
        /// shutters 41+, all mixed 61+.
        /// </summary>
        public static LevelData Generate(int n)
        {
            var rng = new System.Random(n * 7919 + 13);
            var difficulty = n % 10 == 0 ? Difficulty.VeryHard : n % 5 == 0 ? Difficulty.Hard : Difficulty.Normal;

            int colorCount = n <= 10 ? 3 : n <= 20 ? 4 : n <= 50 ? 5 : 6;
            int shopCount = n <= 7 ? 3 : n <= 15 ? 4 : n <= 30 ? 5 : 6;
            int maxOrders = n <= 5 ? 2 : 3;
            int slots = n <= 10 ? 16 : n <= 30 ? 15 : 16;
            if (difficulty == Difficulty.Hard) slots -= 2;
            if (difficulty == Difficulty.VeryHard) slots -= 3;
            double shuffle = System.Math.Min(0.15 + 0.02 * (n - 4), 0.45)
                             + (difficulty == Difficulty.Hard ? 0.1 : difficulty == Difficulty.VeryHard ? 0.15 : 0);

            double hiddenChance = n < 11 ? 0 : n < 41 ? 0.25 : 0.35;
            int lockedShops = n < 16 ? 0 : n < 41 ? 1 : 1 + rng.Next(0, 2);
            double twinChance = n < 21 ? 0 : n < 31 ? 0.3 : 0.25;
            bool rush = (n >= 31 && n <= 40) || (n > 40 && n % 3 == 0);
            int shutterShops = n < 41 ? 0 : n <= 60 ? 1 + rng.Next(0, 2) : rng.Next(0, 3);

            var level = new LevelData
            {
                beltSlots = slots,
                beltSpeed = 2.6f + System.Math.Min(n, 100) * 0.004f,
                rushHour = rush,
                difficulty = difficulty,
                intro = IntroFor(n)
            };

            // Pick which colours this level uses.
            var colors = Enumerable.Range(0, GameColors.Count).Select(i => (ItemColor)i)
                .OrderBy(_ => rng.Next()).Take(colorCount).ToList();

            // Orders. Colours are dealt from a cycling list so every colour appears.
            int colorCursor = 0;
            for (int s = 0; s < shopCount; s++)
            {
                var shop = new ShopDef();
                int orders = rng.Next(maxOrders == 2 ? 1 : 2, maxOrders + 1);
                for (int o = 0; o < orders; o++)
                {
                    var c = colors[colorCursor++ % colors.Count];
                    if (rng.NextDouble() < twinChance)
                    {
                        var c2 = colors[colorCursor++ % colors.Count];
                        if (c2 == c) c2 = colors[(colors.IndexOf(c) + 1) % colors.Count];
                        shop.orders.Add(new OrderDef(c, rng.Next(2, 5), c2, rng.Next(2, 5)));
                    }
                    else shop.orders.Add(new OrderDef(c, rng.Next(3, 7)));
                }
                level.shops.Add(shop);
            }

            // Locked stalls wait for an earlier stall (never a chain onto another locked one).
            for (int k = 0; k < lockedShops; k++)
            {
                int target = shopCount - 1 - k;
                level.shops[target].lockedUntilShop = k;
            }
            // Shutters on open stalls only, so a locked stall never also blinks.
            var openShops = Enumerable.Range(0, shopCount).Where(i => level.shops[i].lockedUntilShop < 0)
                .OrderBy(_ => rng.Next()).Take(shutterShops);
            foreach (int i in openShops) level.shops[i].shutters = true;

            // "Need time" of every unit: wave by wave (all first orders, then all second orders ...).
            var needTimes = colors.ToDictionary(c => c, _ => new List<int>());
            int t = 0;
            int maxWaves = level.shops.Max(sh => sh.orders.Count);
            for (int w = 0; w < maxWaves; w++)
            for (int s = 0; s < level.shops.Count; s++)
            {
                var shop = level.shops[s];
                if (w >= shop.orders.Count) continue;
                // A locked shop needs its goods later.
                int lateBonus = shop.lockedUntilShop >= 0 ? 20 : 0;
                var order = shop.orders[w];
                int a = order.count, b = order.count2;
                // Twin orders: interleave the two colours.
                while (a > 0 || b > 0)
                {
                    if (a > 0) { needTimes[order.color].Add(t++ + lateBonus); a--; }
                    if (b > 0) { needTimes[order.color2].Add(t++ + lateBonus); b--; }
                }
            }

            // Cut each colour's total into crates of 3-5 and give each crate the need time of its first unit.
            var crates = new List<(CrateDef crate, int when)>();
            foreach (var c in colors)
            {
                var times = needTimes[c];
                times.Sort();
                int i = 0;
                while (i < times.Count)
                {
                    int left = times.Count - i;
                    int size = System.Math.Min(left, rng.Next(3, 6));
                    // Never leave a tail of 1-2 goods: take it all (max 6) or leave exactly 3.
                    if (left - size > 0 && left - size < 3) size = left <= 6 ? left : left - 3;
                    crates.Add((new CrateDef(c, size), times[i]));
                    i += size;
                }
            }
            crates.Sort((x, y) => x.when.CompareTo(y.when));

            // Difficulty: swap neighbours at random.
            for (int i = 0; i < crates.Count - 1; i++)
            {
                if (rng.NextDouble() < shuffle)
                {
                    var tmp = crates[i];
                    crates[i] = crates[i + 1];
                    crates[i + 1] = tmp;
                }
            }

            // Deal into columns, front first.
            int columnCount = crates.Count <= 12 ? 3 : crates.Count <= 20 ? 4 : 5;
            for (int c = 0; c < columnCount; c++) level.columns.Add(new CrateColumnDef());
            for (int i = 0; i < crates.Count; i++)
            {
                var col = level.columns[i % columnCount];
                var crate = crates[i].crate;
                // Never hide the front crate.
                crate.hidden = col.crates.Count > 0 && rng.NextDouble() < hiddenChance;
                col.crates.Add(crate);
            }

            return level;
        }
    }
}
