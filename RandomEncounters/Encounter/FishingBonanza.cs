using Crest;
using HarmonyLib;
using UnityEngine;
using static RandomEncounters.RE_Plugin;

namespace RandomEncounters
{
    internal class FishingBonanza
    {
        public static bool IsBonanzaActive { get; set; } = false;

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
                if (!IsBonanzaActive ||
                    __instance.currentFish != null ||
                    ___rod.health <= 0f ||
                    (!(bool)___rod.held && !IdleFishingPluginDetected) ||
                    !___floater.InWater ||
                    ___bobberJoint.linearLimit.limit <= 1f ||
                    __instance.gameObject.layer == 16)
                {
                    return;
                }

                ___fishTimer -= Time.deltaTime;
                float value = Vector3.Distance(__instance.transform.position, ___rod.transform.position);
                float num = Mathf.InverseLerp(3f, 20f, value) * 2.5f + 0.5f;
                if (___fishTimer <= 0f)
                {
                    ___fishTimer = 1f;
                    var multiplier = (bool)___rod.held ? 20f : 2f;
                    if (Random.Range(0f, 100f) < num * multiplier)
                    {
                        __instance.CatchFish();
                    }
                }
            }
        }
    }
}
