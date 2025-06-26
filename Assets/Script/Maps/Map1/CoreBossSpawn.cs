using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoreBossSpawn : MonoBehaviour
{
    [Header("Prefabs & References")]
    public GameObject coreBossDebugPrefab;
    public GameObject satelliteAsteroidPrefab;
    public GameObject bulletAsteroidPrefab;
    public GameObject coreShield;

    [Header("Phase 2 Settings")]
    public GameObject coreBossPhase2Prefab;
    public GameObject phase2AttackAsteroidPrefab;
    public float phase2AttackDelay = 1f;

    [Header("Orbit Settings")]
    public float orbitRadius = 2f;
    public float orbitSpeed = 30f;
    public int satelliteCount = 10;

    [Header("Core Movement")]
    public float coreMoveSpeed = 0.2f;

    [Header("Attack Settings")]
    public float fireCooldown = 2f;
    public int bulletCount = 8;
    public float bulletSpeed = 3f;

    [Header("Debug Settings")]
    public bool debugMode = false;

    private enum StageState
    {
        Idle,
        OrbitingDefense
    }

    private StageState currentState = StageState.Idle;
    private List<GameObject> orbitingAsteroids = new List<GameObject>();
    private List<float> initialAngles = new List<float>();

    private GameObject coreBoss;
    private float orbitTime = 0f;
    private float fireTimer = 0f;
    private int fireCycleCount = 0;

    private float movementStartTime = 0f;

    void Start()
    {
        if (debugMode)
        {
            currentState = StageState.OrbitingDefense;
        }
    }

    void Update()
    {
        if (debugMode && currentState != StageState.OrbitingDefense)
            return;

        OrbitAsteroids();

        if (currentState == StageState.OrbitingDefense)
        {
            MoveCoreBoss();
            HandleFiring();
        }

        if (coreShield != null && coreBoss != null)
        {
            bool hasSatellites = orbitingAsteroids.Exists(a => a != null);
            coreShield.SetActive(hasSatellites);
            coreShield.transform.position = coreBoss.transform.position;
        }
    }

    public void TriggerSpawnBoss()
    {
        if (coreBoss == null)
        {
            StartCoroutine(DelayedSpawnCoreBoss());
        }
    }

    private IEnumerator DelayedSpawnCoreBoss()
    {
        // Vị trí mục tiêu ngay khi spawn
        Vector3 targetPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.86f, 10f));

        // Spawn boss
        coreBoss = Instantiate(coreBossDebugPrefab, targetPos, Quaternion.identity);
        coreBoss.tag = "CoreBoss";

        // Spawn shield ngay lập tức
        if (coreShield != null)
        {
            coreShield = Instantiate(coreShield, targetPos, Quaternion.identity);
            coreShield.transform.SetParent(coreBoss.transform);
            coreShield.SetActive(true);
        }

        // Spawn vệ tinh ngay lập tức
        yield return StartCoroutine(SpawnOrbitingSatellites());

        // Đứng im 1.5 giây
        yield return new WaitForSeconds(1.5f);

        // Bắt đầu di chuyển sau 1.5 giây
        movementStartTime = Time.time;
        currentState = StageState.OrbitingDefense;
    }

    private IEnumerator SpawnOrbitingSatellites()
    {
        if (coreBoss == null) yield break;

        orbitTime = 0f;
        orbitingAsteroids.Clear();
        initialAngles.Clear();

        for (int i = 0; i < satelliteCount; i++)
        {
            float angle = i * (360f / satelliteCount);
            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0) * orbitRadius;

            GameObject satellite = Instantiate(satelliteAsteroidPrefab, coreBoss.transform.position + offset, Quaternion.identity);
            orbitingAsteroids.Add(satellite);
            initialAngles.Add(angle);

            Asteroid script = satellite.GetComponent<Asteroid>();
            if (script != null)
                script.isOrbiting = true;
        }

        yield return null;
    }

    private void OrbitAsteroids()
    {
        if (coreBoss == null) return;

        orbitTime += Time.deltaTime;
        for (int i = 0; i < orbitingAsteroids.Count; i++)
        {
            if (orbitingAsteroids[i] == null) continue;

            float angle = initialAngles[i] + orbitTime * orbitSpeed;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0) * orbitRadius;

            orbitingAsteroids[i].transform.position = coreBoss.transform.position + offset;
        }
    }

    private void MoveCoreBoss()
    {
        if (coreBoss == null) return;

        Vector3 min = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0.7f, 10));
        Vector3 max = Camera.main.ViewportToWorldPoint(new Vector3(1f, 0.95f, 10));

        float time = (Time.time - movementStartTime) * coreMoveSpeed;

        float x = Mathf.Lerp(min.x, max.x, (Mathf.Sin(time) + 1f) / 2f);
        float y = Mathf.Lerp(min.y, max.y, (Mathf.Cos(time * 0.5f) + 1f) / 2f);

        coreBoss.transform.position = new Vector3(x, y, coreBoss.transform.position.z);
    }

    private void HandleFiring()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireCooldown)
        {
            fireTimer = 0f;
            if (fireCycleCount < 3)
            {
                FireAsteroidBullets();
                fireCycleCount++;
            }
            else
            {
                fireCycleCount = 0;
                StartCoroutine(FireHalfCircleAtPlayer());
            }
        }
    }

    private void FireAsteroidBullets()
    {
        if (coreBoss == null || bulletAsteroidPrefab == null) return;

        float spread = 360f / bulletCount;
        float angleOffset = orbitTime * 60f;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = (i * spread + angleOffset) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

            SpawnBullet(coreBoss.transform.position, dir);
        }
    }

    private IEnumerator FireHalfCircleAtPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || coreBoss == null) yield break;

        Vector3 toPlayer = (player.transform.position - coreBoss.transform.position).normalized;
        float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

        int halfBulletCount = bulletCount;
        float spread = 180f / (halfBulletCount - 1);

        for (int i = 0; i < halfBulletCount; i++)
        {
            float angle = (baseAngle - 90f) + i * spread;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);

            SpawnBullet(coreBoss.transform.position, dir);
        }

        yield return null;
    }

    private void SpawnBullet(Vector3 position, Vector3 direction)
    {
        GameObject bullet = Instantiate(bulletAsteroidPrefab, position, Quaternion.identity);

        Collider2D bulletCol = bullet.GetComponent<Collider2D>();
        if (bulletCol != null)
        {
            bulletCol.isTrigger = true;
            foreach (var orbiting in orbitingAsteroids)
            {
                if (orbiting == null) continue;
                Collider2D orbCol = orbiting.GetComponent<Collider2D>();
                if (orbCol != null)
                    Physics2D.IgnoreCollision(bulletCol, orbCol);
            }
        }

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = direction.normalized * bulletSpeed;
        }

        Asteroid script = bullet.GetComponent<Asteroid>();
        if (script != null)
        {
            script.isOrbiting = false;
        }

        Destroy(bullet, 5f);
    }

    public void ResetSatellites()
    {
        foreach (var obj in orbitingAsteroids)
        {
            if (obj != null)
                Destroy(obj);
        }

        orbitingAsteroids.Clear();
        initialAngles.Clear();

        StartCoroutine(SpawnOrbitingSatellites());
    }

    public void EnterPhase2()
    {
        if (coreBoss == null) return;

        Vector3 currentPos = coreBoss.transform.position + new Vector3(0f, 0.2f, 0f);
        Quaternion currentRot = coreBoss.transform.rotation;

        Destroy(coreBoss);

        coreBoss = Instantiate(coreBossPhase2Prefab, currentPos, currentRot);
        coreBoss.tag = "CoreBoss";

        CoreBoss bossScript = coreBoss.GetComponent<CoreBoss>();
        if (bossScript != null)
        {
            bossScript.isPhase2Boss = true;
        }

        coreMoveSpeed = 0.32f;                
        satelliteCount = 16;                 
        orbitSpeed = 60f;                    
        fireCooldown = 1f;                   
        bulletCount = 10;                    

        // Spawn shield ngay lập tức
        if (coreShield != null)
        {
            coreShield = Instantiate(coreShield, currentPos, Quaternion.identity);
            coreShield.transform.SetParent(coreBoss.transform);
            coreShield.SetActive(true);
        }

        // Spawn vệ tinh ngay lập tức
        ResetSatellites();

        // Đứng im 5 giây ở phase 2
        StartCoroutine(DelayPhase2Start());
    }

    private IEnumerator DelayPhase2Start()
    {
        yield return new WaitForSeconds(5f); // Đứng im 5 giây
        movementStartTime = Time.time; // Reset thời gian di chuyển
        currentState = StageState.OrbitingDefense; // Bật trạng thái di chuyển
        StartCoroutine(Phase2StrongAttackRoutine()); // Bắt đầu tấn công phase 2
    }

    private IEnumerator Phase2StrongAttackRoutine()
    {
        yield return new WaitForSeconds(phase2AttackDelay);

        int count = 8;
        float interval = 0.3f;

        for (int i = 0; i < count; i++)
        {
            if (coreBoss == null) yield break;

            Vector3 spawnPos = new Vector3(
                coreBoss.transform.position.x,
                Camera.main.ViewportToWorldPoint(new Vector3(0, 1.1f, 10)).y,
                0);

            GameObject asteroid = Instantiate(phase2AttackAsteroidPrefab, spawnPos, Quaternion.identity);

            Rigidbody2D rb = asteroid.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.down * bulletSpeed * 1.5f;

            Destroy(asteroid, 5f);

            yield return new WaitForSeconds(interval);
        }
    }

    public bool AreAllSatellitesDestroyed()
    {
        orbitingAsteroids.RemoveAll(item => item == null);
        return orbitingAsteroids.Count == 0;
    }

    public bool CanDamageCore()
    {
        return AreAllSatellitesDestroyed();
    }
}