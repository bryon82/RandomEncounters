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
            && !DenseFogEncounter.isRunning
            && WeatherStorms.instance.InvokePrivateMethod<float>("GetNormalizedDistance") >= 0.75f;

        public override void Trigger(MonoBehaviour host)
        {
            var denseFogEncounter = new DenseFogEncounter();
            denseFogEncounter.Trigger(host);
            var whalesEncounter = new WhalesEncounter();
            whalesEncounter.Trigger(host);
        }
    }
}
