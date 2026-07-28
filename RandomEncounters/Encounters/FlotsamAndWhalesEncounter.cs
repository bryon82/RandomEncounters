using UnityEngine;
using RandomEncounters.API;
using static RandomEncounters.Configs;
using static RandomEncounters.RE_Plugin;

namespace RandomEncounters
{
    internal class FlotsamAndWhalesEncounter : Encounter
    {
        public override string Name => "Flotsam and Whales";
        public override int Weight => 10;
        public override bool IsAvailable() =>
            controlSeaLifeMod.Value && SeaLifeModPluginInstance != null && enableFlotsam.Value;

        public override void Trigger()
        {
            var flotsamEncounter = EncounterRegistry.GetEncounterByName("Flotsam");
            flotsamEncounter?.Trigger();
            var whalesEncounter = EncounterRegistry.GetEncounterByName("Whales");
            whalesEncounter?.Trigger();
        }
    }
}
