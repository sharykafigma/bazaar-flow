using UnityEngine;
using UnityEngine.UI;

namespace BazaarFlow.UI
{
    /// <summary>
    /// Lose popup ("Jammed!"): the card thuds in and shakes, then offers Continue (rewarded ad),
    /// Continue for coins, or Try Again.
    /// </summary>
    public class JamScreen : UIScreen
    {
        protected override bool IsPopup => true;

        RectTransform _card, _goods;
        JuicyButton _adContinue, _coinContinue, _retry;

        protected override void Build()
        {
            var content = UIFactory.Card(Safe, "JAMMED!", new Vector2(900f, 1060f), out _card, UITheme.Red);
            _card.Place(new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(900f, 1060f));
            PopTarget = _card;

            // A little row of stuck goods
            _goods = UIFactory.Rect("StuckGoods", content).Place(new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(600f, 100f));
            for (int i = 0; i < 6; i++)
            {
                var g = UIFactory.Rect("Good", _goods).Place(new Vector2(0.5f, 0.5f), new Vector2(-250f + i * 100f, 0f), new Vector2(80f, 80f));
                var e = UIFactory.Image(g, "Edge", UISprites.Circle, UITheme.Ink);
                e.rectTransform.Stretch(-5f, -5f, -5f, -5f);
                var f = UIFactory.Image(g, "Fill", UISprites.Circle, UITheme.Festive[(i / 2 * 2 + 1) % UITheme.Festive.Length]);
                f.rectTransform.Stretch();
            }

            var msg = UIFactory.Label(content, "The belt is full and no shop wants these goods!",
                UITheme.BodySize + 2, UITheme.Ink, false);
            msg.rectTransform.HStretch(1f, -190f, 130f, 10f);

            _adContinue = UIFactory.Button(content, "Continue", UITheme.Green, new Vector2(720f, 180f), UIIcon.Video, 68);
            ((RectTransform)_adContinue.transform).Place(new Vector2(0.5f, 1f), new Vector2(0f, -380f), new Vector2(720f, 180f));
            _adContinue.Clicked += () => Root.ContinueWithAd();
            var hint = UIFactory.Label(_adContinue.Face, "Clears 6 goods back to the godown", 30, Color.white);
            hint.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(640f, 40f));

            _coinContinue = UIFactory.Button(content, "Continue   " + UIRoot.ContinueCoinCost, UITheme.Orange,
                new Vector2(720f, 150f), null, 56);
            ((RectTransform)_coinContinue.transform).Place(new Vector2(0.5f, 1f), new Vector2(0f, -590f), new Vector2(720f, 150f));
            var coin = UIFactory.Coin(_coinContinue.Face, 72f);
            coin.anchorMin = coin.anchorMax = new Vector2(0.5f, 0.5f);
            coin.anchoredPosition = new Vector2(110f, 4f);
            _coinContinue.Clicked += () => Root.ContinueWithCoins();

            _retry = UIFactory.Button(content, "Try Again", UITheme.Cream3D, new Vector2(720f, 140f), UIIcon.Retry, 52);
            ((RectTransform)_retry.transform).Place(new Vector2(0.5f, 1f), new Vector2(0f, -780f), new Vector2(720f, 140f));
            _retry.Clicked += () => Root.Restart(true);
        }

        public void Open(bool canContinue)
        {
            Show();
            _adContinue.gameObject.SetActive(canContinue);
            _coinContinue.gameObject.SetActive(canContinue && UIServices.TrySpendCoins != null);
            if (canContinue) _adContinue.MakeCallToAction();
            else _retry.MakeCallToAction();

            // Thud in from above, then shake
            _card.localScale = Vector3.one;
            var home = new Vector2(0f, -20f);
            _card.anchoredPosition = home + new Vector2(0f, 900f);
            Tween.Move(_card, home, 0.5f, Ease.OutBounce, 0f, () => Tween.Shake(_card, 20f, 0.4f));
            for (int i = 0; i < _goods.childCount; i++)
            {
                var g = (RectTransform)_goods.GetChild(i);
                g.localScale = Vector3.zero;
                Tween.Scale(g, Vector3.one, 0.3f, Ease.OutBack, 0.5f + i * 0.06f);
            }
        }
    }
}
