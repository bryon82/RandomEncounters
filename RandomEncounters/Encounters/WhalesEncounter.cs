using RandomEncounters.API;
using System.Collections;
using UnityEngine;
using static RandomEncounters.Configs;
using static RandomEncounters.RE_Plugin;

namespace RandomEncounters
{
    internal class WhalesEncounter : Encounter
    {
        public override string Name => "Whales";
        public override int Weight => 25;
        public override bool IsAvailable => controlSeaLifeMod.Value && SeaLifeModPluginInstance != null;
        public override void Trigger() => Runner(Run());
        internal void TriggerWasActive(int whaleCount) => Runner(Run(whaleCount));

        private IEnumerator Run(int whaleCount = -1)
        {
            var boatPosition = GameState.currentBoat.position;
            var spawnCount = Random.Range(2, 5);
            if (whaleCount > 0)
            {
                spawnCount = whaleCount;
            }
            var spawnDelay = new WaitForSeconds(2f);

            for (int i = 0; i < spawnCount; i++)
            {
                var randomOffset = new Vector3(Random.Range(-200, 200), -8, Random.Range(-200, 200));
                yield return spawnDelay;
                SeaLifeMod.SpawnWhale(i, boatPosition + randomOffset);
            }
            yield return spawnDelay;
            SeaLifeMod.TriggerEntranceAnimation();

            if (whaleCount == -1)
                EncounterEvents.RaiseEncounterCompleted(this);
        }
    }
}
