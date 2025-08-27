using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventario simple")]
    public int fishCount = 0;

    public void AddFish(int amount = 1)
    {
        fishCount += Mathf.Max(0, amount);
        Debug.Log($"[Inventory] Pescados: {fishCount}");
        UIManager.Instance?.UpdateFishCount(fishCount);
    }

    public bool UseFish(int amount = 1)
    {
        if (fishCount >= amount)
        {
            fishCount -= amount;
            Debug.Log($"[Inventory] Usaste {amount} pescado(s). Restantes: {fishCount}");
            UIManager.Instance?.UpdateFishCount(fishCount);
            return true;
        }
        return false;
    }

    public int FishCount => fishCount;
}
