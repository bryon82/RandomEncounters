using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RandomEncounters.RE_Plugin;

namespace RandomEncounters
{
    internal class SeaLifeMod
    {
        private static List<GameObject> _whaleSpawns;
        private static Type _finWhaleAIType;
        private static Type _effectControllerType;
        private static FastInvokeHandler _triggerRandomAnimation;
        private static Component _whale0Ai;
        private static AssetBundle _assetBundle;
        private static AudioClip[] _blowholeSounds;
        private static AudioClip[] _breachSplashSounds;
        private static AudioClip[] _breachEmergeSounds;
        private static AudioClip[] _tailSplashSounds;
        private static bool _allSoundsLoaded = false;

        internal static int ActiveWhales { get; set; } = 0;
        internal static bool WhalesReady {  get; set; }

        private static int _groupsCompleted = 0;
        private const int TOTAL_GROUPS = 4;
        private const float MAX_DISTANCE = 650f;

        public static void PatchMod()
        {
            // Stop SeaLifeMod from spawning whales
            SeaLifeModPluginInstance.StopAllCoroutines();

            _finWhaleAIType = AccessTools.TypeByName("FinWhaleAI");
            _effectControllerType = AccessTools.TypeByName("EffectController");

            // For triggering an animation
            var triggerRandomAnimationMethod = AccessTools.Method(_finWhaleAIType, "TriggerRandomAnimation");
            _triggerRandomAnimation = MethodInvoker.GetHandler(triggerRandomAnimationMethod);

            // Stop these from running
            var methodsToPatch = new string[] { "FindPlayer", "CheckDistanceToPlayer", "SetRandomScale" };
            foreach (var method in methodsToPatch) 
            {
                var originalMethod = AccessTools.Method(_finWhaleAIType, method);
                var patchMethod = AccessTools.Method(typeof(SeaLifeModPatches), "DoNotRun");
                HarmonyInstance.Patch(originalMethod, new HarmonyMethod(patchMethod));
            }

            // Stop whales from loading sounds when spawned
            var originalLoadAudio = AccessTools.Method(_effectControllerType, "LoadSounds");
            var patchLoadAudio = AccessTools.Method(typeof(SeaLifeModPatches), "DoNotRun");
            HarmonyInstance.Patch(originalLoadAudio, new HarmonyMethod(patchLoadAudio));

            // Load whales and sounds up front at startup
            Instance.StartCoroutine(LoadSoundsAsync());
            Instance.StartCoroutine(InstantiateWhales());
        }

        private static IEnumerator InstantiateWhales()
        {
            if (SeaLifeModPluginInstance == null)
                yield break;

            var whalePrefab = SeaLifeModPluginInstance.GetPrivateField<GameObject>("animalPrefab");
            _whaleSpawns = new List<GameObject>();

            yield return new WaitUntil(() => Refs.shiftingWorld != null && _allSoundsLoaded);

            for (int i = 0; i < 5; i++)
            {
                var whale = GameObject.Instantiate(whalePrefab, Refs.shiftingWorld);
                var ai = whale.AddComponent(_finWhaleAIType);
                if (i == 0) _whale0Ai = ai;
                var effectController = whale.AddComponent(_effectControllerType);
                effectController.SetPrivateField("blowholeSounds", _blowholeSounds);
                effectController.SetPrivateField("breachSplashSounds", _breachSplashSounds);
                effectController.SetPrivateField("breachEmergeSounds", _breachEmergeSounds);
                effectController.SetPrivateField("tailSplashSounds", _tailSplashSounds);
                whale.transform.position = Vector3.zero;
                whale.gameObject.SetActive(false);
                _whaleSpawns.Add(whale);
            }

            WhalesReady = true;
        }

        internal static void SpawnWhale(int i, Vector3 spawnPosition)
        {
            LogDebug("Spawning FinWhale");
            var scale = UnityEngine.Random.Range(0.8f, 1.4f);
            var rotationY = UnityEngine.Random.Range(0, 360);
            var whale = _whaleSpawns[i];
            var whaleTransform = whale.transform;
            whaleTransform.position = spawnPosition;
            whaleTransform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            whaleTransform.localScale = new Vector3(scale, scale, scale);
            whale.SetActive(true);
            ActiveWhales++;
        }

        internal static void TriggerEntranceAnimation()
        {
            _triggerRandomAnimation(_whale0Ai);
        }

        internal static void CheckWhaleDistance()
        {
            if (ActiveWhales == 0)
                return;

            foreach (var whale in _whaleSpawns)
            {
                var distance = Vector3.Distance(whale.transform.position, Refs.observerMirror.transform.position);
                if (whale.activeInHierarchy && distance > MAX_DISTANCE)
                {
                    LogDebug("Removing FinWhale");
                    whale.SetActive(false);
                    ActiveWhales--;
                }
            }
        }

        private static IEnumerator LoadSoundsAsync()
        {
            _groupsCompleted = 0;
            _allSoundsLoaded = false;
            _assetBundle = SeaLifeModPluginInstance.GetPrivateField<AssetBundle>("seaLifeBundle");

            Instance.StartCoroutine(LoadAudioClipsAsync("WhaleBlowMed", 6, clips => _blowholeSounds = clips));
            Instance.StartCoroutine(LoadAudioClipsAsync("BreachSplashLarge", 5, clips => _breachSplashSounds = clips));
            Instance.StartCoroutine(LoadAudioClipsAsync("BreachSplashSmall", 6, clips => _breachEmergeSounds = clips));
            Instance.StartCoroutine(LoadAudioClipsAsync("TailSplash", 4, clips => _tailSplashSounds = clips));

            while (_groupsCompleted < TOTAL_GROUPS)
            {
                yield return null;
            }

            _allSoundsLoaded = true;
        }

        private static IEnumerator LoadAudioClipsAsync(string baseName, int count, Action<AudioClip[]> onComplete)
        {
            var clips = new AudioClip[count];

            for (int i = 0; i < count; i++)
            {
                var clipName = string.Format("{0}{1:00}", baseName, i + 1);
                var request = _assetBundle.LoadAssetAsync<AudioClip>($"Assets/Audio/{clipName}.wav");
                yield return request;
                clips[i] = request.asset as AudioClip;
            }

            onComplete(clips);
            _groupsCompleted++;
        }
    }

    public class SeaLifeModPatches
    {
        [HarmonyPrefix]
        public static bool DoNotRun()
        {
            return false;
        }
    }
}
