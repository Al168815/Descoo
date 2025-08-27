using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class FishPickup : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] int amount = 1;
    [SerializeField] bool requireKey = false;     // si quieres que sea con tecla E dentro del trigger
    [SerializeField] KeyCode pickupKey = KeyCode.E;

    bool picked = false;

    void Reset()
    {
        // Configuración segura del collider/rigidbody del ítem
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        EnsureKinematicRb();
        // El tag del ítem no importa para el pickup
    }

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        EnsureKinematicRb();
    }

    void EnsureKinematicRb()
    {
        var rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void OnTriggerEnter(Collider other) { TryPickup(other); }
    void OnTriggerStay(Collider other) { TryPickup(other); }

    void TryPickup(Collider other)
    {
        if (picked) return;

        // Busca el Inventory en el objeto que entra o en sus padres (por si el collider es un hijo del Player)
        var inv = other.GetComponentInParent<Inventory>();
        if (inv == null) return;

        // (Opcional) Revisa también la salud para verificar que es el Player real
        var hp = other.GetComponentInParent<PlayerHealth>();
        if (hp == null) return; // no es el jugador

        // Si quieres forzar tecla E dentro del trigger
        if (requireKey && !Input.GetKeyDown(pickupKey)) return;

        inv.AddFish(amount);
        picked = true;
        Debug.Log($"[FishPickup] Pescado recogido (+{amount}).");
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        var c = GetComponent<Collider>();
        if (c)
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            if (c is BoxCollider b)
                Gizmos.DrawWireCube(b.center, b.size);
            else if (c is SphereCollider s)
                Gizmos.DrawWireSphere(s.center, s.radius);
        }
    }
#endif
}
