using System.Collections;
using UnityEngine;
using static RandomEncounters.RE_Plugin;

namespace RandomEncounters
{
    internal class Flotsam
    {
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
                var gameObject = Object.Instantiate(crate.GetContainedPrefab(), crate.transform.position + new Vector3(0f, 100.5f, 0f), crate.transform.rotation);
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
