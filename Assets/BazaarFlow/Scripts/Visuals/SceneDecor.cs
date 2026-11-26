using UnityEngine;

namespace BazaarFlow.Visuals
{
    /// <summary>
    /// The bazaar around the board: painted background (sky, old city, tiled floor, wooden godown),
    /// rangoli inside the belt loop, swaying bunting, kites drifting in the sky and floating dust glints.
    /// Lives for the whole session; Place() lines it up with each level's belt.
    /// </summary>
    public class SceneDecor : MonoBehaviour
    {
        // Background art is 12.4 x 24.8 world units, centred on the world origin (see Tools/ArtGen/art.mjs BG).
        const float BgWidth = 12.4f;

        Transform _rangoli, _bunting;
        Transform[] _kites;
        Vector3[] _kiteBase;
        float _beltTop, _ambientT;
        Vector3 _loopCenter;
        Vector2 _loopHalf;

        static readonly Color[] KiteColors =
        {
            new Color(0.95f, 0.35f, 0.35f), new Color(0.3f, 0.6f, 0.95f), new Color(0.98f, 0.75f, 0.2f), new Color(0.6f, 0.4f, 0.9f)
        };

        public void Build()
        {
            BazaarArt.Add(transform, "Background", BazaarArt.Get("background", BgWidth), -100, new Vector3(0, 0, 5f));
            var r = BazaarArt.Add(transform, "Rangoli", BazaarArt.Get("rangoli", 3.4f), -90, new Vector3(0, 0, 4f), new Color(1, 1, 1, 0.92f));
            if (r != null) _rangoli = r.transform;
            var b = BazaarArt.Add(transform, "Bunting", BazaarArt.Get("bunting", 10.6f, new Vector2(0.5f, 1f)), -80, new Vector3(0, 0, 4f));
            if (b != null) _bunting = b.transform;

            _kites = new Transform[3];
            _kiteBase = new Vector3[3];
            for (int i = 0; i < _kites.Length; i++)
            {
                var k = BazaarArt.Add(transform, "Kite", BazaarArt.Get("fx_kite", Random.Range(0.45f, 0.7f)), -95,
                    Vector3.zero, KiteColors[i % KiteColors.Length]);
                if (k != null) _kites[i] = k.transform;
            }
        }

        public void Place(Belt belt)
        {
            var c = belt.transform.position;
            _loopCenter = c;
            _loopHalf = new Vector2(belt.Width * 0.5f, belt.Height * 0.5f);
            _beltTop = c.y + belt.Height * 0.5f + BeltView.Thick * 0.5f;
            if (_rangoli != null) _rangoli.position = new Vector3(c.x, c.y, 4f);
            if (_bunting != null) _bunting.position = new Vector3(0, _beltTop + 1.25f, 4f);
            for (int i = 0; i < _kites.Length; i++)
                _kiteBase[i] = new Vector3(-3.2f + i * 3.3f + Random.Range(-0.6f, 0.6f), _beltTop + 2.1f + Random.Range(0f, 1.4f), 4.5f);
        }

        void LateUpdate()
        {
            float t = Time.time;
            if (_rangoli != null) _rangoli.Rotate(0, 0, -3f * Time.deltaTime);
            if (_bunting != null)
            {
                _bunting.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 0.9f) * 0.6f);
                _bunting.localScale = new Vector3(1f, 1f + Mathf.Sin(t * 1.3f) * 0.04f, 1f);
            }
            for (int i = 0; i < _kites.Length; i++)
            {
                if (_kites[i] == null) continue;
                var p = _kiteBase[i] + new Vector3(Mathf.Sin(t * 0.25f + i * 2f) * 0.8f, Mathf.Sin(t * 0.7f + i) * 0.25f, 0);
                _kites[i].position = p;
                _kites[i].localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 1.1f + i * 1.7f) * 12f);
            }

            // Ambient: a few warm glints drifting up over the floor.
            _ambientT -= Time.deltaTime;
            if (_ambientT <= 0f && _loopHalf.x > 0f)
            {
                _ambientT = 0.35f;
                var pos = _loopCenter + new Vector3(Random.Range(-_loopHalf.x, _loopHalf.x), Random.Range(-_loopHalf.y, _loopHalf.y), 0);
                Fx.Instance.Emit("fx_glow", pos, new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(0.15f, 0.35f)),
                    new Color(1f, 0.9f, 0.6f, 0.35f), Random.Range(2.5f, 4f), 0.12f, 0.2f, 0f, 0f, 0f, 1.5f, -85, true);
            }
        }
    }
}
