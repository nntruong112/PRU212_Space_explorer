
using UnityEngine;
using UnityEngine.InputSystem;

public class Asteroid : MonoBehaviour
{
    public enum AsteroidSize { Small, Medium, Large }
    public AsteroidSize size = AsteroidSize.Small;
    public float minMoveSpeed = 1f;
    public float maxMoveSpeed = 3f;
    public float scoreValue = 100f;
    public float health = 0f;
    public float pushForce = 0.1f;
    public GameObject starPrefab;
    public GameObject[] mediumAsteroid;
    public AudioClip asteroidDestroySound;
    public AudioSource asteroidAudioSource;
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private GameObject powerupPrefab;
    private float moveSpeed;
    private float rotationSpeed;
    private Vector2 moveDirection;
    private Rigidbody2D rb;

    public GameObject explosionPrefab;

    public bool isCoreAsteroid = false;

    public GameObject coreBossPrefab;

    public bool isOrbiting = false;

    private bool isVisible = false;

    private Camera mainCamera;

    private void OnBecameVisible()
    {
        isVisible = true;
    }

    void OnBecameInvisible()
    {
        isVisible = false;
        if (!isOrbiting && !isCoreAsteroid )
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        //Init value
        pushForce = 80f;
        switch (size)
        {
            case AsteroidSize.Small:
                health = 400f;
                scoreValue = 100f;
                break;
            case AsteroidSize.Medium:
                health = 600f;
                scoreValue = 200f;
                break;
            case AsteroidSize.Large:
                health = 1000f;
                scoreValue = 300f;
                break;
        }

        //Difficulty
        float difficultyLevel = 0.3f + (GameManager.Instance.difficultyLevel * 0.5f);

        //Random move and rotation
        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed) * difficultyLevel;
        float rotationSpeedFactor = 40f;
        rotationSpeed = moveSpeed * rotationSpeedFactor * (Random.value < 0.5f ? -1f : 1f);

        float xOffset = Random.Range(-0.3f, 0.3f); 
        moveDirection = new Vector2(xOffset, -1f).normalized;

        //Rigidbody2D config
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        if (isOrbiting) return;

        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        if (!isCoreAsteroid && !isOrbiting)
        {
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
            if (viewportPos.x < -0.1f || viewportPos.x > 1.1f || viewportPos.y < -0.1f || viewportPos.y > 1.1f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"{gameObject.name} was hit by: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");

        if (!isVisible && isCoreAsteroid)
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
            return;
        }

        // Check collider
        if (collision.gameObject.CompareTag("Asteroid"))
        {
            Vector2 pushDirection = (transform.position - collision.transform.position).normalized;
            rb.AddForce(pushDirection * pushForce, ForceMode2D.Force);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isCoreAsteroid && !isVisible)
            return;

        health -= amount;
        if (health <= 0f)
            Die();
    }

    void Die()
    {
        if (isCoreAsteroid && coreBossPrefab != null)
        {
            Instantiate(coreBossPrefab, transform.position, Quaternion.identity);
        }

        if (starPrefab != null && !isCoreAsteroid)
            SpawnStar();

        if (size == AsteroidSize.Large && !isCoreAsteroid)
            SpawnSplitAsteroid(AsteroidSize.Medium, 2);

        if (explosionPrefab != null && !isCoreAsteroid)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            ParticleSystem ps = explosion.GetComponent<ParticleSystem>() ?? explosion.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(explosion, totalDuration);
            }
            else
            {
                Destroy(explosion, 2f);
            }
        }

        // --- DEBUG: Always drop both heart and powerup ---
        //if (!isCoreAsteroid)
        //{
        //    if (heartPrefab != null)
        //        Instantiate(heartPrefab, transform.position + new Vector3(-0.5f, 0.5f, 0), Quaternion.identity);


        //    if (powerupPrefab != null)
        //        Instantiate(powerupPrefab, transform.position + new Vector3(0.5f, -0.5f, 0), Quaternion.identity);
        //}

        // --- NEW: Drop heart and powerup randomly ---
        if (!isCoreAsteroid)
        {
            float roll = Random.value; // value between 0.0 and 1.0

            if (roll <= 0.20f && heartPrefab != null) // 0.00 - 0.20
            {
                Instantiate(heartPrefab, transform.position, Quaternion.identity);
            }
            else if (roll <= 0.70f && powerupPrefab != null) // 0.20 - 0.70 (50% range)
            {
                Instantiate(powerupPrefab, transform.position, Quaternion.identity);
            }
        }


        AudioManager.PlayClip(asteroidDestroySound, transform.position);
        Destroy(gameObject);
    }

    void SpawnStar()
    {
        int starCount = 1;
        switch (size)
        {
            case AsteroidSize.Small:
                starCount = 1; 
                break;
            case AsteroidSize.Medium:
                starCount = 2;
                break;
            case AsteroidSize.Large:
                starCount = 4; 
                break;
        }

        for (int i = 0; i < starCount; i++)
        {
            //Position
            Vector3 offset = new Vector3(Random.Range(-1.5f, 1f), Random.Range(-1.5f, 1f), 0);
            Instantiate(starPrefab, transform.position + offset, Quaternion.identity);
        }
    }

    void SpawnSplitAsteroid(AsteroidSize size, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 0.5f;

            GameObject prefabToUse = mediumAsteroid[Random.Range(0, mediumAsteroid.Length)];
            GameObject newAsteroid = Instantiate(prefabToUse, transform.position + new Vector3(offset.x, offset.y, 0), Quaternion.identity);
            Asteroid asteroidScript = newAsteroid.GetComponent<Asteroid>();
            if (asteroidScript != null)
            {
                asteroidScript.size = size;
            }
        }
    }
}
