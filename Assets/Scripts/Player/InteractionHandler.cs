using UnityEngine;

public class InteractionHandler : MonoBehaviour
{
    public float interactionDistance = 2f;
    public LayerMask interactableLayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
            {
                hit.collider.GetComponent<IInteractable>()?.Interact();
            }
        }
    }
}
