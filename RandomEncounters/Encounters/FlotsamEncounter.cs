using RandomEncounters.API;
using System.Collections;
using UnityEngine;
using static RandomEncounters.Configs;
using static RandomEncounters.RE_Plugin;

namespace RandomEncounters
{
    internal class FlotsamEncounter : Encounter
    {
        public override string Name => "Flotsam";
        public override int Weight => 15;
        public override bool IsAvailable() => enableFlotsam.Value;
        public override void Trigger(MonoBehaviour host) => Run(this);

        internal static void Run(Encounter enc)
        {
            var spawnPoint =
                GameState.currentBoat.position
                + GameState.currentBoat.right * 200f
                + GameState.currentBoat.forward * Random.Range(-30, 30);

            Spawn(spawnPoint);

            EncounterEvents.RaiseEncounterCompleted(enc);
        }

        //cargos
        //1 crate salmon (E)
        //2 crate dates (good)
        //3 crate coconuts (good)
        //4 crate lamb (good)
        //5 crate tea (good)
        //6 crate tuna (A)
        //7 crate cheese (good)
        //8 crate goat cheese (good)
        //9 crate sunspot fish (A)
        //10 barrel water
        //11 barrel rum
        //12 barrel beer // look text says barrel of water,
        //13 barrel wine
        //14 crate north fish (M)
        //15 crate sausages
        //16 crate pork
        //17 crate bananas
        //18 crate trout (M)
        //19 crate eel (E)
        //24 barrel spices
        //25 crate grain        // too heavy
        //26 crate medicine
        //27 crate seafood
        //201 crate venison
        //202 crate truffles
        //206 barrel mead
        //212 crate rice        // too heavy
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
        //229 hemp
        //230 dyes
        //231 rubber
        //232 coffee
        //233 salt
        //234 saffron

        //consumables
        //104 crate of fishing hooks
        //108 crate of firewood
        //131 lantern candle crate
        //132 lantern oil bottle

        //bottles
        //55 water bottle
        //56 coco wine
        //57 honey beer
        //58 rice beer
        //59 wine

        //tobaccos
        //311 crate of tobacco white
        //313 crate of tobacco green
        //315 crate of tobacco black
        //317 crate of tobacco brown
        //319 crate of tobacco blue

        //tea and coffee
        //387 tea box white
        //388 tea box black
        //389 tea box green
        //373 coffee box
        //386 coffee barrel


        private static readonly int[] _cargos =
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            11, 13, 14, 15, 16, 17, 18, 19, 24, 26,
            27, 201, 202, 206, 213, 214, 216, 219, 220, 222,
            223, 224, 227, 228, 229, 230, 231, 232, 233, 234
        };
        private static readonly int[] _consumables = { 104, 108, 131, 132 };
        private static readonly int[] _bottles = { 55, 56, 57, 58, 59 };
        private static readonly int[] _tobaccoCrates = { 311, 313, 315, 319 };
        private static readonly int[] _teaAndCoffeeBoxes = { 387, 388, 389, 373 };

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
                    LogDebug($"Cargo choice: {_cargos[choice]}");
                    var prefabGO = PrefabsDirectory.instance.directory[_cargos[choice]];
                    var amount = Random.Range(1, (int)prefabGO.GetComponent<ShipItem>().amount + 1);
                    Flotsam.SpawnItem(spawnPoint, prefabGO, amount);
                }
            }

            for (int i = 0; i < Random.Range(1, MAX_CONSUME_TYPES); i++)
            {
                var choice = Random.Range(0, _consumables.Length - 1);
                LogDebug($"Consumable choice: {_consumables[choice]}");
                var prefabGO = PrefabsDirectory.instance.directory[_consumables[choice]];
                var amount = Random.Range(1, (int)prefabGO.GetComponent<ShipItem>().amount + 1);
                Flotsam.SpawnItem(spawnPoint, prefabGO, amount);
            }

            for (int i = 0; i < Random.Range(5, 10); i++)
            {
                var choice = Random.Range(0, _bottles.Length - 1);
                LogDebug($"Bottle choice: {_bottles[choice]}");
                var prefabGO = PrefabsDirectory.instance.directory[_bottles[choice]];
                var amount = 0f;
                Flotsam.SpawnItem(spawnPoint, prefabGO, amount);
            }

            var tobaccoChoice = Random.Range(0, _tobaccoCrates.Length - 1);
            LogDebug($"Tobacco choice: {_tobaccoCrates[tobaccoChoice]}");
            var tobaccoPrefabGO = PrefabsDirectory.instance.directory[_tobaccoCrates[tobaccoChoice]];
            var tobaccoAmount = Random.Range(1, (int)tobaccoPrefabGO.GetComponent<ShipItem>().amount + 1);
            Flotsam.SpawnItem(spawnPoint, tobaccoPrefabGO, tobaccoAmount);

            var teaCoffeeChoice = Random.Range(0, _teaAndCoffeeBoxes.Length - 1);
            LogDebug($"Tea/Coffee choice: {_teaAndCoffeeBoxes[teaCoffeeChoice]}");
            var teaCoffeePrefabGO = PrefabsDirectory.instance.directory[_teaAndCoffeeBoxes[teaCoffeeChoice]];
            var teaCoffeeAmount = Random.Range(1, (int)teaCoffeePrefabGO.GetComponent<ShipItem>().amount + 1);
            Flotsam.SpawnItem(spawnPoint, teaCoffeePrefabGO, teaCoffeeAmount);

            Flotsam.SpawnItem(spawnPoint, AssetLoader.SmallWreck, 1f, true);
        }
    }
}
