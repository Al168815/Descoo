using UnityEngine;

public class HealingItem : MonoBehaviour
{
    public float healAmount = 20f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
