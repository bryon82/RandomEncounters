using System.Collections;
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

        internal static IEnumerator LoadAssetBundle()
        {
            LogDebug("Loading bundle");
            var bundlePath = Path.Combine(Path.GetDirectoryName(Instance.Info.Location), "Assets", "wreckage_bundle");            
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
                LogError("Failed to load all assets from the bundle.");
        }
    }
}
