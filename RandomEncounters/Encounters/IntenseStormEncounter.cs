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
        public override bool IsAvailable => enableIntenseStorm.Value && !IsActive;
        public override void Trigger() => Runner(Run());

        private static OceanUpdaterCrest _oceanUpdaterCrest;

        private IEnumerator Run()
        {
            IsActive = true;
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
                var translateSpeed = weatherStorms.InvokePrivateMethod<float>("GetNormalizedDistance") < 0.66 ? 0.0025f : 0.125f;
                storm.transform.Translate(vector * translateSpeed);
                yield return new WaitForSeconds(0.05f);
                stormDist = Vector3.Distance(Camera.main.transform.position, storm.transform.position);
            }

            LogDebug($"{storm.name} arrived");
            _oceanUpdaterCrest.inertiaWindScale = 0.22f;
            _oceanUpdaterCrest.SetPrivateField("windSpeedMult", 5f);
            _oceanUpdaterCrest.SetPrivateField("smallWavesMult", 0.4f);

            var duration = TimeRemaining > 0f ? TimeRemaining : intenseStormDuration.Value;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                TimeRemaining = duration - elapsed;

                Wind.currentBaseWind = vector * 50f;

                yield return null;
            }

            LogDebug($"{storm.name} dying down");
            lightning.SetPrivateField("lightningInterval", origLightningInterval);
            Weather.instance.currentRegion.stormWeather.particles.rainDensity = origRainDensity;
            _oceanUpdaterCrest.inertiaWindScale = origInertiaWindScale;
            _oceanUpdaterCrest.SetPrivateField("windSpeedMult", origWindSpeedMult);
            _oceanUpdaterCrest.SetPrivateField("smallWavesMult", origSmallWavesMult);
            
            IsActive = false;
            TimeRemaining = 0f;
            EncounterEvents.RaiseEncounterCompleted(this);
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

