using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BazaarFlow.UI
{
    /// <summary>iOS-style on/off switch with a springy knob.</summary>
    public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
    {
        public UIGradient Track;
        public RectTransform Knob;
        public event Action<bool> Changed;

        bool _on;
        float _travel;

        public bool On => _on;

        public void Init(bool on, float travel)
        {
            _travel = travel;
            Set(on, false);
        }

        public void Set(bool on, bool animate)
        {
            _on = on;
            var p = on ? UITheme.Green : UITheme.Grey;
            Track.Set(p.Top, p.Bottom);
            float x = on ? _travel : -_travel;
            if (!animate) { Knob.anchoredPosition = new Vector2(x, 0f); return; }
            Vector2 from = Knob.anchoredPosition;
            Tween.Run(Knob, "knob", 0.3f, Ease.OutBack, k => Knob.anchoredPosition = Vector2.LerpUnclamped(from, new Vector2(x, 0f), k));
            Tween.Punch(Knob, 0.2f, 0.3f);
        }

        public void OnPointerClick(PointerEventData e)
        {
            Set(!_on, true);
            UIServices.Sfx("ui_toggle");
            UIServices.Haptic(HapticStrength.Light);
            Changed?.Invoke(_on);
        }
    }
}
