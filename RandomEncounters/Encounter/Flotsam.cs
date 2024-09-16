using SailwindModdingHelper;
using UnityEngine;

namespace RandomEncounters
{
    internal class Flotsam
    {
        private static readonly string[] cargos =
        {
            "1 crate salmon (E)",
            "2 crate dates (good)",
            "3 crate coconuts (good)",
            "4 crate lamb (good)",
            "5 crate tea (good)",
            "6 crate tuna (A)",
            "7 crate cheese (good)",
            "8 crate goat cheese (good)",
            "9 crate sunspot fish (A)",
            "10 barrel water",
            "11 barrel rum",
            //"12 barrel beer" // for some reason look at text says barrel of water,
            "13 barrel wine",
            "14 crate north fish (M)",
            "15 crate sausages",
            "16 crate pork",
            "17 crate bananas",
            "18 crate trout (M)",
            "19 crate eel (E)",
            "24 barrel spices",
            "25 crate grain",
            "26 crate medicine",
            "27 crate seafood"
        };
        private static readonly string[] consumables =
        {
            "104 crate of fishing hooks",
            "108 crate of firewood",
            "131 lantern candle crate",
            "132 lantern oil bottle"

        };        
        private static readonly string[] bottles =
        {
            "55 water bottle",
            "56 coco wine",
            "57 honey beer",
            "58 rice beer",
            "59 wine"
        };
        private static readonly string[] tobaccos =
        {
            "311 crate of tobacco white",
            "313 crate of tobacco green",
            "315 crate of tobacco black",
            "317 crate of tobacco brown",
            "319 crate of tobacco blue"
        };

        public static void Spawn(Vector3 spawnPoint)
        {
            var maxCargosTypes = 4;
            var maxCargos = 3;
            var maxConsumTypes = 2;

            for (int i = 0; i < Random.Range(1, maxCargosTypes); i++)
            {
                var choice = Random.Range(0, cargos.Length - 1);

                for (int j = 0; j < Random.Range(1, maxCargos); j++)
                {
                    Plugin.logger.LogDebug($"Choice: {cargos[choice]}");
                    var prefabGO = GameAssets.GetPrefabGameObject(cargos[choice]);
                    var amount = (float)System.Math.Round((decimal)Random.Range(0, prefabGO.GetComponent<ShipItem>().amount));
                    SpawnItem(spawnPoint, prefabGO, amount);
                }                    
            }

            for (int i = 0; i < Random.Range(1, maxConsumTypes); i++)
            {
                var choice = Random.Range(0, consumables.Length - 1);
                Plugin.logger.LogDebug($"Choice: {consumables[choice]}");
                var prefabGO = GameAssets.GetPrefabGameObject(consumables[choice]);
                var amount = (float)System.Math.Round((decimal)Random.Range(0, prefabGO.GetComponent<ShipItem>().amount));
                SpawnItem(spawnPoint, prefabGO, amount);                
            }

            for (int i = 0; i < Random.Range(5, 10); i++)
            {
                var choice = Random.Range(0, bottles.Length - 1);
                Plugin.logger.LogDebug($"Choice: {bottles[choice]}");
                var prefabGO = GameAssets.GetPrefabGameObject(bottles[choice]);
                var amount = 0f;
                SpawnItem(spawnPoint, prefabGO, amount);
            }

            var tobaccoChoice = Random.Range(0, tobaccos.Length - 1);
            Plugin.logger.LogDebug($"Choice: {tobaccos[tobaccoChoice]}");
            var tobaccoPrefabGO = GameAssets.GetPrefabGameObject(tobaccos[tobaccoChoice]);
            var tobaccoAmount = (float)System.Math.Round((decimal)Random.Range(0, tobaccoPrefabGO.GetComponent<ShipItem>().amount));
            SpawnItem(spawnPoint, tobaccoPrefabGO, tobaccoAmount);

            SpawnItem(spawnPoint, AssetLoader.hull, 1f, true);
            SpawnItem(spawnPoint, AssetLoader.mast, 1f, true);
            SpawnItem(spawnPoint, AssetLoader.bowsprit, 1f, true);
        }

        public static void SpawnItem(Vector3 spawnPoint, GameObject prefabGO, float amount, bool wreckage = false)
        {
            GameObject obj = Object.Instantiate(prefabGO, spawnPoint, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
            obj.GetComponent<ShipItem>().sold = true;
            obj.GetComponent<SaveablePrefab>().RegisterToSave();
            if ((bool)obj.GetComponent<Good>())
                obj.GetComponent<Good>().RegisterAsMissionless();
            obj.GetComponent<ShipItem>().amount = amount;
            obj.GetComponent<ShipItem>().health = amount;
            if (wreckage)
                obj.GetComponent<ShipItem>().unclickable = true;
            Plugin.logger.LogDebug($"Prefab {prefabGO.name} spawned");
            new WaitForSeconds(0.2f);
        }
    }
}
