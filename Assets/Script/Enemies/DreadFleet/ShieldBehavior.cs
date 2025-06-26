using System.Collections;
using UnityEngine;

public class ShieldBehavior : MonoBehaviour
{
    private Vector3 targetPosition;
    public float moveSpeed = 15f;
    private bool reachedTarget = false;

    [Header("Shield Health")]
    public float maxHealth = 5000f;
    public float currentHealth;
    public GameObject explosionPrefab;

    [Header("Shoot")]
    public GameObject shieldBulletPrefab;
    public float fireRate = 2f; // 2 viên/giây
    private float fireCooldown = 0f;
    public Transform firePoint;
    public float flySpeed = 2f;

    private bool flyToPlayer = false;
    private Vector3 flyTarget;
    private float destroyTimer = 5f;

    [Header("Audio")]
    public AudioClip flyToPlayerSound;
    private AudioSource audioSource;

    private void Start()
    {
        currentHealth = maxHealth;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void SetTargetPosition(Vector3 target)
    {
        targetPosition = target;
    }

    void Update()
    {
        if (flyToPlayer)
        {
            transform.position = Vector3.MoveTowards(transform.position, flyTarget, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, flyTarget) < 0.1f)
            {
                Die();
            }

            return; // Skip everything else while flying to player
        }

        if (!reachedTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                reachedTarget = true;

                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.linearVelocity = Vector2.zero;
            }
        }
    }

    public void StopFiring()
    {
        fireRate = 0f;
    }

    public void FlyToPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector3 fallbackTarget = transform.position + transform.up * 5f;
            Vector3 targetPosition = player.transform.position;
            Vector3 direction = (targetPosition - transform.position).normalized;

            transform.up = -direction;

            // ✅ Lower volume only for this sound
            if (flyToPlayerSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(flyToPlayerSound, 0.4f); // volume: 40%
            }

            StartCoroutine(FlyInDirection(direction));
        }
    }


    IEnumerator FlyInDirection(Vector3 direction)
    {
        float lifetime = 5f;
        float timer = 0f;

        while (this != null && timer < lifetime)
        {
            transform.position += direction * flySpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public void Fire()
    {
        if (shieldBulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(shieldBulletPrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.down * 1.5f;
        }
    }

    public void FlyTo(Vector3 target)
    {
        flyToPlayer = true;
        flyTarget = target;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        // Destroy all bullets
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject bullet in bullets)
        {
            Destroy(bullet);
        }

        // Explosion effect
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
