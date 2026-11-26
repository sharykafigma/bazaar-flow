using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BazaarFlow.UI
{
    /// <summary>Vertical two-colour gradient on any uGUI Graphic (multiplies the Graphic's own colour).</summary>
    [AddComponentMenu("Belt Jam/UI Gradient")]
    public class UIGradient : BaseMeshEffect
    {
        public Color Top = Color.white;
        public Color Bottom = Color.gray;

        static readonly List<UIVertex> Verts = new List<UIVertex>();

        public void Set(Color top, Color bottom)
        {
            Top = top;
            Bottom = bottom;
            if (graphic != null) graphic.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0) return;
            Verts.Clear();
            vh.GetUIVertexStream(Verts);
            float min = float.MaxValue, max = float.MinValue;
            foreach (var v in Verts)
            {
                min = Mathf.Min(min, v.position.y);
                max = Mathf.Max(max, v.position.y);
            }
            float h = Mathf.Max(0.0001f, max - min);
            for (int i = 0; i < Verts.Count; i++)
            {
                var v = Verts[i];
                Color g = Color.Lerp(Bottom, Top, (v.position.y - min) / h);
                v.color = (Color32)(g * (Color)v.color);
                Verts[i] = v;
            }
            vh.Clear();
            vh.AddUIVertexTriangleStream(Verts);
        }
    }
}
