using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomEncounters
{
    internal class EncounterGenerator : MonoBehaviour
    {
        public static EncounterGenerator instance;
        public static List<Transform> whaleSpawns;

        public void Awake()
        {
            instance = this;
            whaleSpawns = new List<Transform>();
            StartCoroutine(GenerateEncounters());
        }

        private IEnumerator GenerateEncounters()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(Plugin.generateEncounterMinTime.Value, Plugin.generateEncounterMinTime.Value + 300));
                if (GameState.currentBoat != null)
                    Generate();
            }
        }

        internal void Generate()
        {
            Plugin.logger.LogDebug($"Distance to land {GameState.distanceToLand}");

            if (!GameState.playing || GameState.sleeping || GameState.distanceToLand <= 1000f) return;
            
            var roll = Random.Range(1, 100);
            Plugin.logger.LogDebug($"Roll: {roll}");

            switch (roll)
            {
                case int n when n <= 10:
                    GenerateFlotsam();
                    break;
                case int n when n > 10 && n <= 15:
                    GenerateFlotsam();
                    StartCoroutine(GenerateWhale());
                    break;
                case int n when n > 15 && n <= 40:
                    StartCoroutine(GenerateWhale());
                    break;
                case int n when n > 40 && n <= 45:
                    StartCoroutine(GenerateDenseFog());
                    break;
                case int n when n > 45 && n <= 55:
                    StartCoroutine(GenerateFishingBonanza());
                    break;
                case int n when n > 55 && n <= 60:
                    StartCoroutine(GenerateIntenseStorm());
                    break;
            }            
        }

        internal static void GenerateFlotsam()
        {
            if (!Plugin.enableFlotsam.Value) return;

            var spawnPoint = GameState.currentBoat.position + GameState.currentBoat.right * 100f + GameState.currentBoat.forward * Random.Range(-30, 30);
            Flotsam.Spawn(spawnPoint);
        }

        internal IEnumerator GenerateWhale()
        {
            if (!Plugin.controlSeaLifeMod.Value || Plugin.seaLifeModInstance == null) 
                yield break;            

            for (int i = 0; i < Random.Range(1, 3); i++)
            {
                Plugin.logger.LogDebug($"Spawning Finwhale");
                var seaLifespawnPoint = GameState.currentBoat.position + new Vector3(Random.Range(-200, 200), -8, Random.Range(-200, 200));
                SeaLifeMod.spawnWhale(Plugin.seaLifeModInstance, seaLifespawnPoint);
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(2f);
            whaleSpawns = Refs.shiftingWorld.GetComponentsInChildren<Transform>().Where(t => t.name == "FinWhalePrefab(Clone)").ToList();
            var whaleTransform = whaleSpawns.FirstOrDefault();
            var finWhaleAI = whaleTransform.gameObject.GetComponent("FinWhaleAI");
            SeaLifeMod.triggerRandomAnimation(finWhaleAI);            
        }

        internal IEnumerator GenerateDenseFog()
        {
            if (!Plugin.enableDenseFog.Value ||
                DenseFog.running ||
                WeatherStorms.instance.InvokePrivateMethod<float>("GetNormalizedDistance") < 0.5f)
                yield break;

            DenseFog.Spawn();
            yield return new WaitForSeconds(23f);
            for (int i = 0; i < 4; i++)
            {
                var spawnPoint = 
                    GameState.currentBoat.position +
                    GameState.currentBoat.right * (200f + Random.Range(20f, 60f) * i) +
                    GameState.currentBoat.forward * Random.Range(-200, 200);

                Flotsam.SpawnItem(spawnPoint, AssetLoader.hull, 1f, true);
                yield return new WaitForSeconds(1f);
                Flotsam.SpawnItem(spawnPoint, AssetLoader.mast, 1f, true);
                yield return new WaitForSeconds(1f);
                Flotsam.SpawnItem(spawnPoint, AssetLoader.bowsprit, 1f, true);
                yield return new WaitForSeconds(1f);
            }
            yield return new WaitForSeconds(Plugin.fogDuration.Value);
            DenseFog.ClearFog();
        }

        internal IEnumerator GenerateFishingBonanza()
        {
            if (!Plugin.enableFishingBonanza.Value)
                yield break;
            
            var seagullsGO = Refs.islands[3].GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "seagulls")?.gameObject;
            if (seagullsGO == null)
            {
                Plugin.logger.LogDebug("No seagulls found");
                yield break;
            }

            Plugin.logger.LogDebug("Starting fishing bonanza");
            var seagulls = Instantiate(seagullsGO, Refs.shiftingWorld);
            if (!seagulls.activeInHierarchy) seagulls.SetActive(true);
            seagulls.GetComponent<AudioSource>().PlayOneShot(seagulls.GetComponent<AudioSource>().clip);
            var seagullsPS = seagulls.GetComponent<ParticleSystem>();
            var shape = seagullsPS.shape;
            shape.radius = 100f;
            var emission = seagullsPS.emission;
            if (!emission.enabled) emission.enabled = true;
            FishingBonanza.bonanzaActive = true;
            for (int t = 0; t < Plugin.fishingBonanzaDuration.Value * 1000; t++)
            {
                seagulls.transform.position = GameState.currentBoat.position + GameState.currentBoat.up * 60f;
                yield return new WaitForSeconds(0.0001f);
            }

            Plugin.logger.LogDebug("Stopping fishing bonanza");
            FishingBonanza.bonanzaActive = false;
            Destroy(seagulls);
        }

        internal IEnumerator GenerateIntenseStorm()
        {
            if (!Plugin.enableIntenseStorm.Value)
                yield break;

            var weatherStorms = WeatherStorms.instance;
            var storm = weatherStorms.GetCurrentStorm();
            var lightning = storm.transform.GetChild(3).GetComponent<WanderingStormLightning>();
            var targetRegion = RegionBlender.instance.GetPrivateField<Region>("currentTargetRegion");

            var origInertiaWindScale = IntenseStorm.oceanUpdaterCrest.inertiaWindScale;
            var origWindSpeedMult = IntenseStorm.oceanUpdaterCrest.GetPrivateField<float>("windSpeedMult");
            var origSmallWavesMult = IntenseStorm.oceanUpdaterCrest.GetPrivateField<float>("smallWavesMult");
            var origLightningInterval = lightning.GetPrivateField<float>("lightningInterval");
            var origRainDensity = targetRegion.stormWeather.particles.rainDensity;

            lightning.SetPrivateField("lightningInterval", 5f);
            targetRegion.stormWeather.particles.rainDensity = 70f;

            var stormDist = Vector3.Distance(Camera.main.transform.position, storm.transform.position);
            Vector3 vector = Camera.main.transform.position - storm.transform.position;
            vector.y = 0f;

            Plugin.logger.LogDebug($"{storm.name} approaching");
            while (stormDist > 1500f)
            {                
                vector = Camera.main.transform.position - storm.transform.position;                
                vector.y = 0f;
                Wind.currentBaseWind = vector * 50f;                
                var translateSpeed = weatherStorms.InvokePrivateMethod<float>("GetNormalizedDistance") < weatherStorms.GetPrivateField<float>("rainBorder") ? 0.005f : 0.25f; 
                storm.transform.Translate(vector * translateSpeed);
                yield return new WaitForSeconds(0.3f);
                stormDist = Vector3.Distance(Camera.main.transform.position, storm.transform.position);
            }

            Plugin.logger.LogDebug($"{storm.name} arrived");
            IntenseStorm.oceanUpdaterCrest.inertiaWindScale = 0.22f;
            IntenseStorm.oceanUpdaterCrest.SetPrivateField("windSpeedMult", 5f);
            IntenseStorm.oceanUpdaterCrest.SetPrivateField("smallWavesMult", 0.4f);
            for (int i = 0; i < Plugin.intenseStormDuration.Value; i++)
            {               
                Wind.currentBaseWind = vector * 50f;
                yield return new WaitForSeconds(1f);
            }

            Plugin.logger.LogDebug($"{storm.name} dying down");
            lightning.SetPrivateField("lightningInterval", origLightningInterval);
            Weather.instance.currentRegion.stormWeather.particles.rainDensity = origRainDensity;
            IntenseStorm.oceanUpdaterCrest.inertiaWindScale = origInertiaWindScale;
            IntenseStorm.oceanUpdaterCrest.SetPrivateField("windSpeedMult", origWindSpeedMult);
            IntenseStorm.oceanUpdaterCrest.SetPrivateField("smallWavesMult", origSmallWavesMult);            
        }

       /* 
        // for testing
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Generate();
            }
        }
        */
    }
}
