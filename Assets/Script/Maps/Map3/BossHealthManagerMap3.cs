using UnityEngine;
using UnityEngine.UI;

public class BossHealthManagerMap3 : MonoBehaviour
{
    [Header("UI")]
    public Slider healthSlider;
    public Image fillImage;

    [Header("Health Settings")]
    public float maxHealth = 110000f;
    private float currentHealth;

    [Header("Color")]
    public Color healthBarColor = new Color(0.2f, 0.6f, 1f); // Space Blue

    void Awake()
    {
        // Optional: auto-init
        Init();
    }

    public void Init()
    {
        currentHealth = maxHealth;

        if (fillImage != null)
        {
            fillImage.color = healthBarColor;
        }

        UpdateSlider();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateSlider();
    }

    public bool IsDead()
    {
        return currentHealth <= 0f;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    private void UpdateSlider()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
}
