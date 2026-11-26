using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BazaarFlow.UI
{
    /// <summary>
    /// Chunky 3D "candy" button: a darker lip under a gradient face with a gloss stripe.
    /// Press: face sinks onto the lip and the body squashes. Release: springy overshoot, haptic tick, sound.
    /// Optional idle breathing and a shine sweep for the main call-to-action.
    /// </summary>
    public class JuicyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
        IPointerExitHandler, IPointerEnterHandler
    {
        public RectTransform Body;      // squashes
        public RectTransform Face;      // sinks
        public Image FaceImage, LipImage, ShadowImage, Shine, OutlineImage;
        public UIGradient FaceGradient;
        public Text Label;
        public Image IconImage;
        public float Depth = UITheme.ButtonDepth;
        public string ClickSound = "ui_tap";

        public event Action Clicked;

        ButtonPalette _palette;
        bool _pressed, _inside, _interactable = true;
        float _shineTimer = -1f;

        public bool Interactable
        {
            get => _interactable;
            set
            {
                _interactable = value;
                ApplyPalette(value ? _palette : UITheme.Grey, false);
            }
        }

        public void SetPalette(ButtonPalette p)
        {
            _palette = p;
            ApplyPalette(_interactable ? p : UITheme.Grey, false);
        }

        void ApplyPalette(ButtonPalette p, bool keepStored)
        {
            foreach (var o in GetComponentsInChildren<Outline>(true)) o.effectColor = p.Edge;
            foreach (var sh in GetComponentsInChildren<Shadow>(true)) if (!(sh is Outline) && sh.transform != transform) sh.effectColor = p.Edge;
            if (FaceGradient != null) FaceGradient.Set(p.Top, p.Bottom);
            if (LipImage != null) LipImage.color = p.Lip;
            if (OutlineImage != null) OutlineImage.color = p.Edge;
            if (Label != null) Label.color = p.Text;
            if (IconImage != null) IconImage.color = p.Text;
        }

        public void SetText(string text)
        {
            if (Label != null) Label.text = text;
        }

        /// <summary>Gentle idle pulse plus a periodic shine sweep, for the one button the player should press.</summary>
        public void MakeCallToAction()
        {
            Tween.Breathe(transform, 0.035f, 1.5f);
            _shineTimer = 0.6f;
        }

        void Update()
        {
            if (_shineTimer < 0f || Shine == null) return;
            _shineTimer -= Time.unscaledDeltaTime;
            if (_shineTimer > 0f) return;
            _shineTimer = 2.8f;
            var rt = Shine.rectTransform;
            float w = Face.rect.width;
            Tween.Run(rt, "shine", 0.55f, Ease.InOutSine,
                k => rt.anchoredPosition = new Vector2(Mathf.Lerp(-w * 0.75f, w * 0.75f, k), 0f));
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (!_interactable) return;
            _pressed = true;
            _inside = true;
            PressVisual(true);
            UIServices.Haptic(HapticStrength.Light);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (!_pressed) return;
            _pressed = false;
            PressVisual(false);
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (_pressed && !_inside) { _inside = true; PressVisual(true); }
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (_pressed && _inside) { _inside = false; PressVisual(false); }
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (!_interactable)
            {
                Tween.Shake(Body, 10f, 0.3f);
                UIServices.Sfx("ui_locked");
                return;
            }
            UIServices.Sfx(ClickSound);
            Clicked?.Invoke();
        }

        void PressVisual(bool down)
        {
            if (down)
            {
                Tween.Run(Face, "press", 0.07f, Ease.OutQuad, k => Face.anchoredPosition = new Vector2(0f, -Depth * 0.8f * k));
                Tween.Run(Body, "scale", 0.09f, Ease.OutQuad, k =>
                {
                    float s = Mathf.Lerp(1f, 0.94f, k);
                    Body.localScale = new Vector3(s + (1f - s) * 0.3f, s, 1f);
                });
            }
            else
            {
                Vector2 from = Face.anchoredPosition;
                Tween.Run(Face, "press", 0.18f, Ease.OutBack, k => Face.anchoredPosition = Vector2.LerpUnclamped(from, Vector2.zero, k));
                Vector3 sFrom = Body.localScale;
                Tween.Run(Body, "scale", 0.38f, Ease.OutElastic,
                    k => Body.localScale = Vector3.LerpUnclamped(sFrom, Vector3.one, k));
            }
        }

        void OnDisable()
        {
            _pressed = false;
            if (Face != null) Face.anchoredPosition = Vector2.zero;
            if (Body != null) Body.localScale = Vector3.one;
        }
    }
}
