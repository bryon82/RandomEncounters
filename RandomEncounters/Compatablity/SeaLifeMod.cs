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
            Type seaLifePluginClass = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(type => type.IsClass && type.Name == "SeaLifePlugin")
                .Single();            
            var spawnWhaleMethod = AccessTools.Method(seaLifePluginClass, "SpawnWhale");
            spawnWhale = MethodInvoker.GetHandler(spawnWhaleMethod);
            var fpoOriginal = AccessTools.Method(seaLifePluginClass, "FindPlayerObject");
            var fpoPatch = AccessTools.Method(typeof(SeaLifePluginPatches), "FindPlayerObjectPatch");
            Plugin.harmony.Patch(fpoOriginal, new HarmonyMethod(fpoPatch));
            var grspnpOriginal = AccessTools.Method(seaLifePluginClass, "GetRandomSpawnPositionNearPlayer");
            var grspnpPatch = AccessTools.Method(typeof(SeaLifePluginPatches), "GetRandomSpawnPositionNearPlayerPatch");
            Plugin.harmony.Patch(grspnpOriginal, new HarmonyMethod(grspnpPatch));

            var finWhaleAIClass = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(type => type.IsClass && type.Name == "FinWhaleAI")
                .Single();
            var cdpOriginal = AccessTools.Method(finWhaleAIClass, "CheckDistanceToPlayer");
            var cdpPatch = AccessTools.Method(typeof(FinWhaleAIPatches), "CheckDistanceToPlayerPatch");
            Plugin.harmony.Patch(cdpOriginal, new HarmonyMethod(cdpPatch));
        }

        public class SeaLifePluginPatches
        {
            [HarmonyPrefix]
            public static bool FindPlayerObjectPatch(ref bool __result) 
            {
                __result = false;
                return false;
            }

            [HarmonyPrefix]
            public static bool GetRandomSpawnPositionNearPlayerPatch(ref Vector3 __result)
            {
                __result = Utilities.PlayerTransform.position + Utilities.PlayerTransform.forward * -1000f;
                return false;
            }
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
