using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace RandomEncounters
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(SEALIFEMOD_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(IDLEFISHING_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(HOOKSHANGMORE_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class RE_Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.raddude82.randomencounters";
        public const string PLUGIN_NAME = "RandomEncounters";
        public const string PLUGIN_VERSION = "1.4.0";

        public const string SEALIFEMOD_GUID = "com.yourname.sailwind.sealifeplugin";
        public const string IDLEFISHING_GUID = "ISA_IdleFishing";
        public const string HOOKSHANGMORE_GUID = "com.raddude82.hookshangmore";

        internal static BaseUnityPlugin SeaLifeModPluginInstance { get; private set; }
        internal static bool IdleFishingPluginDetected { get; private set; } = false;
        internal static bool HooksHangMorePluginDetected { get; private set; } = false;

        internal static RE_Plugin Instance { get; private set; }
        internal static Harmony HarmonyInstance { get; private set; }
        private static ManualLogSource _logger;
        
        internal static void LogDebug(string message) => _logger.LogDebug(message);
        internal static void LogInfo(string message) => _logger.LogInfo(message);
        internal static void LogWarning(string message) => _logger.LogWarning(message);
        internal static void LogError(string message) => _logger.LogError(message);

        public static bool IsFlotsamEnabled => Configs.enableFlotsam.Value;
        public static bool IsSeaLifeModEnabled => SeaLifeModPluginInstance != null && Configs.controlSeaLifeMod.Value;
        public static bool IsIntenseStormEnabled => Configs.enableIntenseStorm.Value;
        public static bool IsDenseFogEnabled => Configs.enableDenseFog.Value;
        public static bool IsFishingBonanzaEnabled => Configs.enableFishingBonanza.Value;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _logger = Logger;

            Configs.InitializeConfigs();

            StartCoroutine(AssetLoader.LoadAssetBundle());
            HarmonyInstance = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PLUGIN_GUID);

            foreach (var plugin in Chainloader.PluginInfos)
            {
                var metadata = plugin.Value.Metadata;
                if (Configs.controlSeaLifeMod.Value && metadata.GUID.Equals(SEALIFEMOD_GUID))
                {
                    LogInfo("SealLifeMod mod found");
                    SeaLifeModPluginInstance = plugin.Value.Instance;
                    SeaLifeMod.PatchMod();
                }

                if (metadata.GUID.Equals(IDLEFISHING_GUID))
                {
                    LogInfo("IdleFishing mod found");
                    IdleFishingPluginDetected = true;
                }

                if (metadata.GUID.Equals(HOOKSHANGMORE_GUID))
                {
                    LogInfo("HooksHangMore mod found");
                    HooksHangMorePluginDetected = true;
                }
            }

            gameObject.AddComponent<EncounterGenerator>();
        }
    }
}
