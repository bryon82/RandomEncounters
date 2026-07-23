using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RandomEncounters.RE_Plugin;
using static RandomEncounters.Configs;

namespace RandomEncounters
{
    internal class DenseFog
    {
        private static bool _isRunning;
        private static readonly Dictionary<AudioSource, float> _waveAudioSources = new Dictionary<AudioSource, float>();
        private static (AudioSource source, float origVolume) _windAudioSource;
        private static bool _clearFog = true;
        private static float _currentFogDensity = 0f;
        private static float _originalFogDensity = 0f;

        private const float MAX_FOG_DENSITY = 0.06f;

        internal static IEnumerator Run()
        {
            if (_isRunning || WeatherStorms.instance.InvokePrivateMethod<float>("GetNormalizedDistance") < 0.75f)
                yield break;

            Spawn();
            var waveAudioSources = _waveAudioSources.Keys.ToList();
            var windAudioSource = _windAudioSource.source;
            var windOrigVolume = _windAudioSource.origVolume;
            const float fadeOutDuration = 4f;
            for (var t = 0f; t < fadeOutDuration; t += Time.deltaTime)
            {
                var lerpValue = t / fadeOutDuration;
                foreach (var audioSource in waveAudioSources)
                {
                    audioSource.volume = Mathf.Lerp(_waveAudioSources[audioSource], 0f, lerpValue);
                }
                if (windAudioSource != null)
                {
                    windAudioSource.volume = Mathf.Lerp(windOrigVolume, 0.0001f, lerpValue);
                }
                yield return null;
            }

            for (int i = 0; i < 4; i++)
            {
                var spawnPoint =
                    GameState.currentBoat.position +
                    GameState.currentBoat.right * (200f + Random.Range(20f, 60f) * i) +
                    GameState.currentBoat.forward * Random.Range(-200, 200);

                Flotsam.SpawnItem(spawnPoint, Random.Range(1, 100) > 50 ? AssetLoader.SmallWreck : AssetLoader.Hull, 1f, true);
                yield return new WaitForSeconds(1f);
            }

            yield return new WaitForSeconds(fogDuration.Value);

            ClearFog();
            const float fadeInDuration = 4f;
            for (var t = 0f; t < fadeInDuration; t += Time.deltaTime)
            {
                var lerpValue = t / fadeInDuration;
                foreach (var audioSource in waveAudioSources)
                {
                    audioSource.volume = Mathf.Lerp(0f, _waveAudioSources[audioSource], lerpValue);
                }
                if (windAudioSource != null)
                {
                    windAudioSource.volume = Mathf.Lerp(0.0001f, windOrigVolume, lerpValue);
                }
                yield return null;
            }
        }

        private static void Spawn()
        {
            LogDebug($"Spawning fog");
            foreach (var source in _waveAudioSources.Keys.ToList())
            {
                _waveAudioSources[source] = source.volume;
            }
            if (_windAudioSource.source != null)
            {
                _windAudioSource = (_windAudioSource.source, _windAudioSource.source.volume);
            }
            _clearFog = false;
            _isRunning = true;
        }

        private static void ClearFog()
        {
            LogDebug($"Clearing fog");
            _clearFog = true;
        }


        [HarmonyPatch(typeof(OceanColorBlender))]
        private class OceanColorBlenderPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("ApplyPalette")]
            public static void ApplyFogDensity(ref OceanColorPalette palette)
            {
                if (!_isRunning) 
                    return;

                _originalFogDensity = _originalFogDensity == 0f ? palette.fogDensity : _originalFogDensity;
                _currentFogDensity = _currentFogDensity == 0f ? palette.fogDensity : _currentFogDensity;

                if (_clearFog && _currentFogDensity > _originalFogDensity) _currentFogDensity -= 0.00001f;
                if (!_clearFog && _currentFogDensity < MAX_FOG_DENSITY) _currentFogDensity += 0.00001f;
                palette.fogDensity = _currentFogDensity;

                if (_clearFog && _currentFogDensity <= _originalFogDensity)
                {
                    _isRunning = false;
                    _currentFogDensity = 0f;
                    _originalFogDensity = 0f;
                    GameObject.Find("wind").GetComponent<Wind>().SetPrivateField("timer", 0);
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
                if (!_isRunning)
                    return true;

                ___currentGustTarget = ___currentWindTarget;
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("SetNewWindTarget")]
            public static bool LightWind(ref Vector3 ___currentWindTarget)
            {
                if (!_isRunning)
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
                if (!_isRunning)
                    return true;

                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void GetAudioSource(AudioSource ___audio)
            {
                if (!_waveAudioSources.ContainsKey(___audio))
                {
                    _waveAudioSources.Add(___audio, ___audio.volume);
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
                if (!_isRunning)
                    return true;

                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void GetAudioSource(AudioSource ___audio)
            {
                _windAudioSource = (___audio, ___audio.volume);
            }
        }        
    }
}
