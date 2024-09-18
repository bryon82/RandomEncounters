using System.IO;
using UnityEngine;

namespace RandomEncounters
{
    internal class AssetLoader
    {
        internal static GameObject hull;
        internal static GameObject mast;        
        internal static GameObject bowsprit;

        internal static void LoadFlotsam()
        {
            var bundlePath = Path.Combine(Path.GetDirectoryName(Plugin.instance.Info.Location), "Assets", "wreckage_bundle");
            var assetBundle = LoadAssetBundle(bundlePath);
            hull = assetBundle.LoadAsset<GameObject>("hull");
            mast = assetBundle.LoadAsset<GameObject>("mast");
            bowsprit = assetBundle.LoadAsset<GameObject>("bowsprit");
        }

        public static AssetBundle LoadAssetBundle(string path)
        {
            
            var bundle = AssetBundle.LoadFromFile(path);
            if (bundle == null)
            {
                Plugin.logger.LogError($"Failed to load {path}");
                return null;
            }
            return bundle;
        }
    }
}
