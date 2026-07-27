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

        public override void Trigger(MonoBehaviour host)
        {
            var flotsamEncounter = new FlotsamEncounter();
            flotsamEncounter.Trigger(host);
            var whalesEncounter = new WhalesEncounter();
            whalesEncounter.Trigger(host);
        }
    }
}
