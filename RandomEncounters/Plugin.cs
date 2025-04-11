using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace RandomEncounters
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(SEALIFEMOD_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(IDLEFISHING_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.raddude82.randomencounters";
        public const string PLUGIN_NAME = "RandomEncounters";
        public const string PLUGIN_VERSION = "1.1.6";

        public const string SEALIFEMOD_GUID = "com.yourname.sailwind.sealifeplugin";
        internal static BaseUnityPlugin seaLifeModInstance;

        public const string IDLEFISHING_GUID = "ISA_IdleFishing";
        internal static bool idleFishingFound = false;

        internal static Plugin instance;
        internal static ManualLogSource logger;
        internal static Harmony harmony;

        internal static ConfigEntry<int> generateEncounterMinTime;
        internal static ConfigEntry<bool> enableFlotsam;
        internal static ConfigEntry<bool> controlSeaLifeMod;
        internal static ConfigEntry<bool> enableDenseFog;
        internal static ConfigEntry<int> fogDuration;
        internal static ConfigEntry<bool> enableFishingBonanza;
        internal static ConfigEntry<int> fishingBonanzaDuration;
        internal static ConfigEntry<bool> enableIntenseStorm;
        internal static ConfigEntry<int> intenseStormDuration;

        private void Awake()
        {
            instance = this;
            logger = Logger;            

            generateEncounterMinTime = Config.Bind("Settings", "Minimum encounter generation time", 900, "Minimum time in seconds to get a chance roll for an encounter, the encounter time range max is 5 minutes added to this.");
            enableFlotsam = Config.Bind("Settings", "Enable flotsam encounters", true, "Enable flotsam encounters.");
            controlSeaLifeMod = Config.Bind("Settings", "Control SeaLifeMod spawns", true, "Use this mod to control SeaLifeMod spawns.");
            enableDenseFog = Config.Bind("Settings", "Enable dense fog encounters", true, "Enable dense fog encounters.");                        
            fogDuration = Config.Bind("Settings", "Fog encounter duration", 300, "In seconds, the amount of time the fog encounter lasts.");
            enableFishingBonanza = Config.Bind("Settings", "Enable fishing bonanza encounters", true, "Enable fishing bonanza encounters.");
            fishingBonanzaDuration = Config.Bind("Settings", "Fishing bonanza duration", 300, "In seconds, the amount of time the fishing bonanza encounter lasts.");
            enableIntenseStorm = Config.Bind("Settings", "Enable intense storm encounters", true, "Enable intense storm encounters.");
            intenseStormDuration = Config.Bind("Settings", "Intense storm duration", 300, "In seconds, the amount of time the intense storm encounter lasts.");

            harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PLUGIN_GUID);

            foreach (var plugin in Chainloader.PluginInfos)
            {
                var metadata = plugin.Value.Metadata;
                if (controlSeaLifeMod.Value && metadata.GUID.Equals(SEALIFEMOD_GUID))
                {
                    logger.LogInfo($"{SEALIFEMOD_GUID} found");
                    seaLifeModInstance = plugin.Value.Instance;
                    SeaLifeMod.PatchMod();
                }
                
                if (metadata.GUID.Equals(IDLEFISHING_GUID))
                {
                    logger.LogInfo($"{IDLEFISHING_GUID} found");
                    idleFishingFound = true;
                }
            }

            AssetLoader.LoadFlotsam();                        
            this.gameObject.AddComponent<EncounterGenerator>();            
        }
    }
}
