using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class CoreBoss : MonoBehaviour
{
    public float maxHealth = 50000f;
    private float currentHealth;

    public CoreBossSpawn stageController;

    public int phase = 1;
    private bool triggeredPhase2 = false;
    public bool isPhase2Boss = false;

    private BossHealthBarController healthBar; // ✅ Add this

    void Start()
    {
        currentHealth = maxHealth;

        if (stageController == null)
        {
            stageController = FindObjectOfType<CoreBossSpawn>();
            if (stageController == null)
            {
                Debug.LogError("StageController not found by CoreBoss!");
            }
        }

        healthBar = GameObject.FindObjectOfType<BossHealthBarController>(); // ✅ Reference UI
        if (healthBar != null)
        {
            if (isPhase2Boss)
                healthBar.InitPhase2(maxHealth);
            else
                healthBar.InitPhase1(maxHealth);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
    }

    public void TakeDamage(float amount)
    {
        if (stageController == null || !stageController.CanDamageCore())
            return;

        currentHealth -= amount;

        // ✅ Update health bar
        if (healthBar != null)
        {
            healthBar.TakeDamage(amount);
        }

        if (!triggeredPhase2 && !isPhase2Boss && currentHealth <= maxHealth * 0.5f)
        {
            triggeredPhase2 = true;
            phase = 2;

            stageController.EnterPhase2();
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (healthBar != null)
            healthBar.gameObject.SetActive(false); // ✅ Hide bar on death

        Destroy(gameObject);

        Map1Controller controller = FindObjectOfType<Map1Controller>();
        if (controller != null)
        {
            controller.ShowStageClear();
        }
    }
}
