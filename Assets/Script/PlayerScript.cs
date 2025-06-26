using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerScript : MonoBehaviour
{
    //Playyer property
    public float moveSpeed = 7f;
    public float moveSpeedSlow = 4f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float halfWidth;
    private float minX, maxX, minY, maxY;
    private float halfHeight;
    private Vector3 defaultPosition;
    public AudioClip collisionSound;
    public AudioSource collisionAudioSource;
    public GameObject explosionPrefab;

    public float invulnerabilityDuration = 1.5f;
    public float blinkInterval = 0.1f;


    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;


    //Laser property
    public Camera mainCamera;
    public LineRenderer lineRenderer;
    public Transform firePoint;
    public float maxDistance = 100f;

    public LayerMask hitMask;

    public float damage = 1;

    public float damagePerSecond;
    public AudioSource audioSource;
    public AudioClip laserSound;

    //Particle property
    public GameObject startVFX;
    public GameObject endVFX;
    private List<ParticleSystem> particle = new List<ParticleSystem>();

    //Diz iz Health, ship can't live without dis ok?
    public int maxLives = 3;
    public int currentLives;

    public Image[] heartImages;
    public Sprite fullHeart;


    [SerializeField] private Transform firePointCenter;
    [SerializeField] private Transform firePointLeft;
    [SerializeField] private Transform firePointRight;

    [SerializeField] private LineRenderer lineRendererCenter;
    [SerializeField] private LineRenderer lineRendererLeft;
    [SerializeField] private LineRenderer lineRendererRight;

    [SerializeField] private GameObject endVFXCenter;
    [SerializeField] private GameObject endVFXLeft;
    [SerializeField] private GameObject endVFXRight;

    [SerializeField] private GameObject startVFXCenter;
    [SerializeField] private GameObject startVFXLeft;
    [SerializeField] private GameObject startVFXRight;


    [SerializeField] private Transform laserCenter;
    [SerializeField] private Transform laserLeft;
    [SerializeField] private Transform laserRight;

    private Quaternion leftSpreadRotation;
    private Quaternion rightSpreadRotation;
    private Quaternion forwardRotation;

    private bool hasUnlockedSideLasers = false; //flag to track if side lasers are unlocked
    private bool sideLasersUnlocked = false; // flag to force update


    private float shiftLerpT = 0f;  // 0 = spread, 1 = tight
    [SerializeField] private float rotateDuration = 0.05f; // Duration in seconds



    void Start()
    {

        // Store initial spread rotations
        leftSpreadRotation = laserLeft.localRotation;
        rightSpreadRotation = laserRight.localRotation;

        // Assuming center is the "forward" direction
        forwardRotation = laserCenter.localRotation;
        currentLives = maxLives;
        UpdateHeartsUI();

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        defaultPosition = transform.position;
        mainCamera = Camera.main;


        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            halfWidth = sr.bounds.size.x / 2;
            halfHeight = sr.bounds.size.y / 2;
        }

        float camHeight = mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;

        minX = mainCamera.transform.position.x - camWidth + halfWidth;
        maxX = mainCamera.transform.position.x + camWidth - halfWidth;
        minY = mainCamera.transform.position.y - camHeight + halfHeight;
        maxY = mainCamera.transform.position.y + camHeight - halfHeight;

        particle = new List<ParticleSystem>();
        FillParticles();
        DisableLaser();

        if (damagePerSecond >= 800)
        {
            FillParticles(); // Fill particles for multi-beam lasers
        }
    }


    void Update()
    {

        if (!hasUnlockedSideLasers && damagePerSecond >= 800)
        {
            FillParticles(); // Now adds left/right beams
            hasUnlockedSideLasers = true;

            // If needed: Enable their GameObjects too
            startVFXLeft?.SetActive(true);
            endVFXLeft?.SetActive(true);
            startVFXRight?.SetActive(true);
            endVFXRight?.SetActive(true);
        }

        var keyboard = Keyboard.current;
        moveInput = Vector2.zero;

        if (keyboard.leftArrowKey.isPressed) moveInput.x = -1;
        if (keyboard.rightArrowKey.isPressed) moveInput.x = 1;
        if (keyboard.upArrowKey.isPressed) moveInput.y = 1;
        if (keyboard.downArrowKey.isPressed) moveInput.y = -1;

        moveInput = moveInput.normalized;


        if (keyboard.zKey.wasPressedThisFrame)
        {
            FireLaser(); // Start laser, enable line renderer, play particles, play sound
        }
        else if (keyboard.zKey.isPressed)
        {
            UpdateLaser(); // Continuously update laser endpoint every frame while holding Z
        }
        else if (keyboard.zKey.wasReleasedThisFrame)
        {
            DisableLaser(); // Disable laser on release
        }
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }

    void FixedUpdate()
    {
        var keyboard = Keyboard.current;
        bool isShiftHeld = keyboard.shiftKey.isPressed;

        // Movement
        float currentMoveSpeed = isShiftHeld ? moveSpeedSlow : moveSpeed;
        rb.linearVelocity = moveInput * currentMoveSpeed;

        // Shift rotation interpolation
        float target = isShiftHeld ? 1f : 0f;
        shiftLerpT = Mathf.MoveTowards(shiftLerpT, target, Time.fixedDeltaTime / rotateDuration);

        // Interpolate rotation
        laserLeft.localRotation = Quaternion.Lerp(leftSpreadRotation, forwardRotation, shiftLerpT);
        laserRight.localRotation = Quaternion.Lerp(rightSpreadRotation, forwardRotation, shiftLerpT);
    }

    void FireLaser()
    {
        if (firePointCenter == null || firePointLeft == null || firePointRight == null)
            return;

        if (lineRendererCenter == null || lineRendererLeft == null || lineRendererRight == null)
            return;

        if (audioSource != null && laserSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = laserSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        if (damagePerSecond >= 300)
            lineRendererCenter.enabled = true;

        if (damagePerSecond >= 800)
            lineRendererLeft.enabled = true;

        if (damagePerSecond >= 800)
            lineRendererRight.enabled = true;

        foreach (var p in particle)
        {
            if (!p.isPlaying)
                p.Play();
        }

        ProcessLaser(); // Call the multi-beam version
    }

    void UpdateLaser()
    {
        ProcessLaser(); // Call the same logic every frame
        
    }

    void ProcessLaser()
    {
        // Center Laser
        if (damagePerSecond >= 300)
            ProcessSingleLaser(firePointCenter, lineRendererCenter, endVFXCenter, startVFXCenter);

        // Left Laser
        if (damagePerSecond >= 800)
            ProcessSingleLaser(firePointLeft, lineRendererLeft, endVFXLeft, startVFXLeft);

        //Right Laser
        if (damagePerSecond >= 800)
            ProcessSingleLaser(firePointRight, lineRendererRight, endVFXRight, startVFXRight);



    }
    void ProcessSingleLaser(Transform firePoint, LineRenderer lineRenderer, GameObject endVFX, GameObject startVFX)
    {
        Vector3 startPos = firePoint.position;
        Vector2 dir = firePoint.up.normalized;

        lineRenderer.SetPosition(0, startPos);

        if (startVFX != null)
            startVFX.transform.position = startPos;

        RaycastHit2D hit = Physics2D.Raycast(startPos, dir, maxDistance, hitMask);
        Vector3 endPos;

        if (hit.collider != null)
        {
            endPos = hit.point;

            if (endVFX != null)
                endVFX.transform.position = endPos;

            ApplyDamage(hit.collider);
        }
        else
        {
            endPos = startPos + (Vector3)(dir * maxDistance);

            if (endVFX != null)
                endVFX.transform.position = endPos;
        }

        lineRenderer.SetPosition(1, endPos);
    }



    public void DisableLaser()
    {
        lineRendererCenter.enabled = false;
        lineRendererLeft.enabled = false;
        lineRendererRight.enabled = false;

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        for (int i = 0; i < particle.Count; i++)
        {
            if (particle[i].isPlaying)
                particle[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }


    void FillParticles()
    {
        particle.Clear(); // Prevent duplicates

        void AddParticlesFrom(GameObject vfx)
        {
            if (vfx == null) return;

            ParticleSystem psMain = vfx.GetComponent<ParticleSystem>();
            if (psMain != null)
                particle.Add(psMain);

            for (int i = 0; i < vfx.transform.childCount; i++)
            {
                ParticleSystem psChild = vfx.transform.GetChild(i).GetComponent<ParticleSystem>();
                if (psChild != null)
                    particle.Add(psChild);
            }
        }

        // Always include center lasers
        AddParticlesFrom(startVFXCenter);
        AddParticlesFrom(endVFXCenter);

        // Add side lasers only if allowed
        if (damagePerSecond >= 800)
        {
            AddParticlesFrom(startVFXLeft);
            AddParticlesFrom(endVFXLeft);
            AddParticlesFrom(startVFXRight);
            AddParticlesFrom(endVFXRight);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isInvulnerable) return;

            if (collision.CompareTag("Asteroid") ||
            collision.CompareTag("Bullet") ||
            collision.CompareTag("BossMap3") ||
            collision.CompareTag("Shield") ||
            collision.CompareTag("Enemy") ||
            collision.CompareTag("Enemy2") ||
            collision.CompareTag("Enemy2_2"))
        {
                currentLives--;

                UpdateHeartsUI();

                if (currentLives <= 0)
                {
                    if (explosionPrefab != null)
                        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

                    if (collisionSound != null)
                        AudioManager.PlayClip(collisionSound, transform.position);

                    GameManager.Instance.HandleGameOver();
                    Destroy(gameObject);
                }
                else
                {
                    StartCoroutine(BlinkDuringInvulnerability());
                    GameManager.Instance.HandlePlayerCollision(
                        gameObject,
                        collisionSound,
                        explosionPrefab,
                        1f,
                        new Vector3(0f, -3.513863f, 0f)
                    );
                }
            }
        }
    public IEnumerator BlinkDuringInvulnerability()
    {
        isInvulnerable = true;

        DisableLaser();

        float elapsed = 0f;
        List<SpriteRenderer> allRenderers = new List<SpriteRenderer>();

        // Collect all SpriteRenderers (ShipVisual + Weapon children)
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>(true))
        {
            allRenderers.Add(sr);
        }

        while (elapsed < invulnerabilityDuration)
        {
            foreach (var sr in allRenderers)
            {
                sr.enabled = false;
            }

            yield return new WaitForSeconds(blinkInterval);

            foreach (var sr in allRenderers)
            {
                sr.enabled = true;
            }

            yield return new WaitForSeconds(blinkInterval);

            elapsed += 2f * blinkInterval;
        }

        isInvulnerable = false;
    }

    void ApplyDamage(Collider2D collider)
    {
        float damage = damagePerSecond * Time.deltaTime;

        if (collider.CompareTag("Asteroid"))
        {
            var asteroid = collider.GetComponent<Asteroid>() ?? collider.GetComponentInParent<Asteroid>();
            asteroid?.TakeDamage(damage);
        }
        else if (collider.CompareTag("CoreBoss"))
        {
            var coreBoss = collider.GetComponent<CoreBoss>() ?? collider.GetComponentInParent<CoreBoss>();
            coreBoss?.TakeDamage(damage);
        }
        else if (collider.CompareTag("BossMap3"))
        {
            var boss = collider.GetComponent<BossMovement>() ?? collider.GetComponentInParent<BossMovement>();
            boss?.TakeDamage(damage);
        }
        else if (collider.CompareTag("Shield"))
        {
            var shield = collider.GetComponent<ShieldBehavior>() ?? collider.GetComponentInParent<ShieldBehavior>();
            shield?.TakeDamage(damage);
        }
        else if (collider.CompareTag("Enemy"))
        {
            var enemy = collider.GetComponent<Enemy3Movement>() ?? collider.GetComponentInParent<Enemy3Movement>();
            enemy?.TakeDamage(damage);
        }
        else if (collider.CompareTag("Enemy2"))
        {
            var enemy = collider.GetComponent<EnemyController>() ?? collider.GetComponentInParent<EnemyController>();
            enemy?.TakeDamage(damage);
        }
        else if (collider.CompareTag("Enemy2_2"))
        {
            var enemy = collider.GetComponent<VerticalEnemyController>() ?? collider.GetComponentInParent<VerticalEnemyController>();
            enemy?.TakeDamage(damage);
        }
        else if (collider.CompareTag("BossMap2"))
        {
            var enemy = collider.GetComponent<EnemyBossController>() ?? collider.GetComponentInParent<EnemyBossController>();
            enemy?.TakeDamage(damage);
        }
    }

    void UpdateHeartsUI()
    {
        Debug.Log("Updating Hearts. Lives left: " + currentLives);
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null)
            {
                Debug.LogWarning("Heart image at index " + i + " is not assigned!");
                continue;
            }

            if (i < currentLives)
            {
                heartImages[i].gameObject.SetActive(true);
                heartImages[i].sprite = fullHeart;
            }
            else
            {
                heartImages[i].gameObject.SetActive(false);
            }
        }
    }



    public void IncreaseDamage(float amount)
    {
        if (damagePerSecond == 1000)
        {
            return;
        }

        damagePerSecond += amount;

        Debug.Log("New Damage: " + damage);
        Debug.Log("Updated Damage Per Second: " + damagePerSecond);
    }

    public void IncreaseHeart()
    {
        Debug.Log("Increase Hearts. Lives left: " + currentLives);
        if (currentLives < maxLives)
            currentLives++;

        UpdateHeartsUI();
    }


}
