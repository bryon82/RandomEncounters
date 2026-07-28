using HarmonyLib;
using System;
using System.Globalization;
using static RandomEncounters.RE_Plugin;

namespace RandomEncounters
{
    internal class ModData
    {
        public static void AddEntry<T>(string dataName, T data)
        {
            string dataString;

            if (typeof(T) == typeof(float))
                dataString = ((float)(object)data).ToString(CultureInfo.InvariantCulture);
            else
                dataString = data.ToString();

            if (GameState.modData.ContainsKey(dataName))
                GameState.modData[dataName] = dataString;
            else
                GameState.modData.Add(dataName, dataString);
        }

        public static T GetEntry<T>(string dataName)
        {
            if (!GameState.modData.ContainsKey(dataName))
            {
                LogWarning($"GetEntry: {dataName} not found in modData");
                return default;
            }

            var dataString = GameState.modData[dataName];
            if (typeof(T) == typeof(float))
                return (T)(object)float.Parse(dataString, CultureInfo.InvariantCulture);

            return (T)Convert.ChangeType(dataString, typeof(T));
        }

        [HarmonyPatch(typeof(SaveLoadManager))]
        private class SaveLoadManagerPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("SaveModData")]
            public static void SaveModData()
            {
                EncounterGenerator.Instance.SaveEncounter();
            }

            [HarmonyPrefix]
            [HarmonyPatch("LoadModData")]
            public static void LoadModData()
            {
                EncounterGenerator.Instance.LoadEncounter();
            }
        }
    }
}
