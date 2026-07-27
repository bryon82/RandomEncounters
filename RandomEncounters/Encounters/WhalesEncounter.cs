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
        public override bool IsAvailable() => controlSeaLifeMod.Value && SeaLifeModPluginInstance != null;
        public override void Trigger(MonoBehaviour host) => host.StartCoroutine(Run(this));

        internal static IEnumerator Run(Encounter enc)
        {
            var boatPosition = GameState.currentBoat.position;
            var spawnCount = Random.Range(2, 5);
            var spawnDelay = new WaitForSeconds(2f);

            for (int i = 0; i < spawnCount; i++)
            {
                var randomOffset = new Vector3(Random.Range(-200, 200), -8, Random.Range(-200, 200));
                yield return spawnDelay;
                SeaLifeMod.SpawnWhale(i, boatPosition + randomOffset);
            }
            yield return spawnDelay;
            SeaLifeMod.TriggerEntranceAnimation();

            EncounterEvents.RaiseEncounterCompleted(enc);
        }
    }
}
