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

        /// <summary>
        /// Builds a level that is always balanced. Crates are sorted roughly in the order the shops need them,
        /// then shuffled a little: more shuffle = harder level. Every 5th level is a "mushkil" level.
        /// </summary>
        public static LevelData Generate(int n)
        {
            var rng = new System.Random(n * 7919 + 13);
            bool hard = n % 5 == 0;

            int colorCount = n <= 10 ? 3 : 4;
            int shopCount = n <= 7 ? 3 : n <= 15 ? 4 : 5;
            int maxOrders = n <= 5 ? 2 : 3;
            int slots = n <= 10 ? 16 : n <= 15 ? 15 : 14;
            if (hard) slots -= 2;
            double shuffle = System.Math.Min(0.15 + 0.02 * (n - 4), 0.45) + (hard ? 0.1 : 0);
            bool useHidden = n >= 11;
            bool useLock = n >= 16;

            var level = new LevelData { beltSlots = slots, beltSpeed = 2.6f };

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
                    shop.orders.Add(new OrderDef(c, rng.Next(3, 7)));
                }
                level.shops.Add(shop);
            }
            if (useLock) level.shops[shopCount - 1].lockedUntilShop = 0;

            // "Need time" of every unit: wave by wave (all first orders, then all second orders ...).
            var needTimes = colors.ToDictionary(c => c, _ => new List<int>());
            int t = 0;
            int maxWaves = level.shops.Max(s => s.orders.Count);
            for (int w = 0; w < maxWaves; w++)
            for (int s = 0; s < level.shops.Count; s++)
            {
                var shop = level.shops[s];
                if (w >= shop.orders.Count) continue;
                // A locked shop needs its goods later.
                int lateBonus = shop.lockedUntilShop >= 0 ? 20 : 0;
                for (int k = 0; k < shop.orders[w].count; k++)
                    needTimes[shop.orders[w].color].Add(t++ + lateBonus);
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
            crates.Sort((a, b) => a.when.CompareTo(b.when));

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
            int columnCount = crates.Count <= 12 ? 3 : 4;
            for (int c = 0; c < columnCount; c++) level.columns.Add(new CrateColumnDef());
            for (int i = 0; i < crates.Count; i++)
            {
                var col = level.columns[i % columnCount];
                var crate = crates[i].crate;
                // Never hide the front crate.
                crate.hidden = useHidden && col.crates.Count > 0 && rng.NextDouble() < 0.25;
                col.crates.Add(crate);
            }

            return level;
        }
    }
}
