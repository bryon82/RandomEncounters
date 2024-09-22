using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomEncounters
{
    internal class EncounterGenerator : MonoBehaviour
    {
        public static EncounterGenerator instance;
        public static List<Transform> whaleSpawns;

        public void Awake()
        {
            instance = this;
            whaleSpawns = new List<Transform>();
            StartCoroutine(GenerateEncounters());
        }

        private IEnumerator GenerateEncounters()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(Plugin.generateEncounterMinTime.Value, Plugin.generateEncounterMinTime.Value + 300));
                if (GameState.currentBoat != null)
                    Generate();
            }
        }

        internal void Generate()
        {
            Plugin.logger.LogDebug($"Distance to land {GameState.distanceToLand}");
            if (GameState.playing && !GameState.sleeping && GameState.distanceToLand > 1000f)
            {
                var roll = Random.Range(1, 100);
                Plugin.logger.LogDebug($"Roll: {roll}");

                switch (roll)
                {
                    case int n when n <= 10:
                        GenerateFlotsam();
                        break;
                    case int n when n > 10 && n <= 15:
                        GenerateFlotsam();
                        StartCoroutine(GenerateWhale());
                        break;
                    case int n when n > 15 && n <= 40:
                        StartCoroutine(GenerateWhale());
                        break;
                    case int n when n > 40 && n <= 45:
                        StartCoroutine(GenerateDenseFog());
                        break;
                }
            }
        }

        internal static void GenerateFlotsam()
        {
            var spawnPoint = GameState.currentBoat.position + GameState.currentBoat.right * 100f + GameState.currentBoat.forward * Random.Range(-30, 30);
            Flotsam.Spawn(spawnPoint);
        }

        internal IEnumerator GenerateWhale()
        {
            if (Plugin.controlSeaLifeMod.Value && Plugin.seaLifeModInstance != null)
            {
                for (int i = 0; i < Random.Range(1, 3); i++)
                {
                    var seaLifespawnPoint = GameState.currentBoat.position + new Vector3(Random.Range(-200, 200), -8, Random.Range(-200, 200));
                    SeaLifeMod.spawnWhale(Plugin.seaLifeModInstance, seaLifespawnPoint);
                    yield return new WaitForSeconds(0.5f);
                }
                yield return new WaitForSeconds(2f);
                whaleSpawns = Refs.shiftingWorld.GetComponentsInChildren<Transform>().Where(t => t.name == "FinWhalePrefab(Clone)").ToList();
                var whaleTransform = whaleSpawns.FirstOrDefault();
                var finWhaleAI = whaleTransform.gameObject.GetComponent("FinWhaleAI");
                SeaLifeMod.triggerRandomAnimation(finWhaleAI);
            }
        }

        internal IEnumerator GenerateDenseFog()
        {
            if (DenseFog.running)
                yield break;
            if (Traverse.Create(WeatherStorms.instance).Method("GetNormalizedDistance").GetValue<float>() < 0.5f)
                yield break;            
            DenseFog.Spawn();
            yield return new WaitForSeconds(23f);
            for (int i = 0; i < 4; i++)
            {
                var spawnPoint = 
                    GameState.currentBoat.position +
                    GameState.currentBoat.right * (200f + Random.Range(20f, 60f) * i) +
                    GameState.currentBoat.forward * Random.Range(-200, 200);

                Flotsam.SpawnItem(spawnPoint, AssetLoader.hull, 1f, true);
                yield return new WaitForSeconds(1f);
                Flotsam.SpawnItem(spawnPoint, AssetLoader.mast, 1f, true);
                yield return new WaitForSeconds(1f);
                Flotsam.SpawnItem(spawnPoint, AssetLoader.bowsprit, 1f, true);
                yield return new WaitForSeconds(1f);
            }
            yield return new WaitForSeconds(Plugin.fogDuration.Value);
            DenseFog.ClearFog();
        }

        // for testing
        //void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.P))
        //    {
        //        Generate();
        //    }
        //}
    }
}
