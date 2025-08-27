using UnityEngine;

public class CatNormal : MonoBehaviour
{
    [Header("Daño si no está alimentado")]
    public int damageAmount = 10;        // <-- usado por PlayerInteraction
    public float damageCooldown = 1.0f;

    [Header("Estado")]
    public bool isFed = false;           // <-- usado por PlayerInteraction

    private float lastDamageTime = -999f;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var inv = other.GetComponent<Inventory>();
        var hp = other.GetComponent<PlayerHealth>();

        if (!isFed)
        {
            // ¿Traes pescado? -> alimenta; si no, daño inmediato
            if (inv != null && inv.UseFish(1))
            {
                isFed = true;
                Debug.Log("[CatNormal] Gato alimentado. Ahora puedes recolectarlo.");
            }
            else
            {
                TryDamage(hp);
            }
        }
        else
        {
            // ya alimentado → recolectar
            Debug.Log("[CatNormal] Gato recolectado.");
            GameManager.Instance?.RegisterCatCollected();
            Destroy(gameObject, 0.05f);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isFed) return;

        var hp = other.GetComponent<PlayerHealth>();
        TryDamage(hp);
    }

    void TryDamage(PlayerHealth hp)
    {
        if (hp == null) return;
        if (Time.time - lastDamageTime < damageCooldown) return;

        hp.TakeDamage(damageAmount);
        lastDamageTime = Time.time;
        Debug.Log("[CatNormal] Daño al jugador por intentar recolectar sin pescado.");
    }
}
