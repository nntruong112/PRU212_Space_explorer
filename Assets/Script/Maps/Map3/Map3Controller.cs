using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Map3Controller : MonoBehaviour
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
    public string targetScene = "Stage4";

    private BossMovement bossController;
    private bool bossSpawned = false;
    private bool warningTriggered = false;

    private AsteroidSpawner asteroidSpawnerInstance;
    private EnemySpawner2 enemySpawnerInstance;

    private float timer = 0f;

    void Start()
    {
        //AudioManager.PlayStageMusic();
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
            enemySpawnerInstance = go.GetComponent<EnemySpawner2>();
        }

        // UI Init
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
                warningTriggered = true;
                StartCoroutine(WarningBeforeBossThenStart());
            }
        }
    }

    private IEnumerator WarningBeforeBossThenStart()
    {
        if (warningSound != null)
            AudioManager.PlayClip(warningSound, Camera.main.transform.position, 1f);

        if (dangerUI != null)
        {
            dangerUI.SetActive(true);
            yield return new WaitForSeconds(warningDisplayTime);
            dangerUI.SetActive(false);
        }

        if (bossHealthBar != null)
            bossHealthBar.SetActive(true);

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(StartBossPhase());
    }

    private IEnumerator StartBossPhase()
    {
        bossSpawned = true;
        AudioManager.TransitionToBossMusic();

        if (enemySpawnerInstance != null)
        {
            enemySpawnerInstance.EnableSpawning(false);
            enemySpawnerInstance.enabled = false;
        }

        if (asteroidSpawnerInstance != null)
        {
            asteroidSpawnerInstance.EnableSpawning(false);
            asteroidSpawnerInstance.enabled = false;
        }

        // Clear existing enemies and asteroids
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            Destroy(enemy);
        foreach (GameObject asteroid in GameObject.FindGameObjectsWithTag("Asteroid"))
            Destroy(asteroid);

        // 🔁 Trigger circle-out animation
        if (circleTransitionAnimator != null)
        {
            circleTransitionAnimator.SetTrigger("StartOut");
            yield return new WaitForSeconds(transitionDuration);
        }

        // ✅ Spawn Boss
        GameObject boss = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
        bossController = boss.GetComponent<BossMovement>();

        // Health bar setup
        if (bossHealthBar != null)
        {
            BossHealthManagerMap3 bar = bossHealthBar.GetComponent<BossHealthManagerMap3>();
            if (bar != null)
            {
                bar.Init(); // Only one phase
                bossController.maxHealth = bar.maxHealth;
                bossController.currentHealth = bar.maxHealth;
            }
        }

        // 🔁 Trigger circle-in animation after boss appears
        if (circleTransitionAnimator != null)
        {
            circleTransitionAnimator.SetTrigger("StartIn");
            yield return new WaitForSeconds(transitionDuration * 0.5f); // optional: allow a short pause
        }

        StartCoroutine(CheckBossDefeat());
    }


    private IEnumerator CheckBossDefeat()
    {
        while (bossController != null)
        {
            yield return null;
        }

        ShowStageClear();
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
}
