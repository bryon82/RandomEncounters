using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace RandomEncounters
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(SEALIFEMOD_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.raddude82.randomencounters";
        public const string PLUGIN_NAME = "RandomEncounters";
        public const string PLUGIN_VERSION = "1.1.2";

        public const string SEALIFEMOD_GUID = "com.yourname.sailwind.sealifeplugin";
        internal static BaseUnityPlugin seaLifeModInstance;

        internal static Plugin instance;
        internal static ManualLogSource logger;
        internal static Harmony harmony;

        internal static ConfigEntry<bool> controlSeaLifeMod;
        internal static ConfigEntry<int> generateEncounterMinTime;

        private void Awake()
        {
            instance = this;
            logger = Logger;
            harmony = new Harmony(PLUGIN_GUID);

            generateEncounterMinTime = Config.Bind("Settings", "Minimum encounter generation time", 900, "Minimum time in seconds to get a chance roll for an encounter, the encounter time range max is 5 minutes added to this.");
            controlSeaLifeMod = Config.Bind("Settings", "Control SeaLifeMod spawns", true, "Use this mod to control SeaLifeMod spawns.");

            foreach (var plugin in Chainloader.PluginInfos)
            {
                var metadata = plugin.Value.Metadata;
                if (controlSeaLifeMod.Value && metadata.GUID.Equals(SEALIFEMOD_GUID))
                {
                    logger.LogInfo($"{SEALIFEMOD_GUID} found");
                    seaLifeModInstance = plugin.Value.Instance;
                    SeaLifeMod.PatchMod();
                }
            }
            
            AssetLoader.LoadFlotsam();
            this.gameObject.AddComponent<EncounterGenerator>();            
        }
    }
}
