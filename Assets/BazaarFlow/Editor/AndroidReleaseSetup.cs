#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BazaarFlow.EditorTools
{
    /// <summary>
    /// One-click Google Play release setup for Belt Jam: Conveyor Sort.
    /// Menu: Belt Jam > Apply Android Release Settings, then Belt Jam > Build Release AAB.
    ///
    /// Signing secrets are NEVER stored in the repo. Set these environment variables before
    /// opening Unity (or fill Player Settings > Publishing Settings by hand):
    ///   BF_KEYSTORE_PATH   full path to your .keystore file (keep it OUTSIDE the project folder)
    ///   BF_KEYSTORE_PASS   keystore password
    ///   BF_KEY_ALIAS       key alias (default: beltjam)
    ///   BF_KEY_PASS        key password
    /// </summary>
    public static class AndroidReleaseSetup
    {
        // Package name can NEVER change after the first upload to Play Console. Decide before uploading.
        public const string PackageName = "com.sharystudio.beltjam";
        public const string CompanyName = "Shary Studio";
        public const string ProductName = "Belt Jam";

        // Bump BOTH for every upload. VersionCode must always go up (1, 2, 3...).
        public const string VersionName = "0.1.0";
        public const int VersionCode = 1;

        // Google Play: new apps and updates must target Android 16 (API 36) from 31 Aug 2026.
        const int TargetApi = 36;
        const int MinApi = 24; // Android 7.0, covers nearly all active devices

        const string MainScenePath = "Assets/BazaarFlow/Scenes/Main.unity";
        const string OutputDir = "Builds/Android";

        [MenuItem("Belt Jam/Apply Android Release Settings", priority = 1)]
        public static void ApplySettings()
        {
            var android = NamedBuildTarget.Android;

            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.SetApplicationIdentifier(android, PackageName);
            PlayerSettings.bundleVersion = VersionName;
            PlayerSettings.Android.bundleVersionCode = VersionCode;

            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)MinApi;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)TargetApi;

            // Play requires 64-bit; IL2CPP + ARM64 (+ARMv7 for old phones).
            PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            PlayerSettings.SetManagedStrippingLevel(android, ManagedStrippingLevel.Low);

            // Portrait-only puzzle game.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            PlayerSettings.Android.forceInternetPermission = false; // ads SDK adds INTERNET itself later
            PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;

            // Google Play only accepts App Bundles (.aab) for new apps.
            EditorUserBuildSettings.buildAppBundle = true;

            ApplySigningFromEnvironment();
            EnsureMainSceneInBuild();

            AssetDatabase.SaveAssets();
            Debug.Log($"[BeltJam] Android release settings applied: {PackageName} v{VersionName} ({VersionCode}), target API {TargetApi}, min API {MinApi}, AAB on.");
        }

        [MenuItem("Belt Jam/Build Release AAB", priority = 2)]
        public static void BuildReleaseAab()
        {
            ApplySettings();

            if (!PlayerSettings.Android.useCustomKeystore)
            {
                EditorUtility.DisplayDialog("Belt Jam",
                    "Release keystore set nahi hai. BF_KEYSTORE_PATH, BF_KEYSTORE_PASS, BF_KEY_ALIAS, BF_KEY_PASS set karein " +
                    "ya Player Settings > Publishing Settings mein keystore chunein, phir dobara build karein.", "OK");
                return;
            }

            Directory.CreateDirectory(OutputDir);
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = Path.Combine(OutputDir, $"BeltJam-{VersionName}-{VersionCode}.aab"),
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BeltJam] AAB ready: {options.locationPathName} ({report.summary.totalSize / (1024 * 1024)} MB)");
                EditorUtility.RevealInFinder(options.locationPathName);
            }
            else
            {
                Debug.LogError($"[BeltJam] Build failed: {report.summary.result}. Console mein errors dekhein.");
            }
        }

        static void ApplySigningFromEnvironment()
        {
            string path = Environment.GetEnvironmentVariable("BF_KEYSTORE_PATH");
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Debug.LogWarning("[BeltJam] BF_KEYSTORE_PATH not set or file missing. Signing left as-is (set it in Publishing Settings).");
                return;
            }

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = path;
            PlayerSettings.Android.keystorePass = Environment.GetEnvironmentVariable("BF_KEYSTORE_PASS") ?? "";
            PlayerSettings.Android.keyaliasName = Environment.GetEnvironmentVariable("BF_KEY_ALIAS") ?? "beltjam";
            PlayerSettings.Android.keyaliasPass = Environment.GetEnvironmentVariable("BF_KEY_PASS") ?? "";
        }

        // The prototype builds itself at runtime (Bootstrap.cs), so an empty scene is enough.
        static void EnsureMainSceneInBuild()
        {
            if (EditorBuildSettings.scenes.Any(s => s.enabled && File.Exists(s.path))) return;

            if (!File.Exists(MainScenePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(MainScenePath));
                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, MainScenePath);
            }

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MainScenePath, true) };
            Debug.Log($"[BeltJam] Added {MainScenePath} to the build.");
        }
    }
}
#endif
