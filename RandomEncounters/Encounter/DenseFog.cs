using HarmonyLib;
using UnityEngine;

namespace RandomEncounters
{
    internal class DenseFog
    {
        internal static bool running = false;
        private static bool clearFog = true;
        private static readonly float fogDensityMax = 0.06f;
        private static float currentFogDensity = 0f;
        private static float originalFogDensity = 0f;

        [HarmonyPatch(typeof(OceanColorBlender))]
        private class OceanColorBlenderPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("ApplyPalette")]
            public static void ApplyFogDensity(ref OceanColorPalette palette)
            {
                if (!running) return;
                originalFogDensity = originalFogDensity == 0f ? palette.fogDensity : originalFogDensity;
                currentFogDensity = currentFogDensity == 0f ? palette.fogDensity : currentFogDensity;

                if (clearFog && currentFogDensity > originalFogDensity) currentFogDensity -= 0.00001f;
                if (!clearFog && currentFogDensity < fogDensityMax) currentFogDensity += 0.00001f;
                palette.fogDensity = currentFogDensity;

                if (clearFog && currentFogDensity <= originalFogDensity)
                {
                    running = false;
                    currentFogDensity = 0f;
                    originalFogDensity = 0f;
                    Traverse.Create(GameObject.Find("wind").GetComponent<Wind>()).Field("timer").SetValue(0);
                }                
            }
        }

        [HarmonyPatch(typeof(Wind))]
        private class WindPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("SetNewGustTarget")]
            public static bool NoGust(ref Vector3 ___currentGustTarget, Vector3 ___currentWindTarget)
            {
                if (!running) return true;
                ___currentGustTarget = ___currentWindTarget;
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("SetNewWindTarget")]
            public static bool LightWind(ref Vector3 ___currentWindTarget)
            {
                if (!running) return true;
                ___currentWindTarget = Wind.currentBaseWind.normalized * 3f;
                return false;
            }
        }

        [HarmonyPatch(typeof(WaveSound))]
        private class WaveSoundPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("UpdateIntensity")]
            public static bool SetToMinVolume(ref float ___audioVolume, float ___minVolume)
            {
                if (!running) return true;
                ___audioVolume = ___minVolume;
                return false;
            }
        }

        [HarmonyPatch(typeof(WindSound))]
        private class WindSoundPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("Update")]
            public static bool SetToMinVolume(ref AudioSource ___audio)
            {
                if (!running) return true;
                ___audio.volume = 0.0001f;
                return false;
            }
        }

        public static void Spawn()
        {
            Plugin.logger.LogDebug($"Spawning fog");
            clearFog = false;
            running = true;
        }

        public static void ClearFog()
        {
            Plugin.logger.LogDebug($"Clearing fog");
            clearFog = true;
        }
    }
}
