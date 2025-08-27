using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 2.5f;
    public LayerMask interactMask = ~0; // todo

    void Start()
    {
        if (!playerCamera) playerCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            Interact();
    }

    void Interact()
    {
        if (!playerCamera) return;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Collide))
        {
            // Alimentar/recolectar para gato normal
            var cat = hit.collider.GetComponentInParent<CatNormal>();
            if (cat != null)
            {
                var inv = GetComponent<Inventory>();
                var hp = GetComponent<PlayerHealth>();

                if (!cat.isFed)
                {
                    if (inv != null && inv.UseFish(1))
                    {
                        cat.isFed = true;
                        Debug.Log("[Interact] Alimentaste al gato.");
                    }
                    else
                    {
                        hp?.TakeDamage(cat.damageAmount);
                        Debug.Log("[Interact] No tienes pescado, el gato te dañó.");
                    }
                }
                else
                {
                    Debug.Log("[Interact] Recolectaste al gato.");
                    GameManager.Instance?.RegisterCatCollected();
                    Destroy(cat.gameObject, 0.05f);
                }
                return;
            }

            // Alimentar/recolectar para gato runner
            var runner = hit.collider.GetComponentInParent<CatRunner>();
            if (runner != null)
            {
                var inv = GetComponent<Inventory>();
                var hp = GetComponent<PlayerHealth>();

                if (!runner.isFed)
                {
                    if (inv != null && inv.UseFish(1))
                    {
                        runner.isFed = true;
                        var ag = runner.GetComponent<UnityEngine.AI.NavMeshAgent>();
                        if (ag) { ag.isStopped = true; ag.ResetPath(); }
                        Debug.Log("[Interact] Alimentaste al gato runner.");
                    }
                    else
                    {
                        hp?.TakeDamage(runner.damageAmount);
                        Debug.Log("[Interact] No tienes pescado, el runner te dañó.");
                    }
                }
                else
                {
                    Debug.Log("[Interact] Recolectaste al gato runner.");
                    GameManager.Instance?.RegisterCatCollected();
                    Destroy(runner.gameObject, 0.05f);
                }
            }
        }
    }
}
