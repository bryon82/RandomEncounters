using UnityEngine;
using RandomEncounters.API;
using static RandomEncounters.Configs;
using static RandomEncounters.RE_Plugin;

namespace RandomEncounters
{
    internal class FogAndWhalesEncounter : Encounter
    {
        public override string Name => "Dense Fog and Whales";
        public override int Weight => 5;
        public override bool IsAvailable() =>
            controlSeaLifeMod.Value
            && SeaLifeModPluginInstance != null
            && enableDenseFog.Value
            && !denseFogEncounter.IsActive
            && WeatherStorms.instance.InvokePrivateMethod<float>("GetNormalizedDistance") >= 0.75f;

        private readonly Encounter denseFogEncounter = EncounterRegistry.GetEncounterByName("Dense Fog");

        public override void Trigger()
        {
            denseFogEncounter.Trigger();
            var whalesEncounter = EncounterRegistry.GetEncounterByName("Whales");
            whalesEncounter.Trigger();
        }
    }
}
