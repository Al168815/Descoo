using UnityEngine;
using UnityEngine.AI;

public class DogEnemy : MonoBehaviour
{
    public float detectionRange = 7f;
    private Transform player;
    private Animator animator;
    private AudioSource audioSource;
    public AudioClip barkFar, barkAttack, eatSnack;
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    private bool isPacified = false;
    private NavMeshAgent agent;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        PatrolToNextPoint();
        PlaySound(barkFar);
    }

    void Update()
    {
        if (isPacified) return;
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < detectionRange)
        {
            agent.SetDestination(player.position);
            animator.SetBool("isWalking", true);
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            PatrolToNextPoint();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isPacified) return;
        if (other.CompareTag("Premio"))
        {
            isPacified = true;
            PlaySound(eatSnack);
            animator.SetTrigger("eat");
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            player.GetComponent<PlayerHealth>()?.TakeDamage(0.2f);
            PlaySound(barkAttack);
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
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
