using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Map2Controller : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject enemySpawnerPrefab;
    public GameObject asteroidSpawnerPrefab;
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;

    [Header("Phase Settings")]
    public float waveDuration = 60f;

    [Header("UI Warning")]
    public GameObject dangerUI;
    public float warningDisplayTime = 3.5f;

    [Header("Boss UI")]
    public GameObject bossHealthBar;

    [Header("Stage Clear UI")]
    public GameObject stageClearImage;

    [Header("Audio")]
    public AudioClip warningSound;
    public AudioClip victorySound;

    [Header("Transition")]
    public GameObject circleTransitionObject;
    public Animator circleTransitionAnimator;
    public float transitionDuration = 1f;

    [Header("Scene Transition")]
    public string targetScene = "Stage3";

    private EnemyBossController bossController;
    private bool bossSpawned = false;
    private bool warningTriggered = false;
    private bool phase2Triggered = false;

    private AsteroidSpawner asteroidSpawnerInstance;
    private EnemySpawner enemySpawnerInstance;

    private float timer = 0f;

    void Start()
    {
        //AudioManager.ChangeMusic
        // Asteroid Spawner
        if (asteroidSpawnerPrefab != null)
        {
            var go = Instantiate(asteroidSpawnerPrefab);
            asteroidSpawnerInstance = go.GetComponent<AsteroidSpawner>();
            asteroidSpawnerInstance?.EnableSpawning(true);
        }

        // Enemy Spawner
        if (enemySpawnerPrefab != null)
        {
            var go = Instantiate(enemySpawnerPrefab);
            go.SetActive(true);
            enemySpawnerInstance = go.GetComponent<EnemySpawner>();
        }

        // UI Initialization
        if (dangerUI != null) dangerUI.SetActive(false);
        if (bossHealthBar != null) bossHealthBar.SetActive(false);

        // Transition In
        if (circleTransitionObject != null && circleTransitionAnimator != null)
        {
            circleTransitionObject.SetActive(true);
            var cg = circleTransitionObject.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
            circleTransitionAnimator.SetTrigger("StartIn");
        }
    }

    void Update()
    {
        if (!bossSpawned && !warningTriggered)
        {
            timer += Time.deltaTime;
            if (timer >= waveDuration)
            {
                warningTriggered = true; // Prevent repeated coroutine starts
                StartCoroutine(WarningBeforeBossThenStart());
            }
        }
        else if (bossController != null && !phase2Triggered)
        {
            if (bossController.IsPhase2())
            {
                phase2Triggered = true;
                // Add phase 2 effects or music here
            }
        }
    }

    private IEnumerator WarningBeforeBossThenStart()
    {
        // Warning sound
        if (warningSound != null)
            AudioManager.PlayClip(warningSound, Camera.main.transform.position, 1f);

        // Show danger warning
        if (dangerUI != null)
        {
            dangerUI.SetActive(true);
            yield return new WaitForSeconds(warningDisplayTime);
            dangerUI.SetActive(false);
        }

        // Show boss health bar
        if (bossHealthBar != null)
            bossHealthBar.SetActive(true);

        yield return new WaitForSeconds(1f);

        StartBossPhase();
    }

    public void ShowStageClear()
    {
        StartCoroutine(StageClearRoutine());
    }

    private IEnumerator StageClearRoutine()
    {
        AudioManager.StopMusic();

        if (victorySound != null)
            AudioManager.PlayClip(victorySound, Camera.main.transform.position, 1f);

        if (stageClearImage != null)
            stageClearImage.SetActive(true);

        yield return new WaitForSeconds(6f);

        if (circleTransitionObject != null && circleTransitionAnimator != null)
        {
            var cg = circleTransitionObject.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;

            circleTransitionAnimator.SetTrigger("StartOut");
            yield return new WaitForSeconds(transitionDuration);
        }

        SceneManager.LoadScene(targetScene);
    }

    // 🔥 Your original boss spawn logic
    void StartBossPhase()
    {
        if (enemySpawnerInstance != null)
            enemySpawnerInstance.enabled = false;

        if (asteroidSpawnerInstance != null)
            asteroidSpawnerInstance.EnableSpawning(false);
        AudioManager.TransitionToBossMusic();

        GameObject boss = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
        boss.transform.Rotate(0, 0, -180f);

        bossController = boss.GetComponent<EnemyBossController>();
        bossSpawned = true;

        // ✅ Set up health UI
        if (bossHealthBar != null)
        {
            bossHealthBar.SetActive(true);
            BossHealthManagerMap2 bar = bossHealthBar.GetComponent<BossHealthManagerMap2>();
            if (bar != null)
            {
                bossController.SetHealthBar(bar);
            }
        }
    }

}
