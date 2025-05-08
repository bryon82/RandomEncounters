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
        private static List<GameObject> s_whaleSpawns;
        private static Type s_finWhaleAIType;
        private static Type s_effectControllerType;
        private static FastInvokeHandler s_triggerRandomAnimation;
        private static int s_activeWhales = 0;
        private static Component s_whale0Ai;
        private static AssetBundle s_assetBundle;
        private static AudioClip[] s_blowholeSounds;
        private static AudioClip[] s_breachSplashSounds;
        private static AudioClip[] s_breachEmergeSounds;
        private static AudioClip[] s_tailSplashSounds;
        private static int s_soundsToLoad = 0;
        private static int s_soundsLoaded = 0;
        private static bool s_allSoundsLoaded = false;

        private const float MAX_DISTANCE = 650f;

        public static void PatchMod()
        {
            // Stop SeaLifeMod from spawning whales
            SeaLifeModPluginInstance.StopAllCoroutines();

            s_finWhaleAIType = AccessTools.TypeByName("FinWhaleAI");
            s_effectControllerType = AccessTools.TypeByName("EffectController");

            // For triggering an animation
            var triggerRandomAnimationMethod = AccessTools.Method(s_finWhaleAIType, "TriggerRandomAnimation");
            s_triggerRandomAnimation = MethodInvoker.GetHandler(triggerRandomAnimationMethod);

            // Stop these from running
            var methodsToPatch = new string[] { "FindPlayer", "CheckDistanceToPlayer", "SetRandomScale" };
            foreach (var method in methodsToPatch) 
            {
                var originalMethod = AccessTools.Method(s_finWhaleAIType, method);
                var patchMethod = AccessTools.Method(typeof(SeaLifeModPatches), "DoNotRun");
                HarmonyInstance.Patch(originalMethod, new HarmonyMethod(patchMethod));
            }

            // Stop whales from loading sounds when spawned
            var originalLoadAudio = AccessTools.Method(s_effectControllerType, "LoadSounds");
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
            s_whaleSpawns = new List<GameObject>();

            yield return new WaitUntil(() => Refs.shiftingWorld != null && s_allSoundsLoaded);

            for (int i = 0; i < 5; i++)
            {
                var whale = GameObject.Instantiate(whalePrefab, Refs.shiftingWorld.transform);
                var ai = whale.AddComponent(s_finWhaleAIType);
                if (i == 0) s_whale0Ai = ai;                
                var effectController = whale.AddComponent(s_effectControllerType);
                effectController.SetPrivateField("blowholeSounds", s_blowholeSounds);
                effectController.SetPrivateField("breachSplashSounds", s_breachSplashSounds);
                effectController.SetPrivateField("breachEmergeSounds", s_breachEmergeSounds);
                effectController.SetPrivateField("tailSplashSounds", s_tailSplashSounds);
                whale.transform.position = Vector3.zero;
                whale.gameObject.SetActive(false);
                s_whaleSpawns.Add(whale);                
            }
        }

        internal static void SpawnWhale(int i, Vector3 spawnPosition)
        {
            LogDebug("Spawning FinWhale");
            var scale = UnityEngine.Random.Range(0.8f, 1.4f);
            var rotationY = UnityEngine.Random.Range(0, 360);
            var whale = s_whaleSpawns[i];
            var whaleTransform = whale.transform;
            whaleTransform.position = spawnPosition;
            whaleTransform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            whaleTransform.localScale = new Vector3(scale, scale, scale);
            whale.SetActive(true);
            s_activeWhales++;
        }

        internal static void TriggerEntranceAnimation()
        {
            s_triggerRandomAnimation(s_whale0Ai);
        }

        internal static void CheckWhaleDistance()
        {
            if (s_activeWhales == 0)
                return;

            foreach (var whale in s_whaleSpawns)
            {
                var distance = Vector3.Distance(whale.transform.position, Refs.observerMirror.transform.position);
                if (whale.activeInHierarchy && distance > MAX_DISTANCE)
                {
                    LogDebug("Removing FinWhale");
                    whale.SetActive(false);
                    s_activeWhales--;
                }
            }
        }        

        private static IEnumerator LoadSoundsAsync()
        {
            s_soundsToLoad = 0;
            s_soundsLoaded = 0;
            s_assetBundle = SeaLifeModPluginInstance.GetPrivateField<AssetBundle>("seaLifeBundle");

            Instance.StartCoroutine(LoadAudioClipsAsync("WhaleBlowMed", 6, clips => s_blowholeSounds = clips));
            Instance.StartCoroutine(LoadAudioClipsAsync("BreachSplashLarge", 5, clips => s_breachSplashSounds = clips));
            Instance.StartCoroutine(LoadAudioClipsAsync("BreachSplashSmall", 6, clips => s_breachEmergeSounds = clips));
            Instance.StartCoroutine(LoadAudioClipsAsync("TailSplash", 4, clips => s_tailSplashSounds = clips));            

            while (s_soundsLoaded < s_soundsToLoad)
            {
                yield return null;
            }

            s_allSoundsLoaded = true;
        }

        private static IEnumerator LoadAudioClipsAsync(string baseName, int count, Action<AudioClip[]> onComplete)
        {
            s_soundsToLoad += count;
            AudioClip[] clips = new AudioClip[count];
           
            for (int i = 0; i < count; i++)
            {
                var clipName = string.Format("{0}{1:00}", baseName, i + 1);                
                AssetBundleRequest request = s_assetBundle.LoadAssetAsync<AudioClip>($"Assets/Audio/{clipName}.wav");
                yield return request;
                clips[i] = request.asset as AudioClip;
                s_soundsLoaded++;
            }

            while (s_soundsLoaded < s_soundsToLoad)
            {
                yield return null;
            }

            onComplete(clips);
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
