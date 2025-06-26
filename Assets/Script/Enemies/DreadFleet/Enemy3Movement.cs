using System;
using UnityEngine;

public class Enemy3Movement : MonoBehaviour
{
    private Vector3 midPoint;
    private Vector3 targetPoint;
    private float speed;
    private bool goingDown = true;

    public float maxHealth = 400f;
    private float currentHealth;

    [Header("Explosion FX")]
    public GameObject explosionPrefab;

    [Header("Loot Prefabs")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private GameObject powerupPrefab;
    [SerializeField] private GameObject starPrefab;

    // Thêm sự kiện để báo khi enemy đến targetPos
    public Action OnReachedTarget;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void Init(Vector3 start, Vector3 final, float moveSpeed)
    {
        transform.position = start;
        speed = moveSpeed;
        midPoint = new Vector3(start.x, final.y, 0); // Bay xuống
        targetPoint = final;                         // Rồi bay ngang
    }

    void Update()
    {
        if (goingDown)
        {
            transform.position = Vector3.MoveTowards(transform.position, midPoint, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, midPoint) < 0.05f)
                goingDown = false;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPoint) < 0.05f)
            {
                enabled = false; // Dừng lại khi tới nơi
                OnReachedTarget?.Invoke(); // Gọi sự kiện khi đến targetPos
            }
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
        transform.parent = null; // Ngắt khỏi formationRoot nếu có

        // 💥 Explosion
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // ⭐ Always drop 1 star
        if (starPrefab != null)
        {
            Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(-0.2f, 0.2f), 0f);
            Instantiate(starPrefab, transform.position + offset, Quaternion.identity);
        }

        // 💓 10% chance to drop heart
        float roll = UnityEngine.Random.value;
        if (roll <= 0.10f && heartPrefab != null)
        {
            Instantiate(heartPrefab, transform.position + new Vector3(-0.3f, 0.4f, 0), Quaternion.identity);
        }
        // ⚡ 20% chance to drop powerup (roll between 0.10 and 0.30)
        else if (roll <= 0.30f && powerupPrefab != null)
        {
            Instantiate(powerupPrefab, transform.position + new Vector3(0.4f, -0.3f, 0), Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
