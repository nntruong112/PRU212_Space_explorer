using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarController : MonoBehaviour
{
    [Header("Slider References")]
    public Slider healthSlider;
    public Image fillImage;

    [Header("Colors")]
    public Color phase1Color = Color.cyan;
    public Color phase2Color = Color.yellow;

    private float maxHealth = 1000f;
    private float currentHealth;
    private int currentPhase = 1;

    public void InitPhase1(float max)
    {
        currentPhase = 1;
        maxHealth = max;
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (fillImage != null)
            fillImage.color = phase1Color;
    }

    public void InitPhase2(float max)
    {
        currentPhase = 2;
        maxHealth = max;
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (fillImage != null)
            fillImage.color = phase2Color;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthSlider != null)
            healthSlider.value = currentHealth;
    }

    public bool IsDead()
    {
        return currentHealth <= 0f;
    }
}
