using System;
using UnityEngine;
using UnityEngine.UI;

namespace BazaarFlow.UI
{
    /// <summary>
    /// Base for every screen. Full screens fade in; popups dim the game and pop their card in with an overshoot.
    /// Each screen builds its own hierarchy from code in Build().
    /// </summary>
    public abstract class UIScreen : MonoBehaviour
    {
        protected UIRoot Root;
        protected RectTransform Rt;
        protected RectTransform Safe;      // safe-area child; put content here
        protected CanvasGroup Group;
        protected Image Backdrop;          // popups only
        protected RectTransform PopTarget; // the card that pops in

        public bool IsOpen { get; private set; }
        protected virtual bool IsPopup => false;

        public void Setup(UIRoot root)
        {
            Root = root;
            Rt = (RectTransform)transform;
            Rt.Stretch();
            Group = gameObject.AddComponent<CanvasGroup>();
            if (IsPopup)
            {
                Backdrop = UIFactory.Image(Rt, "Backdrop", UISprites.Square, UITheme.Backdrop, true);
                Backdrop.rectTransform.Stretch(-20f, -20f, -20f, -20f);
            }
            Safe = UIFactory.Rect("Safe", Rt).Stretch();
            Safe.gameObject.AddComponent<SafeArea>();
            Build();
            gameObject.SetActive(false);
        }

        protected abstract void Build();
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        public void Show()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            IsOpen = true;
            Group.blocksRaycasts = true;
            Group.interactable = true;
            OnShow();

            Group.alpha = 0f;
            Tween.Fade(Group, 1f, UITheme.Fade);
            if (PopTarget != null)
            {
                PopTarget.localScale = Vector3.one * 0.6f;
                Tween.Scale(PopTarget, Vector3.one, UITheme.PopIn, Ease.OutBack);
            }
            if (IsPopup) UIServices.Sfx("ui_popup");
        }

        public void Hide(Action done = null)
        {
            if (!IsOpen) { done?.Invoke(); return; }
            IsOpen = false;
            Group.blocksRaycasts = false;
            Group.interactable = false;
            OnHide();
            if (PopTarget != null) Tween.Scale(PopTarget, Vector3.one * 0.85f, UITheme.PopOut, Ease.InBack);
            Tween.Fade(Group, 0f, UITheme.PopOut, 0f, () =>
            {
                gameObject.SetActive(false);
                done?.Invoke();
            });
        }

        public void HideInstant()
        {
            IsOpen = false;
            gameObject.SetActive(false);
        }

        /// <summary>Close button at the card's top-right corner.</summary>
        protected JuicyButton AddCloseButton(RectTransform card, Action onClose)
        {
            var b = UIFactory.IconButton(card, UIIcon.Close, UITheme.Red, 118f);
            var rt = (RectTransform)b.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-6f, 6f);
            b.ClickSound = "ui_close";
            b.Clicked += onClose;
            return b;
        }
    }
}
