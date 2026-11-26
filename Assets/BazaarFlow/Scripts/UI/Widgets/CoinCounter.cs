using UnityEngine;
using UnityEngine.UI;

namespace BazaarFlow.UI
{
    /// <summary>Coin pill with a "+" that opens the shop. Counts up smoothly and punches when coins change.</summary>
    public class CoinCounter : MonoBehaviour
    {
        public RectTransform CoinIcon;
        Text _label;
        int _shown = -1, _target;
        float _poll;

        public static CoinCounter Create(Transform parent, Vector2 size)
        {
            var pill = UIFactory.Pill(parent, size, out var label);
            pill.name = "Coins";
            var cc = pill.gameObject.AddComponent<CoinCounter>();
            cc._label = label;
            var coin = UIFactory.Coin(pill, size.y * 1.15f);
            coin.anchorMin = coin.anchorMax = new Vector2(0f, 0.5f);
            coin.anchoredPosition = new Vector2(size.y * 0.35f, 0f);
            cc.CoinIcon = coin;
            var plus = UIFactory.IconButton(pill, UIIcon.Plus, UITheme.Green, size.y * 0.78f);
            var prt = (RectTransform)plus.transform;
            prt.anchorMin = prt.anchorMax = new Vector2(1f, 0.5f);
            prt.anchoredPosition = new Vector2(-size.y * 0.38f, 2f);
            plus.Clicked += () => UIRoot.Instance.OpenShop();
            UIServices.WalletChanged += cc.OnWallet;
            return cc;
        }

        void OnDestroy() => UIServices.WalletChanged -= OnWallet;
        void OnWallet() => _poll = 0f;

        /// <summary>Show the value right away (no count-up), e.g. while coins are flying in.</summary>
        public void Hold(int value, float seconds)
        {
            Tween.Kill(_label, "count");
            _shown = _target = value;
            _label.text = Format(value);
            _poll = seconds;
        }

        /// <summary>Add to the shown value by hand (one coin landing), without waiting for the poll.</summary>
        public void Bump(int amount)
        {
            _shown += amount;
            _target = _shown;
            _label.text = Format(_shown);
            Tween.Punch(CoinIcon, 0.3f, 0.25f);
        }

        void Update()
        {
            _poll -= Time.unscaledDeltaTime;
            if (_poll <= 0f)
            {
                _poll = 0.25f;
                int coins = UIServices.GetCoins();
                if (_shown < 0) { _shown = _target = coins; _label.text = Format(coins); }
                else if (coins != _target)
                {
                    int from = _shown;
                    _target = coins;
                    Tween.Run(_label, "count", 0.6f, Ease.OutCubic, k =>
                    {
                        _shown = Mathf.RoundToInt(Mathf.Lerp(from, _target, k));
                        _label.text = Format(_shown);
                    });
                    Tween.Punch(CoinIcon, 0.25f, 0.3f);
                }
            }
        }

        public static string Format(int v) => v.ToString("N0");
    }
}
