using BepInEx.Configuration;

namespace RandomEncounters
{
    internal class Configs
    {
        internal static ConfigEntry<int> generateEncounterMinTime;
        internal static ConfigEntry<bool> enableFlotsam;
        internal static ConfigEntry<bool> controlSeaLifeMod;
        internal static ConfigEntry<bool> enableDenseFog;
        internal static ConfigEntry<int> fogDuration;
        internal static ConfigEntry<bool> enableFishingBonanza;
        internal static ConfigEntry<int> fishingBonanzaDuration;
        internal static ConfigEntry<bool> enableIntenseStorm;
        internal static ConfigEntry<int> intenseStormDuration;

        internal static void InitializeConfigs()
        {
            var config = RE_Plugin.Instance.Config;

            generateEncounterMinTime = config.Bind("Settings", "Minimum encounter generation time", 900, "Minimum time in seconds to get a chance roll for an encounter, the encounter time range max is 5 minutes added to this.");
            enableFlotsam = config.Bind("Settings", "Enable flotsam encounters", true, "Enable flotsam encounters.");
            controlSeaLifeMod = config.Bind("Settings", "Control SeaLifeMod spawns", true, "Use this mod to control SeaLifeMod spawns.");
            enableDenseFog = config.Bind("Settings", "Enable dense fog encounters", true, "Enable dense fog encounters.");
            fogDuration = config.Bind("Settings", "Fog encounter duration", 300, "In seconds, the amount of time the fog encounter lasts.");
            enableFishingBonanza = config.Bind("Settings", "Enable fishing bonanza encounters", true, "Enable fishing bonanza encounters.");
            fishingBonanzaDuration = config.Bind("Settings", "Fishing bonanza duration", 300, "In seconds, the amount of time the fishing bonanza encounter lasts.");
            enableIntenseStorm = config.Bind("Settings", "Enable intense storm encounters", true, "Enable intense storm encounters.");
            intenseStormDuration = config.Bind("Settings", "Intense storm duration", 300, "In seconds, the amount of time the intense storm encounter lasts.");
        }
    }
}
