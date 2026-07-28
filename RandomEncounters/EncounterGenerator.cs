using RandomEncounters.API;
using System.Collections;
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
            
            EncounterRegistry.RegisterEncounter(new FlotsamEncounter());
            EncounterRegistry.RegisterEncounter(new WhalesEncounter());
            EncounterRegistry.RegisterEncounter(new DenseFogEncounter());
            EncounterRegistry.RegisterEncounter(new FishingBonanzaEncounter());
            EncounterRegistry.RegisterEncounter(new IntenseStormEncounter());
            EncounterRegistry.RegisterEncounter(new FogAndWhalesEncounter());
            EncounterRegistry.RegisterEncounter(new FlotsamAndWhalesEncounter());

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
                EncounterEvents.RaiseEncounterSkipped();
                return;
            }

            var table = EncounterRegistry.GetAvailable().ToList();
            if (table.Count == 0)
            {
                LogInfo("No encounters enabled");
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
                    LogInfo($"Encounter: {entry.Name}");
                    EncounterEvents.RaiseEncounterTriggered(entry);
                    entry.Trigger();
                    return;
                }
            }
        }

        internal void LoadEncounter()
        {
            StartCoroutine(EncounterLoader());
        }

        IEnumerator EncounterLoader()
        {            
            var encounterName = ModData.GetEntry<string>($"{PLUGIN_NAME}.EncounterName");
            var timeRemaining = ModData.GetEntry<float>($"{PLUGIN_NAME}.EncounterTimeRemaining");
            var whaleCount = ModData.GetEntry<int>($"{PLUGIN_NAME}.WhaleCount");

            yield return new WaitUntil(() => GameState.playing && !GameState.currentlyLoading);

            if (!string.IsNullOrEmpty(encounterName) && timeRemaining > 0f)
            {
                var enc = EncounterRegistry.GetEncounterByName(encounterName);
                if (enc != null && enc.IsAvailable())
                {
                    enc.TimeRemaining = timeRemaining;
                    enc.Trigger();
                }
            }

            if (whaleCount > 0)
            {
                var whalesEncounter = (WhalesEncounter)EncounterRegistry.GetEncounterByName("Whales");

                if (whalesEncounter != null && whalesEncounter.IsAvailable())
                    whalesEncounter.TriggerWasActive(whaleCount);
            }
        }

        internal void SaveEncounter()
        {
            var enc = EncounterRegistry.GetActiveEncounter();
            if (enc != null)
            {
                ModData.AddEntry($"{PLUGIN_NAME}.EncounterName", enc.Name);
                ModData.AddEntry($"{PLUGIN_NAME}.EncounterTimeRemaining", enc.TimeRemaining);
            }
            else
            {
                ModData.AddEntry($"{PLUGIN_NAME}.EncounterName", string.Empty);
                ModData.AddEntry($"{PLUGIN_NAME}.EncounterTimeRemaining", 0f);
            }

            ModData.AddEntry($"{PLUGIN_NAME}.WhaleCount", SeaLifeMod.ActiveWhales);
        }
    }
}
