using System.Collections;
using UnityEngine;

public class VerticalEnemyController : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float speed = 3f;
    public float fireRate = 0.2f;
    public float startDelay = 0.5f;
    private Vector2 moveDirection;

    public Transform firePoint;
    public GameObject explosionPrefab;

    public float maxHealth = 1000f;
    private float currentHealth;

    [Header("Loot Prefabs")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private GameObject powerupPrefab;
    [SerializeField] private GameObject starPrefab;

    public void Initialize(Vector2 direction)
    {
        moveDirection = direction;
    }

    void Start()
    {
        currentHealth = maxHealth;
        StartCoroutine(FireContinuously());
    }

    void Update()
    {
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

        if (transform.position.y < -Camera.main.orthographicSize - 1f)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator FireContinuously()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            Shoot();
            yield return new WaitForSeconds(fireRate);
        }
    }

    void Shoot()
    {
        Vector3 firePos = firePoint != null ? firePoint.position : transform.position;

        GameObject bullet = Instantiate(bulletPrefab, firePos, Quaternion.identity);

        if (bullet.TryGetComponent(out VerticalEnemyBulletController bulletCtrl))
        {
            bulletCtrl.direction = Vector2.down;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        // Spawn explosion
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
            Instantiate(heartPrefab, transform.position + new Vector3(-0.3f, 0.4f, 0), Quaternion.identity);
        }
        // ⚡ 20% chance to drop Powerup (roll > 0.10 to 0.30)
        else if (roll <= 0.30f && powerupPrefab != null)
        {
            Instantiate(powerupPrefab, transform.position + new Vector3(0.4f, -0.3f, 0), Quaternion.identity);
        }

        Destroy(gameObject);
    }

}
