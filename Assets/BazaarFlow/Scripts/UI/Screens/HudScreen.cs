using UnityEngine;
using UnityEngine.UI;

namespace BazaarFlow.UI
{
    /// <summary>In-game HUD: pause button, level ribbon, coin counter, and an optional belt-fill meter.</summary>
    public class HudScreen : UIScreen
    {
        RectTransform _top, _levelPill, _meter;
        Text _levelText, _meterText;
        Image _meterFill;
        UIGradient _meterGrad;
        CoinCounter _coins;
        int _shownLevel = -1;

        public CoinCounter Coins => _coins;

        protected override void Build()
        {
            _top = UIFactory.Rect("TopBar", Safe);
            _top.anchorMin = new Vector2(0f, 1f);
            _top.anchorMax = new Vector2(1f, 1f);
            _top.pivot = new Vector2(0.5f, 1f);
            _top.sizeDelta = new Vector2(0f, 200f);
            _top.anchoredPosition = Vector2.zero;

            var pause = UIFactory.IconButton(_top, UIIcon.Pause, UITheme.Teal3D, 130f);
            ((RectTransform)pause.transform).Place(new Vector2(0f, 1f), new Vector2(105f, -95f), new Vector2(130f, 130f));
            pause.Clicked += () => Root.PauseGame();

            // Level ribbon
            _levelPill = UIFactory.Rect("Level", _top).Place(new Vector2(0.5f, 1f), new Vector2(-20f, -95f), new Vector2(330f, 110f));
            var edge = UIFactory.Image(_levelPill, "Edge", UISprites.Pill, UITheme.Ink);
            edge.rectTransform.Stretch(-7f, -19f, -7f, -7f);
            var face = UIFactory.Image(_levelPill, "Face", UISprites.Pill, Color.white);
            face.rectTransform.Stretch();
            face.Gradient(UITheme.Maroon3D.Top, UITheme.Maroon3D.Bottom);
            var gloss = UIFactory.Image(_levelPill, "Gloss", UISprites.Pill, new Color(1, 1, 1, 0.22f));
            gloss.rectTransform.Stretch(24f, 58f, 24f, 8f);
            _levelText = UIFactory.Label(_levelPill, "Level 1", 64, Color.white);
            _levelText.rectTransform.Stretch(16f, 4f, 16f, 0f);

            _coins = CoinCounter.Create(_top, new Vector2(300f, 88f));
            ((RectTransform)_coins.transform).Place(new Vector2(1f, 1f), new Vector2(-190f, -95f), new Vector2(300f, 88f));

            // Belt meter (shown only when gameplay provides UIServices.GetBeltFill)
            _meter = UIFactory.Rect("BeltMeter", _top).Place(new Vector2(0.5f, 1f), new Vector2(0f, -205f), new Vector2(560f, 54f));
            var mEdge = UIFactory.Image(_meter, "Edge", UISprites.Pill, UITheme.Ink);
            mEdge.rectTransform.Stretch(-5f, -5f, -5f, -5f);
            var mBg = UIFactory.Image(_meter, "Bg", UISprites.Pill, UITheme.Ink.WithAlpha(0.7f));
            mBg.rectTransform.Stretch();
            _meterFill = UIFactory.Image(_meter, "Fill", UISprites.Pill, Color.white);
            _meterFill.rectTransform.anchorMin = Vector2.zero;
            _meterFill.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            _meterFill.rectTransform.offsetMin = new Vector2(4f, 4f);
            _meterFill.rectTransform.offsetMax = new Vector2(-4f, -4f);
            _meterGrad = _meterFill.Gradient(UITheme.Hex("#B8F27A"), UITheme.Hex("#3FC25A"));
            _meterText = UIFactory.Label(_meter, "BELT", 30, Color.white);
            _meterText.rectTransform.Stretch();
            _meter.gameObject.SetActive(false);
        }

        protected override void OnShow()
        {
            _top.anchoredPosition = new Vector2(0f, 260f);
            Tween.Move(_top, Vector2.zero, 0.45f, Ease.OutBack);
        }

        public void Refresh(bool animate)
        {
            var game = Root.Game;
            if (game == null) return;
            if (game.LevelNumber != _shownLevel)
            {
                _shownLevel = game.LevelNumber;
                _levelText.text = "Level " + _shownLevel;
                if (animate) Tween.Punch(_levelPill, 0.2f, 0.4f);
            }
        }

        void Update()
        {
            Refresh(true);
            float fill = UIServices.GetBeltFill();
            bool show = fill >= 0f;
            if (_meter.gameObject.activeSelf != show) _meter.gameObject.SetActive(show);
            if (!show) return;
            fill = Mathf.Clamp01(fill);
            var a = _meterFill.rectTransform.anchorMax;
            a.x = Mathf.Lerp(a.x, Mathf.Max(0.08f, fill), Time.unscaledDeltaTime * 10f);
            _meterFill.rectTransform.anchorMax = a;
            // green -> amber -> red as the belt fills up
            Color top = fill < 0.6f ? UITheme.Hex("#B8F27A") : fill < 0.85f ? UITheme.Hex("#FFE07A") : UITheme.Hex("#FF9C8A");
            Color bot = fill < 0.6f ? UITheme.Hex("#3FC25A") : fill < 0.85f ? UITheme.Hex("#FF9A2E") : UITheme.Hex("#E23B3B");
            _meterGrad.Set(top, bot);
            _meterText.text = fill >= 0.85f ? "BELT ALMOST FULL!" : "BELT";
        }

        public void ShakeTop() => Tween.Shake(_top, 22f, 0.5f);
    }
}
