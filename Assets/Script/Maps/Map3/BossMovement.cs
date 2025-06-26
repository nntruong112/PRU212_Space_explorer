using System.Collections;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float minX = -6f;
    public float maxX = 6f;
    public float fixedY = 1f;

    public AudioClip bossDestroy;
    public AudioSource bossAudioSource;

    public GameObject[] explosionPrefabs; // Size = 3


    public GameObject bulletPrefab;
    public GameObject bulletPrefabFanSpam;
    public Transform firePoint;
    public float fireRate = 5f;

    private int direction = 1;
    private float fireCooldown = 0f;

    private enum BossState { Entering, Moving, Shooting, SpamRandom, FanRandomSpam, ShieldFormation }
    private BossState currentState = BossState.Moving;

    private float stateTimer = 0f;

    [Header("Boss HP")]
    public float maxHealth = 10000f;
    public float currentHealth;

    [Header("Fan Random Spam")]
    public Transform fanSpamFirePoint;
    public float fanSpamRate = 12f; // số viên/giây
    public float fanSpreadAngle = 60f; // tổng góc mở (ví dụ 60 độ)
    private float fanSpamCooldown = 0f;

    [Header("Shield Settings")]
    public GameObject shieldPrefab;
    public Transform shieldFirePoint;
    private GameObject[] shields; // để giữ tham chiếu đến các khiên đã tạo
    private bool shieldsCreated = false;

    private float shieldFireTimer = 0f;
    private int shieldFirePhase = 0;

    private bool shouldMoveAfterAttack = false;

    private BossState previousAttack = BossState.Shooting;

    private BossHealthManagerMap3 healthBar;


    void Start()
    {
        healthBar = GameObject.FindObjectOfType<BossHealthManagerMap3>();
        if (healthBar != null)
        {
            healthBar.Init();                         // Set max and UI color
            maxHealth = healthBar.maxHealth;          // Sync max
            currentHealth = healthBar.GetCurrentHealth(); // Sync current
        }

        currentState = BossState.Entering;
        stateTimer = 5f;
    }


    void Update()
    {
        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case BossState.Entering:
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, fixedY, 0), moveSpeed * Time.deltaTime);
                if (Mathf.Abs(transform.position.y - fixedY) < 0.05f)
                {
                    transform.position = new Vector3(transform.position.x, fixedY, 0);
                    currentState = BossState.Moving;
                    stateTimer = 5f;
                }
                break;

            case BossState.Moving:
                Move();
                if (stateTimer <= 0f)
                {
                    if (shouldMoveAfterAttack)
                    {
                        shouldMoveAfterAttack = false;
                        currentState = GetNextAttackState(); // custom function
                    }
                    else
                    {
                        currentState = BossState.Shooting;
                    }
                    stateTimer = GetStateDuration(currentState);
                }
                break;

            case BossState.Shooting:
                Shoot();
                if (stateTimer <= 0f)
                {
                    previousAttack = BossState.Shooting;
                    currentState = BossState.Moving;
                    stateTimer = 5f;
                    shouldMoveAfterAttack = true;
                }
                break;

            case BossState.FanRandomSpam:
                SpamRandomFanShoot();
                if (stateTimer <= 0f)
                {
                    previousAttack = BossState.FanRandomSpam;
                    currentState = BossState.Moving;
                    stateTimer = 5f;
                    shouldMoveAfterAttack = true;
                }
                break;

            case BossState.ShieldFormation:
                if (!shieldsCreated)
                {
                    CreateShields();
                    shieldsCreated = true;
                }

                HandleShieldFiring();

                if (AllShieldsDestroyed())
                {
                    previousAttack = BossState.ShieldFormation;
                    currentState = BossState.Moving;
                    shieldsCreated = false;
                    stateTimer = 5f;
                    shouldMoveAfterAttack = true;
                }
                break;
        }
    }

    private BossState GetNextAttackState()
    {
        switch (previousAttack)
        {
            case BossState.Shooting:
                return BossState.FanRandomSpam;
            case BossState.FanRandomSpam:
                return BossState.ShieldFormation;
            case BossState.ShieldFormation:
            default:
                return BossState.Shooting;
        }
    }

    private float GetStateDuration(BossState state)
    {
        switch (state)
        {
            case BossState.Shooting: return 6f;
            case BossState.FanRandomSpam: return 6f;
            case BossState.ShieldFormation: return Mathf.Infinity; // until all shields destroyed
            default: return 5f;
        }
    }

    void Move()
    {
        transform.position += new Vector3(direction * moveSpeed * Time.deltaTime, 0, 0);

        if (transform.position.x > maxX)
        {
            transform.position = new Vector3(maxX, fixedY, 0);
            direction = -1;
        }
        else if (transform.position.x < minX)
        {
            transform.position = new Vector3(minX, fixedY, 0);
            direction = 1;
        }

        transform.position = new Vector3(transform.position.x, fixedY, 0);
    }

    void Shoot()
    {
        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f)
        {
            GameObject player = GameObject.FindWithTag("Player");

            if (player != null && bulletPrefab != null && firePoint != null)
            {
                // Tính hướng đến player
                Vector3 direction = (player.transform.position - firePoint.position).normalized;

                // Tạo đạn tại firePoint
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

                // Thiết lập hướng cho đạn
                BossBullet bulletScript = bullet.GetComponent<BossBullet>();
                if (bulletScript != null)
                {
                    bulletScript.SetDirection(direction);
                }

                // Reset cooldown
                fireCooldown = 1f / fireRate;
            }
        }
    }

    void SpamRandomFanShoot()
    {
        fanSpamCooldown -= Time.deltaTime;

        if (fanSpamCooldown <= 0f && fanSpamFirePoint != null && bulletPrefab != null)
        {
            // Chọn ngẫu nhiên một góc trong hình cánh quạt (ví dụ -30° đến +30°)
            float angle = Random.Range(-fanSpreadAngle / 2f, fanSpreadAngle / 2f);
            float rad = angle * Mathf.Deg2Rad;

            // Tính vector hướng từ góc
            Vector3 direction = new Vector3(Mathf.Sin(rad), -Mathf.Cos(rad), 0f).normalized;

            GameObject bullet = Instantiate(bulletPrefabFanSpam, fanSpamFirePoint.position, Quaternion.identity);
            
            float angleDeg = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + -90;
            bullet.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

            BossBullet bulletScript = bullet.GetComponent<BossBullet>();
            if (bulletScript != null)
                bulletScript.SetDirection(direction);

            fanSpamCooldown = 1f / fanSpamRate;
        }
    }

    void CreateShields()
    {
        int shieldCount = 5;
        shields = new GameObject[shieldCount];

        float spacingX = 2f;  // ngang
        float spacingY = 0.7f; // dọc
        float fireDistance = 3f; // khoảng boss bắn ra khiên

        Vector3 basePosition = shieldFirePoint.position + new Vector3(0, -fireDistance, 0); // bắn xuống dưới boss

        for (int i = 0; i < shieldCount; i++)
        {
            // Spawn tại firePoint (tất cả bắt đầu từ cùng 1 điểm)
            GameObject shield = Instantiate(shieldPrefab, shieldFirePoint.position, Quaternion.identity);
            StartCoroutine(DelayedShieldAttackToPlayer());
            shields[i] = shield;

            Vector3 offset = Vector3.zero;

            // Tạo mũi nhọn hướng xuống — cái giữa nằm thấp nhất
            switch (i)
            {
                case 0: offset = new Vector3(-2 * spacingX, 0.5f * spacingY, 0); break; // trái xa hơn chút
                case 1: offset = new Vector3(-1 * spacingX, 1.5f * spacingY, 0); break; // trái gần
                case 2: offset = new Vector3(0, 2f * spacingY, 0); break;             // giữa sâu nhất
                case 3: offset = new Vector3(1 * spacingX, 1.5f * spacingY, 0); break;  // phải gần
                case 4: offset = new Vector3(2 * spacingX, 0.5f * spacingY, 0); break;  // phải xa hơn chút
            }

            Vector3 targetPosition = basePosition - offset; // vì base đã ở dưới boss rồi

            ShieldBehavior shieldScript = shield.GetComponent<ShieldBehavior>();
            if (shieldScript != null)
            {
                shieldScript.SetTargetPosition(targetPosition);
            }
        }
    }

    IEnumerator DelayedShieldAttackToPlayer()
    {
        yield return new WaitForSeconds(8f); // Chờ 15 giây sau khi spawn

        for (int i = 0; i < shields.Length; i++)
        {
            GameObject shield = shields[i];
            if (shield != null)
            {
                ShieldBehavior sb = shield.GetComponent<ShieldBehavior>();
                if (sb != null)
                {
                    sb.StopFiring();         // Ngừng bắn đạn
                    sb.FlyToPlayer();       // Bay về phía player
                }

                yield return new WaitForSeconds(1f); // delay giữa từng shield
            }
        }
    }

    void HandleShieldFiring()
    {
        shieldFireTimer -= Time.deltaTime;
        if (shieldFireTimer > 0f || shields == null) return;

        // Danh sách index tương ứng theo lượt
        int[][] firePhases = new int[][]
        {
        new int[] {0, 2},       // Lượt 1: shield 1 và 3
        new int[] {1, 3, 4}     // Lượt 2: shield 2, 4, 5
        };

        int[] currentPhase = firePhases[shieldFirePhase % firePhases.Length];

        foreach (int index in currentPhase)
        {
            if (index < shields.Length && shields[index] != null)
            {
                ShieldBehavior shieldScript = shields[index].GetComponent<ShieldBehavior>();
                if (shieldScript != null)
                {
                    shieldScript.Fire();
                }
            }
        }

        shieldFirePhase = (shieldFirePhase + 1) % firePhases.Length;
        shieldFireTimer = 3f; // delay giữa mỗi lượt bắn
    }

    bool AllShieldsDestroyed()
    {
        if (shields == null) return true;

        foreach (GameObject shield in shields)
        {
            if (shield != null)
                return false;
        }
        return true;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (healthBar != null)
            healthBar.TakeDamage(damage);

        if (currentHealth <= 0)
            Die();
    }


    void Die()
    {

        //Destroy(gameObject);
        StartCoroutine(ExplodeSequence());

        // TODO: trigger win, boss clear, animation, v.v.
    }

    private IEnumerator ExplodeSequence()
{
    int repeatCount = 1;
    float delayBetween = 0.5f; // time between explosions
        AudioManager.PlayClip(bossDestroy, transform.position);
        AudioManager.PlayClip(bossDestroy, transform.position);
        AudioManager.PlayClip(bossDestroy, transform.position);

        for (int i = 0; i < repeatCount; i++)
    {
        foreach (GameObject prefab in explosionPrefabs)
        {
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
                yield return new WaitForSeconds(delayBetween);
            }
        }
    }

    Destroy(gameObject); // Clean up after all explosions
}

}
