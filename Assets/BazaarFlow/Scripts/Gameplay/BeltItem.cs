using System.Collections;
using UnityEngine;

namespace BazaarFlow
{
    /// <summary>One piece of goods riding the belt.</summary>
    public class BeltItem : MonoBehaviour
    {
        public ItemColor Color { get; private set; }
        public bool Leaving { get; private set; }

        public static BeltItem Spawn(ItemColor color, Vector3 pos, Transform parent)
        {
            var sr = SpriteFactory.Create("Item_" + color, parent, SpriteFactory.Circle, GameColors.Get(color),
                pos, new Vector2(0.5f, 0.5f), 3);
            var item = sr.gameObject.AddComponent<BeltItem>();
            item.Color = color;
            item.StartCoroutine(item.PopIn());
            return item;
        }

        IEnumerator PopIn()
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.15f;
                float s = Mathf.Lerp(0.1f, 0.5f, Mathf.SmoothStep(0, 1, t));
                transform.localScale = new Vector3(s, s, 1);
                yield return null;
            }
        }

        /// <summary>Hop from the belt into a shop, then disappear.</summary>
        public void FlyTo(Vector3 target, System.Action onArrive)
        {
            Leaving = true;
            StartCoroutine(Fly(target, onArrive));
        }

        IEnumerator Fly(Vector3 target, System.Action onArrive)
        {
            Vector3 start = transform.position;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.25f;
                float e = Mathf.SmoothStep(0, 1, t);
                Vector3 p = Vector3.Lerp(start, target, e);
                p.y += Mathf.Sin(e * Mathf.PI) * 0.6f; // little hop
                transform.position = p;
                float s = Mathf.Lerp(0.5f, 0.25f, e);
                transform.localScale = new Vector3(s, s, 1);
                yield return null;
            }
            onArrive?.Invoke();
            Destroy(gameObject);
        }
    }
}
