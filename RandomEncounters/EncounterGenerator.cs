﻿using UnityEngine;

namespace RandomEncounters
{
    internal class EncounterGenerator
    {
        public static void Generate()
        {
            Plugin.logger.LogDebug($"Distant to land {GameState.distanceToLand}");
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
                        GenerateWhale();
                        break;
                    case int n when n > 15 && n <= 40:
                        GenerateWhale();
                        break;
                }
            }
        }

        internal static void GenerateFlotsam()
        {
            //var flotsamSpawnPoint = GameState.currentBoat.position + GameState.currentBoat.right * 30f;
            var flotsamSpawnPoint = GameState.currentBoat.position + GameState.currentBoat.right * 100f + GameState.currentBoat.forward * Random.Range(-30, 30);
            Flotsam.Spawn(flotsamSpawnPoint);
        }

        internal static void GenerateWhale()
        {
            if (Plugin.controlSeaLifeMod.Value && Plugin.seaLifeModInstance != null)
            {
                for (int i = 0; i < Random.Range(1, 3); i++)
                {
                    var seaLifespawnPoint = GameState.currentBoat.position + new Vector3(Random.Range(-200, 200), -8, Random.Range(-200, 200));
                    SeaLifeMod.spawnWhale(Plugin.seaLifeModInstance, seaLifespawnPoint);
                }                
            }                
        }
    }
}
