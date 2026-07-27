using HarmonyLib;
using RandomEncounters.API;
using System.Collections;
using UnityEngine;
using static RandomEncounters.RE_Plugin;
using static RandomEncounters.Configs;

namespace RandomEncounters
{
    internal class IntenseStormEncounter : Encounter
    {
        public override string Name => "Intense Storm";
        public override int Weight => 5;
        public override bool IsAvailable() => enableIntenseStorm.Value && !_isRunning;
        public override void Trigger(MonoBehaviour host) => host.StartCoroutine(Run(this));

        private static OceanUpdaterCrest _oceanUpdaterCrest;
        private static bool _isRunning;

        internal static IEnumerator Run(Encounter enc)
        {
            _isRunning = true;
            var weatherStorms = WeatherStorms.instance;
            var storm = weatherStorms.GetCurrentStorm();
            var lightning = storm.transform.GetChild(3).GetComponent<WanderingStormLightning>();
            var targetRegion = RegionBlender.instance.GetPrivateField<Region>("currentTargetRegion");

            var origInertiaWindScale = _oceanUpdaterCrest.inertiaWindScale;
            var origWindSpeedMult = _oceanUpdaterCrest.GetPrivateField<float>("windSpeedMult");
            var origSmallWavesMult = _oceanUpdaterCrest.GetPrivateField<float>("smallWavesMult");
            var origLightningInterval = lightning.GetPrivateField<float>("lightningInterval");
            var origRainDensity = targetRegion.stormWeather.particles.rainDensity;

            lightning.SetPrivateField("lightningInterval", 5f);
            targetRegion.stormWeather.particles.rainDensity = 70f;

            var stormDist = Vector3.Distance(Camera.main.transform.position, storm.transform.position);
            var vector = Camera.main.transform.position - storm.transform.position;
            vector.y = 0f;

            LogDebug($"{storm.name} approaching");
            while (stormDist > 1500f)
            {
                vector = Camera.main.transform.position - storm.transform.position;
                vector.y = 0f;
                Wind.currentBaseWind = vector * 50f;
                var translateSpeed = weatherStorms.InvokePrivateMethod<float>("GetNormalizedDistance") < 0.66 ? 0.005f : 0.25f;
                storm.transform.Translate(vector * translateSpeed);
                yield return new WaitForSeconds(0.1f);
                stormDist = Vector3.Distance(Camera.main.transform.position, storm.transform.position);
            }

            LogDebug($"{storm.name} arrived");
            _oceanUpdaterCrest.inertiaWindScale = 0.22f;
            _oceanUpdaterCrest.SetPrivateField("windSpeedMult", 5f);
            _oceanUpdaterCrest.SetPrivateField("smallWavesMult", 0.4f);
            for (int i = 0; i < intenseStormDuration.Value; i++)
            {
                Wind.currentBaseWind = vector * 50f;
                yield return new WaitForSeconds(1f);
            }

            LogDebug($"{storm.name} dying down");
            lightning.SetPrivateField("lightningInterval", origLightningInterval);
            Weather.instance.currentRegion.stormWeather.particles.rainDensity = origRainDensity;
            _oceanUpdaterCrest.inertiaWindScale = origInertiaWindScale;
            _oceanUpdaterCrest.SetPrivateField("windSpeedMult", origWindSpeedMult);
            _oceanUpdaterCrest.SetPrivateField("smallWavesMult", origSmallWavesMult);
            _isRunning = false;

            EncounterEvents.RaiseEncounterCompleted(enc);
        }


        [HarmonyPatch(typeof(OceanUpdaterCrest))]
        static class OceanUpdaterCrestPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            public static void Awake(OceanUpdaterCrest __instance)
            {
                _oceanUpdaterCrest = __instance;                    
            }
        }
    }
}

