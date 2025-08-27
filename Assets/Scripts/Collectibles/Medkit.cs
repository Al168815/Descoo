using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Medkit : MonoBehaviour
{
    public int healAmount = 20;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        EnsureKinematicRb();
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

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var hp = other.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.Heal(healAmount);
            Debug.Log("[Medkit] Curado +" + healAmount);
            Destroy(gameObject);
        }
    }
}
