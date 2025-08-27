using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public event Action<float> OnHealthNormalizedChanged; // 0..1

    void Start()
    {
        currentHealth = maxHealth;
        Notify();
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(0, amount));
        Debug.Log($"[PlayerHealth] Daño: -{amount}. Vida: {currentHealth}/{maxHealth}");
        Notify();
        if (currentHealth <= 0)
        {
            Debug.Log("[PlayerHealth] GAME OVER");
            // Aquí puedes cargar escena GameOver si quieres
            // SceneManager.LoadScene("GameOver");
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0, amount));
        Debug.Log($"[PlayerHealth] Curación: +{amount}. Vida: {currentHealth}/{maxHealth}");
        Notify();
    }

    void Notify()
    {
        float t = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        OnHealthNormalizedChanged?.Invoke(t);
    }

    public int Current => currentHealth;
}
