using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UIManager.Instance.UpdateHealth(currentHealth / maxHealth);
    }

    public void TakeDamage(float percentage)
    {
        currentHealth -= maxHealth * percentage;
        UIManager.Instance.UpdateHealth(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UIManager.Instance.UpdateHealth(currentHealth / maxHealth);
    }
}
