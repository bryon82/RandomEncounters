using System.Collections;
using System.Linq;
using UnityEngine;
using static RandomEncounters.Configs;
using static RandomEncounters.RE_Plugin;

namespace RandomEncounters
{
    internal class EncounterGenerator : MonoBehaviour
    {
        public static EncounterGenerator Instance { get; private set; }

        private bool _moveSeagulls;

        private const float MIN_DISTANCE = 1000F;

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
            yield return new WaitUntil(() => GameState.playing);

            var minTime = Mathf.Abs(generateEncounterMinTime.Value);
            var range = Mathf.Abs(generateEncounterTimeRange.Value);
            var timeToNextEncounter = minTime + Random.Range(0, range);

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

            var rollRange = 100 + Mathf.Abs(encounterRollMaxIncrease.Value);
            var roll = Random.Range(1, rollRange);
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
                case int n when n > 60:
                    LogDebug("No encounter this time");
                    break;
            }
        }

        private static void GenerateFlotsam()
        {
            if (!enableFlotsam.Value) 
                return;

            var spawnPoint = GameState.currentBoat.position + GameState.currentBoat.right * 200f + GameState.currentBoat.forward * Random.Range(-30, 30);
            Flotsam.Spawn(spawnPoint);
        }

        #region whales

        private IEnumerator GenerateWhales()
        {
            if (!controlSeaLifeMod.Value || SeaLifeModPluginInstance == null) 
                yield break;

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

        #endregion

        #region dense fog

        private IEnumerator GenerateDenseFog()
        {
            if (!enableDenseFog.Value ||
                DenseFog.IsRunning ||
                WeatherStorms.instance.InvokePrivateMethod<float>("GetNormalizedDistance") < 0.75f)
                yield break;

            DenseFog.Spawn();
            var waveAudioSources = DenseFog.WaveAudioSources.Keys.ToList();
            var windAudioSource = DenseFog.WindAudioSource.source;
            var windOrigVolume = DenseFog.WindAudioSource.origVolume;
            const float fadeOutDuration = 4f;
            for (var t = 0f; t < fadeOutDuration; t += Time.deltaTime)
            {
                var lerpValue = t / fadeOutDuration;
                foreach (var audioSource in waveAudioSources)
                {
                    audioSource.volume = Mathf.Lerp(DenseFog.WaveAudioSources[audioSource], 0f, lerpValue);
                }
                if (windAudioSource != null)
                {
                    windAudioSource.volume = Mathf.Lerp(windOrigVolume, 0.0001f, lerpValue);
                }
                yield return null;
            }

            for (int i = 0; i < 4; i++)
            {
                var spawnPoint = 
                    GameState.currentBoat.position +
                    GameState.currentBoat.right * (200f + Random.Range(20f, 60f) * i) +
                    GameState.currentBoat.forward * Random.Range(-200, 200);

                Flotsam.SpawnItem(spawnPoint, Random.Range(1, 100) > 50 ? AssetLoader.SmallWreck : AssetLoader.Hull, 1f, true);
                yield return new WaitForSeconds(1f);
            }

            yield return new WaitForSeconds(fogDuration.Value);

            DenseFog.ClearFog();
            const float fadeInDuration = 4f;
            for (var t = 0f; t < fadeInDuration; t += Time.deltaTime)
            {
                var lerpValue = t / fadeInDuration;
                foreach (var audioSource in waveAudioSources)
                {
                    audioSource.volume = Mathf.Lerp(0f, DenseFog.WaveAudioSources[audioSource], lerpValue);
                }
                if (windAudioSource != null)
                {
                    windAudioSource.volume = Mathf.Lerp(0.0001f, windOrigVolume, lerpValue);
                }
                yield return null;
            }
        }

        #endregion

        #region fishing bonanza

        private IEnumerator GenerateFishingBonanza()
        {
            var stormDistance = WeatherStorms.instance.InvokePrivateMethod<float>("GetNormalizedDistance");

            if (!enableFishingBonanza.Value || GameState.currentBoat == null || stormDistance < 0.75f)
            {
                LogDebug($"Storm too close for fishing bonanza {stormDistance} {stormDistance < 0.75f}");
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
            
            var main = seagullsPS.main;
            main.maxParticles = 25;
            main.startLifetime = fishingBonanzaDuration.Value;                        
            main.startRotation = 0f;
            main.startRotation3D = false;
                        
            var rol = seagullsPS.rotationOverLifetime;
            rol.enabled = false;
            rol.x = 0f;
            rol.y = 0f;
            rol.z = 0f;
                        
            var vol = seagullsPS.velocityOverLifetime;
            vol.enabled = true;
            vol.orbitalX = 0f;
            vol.orbitalY = 0f;
            vol.orbitalZ = 0f;
            vol.orbitalXMultiplier = 0;
            vol.orbitalYMultiplier = 0;
            vol.orbitalZMultiplier = 0;
                        
            var rbs = seagullsPS.rotationBySpeed;
            rbs.enabled = false;

            var seagullPSR = seagulls.GetComponent<ParticleSystemRenderer>();
            seagullPSR.alignment = ParticleSystemRenderSpace.Local;

            var shape = seagullsPS.shape;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            shape.scale = new Vector3(25, 25, 0.2f);

            var emission = seagullsPS.emission;
            if (!emission.enabled) emission.enabled = true;

            LogDebug("Starting fishing bonanza");
            FishingBonanza.IsBonanzaActive = true;
            _moveSeagulls = true;
            StartCoroutine(MoveSeagulls(seagulls.transform, GameState.currentBoat));
            yield return new WaitForSeconds(fishingBonanzaDuration.Value);

            LogDebug("Stopping fishing bonanza");
            FishingBonanza.IsBonanzaActive = false;
            _moveSeagulls = false;
            Destroy(seagulls);
        }

        private IEnumerator MoveSeagulls(Transform seagulls, Transform boat)
        {
            while (_moveSeagulls)
            {
                var targetPosition = boat.position + boat.up * 40f;
                seagulls.position = Vector3.Lerp(seagulls.position, targetPosition, 0.2f * Time.deltaTime);

                var newRotation = seagulls.eulerAngles;
                newRotation.y = Mathf.LerpAngle(seagulls.eulerAngles.y, boat.eulerAngles.y - 90, 0.2f * Time.deltaTime);
                seagulls.rotation = Quaternion.Euler(newRotation);

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
