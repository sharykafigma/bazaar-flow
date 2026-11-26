using System;
using System.Collections.Generic;
using UnityEngine;
#if BF_IAP
using System.Linq;
using UnityEngine.Purchasing;
#endif

namespace BazaarFlow
{
    /// <summary>
    /// Real-money purchases through Unity IAP v5 (Google Play Billing on Android).
    ///
    /// Compiles with or without the "In-App Purchasing" package (com.unity.purchasing 5.x):
    ///  - Installed -> BF_IAP is defined and the real store is used (Editor uses Unity's fake store).
    ///  - Missing   -> Editor / development builds grant purchases instantly for testing; release builds fail them.
    ///
    /// Usage:
    ///   IapService.Purchase(MonetizationConfig.ProductNoAds, ok => ...);
    ///   string price = IapService.GetPrice(MonetizationConfig.ProductCoins1);
    /// </summary>
    public class IapService : MonoBehaviour
    {
        public static IapService Instance { get; private set; }

        /// <summary>True once products (with local prices) are loaded from the store.</summary>
        public static bool Ready { get; private set; }

        /// <summary>Fires when prices arrive or a purchase finishes, so shop screens can refresh.</summary>
        public static event Action StoreChanged;

        /// <summary>Fires after content was granted for a product id (for "thank you" popups or analytics).</summary>
        public static event Action<string> PurchaseGranted;

        readonly Dictionary<string, Action<bool>> _pending = new Dictionary<string, Action<bool>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Create()
        {
            if (Instance != null) return;
            var go = new GameObject("IapService");
            DontDestroyOnLoad(go);
            go.AddComponent<IapService>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitStore();
        }

        // ================= Public API =================

        public static void Purchase(string productId, Action<bool> done)
        {
            var p = ShopCatalog.Get(productId);
            if (Instance == null || p == null)
            {
                Debug.LogWarning($"[IapService] Unknown product or store missing: {productId}");
                done?.Invoke(false);
                return;
            }
            if (!p.Consumable && Owns(productId))
            {
                done?.Invoke(true);
                return;
            }
            Instance.PurchaseInternal(p, done);
        }

        /// <summary>Local price string from Google Play (e.g. "Rs 1,100.00"), or the plan price until loaded.</summary>
        public static string GetPrice(string productId)
        {
#if BF_IAP
            if (Instance != null && Instance._store != null)
            {
                var prod = Instance._store.GetProductById(productId);
                if (prod != null && prod.metadata != null && !string.IsNullOrEmpty(prod.metadata.localizedPriceString))
                    return prod.metadata.localizedPriceString;
            }
#endif
            var p = ShopCatalog.Get(productId);
            return p != null ? p.FallbackPrice : "";
        }

        /// <summary>Re-delivers No Ads / Starter Pack (needed after reinstall; required button on iOS).</summary>
        public static void Restore(Action<bool> done)
        {
            if (Instance == null)
            {
                done?.Invoke(false);
                return;
            }
            Instance.RestoreInternal(done);
        }

        public static bool Owns(string productId)
        {
            if (productId == MonetizationConfig.ProductNoAds) return Economy.NoAds;
            if (productId == MonetizationConfig.ProductStarterPack) return Economy.Data.starterPackBought;
            return false;
        }

        // ================= Granting (shared) =================

        void GrantOnce(string productId, string transactionId)
        {
            var p = ShopCatalog.Get(productId);
            if (p == null)
            {
                Debug.LogWarning($"[IapService] Purchase for unknown product {productId}");
                return;
            }
            bool already = Economy.AlreadyGranted(transactionId) || (!p.Consumable && Owns(productId));
            if (!already)
            {
                ShopCatalog.Grant(p);
                Economy.MarkGranted(transactionId);
                if (p.Kind == ProductKind.NoAds || p.Kind == ProductKind.StarterPack) AdsService.OnNoAdsBought();
                PurchaseGranted?.Invoke(productId);
            }
            Finish(productId, true);
        }

        void Finish(string productId, bool ok)
        {
            if (_pending.TryGetValue(productId, out var cb))
            {
                _pending.Remove(productId);
                cb?.Invoke(ok);
            }
            StoreChanged?.Invoke();
        }

#if BF_IAP
        // ================= Unity IAP v5 =================

        StoreController _store;

        static List<ProductDefinition> Definitions()
        {
            var list = new List<ProductDefinition>();
            foreach (var p in ShopCatalog.Products)
                list.Add(new ProductDefinition(p.Id, p.Consumable ? ProductType.Consumable : ProductType.NonConsumable));
            return list;
        }

        async void InitStore()
        {
            _store = UnityIAPServices.StoreController();

            // Every event BEFORE Connect: unfinished purchases from last session can arrive right away.
            _store.OnPurchasePending += OnPurchasePending;
            _store.OnPurchaseConfirmed += OnPurchaseConfirmed;
            _store.OnPurchaseFailed += OnPurchaseFailed;
            _store.OnPurchaseDeferred += OnPurchaseDeferred;
            _store.OnStoreConnected += OnStoreConnected;
            _store.OnStoreDisconnected += OnStoreDisconnected;
            _store.OnProductsFetched += OnProductsFetched;
            _store.OnProductsFetchFailed += OnProductsFetchFailed;
            _store.OnPurchasesFetched += OnPurchasesFetched;
            _store.OnPurchasesFetchFailed += OnPurchasesFetchFailed;

            try
            {
                await _store.Connect();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[IapService] Store connect failed: " + e.Message);
            }
        }

