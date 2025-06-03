using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerScript : MonoBehaviour
{
    //Playyer property
    public float moveSpeed = 5f;
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
    public float damagePerSecond = 100f;
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


    void Start()
    {
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
    }

    void Update()
    {
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
        rb.linearVelocity = moveInput * moveSpeed;
    }
    void FireLaser()
    {
        if (lineRenderer == null || firePoint == null)
            return;

        if (audioSource != null && laserSound != null && !audioSource.isPlaying)
        {
            //audioSource.PlayOneShot(laserSound);
            audioSource.clip = laserSound;
            audioSource.loop = true; // Enable looping
            audioSource.Play();
        }

        lineRenderer.enabled = true;

        for (int i = 0; i < particle.Count; i++)
        {
            if (!particle[i].isPlaying)
                particle[i].Play();
        }

        ProcessLaser(); // << Core logic here
    }

    void UpdateLaser()
    {
        ProcessLaser(); // Call the same logic every frame
    }

    void ProcessLaser()
    {
        Vector3 startPos = firePoint.position;
        Vector2 dir = firePoint.up.normalized;

        lineRenderer.SetPosition(0, startPos);

        RaycastHit2D hit = Physics2D.Raycast(startPos, dir, maxDistance, hitMask);
        Vector3 endPos;

        if (hit.collider != null)
        {
            endPos = hit.point;

            if (endVFX != null)
                endVFX.transform.position = hit.point;

            if (hit.collider.CompareTag("Asteroid"))
            {
                Asteroid asteroid = hit.collider.GetComponent<Asteroid>() ??
                                    hit.collider.GetComponentInParent<Asteroid>();

                if (asteroid != null)
                {
                    float damage = damagePerSecond * Time.deltaTime;
                    asteroid.TakeDamage(damage);
                }
            }
        }
        else
        {
            endPos = startPos + (Vector3)(dir * maxDistance);

            if (endVFX != null)
                endVFX.transform.position = endPos;
        }

        lineRenderer.SetPosition(1, endPos);

        //Debug.Log($"StartPos: {startPos}, EndPos: {endPos}");
    }



    public void DisableLaser()
    {
        lineRenderer.enabled = false;
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
        // Include ParticleSystem on startVFX itself
        ParticleSystem psStart = startVFX.GetComponent<ParticleSystem>();
        if (psStart != null)
            particle.Add(psStart);

        // Include child ParticleSystems of startVFX
        for (int i = 0; i < startVFX.transform.childCount; i++)
        {
            ParticleSystem ps = startVFX.transform.GetChild(i).GetComponent<ParticleSystem>();
            if (ps != null)
                particle.Add(ps);
        }

        // Include ParticleSystem on endVFX itself
        ParticleSystem psEnd = endVFX.GetComponent<ParticleSystem>();
        if (psEnd != null)
            particle.Add(psEnd);

        // Include child ParticleSystems of endVFX
        for (int i = 0; i < endVFX.transform.childCount; i++)
        {
            ParticleSystem ps = endVFX.transform.GetChild(i).GetComponent<ParticleSystem>();
            if (ps != null)
                particle.Add(ps);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isInvulnerable) return;
        if (collision.CompareTag("Asteroid"))
        {
            GameManager.Instance.HandlePlayerCollision(
                gameObject,
                collisionSound,
                explosionPrefab,
                1f,
                new Vector3(0f, -3.513863f, 0f)
            );
            currentLives--;

            UpdateHeartsUI();

            if (currentLives <= 0)
            {
                // Game over: destroy player or trigger death
                GameManager.Instance.HandleGameOver();
                Destroy(gameObject);
                // Optionally: trigger game over screen
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

    void UpdateHeartsUI()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
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

}
