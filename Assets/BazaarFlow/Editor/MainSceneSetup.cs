using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BazaarFlow.EditorTools
{
    /// <summary>
    /// Makes sure the project has a "Main" scene to press Play in. The game builds itself at runtime,
    /// so the scene only needs a camera. On the first editor load (and from the menu Belt Jam > Open Main Scene)
    /// it creates the scene next to this script's BazaarFlow folder, adds it as the first scene in the build,
    /// and opens it.
    /// </summary>
    [InitializeOnLoad]
    public static class MainSceneSetup
    {
        const string SceneName = "Main";
        const string DoneKey = "BeltJam.MainSceneSetupDone";

        static MainSceneSetup()
        {
            // Wait until the editor has finished loading, and never run while playing.
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (SessionState.GetBool(DoneKey, false)) return; // once per editor session
                SessionState.SetBool(DoneKey, true);
                string path = ScenePath();
                bool missing = !File.Exists(path);
                bool notInBuild = !EditorBuildSettings.scenes.Any(s => s.path == path);
                if (missing || notInBuild) OpenMainScene();
            };
        }

        [MenuItem("Belt Jam/Open Main Scene", priority = 0)]
        public static void OpenMainScene()
        {
            string path = ScenePath();
            if (!File.Exists(path)) CreateScene(path);
            AddToBuild(path);

            var active = SceneManager.GetActiveScene();
            if (active.path != path && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            Debug.Log($"Belt Jam: open scene '{path}' and press Play.");
        }

        static void CreateScene(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 8.2f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = GameColors.Background;
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();

            EditorSceneManager.SaveScene(scene, path);
            AssetDatabase.Refresh();
        }

        static void AddToBuild(string path)
        {
            var list = EditorBuildSettings.scenes.Where(s => s.path != path).ToList();
            list.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        /// <summary>Scenes/Main.unity inside the BazaarFlow folder, wherever that folder sits in Assets.</summary>
        static string ScenePath()
        {
            string root = "Assets/BazaarFlow";
            var guids = AssetDatabase.FindAssets("MainSceneSetup t:MonoScript");
            foreach (var g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g).Replace('\\', '/');
                int i = p.IndexOf("/BazaarFlow/", System.StringComparison.Ordinal);
                if (i >= 0) { root = p.Substring(0, i + "/BazaarFlow".Length); break; }
            }
            return root + "/Scenes/" + SceneName + ".unity";
        }
    }
}
