using BepInEx.Configuration;

namespace RandomEncounters
{
    internal class Configs
    {
        internal static ConfigEntry<int> encounterRollChance;
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

            encounterRollChance = config.Bind(
                "Encounter Generation Settings",
                "Chance an encounter occurs",
                60,
                new ConfigDescription(
                    "Percent chance an encounter occurs.",
                    new AcceptableValueRange<int>(0, 100)));
            generateEncounterMinTime = config.Bind(
                "Encounter Generation Settings",
                "Minimum encounter chance time",
                900,
                "Minimum time in seconds to get a chance roll for an encounter, a random amount of time from the 'Variation in encounter chance time' setting will be added to this.");
            generateEncounterTimeRange = config.Bind(
                "Encounter Generation Settings",
                "Variation in encounter chance time",
                300,
                "A random number of seconds from 0 up to the value specified will be added to the 'Minimum encounter chance time' setting.");
            enableFlotsam = config.Bind(
                "Encounter Types",
                "Enable flotsam encounters",
                true);
            controlSeaLifeMod = config.Bind(
                "Encounter Types",
                "Control SeaLifeMod spawns",
                true,
                "Use this mod to control SeaLifeMod spawns. <b>Requires restart to take effect.</b>");
            enableDenseFog = config.Bind(
                "Encounter Types",
                "Enable dense fog encounters",
                true);
            enableFishingBonanza = config.Bind(
                "Encounter Types",
                "Enable fishing bonanza encounters",
                true);
            enableIntenseStorm = config.Bind(
                "Encounter Types",
                "Enable intense storm encounters",
                true);
            fogDuration = config.Bind(
                "Encounter Settings",
                "Fog encounter duration",
                300,
                "In seconds, the amount of time the fog encounter lasts.");
            fishingBonanzaDuration = config.Bind(
                "Encounter Settings",
                "Fishing bonanza duration",
                300,
                "In seconds, the amount of time the fishing bonanza encounter lasts.");
            intenseStormDuration = config.Bind(
                "Encounter Settings",
                "Intense storm duration",
                300,
                "In seconds, the amount of time the intense storm encounter lasts.");
        }
    }
}
