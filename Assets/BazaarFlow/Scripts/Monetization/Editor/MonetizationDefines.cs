using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace BazaarFlow.EditorTools
{
    /// <summary>
    /// Turns the real ad / IAP code on when its package is installed, and off when it is removed:
    ///   Ads Mediation (com.unity.services.levelplay) -> BF_LEVELPLAY
    ///   In-App Purchasing 5.x (com.unity.purchasing) -> BF_IAP
    /// Lives in its own editor-only assembly so it still compiles (and can fix the defines)
    /// even if game code fails to compile after a package is removed.
    /// </summary>
    [InitializeOnLoad]
    static class MonetizationDefines
    {
        const string LevelPlayDefine = "BF_LEVELPLAY";
        const string IapDefine = "BF_IAP";

        static readonly NamedBuildTarget[] Targets =
        {
            NamedBuildTarget.Android, NamedBuildTarget.iOS, NamedBuildTarget.Standalone
        };

        static MonetizationDefines()
        {
            EditorApplication.delayCall += Apply;
        }

        [MenuItem("Bazaar Flow/Monetization/Refresh SDK Defines")]
        static void Apply()
        {
            bool levelPlay = TypeExists("Unity.Services.LevelPlay.LevelPlay");
            bool iap = TypeExists("UnityEngine.Purchasing.UnityIAPServices"); // only exists in IAP v5+

            foreach (var t in Targets)
            {
                string current = PlayerSettings.GetScriptingDefineSymbols(t);
                var set = new List<string>(current.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                bool changed = Toggle(set, LevelPlayDefine, levelPlay) | Toggle(set, IapDefine, iap);
                if (changed)
                {
                    PlayerSettings.SetScriptingDefineSymbols(t, string.Join(";", set));
                    Debug.Log($"[Bazaar Flow] {t.TargetName} defines: LevelPlay={(levelPlay ? "on" : "off")}, IAP={(iap ? "on" : "off")}");
                }
            }
        }

        static bool Toggle(List<string> set, string define, bool on)
        {
            bool has = set.Contains(define);
            if (on && !has) { set.Add(define); return true; }
            if (!on && has) { set.Remove(define); return true; }
            return false;
        }

        static bool TypeExists(string fullName) =>
            AppDomain.CurrentDomain.GetAssemblies().Any(a =>
            {
                try { return a.GetType(fullName, false) != null; }
                catch { return false; }
            });

        [MenuItem("Bazaar Flow/Monetization/Reset Wallet (coins, boosters, No Ads)")]
        static void ResetWallet()
        {
            PlayerPrefs.DeleteKey("bf_save");
            PlayerPrefs.DeleteKey("bf_coins");
            PlayerPrefs.Save();
            Debug.Log("[Bazaar Flow] Wallet reset.");
        }
    }
}
