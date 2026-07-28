using Crest;
using HarmonyLib;
using RandomEncounters.API;
using System.Collections;
using UnityEngine;
using static RandomEncounters.RE_Plugin;
using static RandomEncounters.Configs;

namespace RandomEncounters
{
    internal class FishingBonanzaEncounter : Encounter
    {
        private static Seagulls _seagulls;

        public override string Name => "Fishing Bonanza";
        public override int Weight => 15;
        public override bool IsAvailable() =>
            enableFishingBonanza.Value
            && GameState.playing
            && GameState.currentBoat
            && !IsActive
            && WeatherStorms.instance.InvokePrivateMethod<float>("GetNormalizedDistance") >= 0.75f;

        public override void Trigger() => Runner(Run());


        private IEnumerator Run()
        {
            TimeRemaining = TimeRemaining > 0f ? TimeRemaining : fishingBonanzaDuration.Value;

            if (_seagulls == null)
                _seagulls = Refs.shiftingWorld.GetComponentInChildren<Seagulls>(true);

            if (_seagulls == null)
            {
                LogDebug("No seagulls found");
                yield break;
            }

            var seagulls = GameObject.Instantiate(_seagulls.gameObject, Refs.shiftingWorld);
            if (!seagulls.activeInHierarchy)
                seagulls.SetActive(true);
            seagulls.GetComponent<AudioSource>().PlayOneShot(seagulls.GetComponent<AudioSource>().clip);
            var seagullsPS = seagulls.GetComponent<ParticleSystem>();

            var main = seagullsPS.main;
            main.maxParticles = 25;
            main.startLifetime = TimeRemaining;
            main.startRotation = 0f;
            main.startRotation3D = false;

            var rol = seagullsPS.rotationOverLifetime;
            rol.enabled = false;
            rol.x = 0f;
            rol.y = 0f;
            rol.z = 0f;

            var vol = seagullsPS.velocityOverLifetime;
            vol.enabled = true;
            vol.orbitalX = 0f;
            vol.orbitalY = 0f;
            vol.orbitalZ = 0f;
            vol.orbitalXMultiplier = 0;
            vol.orbitalYMultiplier = 0;
            vol.orbitalZMultiplier = 0;

            var rbs = seagullsPS.rotationBySpeed;
            rbs.enabled = false;

            var seagullPSR = seagulls.GetComponent<ParticleSystemRenderer>();
            seagullPSR.alignment = ParticleSystemRenderSpace.Local;

            var shape = seagullsPS.shape;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            shape.scale = new Vector3(25, 25, 0.2f);

            var emission = seagullsPS.emission;
            if (!emission.enabled)
                emission.enabled = true;

            LogDebug("Starting fishing bonanza");
            IsActive = true;

            var duration = TimeRemaining;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                TimeRemaining -= duration;

                var targetPosition = GameState.currentBoat.position + GameState.currentBoat.up * 40f;
                seagulls.transform.position = Vector3.Lerp(
                    seagulls.transform.position,
                    targetPosition,
                    0.2f * Time.deltaTime);

                var euler = seagulls.transform.eulerAngles;
                euler.y = Mathf.LerpAngle(
                    euler.y,
                    GameState.currentBoat.eulerAngles.y - 90f,
                    0.2f * Time.deltaTime);
                seagulls.transform.rotation = Quaternion.Euler(euler);                

                yield return null;
            }

            LogDebug("Stopping fishing bonanza");
            IsActive = false;
            GameObject.Destroy(seagulls);

            TimeRemaining = 0f;
            EncounterEvents.RaiseEncounterCompleted(this);
        }

        [HarmonyPatch(typeof(FishingRodFish))]
        [HarmonyPatch("Update")]
        private class FishingRodFishPatches
        {
            [HarmonyPostfix]
            public static void IncreaseCatchChance(
                FishingRodFish __instance,
                ShipItemFishingRod ___rod,
                SimpleFloatingObject ___floater,
                ConfigurableJoint ___bobberJoint,
                ref float ___fishTimer)
            {
                var notValid =
                    !___floater.InWater
                    || !EncounterRegistry.GetEncounterByName("FishingBonanza").IsActive
                    || __instance.currentFish != null
                    || ___rod.health <= 0f
                    || (!(bool)___rod.held && !IdleFishingPluginDetected && !HooksHangMorePluginDetected)                    
                    || ___bobberJoint.linearLimit.limit <= 1f
                    || __instance.gameObject.layer == 16;

                if (notValid)
                    return;

                ___fishTimer -= Time.deltaTime;
                float value = Vector3.Distance(__instance.transform.position, ___rod.transform.position);
                float num = Mathf.InverseLerp(3f, 20f, value) * 2.5f + 0.5f;
                if (___fishTimer <= 0f)
                {
                    ___fishTimer = 1f;
                    var multiplier = (bool)___rod.held ? 20f : 3f;
                    if (Random.Range(0f, 100f) < num * multiplier)
                    {
                        __instance.CatchFish();
                    }
                }
            }
        }
    }
}
