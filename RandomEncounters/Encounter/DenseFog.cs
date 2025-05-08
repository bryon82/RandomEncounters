using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RandomEncounters.RE_Plugin;

namespace RandomEncounters
{
    internal class DenseFog
    {
        public static bool IsRunning { get; private set; } = false;
        public static Dictionary<AudioSource, float> WaveAudioSources { get; private set; } = new Dictionary<AudioSource, float>();
        public static (AudioSource source, float origVolume) WindAudioSource { get; private set; } = (null, 0f);

        private static bool s_clearFog = true;        
        private static float s_currentFogDensity = 0f;
        private static float s_originalFogDensity = 0f;

        private const float MAX_FOG_DENSITY = 0.06f;        

        [HarmonyPatch(typeof(OceanColorBlender))]
        private class OceanColorBlenderPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("ApplyPalette")]
            public static void ApplyFogDensity(ref OceanColorPalette palette)
            {
                if (!IsRunning) 
                    return;

                s_originalFogDensity = s_originalFogDensity == 0f ? palette.fogDensity : s_originalFogDensity;
                s_currentFogDensity = s_currentFogDensity == 0f ? palette.fogDensity : s_currentFogDensity;

                if (s_clearFog && s_currentFogDensity > s_originalFogDensity) s_currentFogDensity -= 0.00001f;
                if (!s_clearFog && s_currentFogDensity < MAX_FOG_DENSITY) s_currentFogDensity += 0.00001f;
                palette.fogDensity = s_currentFogDensity;

                if (s_clearFog && s_currentFogDensity <= s_originalFogDensity)
                {
                    IsRunning = false;
                    s_currentFogDensity = 0f;
                    s_originalFogDensity = 0f;
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
                if (!IsRunning)
                    return true;

                ___currentGustTarget = ___currentWindTarget;
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("SetNewWindTarget")]
            public static bool LightWind(ref Vector3 ___currentWindTarget)
            {
                if (!IsRunning)
                    return true;

                ___currentWindTarget = Wind.currentBaseWind.normalized * 3f;
                return false;
            }
        }

        [HarmonyPatch(typeof(WaveSound))]
        private class WaveSoundPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("UpdateIntensity")]
            public static bool SetToMinVolume()
            {
                if (!IsRunning)
                    return true;

                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void GetAudioSource(AudioSource ___audio)
            {
                WaveAudioSources.Add(___audio, ___audio.volume);
            }
        }


        [HarmonyPatch(typeof(WindSound))]
        private class WindSoundPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("Update")]
            public static bool SetToMinVolume()
            {
                if (!IsRunning)
                    return true;

                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void GetAudioSource(AudioSource ___audio)
            {
                WindAudioSource = (___audio, ___audio.volume);
            }
        }

        public static void Spawn()
        {
            LogDebug($"Spawning fog");
            foreach (var source in WaveAudioSources.Keys.ToList())
            {
                WaveAudioSources[source] = source.volume;
            }
            WindAudioSource = (WindAudioSource.source, WindAudioSource.source.volume);
            s_clearFog = false;
            IsRunning = true;
        }

        public static void ClearFog()
        {
            LogDebug($"Clearing fog");
            s_clearFog = true;
        }
    }
}
