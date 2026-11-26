using UnityEngine;
using UnityEngine.UI;

namespace BazaarFlow.UI
{
    /// <summary>
    /// Level Complete: spinning rays, three stars that slam in one by one, confetti, a coin reward that flies
    /// into the HUD counter, NEXT LEVEL, and an optional "watch ad for 2x coins".
    /// </summary>
    public class WinScreen : UIScreen
    {
        protected override bool IsPopup => true;

        RectTransform _card, _rays, _glow, _coinRow;
        readonly RectTransform[] _stars = new RectTransform[3];
        readonly Image[] _starFills = new Image[3];
        Text _subtitle, _rewardText;
        JuicyButton _next, _double;
        int _level, _reward;
        bool _doubled;

        protected override void Build()
        {
            _glow = UIFactory.Image(Safe, "Glow", UISprites.Glow, UITheme.Gold.WithAlpha(0.6f)).rectTransform;
            _glow.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, 420f), new Vector2(1100f, 1100f));
            _rays = UIFactory.Image(Safe, "Rays", UISprites.Rays, UITheme.Gold.WithAlpha(0.5f)).rectTransform;
            _rays.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, 420f), new Vector2(1500f, 1500f));

            var content = UIFactory.Card(Safe, "LEVEL COMPLETE!", new Vector2(900f, 880f), out _card, UITheme.Green);
            _card.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(900f, 880f));
            PopTarget = _card;

            // Stars sit above the ribbon
            var starRow = UIFactory.Rect("Stars", _card).Place(new Vector2(0.5f, 1f), new Vector2(0f, 190f), new Vector2(700f, 260f));
            for (int i = 0; i < 3; i++)
            {
                bool mid = i == 1;
                float size = mid ? 250f : 190f;
                var s = UIFactory.Rect("Star" + i, starRow).Place(new Vector2(0.5f, 0.5f),
                    new Vector2((i - 1) * 230f, mid ? 40f : -10f), new Vector2(size, size));
                s.localRotation = Quaternion.Euler(0, 0, (1 - i) * 14f);
                var edge = UIFactory.Image(s, "Edge", UISprites.Icon(UIIcon.Star), UITheme.Ink);
                edge.rectTransform.Stretch(-12f, -20f, -12f, -4f);
                var empty = UIFactory.Image(s, "Empty", UISprites.Icon(UIIcon.Star), UITheme.Hex("#6E5444"));
                empty.rectTransform.Stretch();
                var fill = UIFactory.Image(s, "Fill", UISprites.Icon(UIIcon.Star), Color.white);
                fill.rectTransform.Stretch();
                fill.Gradient(UITheme.Hex("#FFF59A"), UITheme.Hex("#FFAE1A"));
                var hi = UIFactory.Image(fill.transform, "Hi", UISprites.Circle, new Color(1, 1, 1, 0.55f));
                hi.rectTransform.Place(new Vector2(0.4f, 0.62f), Vector2.zero, new Vector2(size * 0.14f, size * 0.1f));
                _stars[i] = s;
                _starFills[i] = fill;
            }

            _subtitle = UIFactory.Label(content, "Level 1", UITheme.BodySize + 6, UITheme.Ink, false);
            _subtitle.rectTransform.HStretch(1f, -40f, 70f, 0f);

            _coinRow = UIFactory.Rect("Reward", content).Place(new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(520f, 130f));
            var coin = UIFactory.Coin(_coinRow, 120f);
            coin.anchorMin = coin.anchorMax = new Vector2(0f, 0.5f);
            coin.anchoredPosition = new Vector2(100f, 0f);
            _rewardText = UIFactory.Title(_coinRow, "+0", 110, UITheme.Gold);
            _rewardText.rectTransform.Stretch(180f, 0f, 0f, 0f);
            _rewardText.alignment = TextAnchor.MiddleLeft;

            _next = UIFactory.Button(content, "NEXT LEVEL", UITheme.Green, new Vector2(720f, 180f), UIIcon.Play, 70);
            ((RectTransform)_next.transform).Place(new Vector2(0.5f, 0f), new Vector2(0f, 270f), new Vector2(720f, 180f));
            _next.Clicked += () => Root.NextLevel();

            _double = UIFactory.Button(content, "2x Coins", UITheme.Purple, new Vector2(560f, 130f), UIIcon.Video, 50);
            ((RectTransform)_double.transform).Place(new Vector2(0.5f, 0f), new Vector2(0f, 80f), new Vector2(560f, 130f));
            _double.Clicked += DoubleCoins;
        }

        public void Open(int level, int reward, int stars)
        {
            _level = level;
            _reward = reward;
            _doubled = false;
            Show();
            _subtitle.text = "Level " + level + " cleared!";
            _rewardText.text = "+0";
            _double.gameObject.SetActive(UIServices.AddCoins != null);
            _double.Interactable = true;

            // Coins were already added by gameplay; show the old total and let the coins fly in.
            var hudCoins = Root.Hud.Coins;
            hudCoins.Hold(UIServices.GetCoins() - reward, 3.5f);

            for (int i = 0; i < 3; i++)
            {
                var s = _stars[i];
                bool earned = i < stars;
                _starFills[i].enabled = earned;
                s.localScale = Vector3.zero;
                int idx = i;
                Tween.Scale(s, Vector3.one, 0.45f, Ease.OutBack, 0.35f + i * 0.22f, () =>
                {
                    if (!earned) return;
                    UIServices.Sfx("ui_star");
                    UIServices.Haptic(HapticStrength.Light);
                    Tween.Punch(_stars[idx], 0.25f, 0.3f);
                });
            }

            int flying = UIFx.CoinCount(Mathf.Min(reward, 10));
            int perCoin = Mathf.Max(1, reward / flying);
            Tween.Run(_rewardText, "count", 0.8f, Ease.OutCubic,
                k => _rewardText.text = "+" + Mathf.RoundToInt(reward * k), 0.9f,
                () => Root.Fx.CoinFly(_coinRow, hudCoins.CoinIcon, flying,
                    () => hudCoins.Bump(perCoin), () => hudCoins.Hold(UIServices.GetCoins(), 0f)));

            Root.Fx.Confetti();
            _next.MakeCallToAction();
        }

        protected override void OnShow()
        {
            Tween.Run(_rays, "spin", 18f, Ease.Linear, k => _rays.localRotation = Quaternion.Euler(0, 0, 360f * k), 0f, null, true);
            Tween.Run(_glow, "pulse", 1.6f, Ease.Linear, k =>
            {
                float s = 1f + Mathf.Sin(k * Mathf.PI * 2f) * 0.06f;
                _glow.localScale = new Vector3(s, s, 1f);
            }, 0f, null, true);
        }

        void DoubleCoins()
        {
            if (_doubled) return;
            UIServices.ShowRewarded("double_coins", ok =>
            {
                if (!ok) { Root.Fx.Toast("Ad not available right now"); return; }
                _doubled = true;
                UIServices.AddCoins?.Invoke(_reward);
                UIServices.NotifyWalletChanged();
                _double.Interactable = false;
                _double.SetText("Doubled!");
                int from = _reward;
                Tween.Run(_rewardText, "count", 0.6f, Ease.OutCubic,
                    k => _rewardText.text = "+" + Mathf.RoundToInt(Mathf.Lerp(from, from * 2, k)));
                Tween.Punch(_coinRow, 0.25f, 0.4f);
                Root.Fx.CoinFly(_coinRow, Root.Hud.Coins.CoinIcon, 10, null, null);
            });
        }
    }
}
