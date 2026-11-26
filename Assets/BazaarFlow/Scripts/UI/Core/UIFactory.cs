using UnityEngine;
using UnityEngine.UI;

namespace BazaarFlow.UI
{
    /// <summary>Builders for the Bazaar Flow widget kit. All sizes are in reference pixels (1080 x 1920 canvas).</summary>
    public static class UIFactory
    {
        // ------------------------------------------------------------ layout basics

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5; // UI
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        /// <summary>Fill the parent with optional insets (left, bottom, right, top).</summary>
        public static RectTransform Stretch(this RectTransform rt, float l = 0, float b = 0, float r = 0, float t = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(l, b);
            rt.offsetMax = new Vector2(-r, -t);
            return rt;
        }

        /// <summary>Anchor at a point of the parent (0..1), placed at pos with a fixed size.</summary>
        public static RectTransform Place(this RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        /// <summary>Stretch horizontally with side margins, anchored at a vertical point.</summary>
        public static RectTransform HStretch(this RectTransform rt, float anchorY, float y, float height, float margin)
        {
            rt.anchorMin = new Vector2(0f, anchorY);
            rt.anchorMax = new Vector2(1f, anchorY);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(-margin * 2f, height);
            return rt;
        }

        // ------------------------------------------------------------ graphics

        public static Image Image(Transform parent, string name, Sprite sprite, Color color, bool raycast = false)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            if (sprite != null && sprite.border != Vector4.zero)
            {
                img.type = UnityEngine.UI.Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 1f / UITheme.CornerScale;
            }
            else img.preserveAspect = true;
            return img;
        }

        public static UIGradient Gradient(this Graphic g, Color top, Color bottom)
        {
            var grad = g.gameObject.AddComponent<UIGradient>();
            grad.Set(top, bottom);
            return grad;
        }

        public static Image Icon(Transform parent, UIIcon icon, Color color, float size)
        {
            var img = Image(parent, "Icon_" + icon, UISprites.Icon(icon), color);
            img.rectTransform.sizeDelta = new Vector2(size, size);
            return img;
        }

        /// <summary>Gold coin made of layered circles with a highlight.</summary>
        public static RectTransform Coin(Transform parent, float size)
        {
            var rt = Rect("Coin", parent);
            rt.sizeDelta = new Vector2(size, size);
            var rim = Image(rt, "Rim", UISprites.Circle, UITheme.Ink);
            rim.rectTransform.Stretch();
            var body = Image(rt, "Body", UISprites.Circle, Color.white);
            body.rectTransform.Stretch(size * 0.06f, size * 0.06f, size * 0.06f, size * 0.06f);
            body.Gradient(UITheme.Hex("#FFF0A0"), UITheme.GoldDark);
            var inner = Image(rt, "Inner", UISprites.Ring, UITheme.GoldDark.WithAlpha(0.8f));
            inner.rectTransform.Stretch(size * 0.2f, size * 0.2f, size * 0.2f, size * 0.2f);
            var star = Icon(rt, UIIcon.Star, UITheme.Hex("#FFF6C8"), size * 0.4f);
            star.rectTransform.anchoredPosition = Vector2.zero;
            var hi = Image(rt, "Highlight", UISprites.Circle, new Color(1f, 1f, 1f, 0.55f));
            hi.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(-size * 0.2f, size * 0.22f), new Vector2(size * 0.22f, size * 0.14f));
            return rt;
        }

        // ------------------------------------------------------------ text

        public static Text Label(Transform parent, string text, int size, Color color, bool outline = true,
            TextAnchor align = TextAnchor.MiddleCenter)
            => Label(parent, text, size, color, outline ? UITheme.Ink : (Color?)null, align);

