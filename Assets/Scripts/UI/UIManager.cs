using UnityEngine;
using TMPro;
using UnityEngine.UI;  // Necesario para usar Image


public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Referencias UI")]
    public TextMeshProUGUI catsText;  // Contador de gatos
    public TextMeshProUGUI fishText;  // Contador de peces

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Actualiza el contador de gatos en UI
    public void UpdateCats(int current, int target)
    {
        if (catsText)
        {
            catsText.text = $"{current} / {target}";
        }
    }

    // Actualiza el contador de peces en UI
    public void UpdateFishCount(int count)
    {
        if (fishText)
        {
            fishText.text = $" {count}";
        }
    }
}
