using System.Collections.Generic;
using UnityEngine;

namespace BazaarFlow
{
    /// <summary>
    /// Plays game sounds by watching the board, so gameplay scripts need no audio code at all.
    /// It only reads public state (GameManager.State, Shop.Remaining / IsLocked / IsDone,
    /// Belt.Slots, Godown.Columns), which keeps it safe while gameplay code keeps changing.
    /// If gameplay later raises its own events, swap the polling here for those events.
    /// </summary>
    public class GameAudioDirector : MonoBehaviour
    {
        GameManager _game;
        Belt _belt;
        Godown _godown;
        Shop[] _shops = new Shop[0];

        int _level = -1;
        GameState _state;
        int _crates;
        readonly HashSet<BeltItem> _onBelt = new HashSet<BeltItem>();
        int[] _remaining = new int[0];
        bool[] _locked = new bool[0];

        void LateUpdate()
        {
            if (_game == null)
            {
                _game = FindFirstObjectByType<GameManager>();
                if (_game == null) return;
            }

            // New level (or the board was rebuilt): grab fresh references, no sounds this frame.
            if (_game.LevelNumber != _level || _belt == null || _godown == null)
            {
                Rescan();
                return;
            }

            // Win / Belt Jam
            if (_game.State != _state)
            {
                if (_game.State == GameState.Won) AudioManager.PlayStinger(Sfx.Win, 3f);
                else if (_game.State == GameState.Jammed) AudioManager.PlayStinger(Sfx.Jam, 2.5f);
                else if (_state == GameState.Jammed) AudioManager.Play(Sfx.UiClick); // continued after jam
                _state = _game.State;
            }

            // Crate tapped: godown has fewer crates than before.
            int crates = CountCrates();
            if (crates < _crates) AudioManager.Play(Sfx.Tap);
            _crates = crates;

            // Goods loaded onto the belt through the gate.
            bool loaded = false;
            var slots = _belt.Slots;
            for (int i = 0; i < slots.Length; i++)
            {
                var item = slots[i];
                if (item != null && _onBelt.Add(item)) loaded = true;
            }
            _onBelt.RemoveWhere(x => x == null || !IsOnBelt(x, slots));
            if (loaded) AudioManager.Play(Sfx.BeltLoad);

            // Shops: pickup, order done, unlocked.
            for (int i = 0; i < _shops.Length; i++)
            {
                var s = _shops[i];
                if (s == null) continue;
                if (s.Remaining < _remaining[i] && !s.IsDone)
                {
                    AudioManager.PlayPickup();
                    if (s.Remaining <= 0) AudioManager.Play(Sfx.ShopDone);
                }
                if (_locked[i] && !s.IsLocked) AudioManager.Play(Sfx.Unlock);
                _remaining[i] = s.Remaining;
                _locked[i] = s.IsLocked;
            }
        }

        void Rescan()
        {
            _level = _game.LevelNumber;
            _state = _game.State;
            _belt = FindFirstObjectByType<Belt>();
            _godown = FindFirstObjectByType<Godown>();
            _shops = FindObjectsByType<Shop>(FindObjectsSortMode.None);
            _remaining = new int[_shops.Length];
            _locked = new bool[_shops.Length];
            for (int i = 0; i < _shops.Length; i++)
            {
                _remaining[i] = _shops[i].Remaining;
                _locked[i] = _shops[i].IsLocked;
            }
            _onBelt.Clear();
            if (_belt != null)
                foreach (var item in _belt.Slots)
                    if (item != null) _onBelt.Add(item);
            _crates = CountCrates();
        }

        int CountCrates()
        {
            if (_godown == null) return 0;
            int n = 0;
            foreach (var col in _godown.Columns) n += col.Count;
            return n;
        }

        static bool IsOnBelt(BeltItem item, BeltItem[] slots)
        {
            for (int i = 0; i < slots.Length; i++) if (slots[i] == item) return true;
            return false;
        }
    }
}
