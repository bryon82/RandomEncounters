using Crest;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using static RandomEncounters.RE_Plugin;
using static RandomEncounters.Configs;

namespace RandomEncounters
{
    internal class FishingBonanza
    {
        private static bool _isBonanzaActive;
        private static Seagulls _seagulls;
        private static bool _moveSeagulls;

        internal static IEnumerator Run(MonoBehaviour instance)
        {
            if (GameState.currentBoat == null || _isBonanzaActive)
                yield break;

            var stormDistance = WeatherStorms.instance.InvokePrivateMethod<float>("GetNormalizedDistance");
            if (stormDistance < 0.75f)
            {
                LogDebug($"Storm too close for fishing bonanza {stormDistance}");
                yield break;
            }

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
            main.startLifetime = fishingBonanzaDuration.Value;
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
            _isBonanzaActive = true;
            _moveSeagulls = true;
            instance.StartCoroutine(MoveSeagulls(seagulls.transform, GameState.currentBoat));
            yield return new WaitForSeconds(fishingBonanzaDuration.Value);

            LogDebug("Stopping fishing bonanza");
            _isBonanzaActive = false;
            _moveSeagulls = false;
            GameObject.Destroy(seagulls);
        }

        private static IEnumerator MoveSeagulls(Transform seagulls, Transform boat)
        {
            while (_moveSeagulls)
            {
                var targetPosition = boat.position + boat.up * 40f;
                seagulls.position = Vector3.Lerp(seagulls.position, targetPosition, 0.2f * Time.deltaTime);

                var newRotation = seagulls.eulerAngles;
                newRotation.y = Mathf.LerpAngle(seagulls.eulerAngles.y, boat.eulerAngles.y - 90, 0.2f * Time.deltaTime);
                seagulls.rotation = Quaternion.Euler(newRotation);

                yield return null;
            }
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
                    !_isBonanzaActive
                    || __instance.currentFish != null
                    || ___rod.health <= 0f
                    || (!(bool)___rod.held && !IdleFishingPluginDetected && !HooksHangMorePluginDetected)
                    || !___floater.InWater
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
