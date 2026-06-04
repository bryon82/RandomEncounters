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

        private static bool _clearFog = true;
        private static float _currentFogDensity = 0f;
        private static float _originalFogDensity = 0f;

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

                _originalFogDensity = _originalFogDensity == 0f ? palette.fogDensity : _originalFogDensity;
                _currentFogDensity = _currentFogDensity == 0f ? palette.fogDensity : _currentFogDensity;

                if (_clearFog && _currentFogDensity > _originalFogDensity) _currentFogDensity -= 0.00001f;
                if (!_clearFog && _currentFogDensity < MAX_FOG_DENSITY) _currentFogDensity += 0.00001f;
                palette.fogDensity = _currentFogDensity;

                if (_clearFog && _currentFogDensity <= _originalFogDensity)
                {
                    IsRunning = false;
                    _currentFogDensity = 0f;
                    _originalFogDensity = 0f;
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
                if (!WaveAudioSources.ContainsKey(___audio))
                {
                    WaveAudioSources.Add(___audio, ___audio.volume);
                }
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
            if (WindAudioSource.source != null)
            {
                WindAudioSource = (WindAudioSource.source, WindAudioSource.source.volume);
            }
            _clearFog = false;
            IsRunning = true;
        }

        public static void ClearFog()
        {
            LogDebug($"Clearing fog");
            _clearFog = true;
        }
    }
}
