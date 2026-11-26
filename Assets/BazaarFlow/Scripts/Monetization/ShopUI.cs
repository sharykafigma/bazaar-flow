using System;
using UnityEngine;

namespace BazaarFlow
{
    /// <summary>
    /// Simple OnGUI shop (Dukaan) so ads, IAP and the wallet can be tested end to end today.
    /// The art/UI thread's real shop screen can replace this; it only needs ShopCatalog, IapService and Economy.
    /// Open with ShopUI.Open().
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        public static bool IsOpen { get; private set; }

        static ShopUI _instance;
        Vector2 _scroll;
        string _toast;
        float _toastUntil;
        bool _busy;
        GUIStyle _title, _row, _btn;
        int _styleSize;

        public static void Open()
        {
            if (_instance == null)
            {
                var go = new GameObject("ShopUI");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<ShopUI>();
            }
            IsOpen = true;
        }

        public static void Close() => IsOpen = false;

        void OnEnable() => IapService.PurchaseGranted += OnGranted;
        void OnDisable() => IapService.PurchaseGranted -= OnGranted;

        void OnGranted(string id)
        {
            var p = ShopCatalog.Get(id);
            Toast(p != null ? $"Thank you! {p.Title} added" : "Thank you!");
        }

        void Toast(string text)
        {
            _toast = text;
            _toastUntil = Time.realtimeSinceStartup + 2.5f;
        }

        void OnGUI()
        {
            if (!IsOpen) return;
            GUI.depth = -10;
            float u = Screen.height / 100f;
            Styles(u);

            var full = new Rect(0, 0, Screen.width, Screen.height);
            GUI.Box(full, GUIContent.none);
            GUI.Box(full, GUIContent.none);

            float pad = u * 2;
            float w = Screen.width - pad * 2;
            GUI.Label(new Rect(pad, pad, w, u * 6), "Shop", _title);
            GUI.Label(new Rect(pad, pad + u * 6, w, u * 4), $"Coins: {Economy.Coins}", _row);
            if (GUI.Button(new Rect(Screen.width - pad - u * 12, pad, u * 12, u * 6), "Close", _btn)) Close();

            float top = pad + u * 11;
            float viewH = Screen.height - top - u * 10;
            float rowH = u * 8;
            int rows = CountRows();
            _scroll = GUI.BeginScrollView(new Rect(pad, top, w, viewH), _scroll, new Rect(0, 0, w - u * 2, rows * rowH));
            float y = 0;
            float bw = u * 16;
            float lw = w - bw - u * 4;

            // Free daily booster (rewarded ad)
            if (Economy.FreeBoosterAvailable)
            {
                GUI.Label(new Rect(0, y, lw, rowH), "Free daily booster\n(watch an ad)", _row);
                if (Button(new Rect(lw, y + u, bw, rowH - u * 2), "Watch ad"))
                {
                    _busy = true;
                    AdsService.ShowRewarded("free_booster", ok =>
                    {
                        _busy = false;
                        if (!ok) { Toast("No ad available, try again soon"); return; }
                        var live = Economy.LiveBoosters;
                        var b = live[UnityEngine.Random.Range(0, live.Length)];
                        Economy.ClaimFreeBooster(b);
                        Toast($"You got {Boosters.Name(b)}!");
                    });
                }
                y += rowH;
            }

            // Starter pack (72 h after first loss)
            if (Economy.StarterOfferActive)
            {
                var left = Economy.StarterOfferTimeLeft;
                ProductRow(ShopCatalog.Get(MonetizationConfig.ProductStarterPack),
                    $"Starter Pack ({(int)left.TotalHours}h {left.Minutes}m)\nNo Ads + {MonetizationConfig.StarterPackCoins} coins + boosters",
                    ref y, lw, bw, rowH, u);
            }

            // No Ads
            if (!Economy.NoAds)
                ProductRow(ShopCatalog.Get(MonetizationConfig.ProductNoAds), "No Ads\nRemoves ads between levels (reward ads stay optional)",
                    ref y, lw, bw, rowH, u);

            // Boosters for coins
            foreach (var b in Economy.LiveBoosters)
            {
                GUI.Label(new Rect(0, y, lw, rowH), $"{Boosters.Name(b)} (owned: {Economy.BoosterCount(b)})\n{Boosters.Description(b)}", _row);
                if (Button(new Rect(lw, y + u, bw, rowH - u * 2), $"{MonetizationConfig.BoosterCoinPrice} coins"))
                    Toast(Economy.TryBuyBoosterWithCoins(b) ? $"Bought {Boosters.Name(b)}" : "Not enough coins");
                y += rowH;
            }

            // Booster bundle + coin packs
            foreach (var p in ShopCatalog.Products)
            {
                if (p.Kind == ProductKind.BoosterBundle)
                    ProductRow(p, $"{p.Title}\nEvery booster x{MonetizationConfig.BoosterBundleEach}", ref y, lw, bw, rowH, u);
                else if (p.Kind == ProductKind.Coins)
                    ProductRow(p, $"{p.Title}\n{p.Coins} coins", ref y, lw, bw, rowH, u);
            }
            GUI.EndScrollView();

            if (GUI.Button(new Rect(pad, Screen.height - u * 8, w, u * 6), "Restore purchases", _btn) && !_busy)
            {
                _busy = true;
                IapService.Restore(ok =>
                {
                    _busy = false;
                    Toast(ok ? "Purchases restored" : "Restore failed");
                });
            }

            if (_busy) GUI.Label(new Rect(0, Screen.height * 0.45f, Screen.width, u * 5), "Please wait...", _title);
            if (Time.realtimeSinceStartup < _toastUntil)
                GUI.Label(new Rect(0, Screen.height * 0.5f, Screen.width, u * 5), _toast, _title);
        }

        int CountRows()
        {
            int n = Economy.LiveBoosters.Length;
            foreach (var p in ShopCatalog.Products)
                if (p.Kind == ProductKind.Coins || p.Kind == ProductKind.BoosterBundle) n++;
            if (Economy.FreeBoosterAvailable) n++;
            if (Economy.StarterOfferActive) n++;
            if (!Economy.NoAds) n++;
            return n;
        }

        void ProductRow(ShopProduct p, string label, ref float y, float lw, float bw, float rowH, float u)
        {
            if (p == null) return;
            GUI.Label(new Rect(0, y, lw, rowH), label, _row);
            if (Button(new Rect(lw, y + u, bw, rowH - u * 2), IapService.GetPrice(p.Id)))
            {
                _busy = true;
                IapService.Purchase(p.Id, ok =>
                {
                    _busy = false;
                    if (!ok) Toast("Purchase not completed");
                });
            }
            y += rowH;
        }

        bool Button(Rect r, string text) => GUI.Button(r, text, _btn) && !_busy;

        void Styles(float u)
        {
            int size = Mathf.RoundToInt(u * 2.2f);
            if (_title != null && _styleSize == size) return;
            _styleSize = size;
            _title = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(u * 4f), fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white }
            };
            _row = new GUIStyle(GUI.skin.label)
            {
                fontSize = size, alignment = TextAnchor.MiddleLeft, wordWrap = true,
                normal = { textColor = Color.white }
            };
            _btn = new GUIStyle(GUI.skin.button) { fontSize = size, fontStyle = FontStyle.Bold };
        }
    }
}
