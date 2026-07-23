using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RandomEncounters.Configs;
using static RandomEncounters.RE_Plugin;

namespace RandomEncounters
{
    internal class EncounterGenerator : MonoBehaviour
    {
        public static EncounterGenerator Instance { get; private set; }

        private const float MIN_DISTANCE = 1000F;

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            StartCoroutine(ScheduleEncounter());
        }

        public void Update()
        {
            SeaLifeMod.CheckWhaleDistance();

            /*
            // for testing
            if (Input.GetKeyDown(KeyCode.P))
            {
                Generate();
            }
            */
        }        

        private IEnumerator ScheduleEncounter()
        {
            yield return new WaitUntil(() => GameState.playing);

            var minTime = Mathf.Abs(generateEncounterMinTime.Value);
            var range = Mathf.Abs(generateEncounterTimeRange.Value);
            var timeToNextEncounter = minTime + Random.Range(0, range);

            yield return new WaitForSeconds(timeToNextEncounter);
            if (GameState.currentBoat != null)
                Generate();

            StartCoroutine(ScheduleEncounter());
        }

        private void Generate()
        {
            if (!GameState.playing)
                return;

            if (GameState.distanceToLand <= MIN_DISTANCE)
            {
                LogDebug($"Too close to land (distance: {GameState.distanceToLand}), skipping encounter generation.");
                return;
            }

            if (GameState.sleeping)
            {
                LogDebug("Player sleeping, skipping encounter generation.");
                return;
            }

            if (Random.value > Mathf.Abs(encounterRollChance.Value / 100f))
            {
                LogInfo("No encounter this time");
                return;
            }

            var table = BuildEncounterTable();
            if (table.Count == 0)
            {
                LogDebug("No encounters enabled");
                return;
            }

            var totalWeight = table.Sum(e => e.Weight);
            var roll = Random.Range(0, totalWeight);

            var cumulative = 0;
            foreach (var entry in table)
            {
                cumulative += entry.Weight;
                if (roll < cumulative)
                {
                    LogDebug($"Encounter: {entry.Name}");
                    entry.Trigger();
                    return;
                }
            }
        }

        private List<EncounterEntry> BuildEncounterTable()
        {
            var table = new List<EncounterEntry>();
            var seaLifeModActive = controlSeaLifeMod.Value && SeaLifeModPluginInstance != null;

            if (enableFlotsam.Value)
                table.Add(new EncounterEntry("Flotsam", 15, GenerateFlotsam));

            if (enableFlotsam.Value && seaLifeModActive)
                table.Add(new EncounterEntry("Flotsam and Whales", 5, () => { GenerateFlotsam(); GenerateWhales(); }));

            if (seaLifeModActive)
                table.Add(new EncounterEntry("Whales", 25, () => GenerateWhales()));

            if (enableDenseFog.Value)
                table.Add(new EncounterEntry("Dense Fog", 5, () => GenerateDenseFog()));

            if (enableDenseFog.Value && seaLifeModActive)
                table.Add(new EncounterEntry("Dense Fog and Whales", 5, () => { GenerateDenseFog(); GenerateWhales(); }));

            if (enableFishingBonanza.Value)
                table.Add(new EncounterEntry("Fishing Bonanza", 15, () => GenerateFishingBonanza()));

            if (enableIntenseStorm.Value)
                table.Add(new EncounterEntry("Intense Storm", 5, () => GenerateIntenseStorm()));

            return table;
        }

        private static void GenerateFlotsam()
        {
            var spawnPoint = 
                GameState.currentBoat.position
                + GameState.currentBoat.right * 200f
                + GameState.currentBoat.forward * Random.Range(-30, 30);
            Flotsam.Spawn(spawnPoint);
        }

        private void GenerateWhales() =>  StartCoroutine(Whales.Run());
        private void GenerateDenseFog() => StartCoroutine(DenseFog.Run());
        private void GenerateFishingBonanza() => StartCoroutine(FishingBonanza.Run(this));
        private void GenerateIntenseStorm() => StartCoroutine(IntenseStorm.Run());
    }
}
