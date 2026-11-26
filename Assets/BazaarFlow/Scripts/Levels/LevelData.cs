using System;
using System.Collections.Generic;

namespace BazaarFlow
{
    /// <summary>
    /// One order at a shop: "N goods of this colour". A twin order (count2 &gt; 0) also wants M goods of a second
    /// colour; the shop takes both colours in any order until both are filled.
    /// </summary>
    [Serializable]
    public class OrderDef
    {
        public ItemColor color;
        public int count;
        public ItemColor color2;
        public int count2;

        public bool IsTwin => count2 > 0;
        public int Total => count + count2;

        public OrderDef(ItemColor color, int count)
        {
            this.color = color;
            this.count = count;
        }

        public OrderDef(ItemColor color, int count, ItemColor color2, int count2)
        {
            this.color = color;
            this.count = count;
            this.color2 = color2;
            this.count2 = count2;
        }
    }

    [Serializable]
    public class ShopDef
    {
        public List<OrderDef> orders = new List<OrderDef>();

        /// <summary>-1 = open from the start. Otherwise the index of the shop that must finish first.</summary>
        public int lockedUntilShop = -1;

        /// <summary>Monsoon twist: the shop pulls its shutter down for a few seconds now and then.</summary>
        public bool shutters;
    }

    /// <summary>A crate in the godown: one colour, a few goods. Hidden crates show "?" until they reach the front.</summary>
    [Serializable]
    public class CrateDef
    {
        public ItemColor color;
        public int count;
        public bool hidden;

        public CrateDef(ItemColor color, int count, bool hidden = false)
        {
            this.color = color;
            this.count = count;
            this.hidden = hidden;
        }
    }

    [Serializable]
    public class CrateColumnDef
    {
        /// <summary>Index 0 is the front crate (the one the player can tap).</summary>
        public List<CrateDef> crates = new List<CrateDef>();
    }

    public enum Difficulty { Normal, Hard, VeryHard }

    [Serializable]
    public class LevelData
    {
        public int number;
        public int beltSlots = 20;
        public float beltSpeed = 2.6f;

        /// <summary>Rush hour twist: the belt speeds up for a few seconds every so often.</summary>
        public bool rushHour;

        /// <summary>Every 5th level is hard, every 10th very hard (bigger reward).</summary>
        public Difficulty difficulty;

        /// <summary>Short text shown when a level brings in a new twist, or null.</summary>
        public string intro;
        public List<ShopDef> shops = new List<ShopDef>();
        public List<CrateColumnDef> columns = new List<CrateColumnDef>();

        /// <summary>Every colour's goods in crates must equal that colour's total in orders, or the level cannot be won.</summary>
        public bool IsBalanced(out string problem)
        {
            var need = new int[GameColors.Count];
            var have = new int[GameColors.Count];
            foreach (var s in shops)
            foreach (var o in s.orders)
            {
                need[(int)o.color] += o.count;
                if (o.IsTwin) need[(int)o.color2] += o.count2;
            }
            foreach (var col in columns)
            foreach (var c in col.crates)
                have[(int)c.color] += c.count;

            for (int i = 0; i < GameColors.Count; i++)
            {
                if (need[i] != have[i])
                {
                    problem = $"{(ItemColor)i}: orders {need[i]}, crates {have[i]}";
                    return false;
                }
            }
            problem = null;
            return true;
        }
    }
}
