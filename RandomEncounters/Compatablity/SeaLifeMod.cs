using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace RandomEncounters
{
    internal class SeaLifeMod
    {
        public static FastInvokeHandler spawnWhale;
        public static FastInvokeHandler triggerRandomAnimation;        

        public static void PatchMod()
        {
            // Stops SeaLifePlugin from spawning whales
            Plugin.seaLifeModInstance.StopAllCoroutines();

            // Sets up ability to spawn whales
            Type seaLifePluginClass = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(type => type.IsClass && type.Name == "SeaLifePlugin")
                .Single();            
            var spawnWhaleMethod = AccessTools.Method(seaLifePluginClass, "SpawnWhale");
            spawnWhale = MethodInvoker.GetHandler(spawnWhaleMethod);
            
            // Patch to destroy whale GameObjects when far enough away
            Type finWhaleAIClass = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(type => type.IsClass && type.Name == "FinWhaleAI")
                .Single();
            var cdpOriginal = AccessTools.Method(finWhaleAIClass, "CheckDistanceToPlayer");
            var cdpPatch = AccessTools.Method(typeof(FinWhaleAIPatches), "CheckDistanceToPlayerPatch");
            Plugin.harmony.Patch(cdpOriginal, new HarmonyMethod(cdpPatch));

            // For triggering an animation on spawn
            var triggerRandomAnimationMethod = AccessTools.Method(finWhaleAIClass, "TriggerRandomAnimation");
            triggerRandomAnimation = MethodInvoker.GetHandler(triggerRandomAnimationMethod);            
        }

        public class FinWhaleAIPatches
        {
            [HarmonyPrefix]
            public static bool CheckDistanceToPlayerPatch()
            {
                for (int i = 0; i < EncounterGenerator.whaleSpawns.Count; i++) 
                {
                    if (Vector3.Distance(EncounterGenerator.whaleSpawns[i].position, Refs.observerMirror.transform.position) > 650f)
                    {
                        Plugin.logger.LogDebug($"Destroying {EncounterGenerator.whaleSpawns[i].name}");
                        UnityEngine.Object.Destroy(EncounterGenerator.whaleSpawns[i].gameObject);
                        EncounterGenerator.whaleSpawns.Remove(EncounterGenerator.whaleSpawns[i]);
                    }
                }                
                return false;
            }
        }
    }
}
