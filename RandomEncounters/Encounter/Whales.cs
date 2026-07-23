using System.Collections;
using UnityEngine;

namespace RandomEncounters
{
    internal class Whales
    {
        internal static IEnumerator Run()
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
        }
    }
}
