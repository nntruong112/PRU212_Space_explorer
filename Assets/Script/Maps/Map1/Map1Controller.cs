using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Map1Controller : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject asteroidSpawnerPrefab;
    public GameObject coreBossSpawnPrefab;

    [Header("Phase Settings")]
    public float asteroidPhaseDuration = 40f;

    //[Header("Music")]
    //public AudioClip map1Music;
    //public float musicVolume = 0.5f;

    [Header("UI Warning")]
    public GameObject dangerUI;
    public float warningDisplayTime = 3.5f;

    [Header("Audio Warning")]
    public AudioClip warningSound;

    [Header("Boss UI")]
    public GameObject bossHealthBar;

    [Header("Stage Clear UI")]
    public GameObject stageClearImage;

    [Header("Victory Settings")]
    public AudioClip victorySound;
    public float postVictoryDelay = 5f;

    [Header("Transition")]
    public GameObject circleTransitionObject; // Assign in Inspector
    public Animator circleTransitionAnimator; // Assign in Inspector
    public float transitionDuration = 1f;     // Match Animator clip

    [Header("Next Scene")]
    public string targetScene = "Stage2"; // Replace with actual scene name

    private AsteroidSpawner asteroidSpawnerInstance;
    private CoreBossSpawn coreBossSpawnerInstance;

    private float timer = 0f;
    private bool spawningCoroutineStarted = false;

    void Awake()
    {
        if (circleTransitionObject != null && circleTransitionAnimator != null)
        {
            // Enable the UI object if it’s disabled
            circleTransitionObject.SetActive(true);

            // Make sure the canvas group is visible
            var cg = circleTransitionObject.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;

            // Trigger the "StartIn" animation
            circleTransitionAnimator.SetTrigger("StartIn");
        }
    }


    void Start()
    {   
        //AudioManager.PlayStageMusic();

        // Spawn asteroid spawner
        if (asteroidSpawnerPrefab != null)
        {
            GameObject spawnerGO = Instantiate(asteroidSpawnerPrefab);
            asteroidSpawnerInstance = spawnerGO.GetComponent<AsteroidSpawner>();

            if (asteroidSpawnerInstance != null)
                asteroidSpawnerInstance.EnableSpawning(true);
            else
                Debug.LogError("Missing AsteroidSpawner component!");
        }

        // Spawn boss spawner
        if (coreBossSpawnPrefab != null)
        {
            GameObject bossGO = Instantiate(coreBossSpawnPrefab, Vector3.zero, Quaternion.identity);
            coreBossSpawnerInstance = bossGO.GetComponent<CoreBossSpawn>();

            if (coreBossSpawnerInstance == null)
                Debug.LogError("Missing CoreBossSpawn component!");
        }

        // Hide danger UI at start
        if (dangerUI != null)
        {
            dangerUI.SetActive(false);
        }

        // Hide health bar at start
        if (bossHealthBar != null)
        {
            bossHealthBar.SetActive(false);
        }
    }

    void Update()
    {
        if (spawningCoroutineStarted || asteroidSpawnerInstance == null || coreBossSpawnerInstance == null)
            return;

        timer += Time.deltaTime;

        if (timer >= asteroidPhaseDuration)
        {
            spawningCoroutineStarted = true;
            asteroidSpawnerInstance.EnableSpawning(false);
            StartCoroutine(WaitUntilAsteroidsClearedThenSpawnBoss());
        }
    }

    private IEnumerator WaitUntilAsteroidsClearedThenSpawnBoss()
    {
        // Wait until all asteroids are gone
        while (GameObject.FindObjectsOfType<Asteroid>().Length > 0)
        {
            yield return new WaitForSeconds(0.3f);
        }

        // Play warning sound
        if (warningSound != null)
        {
            AudioManager.PlayClip(warningSound, Camera.main.transform.position, 1f);
        }

        // Show danger warning
        if (dangerUI != null)
        {
            dangerUI.SetActive(true);
            yield return new WaitForSeconds(warningDisplayTime);
            dangerUI.SetActive(false);
        }

        // Show health bar before boss spawns
        if (bossHealthBar != null)
        {
            bossHealthBar.SetActive(true);
        }

        yield return new WaitForSeconds(1f);


        // Spawn boss and switch music
        coreBossSpawnerInstance.TriggerSpawnBoss();
        AudioManager.TransitionToBossMusic();
    }

    public void ShowStageClear()
    {
        StartCoroutine(StageClearRoutine());
    }

    private IEnumerator StageClearRoutine()
    {
        // Stop background/boss music
        AudioManager.StopMusic();

        // Play victory sound
        if (victorySound != null)
        {
            AudioManager.PlayClip(victorySound, Camera.main.transform.position, 1f);
        }

        // Show "Stage Clear" image
        if (stageClearImage != null)
        {
            stageClearImage.SetActive(true);
        }

        // Wait a moment before transition
        yield return new WaitForSeconds(postVictoryDelay);

        // Trigger circle transition and load scene
        if (circleTransitionObject != null && circleTransitionAnimator != null)
        {
            var cg = circleTransitionObject.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;

            circleTransitionAnimator.SetTrigger("StartOut");
            yield return new WaitForSeconds(transitionDuration);
        }

        // Load the next scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
    }

}
