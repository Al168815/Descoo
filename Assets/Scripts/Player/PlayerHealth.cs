using System;
using UnityEngine;
using UnityEngine.UI;  // Necesario para Image

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public Image healthBarImage;  // Barra de salud

    public event Action<float> OnHealthNormalizedChanged; // 0..1

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(0, amount));
        Debug.Log($"[PlayerHealth] Daño: -{amount}. Vida: {currentHealth}/{maxHealth}");

        // Actualiza la barra de salud
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Debug.Log("[PlayerHealth] GAME OVER");
            // Aquí puedes cargar escena GameOver si lo deseas
            // SceneManager.LoadScene("GameOver");
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0, amount));
        Debug.Log($"[PlayerHealth] Curación: +{amount}. Vida: {currentHealth}/{maxHealth}");

        // Actualiza la barra de salud
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBarImage != null)
        {
            float healthPercentage = (float)currentHealth / maxHealth;
            healthBarImage.fillAmount = healthPercentage;  // Actualiza la barra de salud

            Debug.Log($"[PlayerHealth] Barra de salud actualizada: {healthPercentage * 100}%");
        }
    }

    public int Current => currentHealth; // Getter para la salud actual
}
