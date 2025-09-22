using UnityEngine;

public class CatNormal : MonoBehaviour
{
    [Header("Daño si no está alimentado")]
    public int damageAmount = 10;
    public float damageCooldown = 1.0f;

    [Header("Estado")]
    public bool isFed = false;

    private float lastDamageTime = -999f;  // Última vez que se aplicó daño
    private bool isDamaging = false;      // Para evitar que se aplique daño continuamente

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var inv = other.GetComponent<Inventory>();
        var hp = other.GetComponent<PlayerHealth>();

        if (!isFed)
        {
            // Si el gato no está alimentado, aplica daño
            if (!isDamaging)
            {
                TryDamage(hp);
                isDamaging = true;  // Marca que el daño ha sido aplicado
            }
        }
        else
        {
            // Si está alimentado, recolectar
            Debug.Log("[CatNormal] Gato alimentado y recolectado.");
            GameManager.Instance?.RegisterCatCollected();
            Destroy(gameObject, 0.05f);  // Destruir el gato
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isFed) return;

        // Solo aplicar daño una vez cada cierto tiempo
        var hp = other.GetComponent<PlayerHealth>();
        TryDamage(hp);
    }

    void TryDamage(PlayerHealth hp)
    {
        if (hp == null) return;
        if (Time.time - lastDamageTime < damageCooldown) return;  // Verifica que haya pasado el tiempo de cooldown

        hp.TakeDamage(damageAmount);  // Aplica el daño al jugador
        lastDamageTime = Time.time;   // Actualiza el tiempo del último daño
        Debug.Log("[CatNormal] Daño al jugador por tocar el gato sin alimentarlo.");
    }

    void ResetDamageFlag()
    {
        isDamaging = false;  // Resetea la bandera de daño después de un pequeño tiempo
    }
}
