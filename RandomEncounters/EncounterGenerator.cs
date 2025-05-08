using System.Collections;
using System.Linq;
using UnityEngine;
using static RandomEncounters.RE_Plugin;
using static RandomEncounters.Configs;

namespace RandomEncounters
{
    internal class EncounterGenerator : MonoBehaviour
    {
        public static EncounterGenerator Instance { get; private set; }

        private bool _moveSeagulls;

        private const float MIN_DISTANCE = 1000F;
        private const int ENCOUNTER_TIME_RANGE = 300;

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
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
            var maxTime = generateEncounterMinTime.Value + ENCOUNTER_TIME_RANGE;
            var timeToNextEncounter = Random.Range(generateEncounterMinTime.Value, maxTime);
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

            var roll = Random.Range(1, 100);
            LogDebug($"Roll: {roll}");

            switch (roll)
            {
                case int n when n <= 10:
                    GenerateFlotsam();
                    break;
                case int n when n > 10 && n <= 15:
                    GenerateFlotsam();
                    StartCoroutine(GenerateWhales());
                    break;
                case int n when n > 15 && n <= 40:
                    StartCoroutine(GenerateWhales());
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

        private static void GenerateFlotsam()
        {
            if (!enableFlotsam.Value) 
                return;

            var spawnPoint = GameState.currentBoat.position + GameState.currentBoat.right * 100f + GameState.currentBoat.forward * Random.Range(-30, 30);
            Flotsam.Spawn(spawnPoint);
        }

        #region whales

        private IEnumerator GenerateWhales()
        {
            if (!controlSeaLifeMod.Value || SeaLifeModPluginInstance == null) 
                yield break;

            var boatPosition = GameState.currentBoat.position;            

            for (int i = 0; i < Random.Range(2, 5); i++)
            {
                var randomOffset = new Vector3(Random.Range(-200, 200), -8, Random.Range(-200, 200));
                yield return new WaitForSeconds(2f);                
                SeaLifeMod.SpawnWhale(i, boatPosition + randomOffset);
            }
            yield return new WaitForSeconds(2f);
            SeaLifeMod.TriggerEntranceAnimation();                      
        }        

        #endregion

        #region dense fog

        private IEnumerator GenerateDenseFog()
        {
            if (!enableDenseFog.Value ||
                DenseFog.IsRunning ||
                WeatherStorms.instance.InvokePrivateMethod<float>("GetNormalizedDistance") < 0.75f)
                yield break;

            DenseFog.Spawn();
            for (int i = 0; i < 4000; i++)
            {
                foreach (var audioSource in DenseFog.WaveAudioSources.Keys)
                {
                    audioSource.volume = Mathf.Lerp(DenseFog.WaveAudioSources[audioSource], 0f, i / 1000f);
                }
                DenseFog.WindAudioSource.source.volume = Mathf.Lerp(DenseFog.WindAudioSource.origVolume, 0.0001f, i / 1000f);
                yield return new WaitForSeconds(0.001f);
            }

            for (int i = 0; i < 4; i++)
            {
                var spawnPoint = 
                    GameState.currentBoat.position +
                    GameState.currentBoat.right * (200f + Random.Range(20f, 60f) * i) +
                    GameState.currentBoat.forward * Random.Range(-200, 200);

                Flotsam.SpawnItem(spawnPoint, AssetLoader.Hull, 1f, true);
                yield return new WaitForSeconds(1f);
                Flotsam.SpawnItem(spawnPoint, AssetLoader.Mast, 1f, true);
                yield return new WaitForSeconds(1f);
                Flotsam.SpawnItem(spawnPoint, AssetLoader.Bowsprit, 1f, true);
                yield return new WaitForSeconds(1f);
            }

            yield return new WaitForSeconds(fogDuration.Value);

            DenseFog.ClearFog();
            for (int i = 0; i < 1000; i++)
            {
                foreach (var audioSource in DenseFog.WaveAudioSources.Keys)
                {
                    audioSource.volume = Mathf.Lerp(0f, DenseFog.WaveAudioSources[audioSource], i / 1000f);
                }
                DenseFog.WindAudioSource.source.volume = Mathf.Lerp(0.0001f, DenseFog.WindAudioSource.origVolume,  i / 1000f);
                yield return new WaitForSeconds(0.001f);
            }
        }

        #endregion

        #region fishing bonanza

        private IEnumerator GenerateFishingBonanza()
        {
            if (!enableFishingBonanza.Value ||
            GameState.currentBoat == null ||
            WeatherStorms.instance.InvokePrivateMethod<float>("GetNormalizedDistance") < 0.75f)
            {
                yield break;
            }

            var seagullsGO = Refs.islands[3].GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "seagulls")?.gameObject;
            if (seagullsGO == null)
            {
                LogDebug("No seagulls found");
                yield break;
            }

            var seagulls = Instantiate(seagullsGO, Refs.shiftingWorld);
            if (!seagulls.activeInHierarchy) seagulls.SetActive(true);
            seagulls.GetComponent<AudioSource>().PlayOneShot(seagulls.GetComponent<AudioSource>().clip);
            var seagullsPS = seagulls.GetComponent<ParticleSystem>();
            var shape = seagullsPS.shape;
            shape.radius = 100f;
            var emission = seagullsPS.emission;
            if (!emission.enabled) emission.enabled = true;

            LogDebug("Starting fishing bonanza");
            FishingBonanza.IsBonanzaActive = true;
            _moveSeagulls = true;
            StartCoroutine(MoveSegulls(seagulls.transform, GameState.currentBoat));
            yield return new WaitForSeconds(fishingBonanzaDuration.Value);            

            LogDebug("Stopping fishing bonanza");
            FishingBonanza.IsBonanzaActive = false;
            _moveSeagulls = false;
            Destroy(seagulls);
        }

        private IEnumerator MoveSegulls(Transform seagulls, Transform boat)
        {
            while (_moveSeagulls)
            {
                seagulls.position = boat.position + boat.up * 60f;
                yield return null;
            }
        }

        #endregion

        #region intense storm

        private IEnumerator GenerateIntenseStorm()
        {
            if (!enableIntenseStorm.Value)
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
            var vector = Camera.main.transform.position - storm.transform.position;
            vector.y = 0f;

            LogDebug($"{storm.name} approaching");
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

            LogDebug($"{storm.name} arrived");
            IntenseStorm.oceanUpdaterCrest.inertiaWindScale = 0.22f;
            IntenseStorm.oceanUpdaterCrest.SetPrivateField("windSpeedMult", 5f);
            IntenseStorm.oceanUpdaterCrest.SetPrivateField("smallWavesMult", 0.4f);
            for (int i = 0; i < intenseStormDuration.Value; i++)
            {
                Wind.currentBaseWind = vector * 50f;
                yield return new WaitForSeconds(1f);
            }

            LogDebug($"{storm.name} dying down");
            lightning.SetPrivateField("lightningInterval", origLightningInterval);
            Weather.instance.currentRegion.stormWeather.particles.rainDensity = origRainDensity;
            IntenseStorm.oceanUpdaterCrest.inertiaWindScale = origInertiaWindScale;
            IntenseStorm.oceanUpdaterCrest.SetPrivateField("windSpeedMult", origWindSpeedMult);
            IntenseStorm.oceanUpdaterCrest.SetPrivateField("smallWavesMult", origSmallWavesMult);            
        }

        #endregion
    }
}
