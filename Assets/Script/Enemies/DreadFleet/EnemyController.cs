using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float maxHealth = 1000;
    private float currentHealth;
    private float speed;
    private float offScreenMargin = 1f;
    private float leftBound;
    private float rightBound;

    private float waveAmplitude = 0.5f;
    private float waveFrequency = 2f;
    private float startY;
    private float waveOffset;

    public GameObject bulletPrefab;
    public float shootInterval = 2f;
    public float bulletSpreadAngle = 12f;
    public Transform firePoint;
    public GameObject explosionPrefab;

    private float shootTimer;
    private bool isDiagonal = false;
    private Vector2 moveDirection;

    [Header("Loot Prefabs")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private GameObject powerupPrefab;
    [SerializeField] private GameObject starPrefab;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void Initialize(float moveSpeed)
    {
        speed = moveSpeed;

        float screenHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        leftBound = -screenHalfWidth - offScreenMargin;
        rightBound = screenHalfWidth + offScreenMargin;

        startY = transform.position.y;
        waveOffset = Random.Range(0f, Mathf.PI * 2);

        if (firePoint == null)
        {
            Debug.LogWarning($"FirePoint is not assigned on {gameObject.name}. Using default position.");
            firePoint = transform;
        }
    }

    void Update()
    {
        if (isDiagonal)
        {
            transform.position += (Vector3)(moveDirection * Time.deltaTime);

            if (transform.position.y < -Camera.main.orthographicSize - 1f)
                Destroy(gameObject);
        }
        else
        {
            float xMove = speed * Time.deltaTime;
            float yOffset = Mathf.Sin(Time.time * waveFrequency + waveOffset) * waveAmplitude;
            transform.position += new Vector3(xMove, yOffset * Time.deltaTime, 0);

            if (transform.position.x < leftBound || transform.position.x > rightBound)
                Destroy(gameObject);
        }

        shootTimer += Time.deltaTime;
        if (shootTimer >= shootInterval)
        {
            ShootBullets();
            shootTimer = 0f;
        }
    }

    void ShootBullets()
    {
        Vector3 firePos = firePoint.position;
        float angleRad = bulletSpreadAngle * Mathf.Deg2Rad;

        Vector2 dir1 = new Vector2(-Mathf.Sin(angleRad), -Mathf.Cos(angleRad));
        Vector2 dir2 = new Vector2(Mathf.Sin(angleRad), -Mathf.Cos(angleRad));

        GameObject bullet1 = Instantiate(bulletPrefab, firePos, Quaternion.identity);
        bullet1.GetComponent<EnemyBulletController>().direction = dir1;

        GameObject bullet2 = Instantiate(bulletPrefab, firePos, Quaternion.identity);
        bullet2.GetComponent<EnemyBulletController>().direction = dir2;
    }

    public void InitializeDiagonal(Vector2 direction)
    {
        isDiagonal = true;
        moveDirection = direction;

        float screenHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        leftBound = -screenHalfWidth - offScreenMargin;
        rightBound = screenHalfWidth + offScreenMargin;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // ⭐ Always drop 2 stars
        if (starPrefab != null)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0f);
                Instantiate(starPrefab, transform.position + offset, Quaternion.identity);
            }
        }

        // 💓 10% chance to drop Heart
        float roll = Random.value;
        if (roll <= 0.10f && heartPrefab != null)
        {
            Instantiate(heartPrefab, transform.position + new Vector3(-0.3f, 0.3f, 0f), Quaternion.identity);
        }
        // ⚡ 20% chance to drop Powerup (roll > 0.10 to 0.30)
        else if (roll <= 0.30f && powerupPrefab != null)
        {
            Instantiate(powerupPrefab, transform.position + new Vector3(0.3f, -0.3f, 0f), Quaternion.identity);
        }

        Destroy(gameObject);
    }

}
