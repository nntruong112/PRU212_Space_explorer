using System.Collections.Generic;
using UnityEngine;

public class EnemyBossController : MonoBehaviour
{
    [Header("Boss Stats")]
    public float maxHealth = 150000f;
    private float currentHealth;
    public GameObject explosionPrefab;

    [Header("Movement Control")]
    public float[] movePointsX = { -3f, 0f, 3f };
    public float moveSpeed = 4f;
    public float pauseTimeAtPoint = 2f;
    public float stopBeamBeforeMove = 1f;

    private int currentTargetIndex = 0;
    private bool isMoving = true;
    private float pauseTimer = 0f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    private float nextFireTime;
    public int bulletCount = 12;
    public float bulletSpeed = 5f;

    [Header("Beam Attack")]
    public GameObject linePrefab;
    private float totalBeamLifetime = 0f;
    public float lineFireRate = 2f;
    private float nextLineFireTime;
    public float postBeamDelay = 1f;
    private bool canStartBeamPhase = false;
    private float postBeamDelayTimer = 0f;

    public float fanAngle = 60f;
    private float currentAngleOffset = 0f;

    [Header("Debug Settings")]
    public bool debugForcePhase2 = false;

    [Header("Phase 2 settings")]
    private bool isPhase2 = false;
    private float phase2HealthThreshold;

    public float beamDuration = 5f;
    public float beamCooldown = 8f;

    private float beamPhaseStartTime = -Mathf.Infinity;
    private bool isBeamPhase = false;
    private bool allowCircleFire = true;

    private float nextBeamShootTime = 0f;

    private bool isEnteringScene = true;
    private Vector3 entryTargetPos;

    private BossHealthManagerMap2 healthBar;

    void Start()
    {
        currentHealth = maxHealth;
        phase2HealthThreshold = maxHealth * 0.5f;

        // Auto-assign health bar like CoreBoss
        healthBar = GameObject.FindObjectOfType<BossHealthManagerMap2>();
        if (healthBar != null)
        {
            healthBar.InitPhase1();
        }

        Vector3 spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.2f, Mathf.Abs(Camera.main.transform.position.z)));
        spawnPos.z = 0f;
        transform.position = spawnPos;

        entryTargetPos = new Vector3(0f, 5f, 0f);

        LaserWarningLine warning = linePrefab.GetComponent<LaserWarningLine>();
        if (warning != null)
        {
            totalBeamLifetime = warning.delayBeforeFire + warning.beamDuration;
        }

        if (debugForcePhase2)
        {
            isPhase2 = true;
            StartBeamPhase();
        }
    }

    void Update()
    {
        if (isEnteringScene)
        {
            transform.position = Vector3.MoveTowards(transform.position, entryTargetPos, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, entryTargetPos) < 0.01f)
                isEnteringScene = false;

            return;
        }

        HandleMovement();

        if (!isPhase2 && currentHealth <= phase2HealthThreshold)
        {
            EnterPhase2();
        }

        if (isPhase2)
        {
            if (isBeamPhase)
            {
                if (Time.time - beamPhaseStartTime <= beamDuration)
                {
                    if (!isMoving && Time.time >= nextBeamShootTime)
                    {
                        ShootRandomBeams();
                        nextBeamShootTime = Time.time + 1f;
                    }
                }
                else
                {
                    EndBeamPhase();
                }
            }
            else if (Time.time - beamPhaseStartTime >= beamCooldown && canStartBeamPhase)
            {
                StartBeamPhase();
            }
        }

        if (!isMoving && pauseTimer > stopBeamBeforeMove && Time.time >= nextLineFireTime && !isBeamPhase)
        {
            ShootLine();
            nextLineFireTime = Time.time + 1f / lineFireRate;
        }

        if (allowCircleFire && Time.time >= nextFireTime)
        {
            ShootCircle();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    public void SetHealthBar(BossHealthManagerMap2 bar)
    {
        healthBar = bar;
        if (healthBar != null)
        {
            healthBar.InitPhase1();
        }
    }

    public bool IsPhase2() => isPhase2;

    void EnterPhase2()
    {
        healthBar.InitPhase2();
        isPhase2 = true;
        moveSpeed *= 3f;
        fireRate *= 1.5f;
        lineFireRate *= 2f;
        bulletSpeed *= 1.1f;
        pauseTimeAtPoint *= 0.5f;
        beamCooldown *= 0.6f;
        StartBeamPhase();
    }

    void StartBeamPhase()
    {
        isBeamPhase = true;
        canStartBeamPhase = false;
        allowCircleFire = false;
        beamPhaseStartTime = Time.time;
        nextBeamShootTime = Time.time;
    }

    void EndBeamPhase()
    {
        isBeamPhase = false;
        allowCircleFire = true;
        beamPhaseStartTime = Time.time;
        postBeamDelayTimer = totalBeamLifetime + postBeamDelay;
    }

    void HandleMovement()
    {
        if (isMoving)
        {
            Vector3 targetPos = new Vector3(movePointsX[currentTargetIndex], transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                isMoving = false;
                pauseTimer = pauseTimeAtPoint;

                if (isPhase2)
                    canStartBeamPhase = true;
            }
        }
        else
        {
            if (isBeamPhase || postBeamDelayTimer > 0f)
            {
                postBeamDelayTimer -= Time.deltaTime;
                return;
            }

            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                currentTargetIndex = (currentTargetIndex + 1) % movePointsX.Length;
                isMoving = true;
            }
        }
    }

    void ShootCircle()
    {
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = currentAngleOffset + angleStep * i;
            Vector2 localDir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector2 worldDir = firePoint.TransformDirection(localDir).normalized;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = worldDir * bulletSpeed;
            }
        }

        currentAngleOffset += 10f;
        if (currentAngleOffset >= 360f) currentAngleOffset -= 360f;
    }

    void ShootRandomBeams()
    {
        int beamCount = 5;
        for (int i = 0; i < beamCount; i++)
        {
            float angle = Random.Range(0f, 360f);
            Quaternion rotation = Quaternion.Euler(0, 0, angle + 90f);
            Vector2 direction = rotation * Vector2.up;
            Vector3 firePos = firePoint.position;

            GameObject line = Instantiate(linePrefab, firePos, rotation);
            float maxDistance = 130f;

            LaserWarningLine warning = line.GetComponent<LaserWarningLine>();
            if (warning != null)
            {
                warning.Setup(direction, maxDistance);
            }
        }
    }

    void ShootLine()
    {
        float randomAngle = Random.Range(-fanAngle / 2f, fanAngle / 2f);
        Quaternion rotation = Quaternion.Euler(0, 0, randomAngle + 90);
        Vector2 direction = rotation * Vector2.down;
        Vector3 firePos = firePoint.position;

        GameObject line = Instantiate(linePrefab, firePos, rotation);
        float maxDistance = 130f;

        LaserWarningLine warning = line.GetComponent<LaserWarningLine>();
        if (warning != null)
        {
            warning.Setup(direction, maxDistance);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (healthBar != null)
        {
            healthBar.TakeDamage(amount);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        GameObject[] existingLines = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject line in existingLines)
        {
            Destroy(line);
        }

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.Euler(0, 0, -180));
        }

        Map2Controller controller = FindObjectOfType<Map2Controller>();
        if (controller != null)
        {
            controller.ShowStageClear();
        }

        Destroy(gameObject);
    }
}