        void OnDestroy()
        {
            if (Instance != this || _store == null) return;
            _store.OnPurchasePending -= OnPurchasePending;
            _store.OnPurchaseConfirmed -= OnPurchaseConfirmed;
            _store.OnPurchaseFailed -= OnPurchaseFailed;
            _store.OnPurchaseDeferred -= OnPurchaseDeferred;
            _store.OnStoreConnected -= OnStoreConnected;
            _store.OnStoreDisconnected -= OnStoreDisconnected;
            _store.OnProductsFetched -= OnProductsFetched;
            _store.OnProductsFetchFailed -= OnProductsFetchFailed;
            _store.OnPurchasesFetched -= OnPurchasesFetched;
            _store.OnPurchasesFetchFailed -= OnPurchasesFetchFailed;
            Instance = null;
        }

        void OnStoreConnected()
        {
            _store.FetchProducts(Definitions());
        }

        void OnStoreDisconnected(StoreConnectionFailureDescription failure)
        {
            Debug.LogWarning($"[IapService] Store disconnected: {failure.Message}");
            Ready = false;
            StoreChanged?.Invoke();
        }

        void OnProductsFetched(List<Product> products)
        {
            Ready = true;
            _store.FetchPurchases(); // re-delivers owned No Ads / Starter Pack on a new install
            StoreChanged?.Invoke();
        }

        void OnProductsFetchFailed(ProductFetchFailed failure)
        {
            Debug.LogWarning($"[IapService] Product fetch failed: {failure.FailureReason}");
        }

        void OnPurchasesFetched(Orders orders)
        {
            // Owned non-consumables (No Ads, Starter Pack) come back here after reinstall.
            foreach (var order in orders.ConfirmedOrders)
            {
                var product = order.CartOrdered.Items().FirstOrDefault()?.Product;
                if (product == null) continue;
                var p = ShopCatalog.Get(product.definition.id);
                if (p != null && !p.Consumable && !Owns(p.Id))
                {
                    ShopCatalog.Grant(p);
                    if (p.Kind == ProductKind.NoAds || p.Kind == ProductKind.StarterPack) AdsService.OnNoAdsBought();
                }
            }
            StoreChanged?.Invoke();
        }

        void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            Debug.LogWarning($"[IapService] Purchases fetch failed: {failure.Message}");
        }

        void PurchaseInternal(ShopProduct p, Action<bool> done)
        {
            var product = _store != null ? _store.GetProductById(p.Id) : null;
            if (product == null || !Ready)
            {
                Debug.LogWarning($"[IapService] Store not ready for {p.Id}");
                done?.Invoke(false);
                return;
            }
            _pending[p.Id] = done;
            _store.PurchaseProduct(product);
        }

        void OnPurchasePending(PendingOrder order)
        {
            // Google Play receipt validation can be added here with CrossPlatformValidator once
            // GooglePlayTangle is generated (Services > In-App Purchasing > Receipt Validation Obfuscator).
            var product = order.CartOrdered.Items().FirstOrDefault()?.Product;
            if (product != null) GrantOnce(product.definition.id, order.Info.TransactionID);
            _store.ConfirmPurchase(order); // always confirm after granting, or Google refunds after 3 days
        }

        void OnPurchaseConfirmed(Order order)
        {
            if (order is FailedOrder failed)
                Debug.LogWarning($"[IapService] Confirm failed: {failed.FailureReason} {failed.Details}");
        }

        void OnPurchaseFailed(FailedOrder failed)
        {
            var product = failed.CartOrdered.Items().FirstOrDefault()?.Product;
            Debug.LogWarning($"[IapService] Purchase failed: {failed.FailureReason} {failed.Details}");
            if (product != null) Finish(product.definition.id, false);
        }

        void OnPurchaseDeferred(DeferredOrder deferred)
        {
            // Payment pending (e.g. cash payment in Pakistan). Content is granted later through OnPurchasePending.
            var product = deferred.CartOrdered.Items().FirstOrDefault()?.Product;
            Debug.Log("[IapService] Purchase deferred, waiting for payment.");
            if (product != null) Finish(product.definition.id, false);
        }

        void RestoreInternal(Action<bool> done)
        {
            if (_store == null)
            {
                done?.Invoke(false);
                return;
            }
            _store.RestoreTransactions((ok, error) =>
            {
                if (!ok) Debug.LogWarning("[IapService] Restore failed: " + error);
                _store.FetchPurchases();
                done?.Invoke(ok);
            });
        }

#else
        // ================= No IAP package: simulation =================

        static bool Simulate => Application.isEditor || Debug.isDebugBuild;

        void InitStore()
        {
            Ready = Simulate;
            Debug.Log("[IapService] In-App Purchasing package not installed. " +
                      (Simulate ? "Purchases are simulated (editor/dev build)." : "Purchases disabled."));
        }

        void PurchaseInternal(ShopProduct p, Action<bool> done)
        {
            if (!Simulate)
            {
                done?.Invoke(false);
                return;
            }
            _pending[p.Id] = done;
            GrantOnce(p.Id, "sim_" + Guid.NewGuid().ToString("N"));
        }

        void RestoreInternal(Action<bool> done) => done?.Invoke(Simulate);
#endif
    }
}
