// Assets/Scripts/Collectibles/CatBehavior.cs
using UnityEngine;
using UnityEngine.AI;

public class CatBehavior : MonoBehaviour, IInteractable // ← agrega IInteractable
{
    private bool isFed = false;
    private bool isCollected = false;

    private Animator animator;
    private AudioSource audioSource;
    public AudioClip meowFar, hissAttack, eatFish, collectSound;

    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    private NavMeshAgent agent;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        PatrolToNextPoint();
        PlaySound(meowFar);
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            PatrolToNextPoint();
    }

    // Si cae un objeto con tag Pescado sobre el gato, se alimenta (esto ya lo tenías)
    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Pescado"))
        {
            isFed = true;
            PlaySound(eatFish);
            animator.SetTrigger("eat");
            Destroy(other.gameObject);
        }
    }

    // ← NUEVO: Responde a la tecla E del jugador (raycast)
    public void Interact()
    {
        if (isCollected) return;

        // Obtén referencia al jugador
        var player = GameObject.FindGameObjectWithTag("Player");
        var playerHealth = player ? player.GetComponent<PlayerHealth>() : null;

        if (isFed)
        {
            // Recolección
            PlaySound(collectSound);
            animator.SetTrigger("collected");
            GameManager.Instance.RegisterCatCollected();
            InventoryManager.Instance.AddCat(gameObject.name);
            isCollected = true;
            Destroy(gameObject, 1f); // espera la animación
        }
        else
        {
            // Arañazo si intentas sin pescado
            if (playerHealth != null)
                playerHealth.TakeDamage(0.1f); // 10%

            PlaySound(hissAttack);
            animator.SetTrigger("attack");
        }
    }

    void PatrolToNextPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        animator.SetBool("isWalking", true);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();
    }
}
