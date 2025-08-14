using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item") || other.CompareTag("Premio") || other.CompareTag("Pescado"))
        {
            InventoryManager.Instance.AddItem(other.gameObject.name);
            Destroy(other.gameObject);
        }
    }
}
