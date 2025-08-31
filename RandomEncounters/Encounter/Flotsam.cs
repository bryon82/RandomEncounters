using System.Collections;
using UnityEngine;
using static RandomEncounters.RE_Plugin;

namespace RandomEncounters
{
    internal class Flotsam
    {
        //cargos
        //"1 crate salmon (E)",
        //"2 crate dates (good)",
        //"3 crate coconuts (good)",
        //"4 crate lamb (good)",
        //"5 crate tea (good)",
        //"6 crate tuna (A)",
        //"7 crate cheese (good)",
        //"8 crate goat cheese (good)",
        //"9 crate sunspot fish (A)",
        //"10 barrel water",
        //"11 barrel rum",
        //"12 barrel beer" // for some reason look at text says barrel of water,
        //"13 barrel wine",
        //"14 crate north fish (M)",
        //"15 crate sausages",
        //"16 crate pork",
        //"17 crate bananas",
        //"18 crate trout (M)",
        //"19 crate eel (E)",
        //"24 barrel spices",
        //"25 crate grain",
        //"26 crate medicine",
        //"27 crate seafood",
        //201 crate venison
        //202 crate truffles
        //206 barrel mead
        //212 crate rice
        //213 crate oranges
        //214 crate forest mushrooms
        //216 crate cave mushrooms
        //219 leather
        //220 rabbit furs
        //222 wool
        //223 olive oil
        //224 apples
        //227 sulfur
        //228 barrel cider

        //consumables
        //"104 crate of fishing hooks",
        //"108 crate of firewood",
        //"131 lantern candle crate",
        //"132 lantern oil bottle"

        //bottles
        //"55 water bottle",
        //"56 coco wine",
        //"57 honey beer",
        //"58 rice beer",
        //"59 wine"

        //tobaccos
        //"311 crate of tobacco white",
        //"313 crate of tobacco green",
        //"315 crate of tobacco black",
        //"317 crate of tobacco brown",
        //"319 crate of tobacco blue"

        private static readonly int[] _cargos = 
        { 
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 
            11, 13, 14, 15, 16, 17, 18, 19, 24, 25,
            26, 27, 201, 202, 206, 212, 213, 214, 216, 219,
            220, 222, 223, 224, 227, 228,
        };
        private static readonly int[] _consumables = { 104, 108, 131, 132 };
        private static readonly int[] _bottles = { 55, 56, 57, 58, 59 };
        private static readonly int[] _tobaccoCrates = { 311, 313, 315, 319 };

        private const int MAX_CARGO_TYPES = 4;
        private const int MAX_CARGOS = 3;
        private const int MAX_CONSUME_TYPES = 2;

        internal static void Spawn(Vector3 spawnPoint)
        {
            for (int i = 0; i < Random.Range(1, MAX_CARGO_TYPES); i++)
            {
                var choice = Random.Range(0, _cargos.Length - 1);

                for (int j = 0; j < Random.Range(1, MAX_CARGOS); j++)
                {
                    LogDebug($"Choice: {_cargos[choice]}");
                    var prefabGO = PrefabsDirectory.instance.directory[_cargos[choice]];
                    var amount = (float)System.Math.Round((decimal)Random.Range(0, prefabGO.GetComponent<ShipItem>().amount));
                    SpawnItem(spawnPoint, prefabGO, amount);
                }
            }

            for (int i = 0; i < Random.Range(1, MAX_CONSUME_TYPES); i++)
            {
                var choice = Random.Range(0, _consumables.Length - 1);
                LogDebug($"Choice: {_consumables[choice]}");
                var prefabGO = PrefabsDirectory.instance.directory[_consumables[choice]];
                var amount = (float)System.Math.Round((decimal)Random.Range(0, prefabGO.GetComponent<ShipItem>().amount));
                SpawnItem(spawnPoint, prefabGO, amount);
            }

            for (int i = 0; i < Random.Range(5, 10); i++)
            {
                var choice = Random.Range(0, _bottles.Length - 1);
                LogDebug($"Choice: {_bottles[choice]}");
                var prefabGO = PrefabsDirectory.instance.directory[_bottles[choice]];
                var amount = 0f;
                SpawnItem(spawnPoint, prefabGO, amount);
            }

            var tobaccoChoice = Random.Range(0, _tobaccoCrates.Length - 1);
            LogDebug($"Choice: {_tobaccoCrates[tobaccoChoice]}");
            var tobaccoPrefabGO = PrefabsDirectory.instance.directory[_tobaccoCrates[tobaccoChoice]];
            var tobaccoAmount = (float)System.Math.Round((decimal)Random.Range(0, tobaccoPrefabGO.GetComponent<ShipItem>().amount));
            SpawnItem(spawnPoint, tobaccoPrefabGO, tobaccoAmount);

            SpawnItem(spawnPoint, AssetLoader.SmallWreck, 1f, true);
        }

        internal static void SpawnItem(Vector3 spawnPoint, GameObject prefabGO, float amount, bool wreckage = false)
        {
            var obj = Object.Instantiate(prefabGO, spawnPoint, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
            var shipItem = obj.GetComponent<ShipItem>();
            shipItem.sold = true;
            shipItem.amount = amount;
            shipItem.health = amount;
            obj.GetComponent<SaveablePrefab>().RegisterToSave();
            var good = obj.GetComponent<Good>();
            if (good != null)
                good.RegisterAsMissionless();
            if (shipItem is ShipItemCrate crate)
                EncounterGenerator.Instance.StartCoroutine(UnsealCrate(crate));
            if (wreckage)
            {
                obj.GetComponent<ShipItem>().unclickable = true;
                obj.transform.parent = Refs.shiftingWorld;
            }
            LogDebug($"Prefab {prefabGO.name} spawned");
        }

        internal static IEnumerator UnsealCrate(ShipItemCrate crate)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //crate.UnsealCrate();
            var crateInventory = crate.GetComponent<CrateInventory>();
            int num = (int)crate.amount;
            for (int i = 0; i < num; i++)
            {
                LogDebug("Inserting item " + crate.amount);
                GameObject gameObject = Object.Instantiate(crate.GetContainedPrefab(), crate.transform.position + new Vector3(0f, 100.5f, 0f), crate.transform.rotation);
                crate.amount -= 1f;
                gameObject.GetComponent<SaveablePrefab>().RegisterToSave();
                if ((bool)gameObject.GetComponent<CookableFood>())
                {
                    if (crate.smokedFood)
                    {
                        gameObject.GetComponent<FoodState>().smoked = 1f;
                        gameObject.GetComponent<ShipItem>().amount = 1.01f;
                    }

                    gameObject.GetComponent<FoodState>().dried = 1f;
                    gameObject.GetComponent<CookableFood>().UpdateMaterial();
                }

                EncounterGenerator.Instance.StartCoroutine(InsertItem(crateInventory, gameObject.GetComponent<ShipItem>()));
            }

            crate.UpdateLookText();
            crate.itemRigidbodyC.UpdateMass();
            LogDebug("Unsealed crate.");
        }

        private static IEnumerator InsertItem(CrateInventory crateInventory, ShipItem item)
        {            
            yield return new WaitForEndOfFrame();
            item.sold = true;
            crateInventory.InsertItem(item);
        }
    }
}