        /// <summary>Label with a stroke in a chosen colour (null = no stroke).</summary>
        public static Text Label(Transform parent, string text, int size, Color color, Color? stroke,
            TextAnchor align = TextAnchor.MiddleCenter)
        {
            bool outline = stroke.HasValue;
            var rt = Rect("Label", parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = UITheme.Font;
            t.lineSpacing = 0.9f;
            t.text = text;
            t.fontSize = size;
            t.fontStyle = UITheme.HasDisplayFont ? FontStyle.Normal : FontStyle.Bold;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = Mathf.Max(12, size / 2);
            t.resizeTextMaxSize = size;
            if (outline)
            {
                float o = Mathf.Max(2f, size * 0.06f);
                var ol = rt.gameObject.AddComponent<Outline>();
                ol.effectColor = stroke.Value;
                ol.effectDistance = new Vector2(o, -o);
                var sh = rt.gameObject.AddComponent<Shadow>();
                sh.effectColor = stroke.Value;
                sh.effectDistance = new Vector2(0f, -o * 2f);
            }
            return t;
        }

        /// <summary>Title with a slight tilt and a dark outline, for logos and banners.</summary>
        public static Text Title(Transform parent, string text, int size, Color color)
        {
            var t = Label(parent, text, size, Color.white);
            // light top to full colour bottom, like embossed sign lettering
            t.gameObject.AddComponent<UIGradient>().Set(Color.Lerp(color, Color.white, 0.55f), color);
            return t;
        }

        // ------------------------------------------------------------ buttons

        /// <summary>
        /// Chunky 3D button. Pass text, an icon, or both (icon on the left).
        /// Size is the full footprint including the lip.
        /// </summary>
        public static JuicyButton Button(Transform parent, string text, ButtonPalette palette, Vector2 size,
            UIIcon? icon = null, int fontSize = UITheme.ButtonTextSize)
        {
            var root = Rect("Button_" + (text ?? icon?.ToString()), parent);
            root.sizeDelta = size;
            var hit = root.gameObject.AddComponent<Image>(); // invisible hit area
            hit.color = new Color(0, 0, 0, 0);
            var btn = root.gameObject.AddComponent<JuicyButton>();
            float depth = Mathf.Clamp(size.y * 0.12f, 8f, UITheme.ButtonDepth);
            btn.Depth = depth;

            var body = Rect("Body", root).Stretch();
            btn.Body = body;

            var shadow = Image(body, "Shadow", UISprites.SoftShadow, UITheme.Shadow);
            shadow.rectTransform.Stretch(-14f, -depth - 18f, -14f, 6f);
            btn.ShadowImage = shadow;

            // Ink outline behind everything gives the cartoon edge.
            // Outline in a dark tint of the button's own hue (reads richer than flat black).
            var outline = Image(body, "Outline", UISprites.Rounded, palette.Edge);
            outline.rectTransform.Stretch(-6f, -depth - 6f, -6f, -6f);
            btn.OutlineImage = outline;

            var lip = Image(body, "Lip", UISprites.Rounded, palette.Lip);
            lip.rectTransform.Stretch(0f, -depth, 0f, 0f);
            btn.LipImage = lip;

            var face = Rect("Face", body).Stretch();
            btn.Face = face;
            // Light rim: a pale ring just inside the outline, like a bevel catching the light.
            var rim = Image(face, "Rim", UISprites.Rounded, new Color(1f, 1f, 1f, 0.55f));
            rim.rectTransform.Stretch();
            var faceImg = Image(face, "FaceImage", UISprites.Rounded, Color.white);
            faceImg.rectTransform.Stretch(5f, 5f, 5f, 5f);
            btn.FaceImage = faceImg;
            btn.FaceGradient = faceImg.Gradient(palette.Top, palette.Bottom);
            faceImg.gameObject.AddComponent<Mask>().showMaskGraphic = true;

            // Inner bottom shade + gloss stripe (clipped by the face mask)
            var shade = Image(faceImg.transform, "Shade", UISprites.Rounded, new Color(0f, 0f, 0f, 0.12f));
            shade.rectTransform.Stretch(0f, 0f, 0f, size.y * 0.55f);
            var gloss = Image(faceImg.transform, "Gloss", UISprites.Pill, new Color(1f, 1f, 1f, 0.32f));
            gloss.rectTransform.anchorMin = new Vector2(0f, 0.55f);
            gloss.rectTransform.anchorMax = new Vector2(1f, 1f);
            gloss.rectTransform.offsetMin = new Vector2(16f, 0f);
            gloss.rectTransform.offsetMax = new Vector2(-16f, -8f);

            var shine = Image(faceImg.transform, "Shine", UISprites.Square, new Color(1f, 1f, 1f, 0.45f));
            shine.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            shine.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            shine.rectTransform.sizeDelta = new Vector2(size.y * 0.35f, 40f);
            shine.rectTransform.localRotation = Quaternion.Euler(0, 0, -20f);
            shine.rectTransform.anchoredPosition = new Vector2(-size.x, 0f);
            btn.Shine = shine;

            // Content
            var content = Rect("Content", face).Stretch(12f, 6f, 12f, 4f);
            var h = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleCenter;
            h.spacing = 14f;
            h.childControlWidth = false;
            h.childControlHeight = false;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;

            if (icon.HasValue)
            {
                float isz = text == null ? Mathf.Min(size.x, size.y) * 0.5f : fontSize * 1.05f;
                var ic = Icon(content, icon.Value, palette.Text, isz);
                var sh = ic.gameObject.AddComponent<Shadow>();
                sh.effectColor = palette.Edge;
                sh.effectDistance = new Vector2(0f, -4f);
                btn.IconImage = ic;
            }
            if (text != null)
            {
                var label = Label(content, text, fontSize, palette.Text,
                    palette.Text != UITheme.Ink ? palette.Edge : (Color?)null);
                float iconW = icon.HasValue ? fontSize * 1.05f + 14f : 0f;
                label.rectTransform.sizeDelta = new Vector2(size.x - 40f - iconW, size.y - 20f);
                label.alignment = TextAnchor.MiddleCenter;
                btn.Label = label;
            }

            btn.SetPalette(palette);
            return btn;
        }

        /// <summary>Round icon button (close, back, settings).</summary>
        public static JuicyButton IconButton(Transform parent, UIIcon icon, ButtonPalette palette, float size)
        {
            var b = Button(parent, null, palette, new Vector2(size, size), icon);
            return b;
        }

        // ------------------------------------------------------------ cards and pills

        /// <summary>
        /// Cream paper card with an ink border and a maroon ribbon title. Returns the content area.
        /// </summary>
        public static RectTransform Card(Transform parent, string title, Vector2 size, out RectTransform card,
            ButtonPalette? ribbon = null)
        {
            card = Rect("Card", parent);
            card.sizeDelta = size;

            var shadow = Image(card, "Shadow", UISprites.SoftShadow, new Color(0, 0, 0, 0.45f));
            shadow.rectTransform.Stretch(-30f, -50f, -30f, -10f);
            var edge = Image(card, "Edge", UISprites.Rounded, UITheme.Ink);
            edge.rectTransform.Stretch(-7f, -23f, -7f, -7f);
            var lip = Image(card, "Lip", UISprites.Rounded, UITheme.PaperDark);
            lip.rectTransform.Stretch(0f, -16f, 0f, 0f);
            var fill = Image(card, "Fill", UISprites.Rounded, Color.white, true);
            fill.rectTransform.Stretch();
            fill.Gradient(UITheme.Cream, UITheme.Paper);

            // subtle inner pattern: a thin dotted border like block-printed fabric
            var inner = Image(card, "InnerLine", UISprites.Rounded, UITheme.PaperDark.WithAlpha(0.6f));
            inner.rectTransform.Stretch(18f, 18f, 18f, 18f);
            var innerFill = Image(card, "InnerFill", UISprites.Rounded, Color.white);
            innerFill.rectTransform.Stretch(22f, 22f, 22f, 22f);
            innerFill.Gradient(UITheme.Cream, UITheme.Paper);

            if (!string.IsNullOrEmpty(title)) Ribbon(card, title, size.x - 120f, ribbon ?? UITheme.Maroon3D);

            var content = Rect("Content", card).Stretch(44f, 44f, 44f, string.IsNullOrEmpty(title) ? 44f : 120f);
            return content;
        }

        /// <summary>Ribbon banner with folded tails, centred on the top edge of its parent.</summary>
        public static RectTransform Ribbon(Transform parent, string text, float width, ButtonPalette palette)
        {
            var rt = Rect("Ribbon", parent);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 6f);
            rt.sizeDelta = new Vector2(width, 140f);

            for (int side = -1; side <= 1; side += 2)
            {
                var tailEdge = Image(rt, "TailEdge", UISprites.RoundedSmall, UITheme.Ink);
                tailEdge.rectTransform.Place(new Vector2(side < 0 ? 0f : 1f, 0.5f), new Vector2(side * 10f, -26f), new Vector2(96f, 104f));
                var tail = Image(rt, "Tail", UISprites.RoundedSmall, palette.Lip);
                tail.rectTransform.Place(new Vector2(side < 0 ? 0f : 1f, 0.5f), new Vector2(side * 10f, -26f), new Vector2(84f, 92f));
                var notch = Icon(tail.transform, UIIcon.Play, UITheme.Ink, 56f);
                notch.rectTransform.anchorMin = notch.rectTransform.anchorMax = new Vector2(side < 0 ? 0f : 1f, 0.5f);
                notch.rectTransform.anchoredPosition = new Vector2(side * 4f, 0f);
                notch.rectTransform.localRotation = Quaternion.Euler(0, 0, side < 0 ? 0f : 180f);
            }
            var edge = Image(rt, "Edge", UISprites.RoundedSmall, UITheme.Ink);
            edge.rectTransform.Stretch(-7f, -7f, -7f, -7f);
            var face = Image(rt, "Face", UISprites.RoundedSmall, Color.white);
            face.rectTransform.Stretch();
            face.Gradient(palette.Top, palette.Bottom);
            var gloss = Image(rt, "Gloss", UISprites.Pill, new Color(1, 1, 1, 0.25f));
            gloss.rectTransform.Stretch(20f, 80f, 20f, 10f);
            var label = Label(rt, text, UITheme.HeaderSize + 6, Color.white);
            label.rectTransform.Stretch(30f, 8f, 30f, 4f);
            return rt;
        }

        /// <summary>Dark translucent pill for counters (coins, level).</summary>
        public static RectTransform Pill(Transform parent, Vector2 size, out Text label)
        {
            var rt = Rect("Pill", parent);
            rt.sizeDelta = size;
            var edge = Image(rt, "Edge", UISprites.Pill, UITheme.Ink);
            edge.rectTransform.Stretch(-5f, -5f, -5f, -5f);
            var fill = Image(rt, "Fill", UISprites.Pill, UITheme.Ink.WithAlpha(0.75f));
            fill.rectTransform.Stretch();
            fill.Gradient(new Color(0.42f, 0.27f, 0.18f), new Color(0.23f, 0.13f, 0.08f));
            label = Label(rt, "0", UITheme.BodySize + 4, Color.white);
            label.rectTransform.Stretch(size.y * 0.9f, 0f, size.y * 0.7f, 0f);
            return rt;
        }
    }
}
