using System;
using System.Collections.Generic;

namespace BazaarFlow
{
    /// <summary>One order at a shop: "N goods of this colour".</summary>
    [Serializable]
    public class OrderDef
    {
        public ItemColor color;
        public int count;

        public OrderDef(ItemColor color, int count)
        {
            this.color = color;
            this.count = count;
        }
    }

    [Serializable]
    public class ShopDef
    {
        public List<OrderDef> orders = new List<OrderDef>();

        /// <summary>-1 = open from the start. Otherwise the index of the shop that must finish first.</summary>
        public int lockedUntilShop = -1;
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

    [Serializable]
    public class LevelData
    {
        public int number;
        public int beltSlots = 20;
        public float beltSpeed = 2.6f;
        public List<ShopDef> shops = new List<ShopDef>();
        public List<CrateColumnDef> columns = new List<CrateColumnDef>();

        /// <summary>Every colour's goods in crates must equal that colour's total in orders, or the level cannot be won.</summary>
        public bool IsBalanced(out string problem)
        {
            var need = new int[GameColors.Count];
            var have = new int[GameColors.Count];
            foreach (var s in shops)
            foreach (var o in s.orders)
                need[(int)o.color] += o.count;
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
