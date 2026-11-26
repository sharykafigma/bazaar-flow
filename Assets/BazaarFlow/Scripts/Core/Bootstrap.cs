using UnityEngine;

namespace BazaarFlow
{
    /// <summary>
    /// Starts the prototype in any scene: press Play in an empty scene and the game builds itself.
    /// Sets up a portrait orthographic camera and the GameManager.
    /// </summary>
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Start()
        {
            if (Object.FindFirstObjectByType<GameManager>() != null) return;

            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;

            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = GameColors.Background;
            cam.gameObject.AddComponent<CameraFit>();

            new GameObject("BazaarFlow").AddComponent<GameManager>();
        }
    }

    /// <summary>Keeps the whole board visible on any phone shape (needs about 9.4 x 16.4 world units).</summary>
    public class CameraFit : MonoBehaviour
    {
        Camera _cam;

        void Awake()
        {
            _cam = GetComponent<Camera>();
            Fit();
        }

        void Update() => Fit();

        void Fit()
        {
            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            _cam.orthographicSize = Mathf.Max(8.2f, 4.7f / aspect);
        }
    }
}
