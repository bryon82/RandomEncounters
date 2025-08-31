using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static RandomEncounters.RE_Plugin;

namespace RandomEncounters
{
    internal class AssetLoader
    {
        public static GameObject Hull { get; private set; }
        public static GameObject SmallWreck { get; private set; }

        private static readonly List<string> assetPaths = new List<string>() {
            Path.Combine(Path.GetDirectoryName(Instance.Info.Location), "Assets"),
            Path.Combine(Path.GetDirectoryName(Instance.Info.Location))
        };

        public static string FindAssetPath(string fileName)
        {
            foreach (string basePath in assetPaths)
            {
                string fullPath = Path.Combine(basePath, fileName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            return null;
        }

        internal static IEnumerator LoadAssetBundle()
        {
            LogDebug("Loading bundle");
            var bundlePath = FindAssetPath("wreckage_bundle");
            if (string.IsNullOrEmpty(bundlePath))
            {
                LogError("Asset bundle path not found");
                yield break;
            }

            var assetBundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
            yield return assetBundleRequest;

            var assetBundle = assetBundleRequest.assetBundle;
            if (assetBundle == null)
                LogError($"Failed to load {bundlePath}");
            var request = assetBundle.LoadAllAssetsAsync();
            yield return request;

            Hull = request.allAssets.FirstOrDefault(a => a.name == "hull") as GameObject;
            SmallWreck = request.allAssets.FirstOrDefault(a => a.name == "small_wreck") as GameObject;

            if (Hull == null || SmallWreck == null)
            {
                LogError("Failed to load all assets from the bundle");
                yield break;
            }
                
            LogInfo("Assets loaded");
        }
    }
}
