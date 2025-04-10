using HarmonyLib;

namespace RandomEncounters
{
    internal class IntenseStorm
    {
        internal static OceanUpdaterCrest oceanUpdaterCrest;

        [HarmonyPatch(typeof(OceanUpdaterCrest))]
        static class OceanUpdaterCrestPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            public static void Awake(OceanUpdaterCrest __instance)
            {
                oceanUpdaterCrest = __instance;                    
            }
        }
    }
}

