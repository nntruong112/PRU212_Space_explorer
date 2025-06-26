using UnityEngine;
using UnityEngine.UI;

public class BossHealthManagerMap2 : MonoBehaviour
{
    [Header("UI")]
    public Slider healthSlider;
    public Image fillImage;

    [Header("Phase Colors")]
    public Color phase1Color = new Color(1f, 0.4f, 0.7f); // Pink
    public Color phase2Color = new Color(0.6f, 0.3f, 0.9f); // Purple

    [Header("Health Values")]
    public float phase1MaxHealth = 75000f;
    public float phase2MaxHealth = 75000f;

    private float currentHealth;
    private float maxHealth;
    private int currentPhase = 1;

    public void InitPhase1()
    {
        currentPhase = 1;
        maxHealth = phase1MaxHealth;
        currentHealth = maxHealth;

        UpdateUIColor(phase1Color);
        UpdateSlider();
    }

    public void InitPhase2()
    {
        currentPhase = 2;
        maxHealth = phase2MaxHealth;
        currentHealth = maxHealth;

        UpdateUIColor(phase2Color);
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

    private void UpdateSlider()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    private void UpdateUIColor(Color newColor)
    {
        if (fillImage != null)
        {
            fillImage.color = newColor;
        }
    }
}
