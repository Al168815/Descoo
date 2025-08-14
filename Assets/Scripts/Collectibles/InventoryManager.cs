using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    private List<string> collectedCats = new();
    private List<string> collectedItems = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddCat(string name) => collectedCats.Add(name);
    public void AddItem(string name) => collectedItems.Add(name);
}
