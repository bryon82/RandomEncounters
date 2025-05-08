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
        public static GameObject Mast { get; private set; }
        public static GameObject Bowsprit { get; private set; }

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
            Mast = request.allAssets.FirstOrDefault(a => a.name == "mast") as GameObject;
            Bowsprit = request.allAssets.FirstOrDefault(a => a.name == "bowsprit") as GameObject;

            if(Hull == null || Mast == null || Bowsprit == null)            
                LogError("Failed to load all assets from the bundle.");            
        }
    }
}
