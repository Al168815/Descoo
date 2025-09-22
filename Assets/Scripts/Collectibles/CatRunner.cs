using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Collider))]
public class CatRunner : MonoBehaviour
{
    [Header("Huida")]
    public float fleeDistance = 8f;
    public float repathInterval = 0.25f;

    [Header("Daño si no está alimentado")]
    public int damageAmount = 10;
    public float damageCooldown = 1.0f;

    [Header("Estado")]
    public bool isFed = false;

    private NavMeshAgent agent;
    private Transform player;
    private float lastRepath = -999f;
    private float lastDamageTime = -999f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;
    }

    void Update()
    {
        if (!agent || !player) return;

        if (!isFed)
        {
            if (Time.time - lastRepath >= repathInterval)
            {
                lastRepath = Time.time;
                Vector3 away = (transform.position - player.position).normalized;
                Vector3 dest = transform.position + away * fleeDistance;
                agent.SetDestination(dest);
                Debug.Log("[CatRunner] Corriendo para huir del jugador.");
            }
        }
        else
        {
            if (!agent.isStopped) { agent.isStopped = true; agent.ResetPath(); }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var inv = other.GetComponent<Inventory>();
        var hp = other.GetComponent<PlayerHealth>();

        if (!isFed)
        {
            if (inv != null && inv.UseFish(1))
            {
                isFed = true;
                if (agent) { agent.isStopped = true; agent.ResetPath(); }
                Debug.Log("[CatRunner] Alimentado. Puedes recolectarlo en la siguiente interacción.");
            }
            else
            {
                TryDamage(hp);
            }
        }
        else
        {
            Debug.Log("[CatRunner] Gato recolectado.");
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
        Debug.Log("[CatRunner] Daño al jugador por intentar recolectar sin pescado.");
    }
}
