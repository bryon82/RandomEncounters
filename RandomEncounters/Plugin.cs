using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace RandomEncounters
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(SMH_GUID, SMH_VERSION)]
    [BepInDependency(SEALIFEMOD_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.raddude82.randomencounters";
        public const string PLUGIN_NAME = "RandomEncounters";
        public const string PLUGIN_VERSION = "1.0.1";

        public const string SMH_GUID = "com.app24.sailwindmoddinghelper";
        public const string SMH_VERSION = "2.0.3";

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
            //harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PLUGIN_GUID);

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

            StartCoroutine(GenerateEncounter());
        }        

        private IEnumerator GenerateEncounter()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(generateEncounterMinTime.Value, generateEncounterMinTime.Value + 300));
                if (GameState.currentBoat != null)
                    EncounterGenerator.Generate();
            }
        }

        //void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.P))
        //    {
        //        EncounterGenerator.Generate();
        //    }
        //}
    }
}
