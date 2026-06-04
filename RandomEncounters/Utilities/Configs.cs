using BepInEx.Configuration;

namespace RandomEncounters
{
    internal class Configs
    {
        internal static ConfigEntry<int> encounterRollMaxIncrease;
        internal static ConfigEntry<int> generateEncounterMinTime;
        internal static ConfigEntry<int> generateEncounterTimeRange;
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

            encounterRollMaxIncrease = config.Bind(
                "Settings",
                "Encounter roll max increase",
                0,
                "Default chance for an encounter to happen is 60/100. Increasing this increases the bottom number, so your chances of an encounter decrease.");
            generateEncounterMinTime = config.Bind(
                "Settings",
                "Minimum encounter chance time",
                900,
                "Minimum time in seconds to get a chance roll for an encounter, a random amount of time in the configured time range will be added to this.");
            generateEncounterTimeRange = config.Bind(
                "Settings",
                "Time range for encounter chance",
                300,
                "Time range in seconds after minimum time when an encounter chance will happen.");
            enableFlotsam = config.Bind(
                "Settings",
                "Enable flotsam encounters",
                true,
                "Enable flotsam encounters.");
            controlSeaLifeMod = config.Bind(
                "Settings",
                "Control SeaLifeMod spawns",
                true,
                "Use this mod to control SeaLifeMod spawns.");
            enableDenseFog = config.Bind(
                "Settings",
                "Enable dense fog encounters",
                true,
                "Enable dense fog encounters.");
            fogDuration = config.Bind(
                "Settings",
                "Fog encounter duration",
                300,
                "In seconds, the amount of time the fog encounter lasts.");
            enableFishingBonanza = config.Bind(
                "Settings",
                "Enable fishing bonanza encounters",
                true,
                "Enable fishing bonanza encounters.");
            fishingBonanzaDuration = config.Bind(
                "Settings",
                "Fishing bonanza duration",
                300,
                "In seconds, the amount of time the fishing bonanza encounter lasts.");
            enableIntenseStorm = config.Bind(
                "Settings",
                "Enable intense storm encounters",
                true,
                "Enable intense storm encounters.");
            intenseStormDuration = config.Bind(
                "Settings",
                "Intense storm duration",
                300,
                "In seconds, the amount of time the intense storm encounter lasts.");
        }
    }
}
