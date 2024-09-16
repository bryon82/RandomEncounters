using HarmonyLib;
using SailwindModdingHelper;
using System;
using System.Linq;
using UnityEngine;

namespace RandomEncounters
{
    internal class SeaLifeMod
    {
        public static FastInvokeHandler spawnWhale;

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
            var finWhaleAIClass = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(type => type.IsClass && type.Name == "FinWhaleAI")
                .Single();
            var cdpOriginal = AccessTools.Method(finWhaleAIClass, "CheckDistanceToPlayer");
            var cdpPatch = AccessTools.Method(typeof(FinWhaleAIPatches), "CheckDistanceToPlayerPatch");
            Plugin.harmony.Patch(cdpOriginal, new HarmonyMethod(cdpPatch));
        }

        public class FinWhaleAIPatches
        {
            [HarmonyPrefix]
            public static bool CheckDistanceToPlayerPatch()
            {
                var whaleTransform = Refs.shiftingWorld.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "FinWhalePrefab(Clone)");

                if (whaleTransform != null && Vector3.Distance(whaleTransform.position, Utilities.PlayerTransform.position) > 650f)
                {
                    Plugin.logger.LogDebug($"Destroying {whaleTransform.name}");
                    UnityEngine.Object.Destroy(whaleTransform.gameObject);
                }
                return false;
            }
        }
    }
}
