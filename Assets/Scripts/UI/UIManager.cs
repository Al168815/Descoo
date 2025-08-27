using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Referencias UI")]
    public Slider healthSlider;
    public Text catsText;
    public Text fishText;

    PlayerHealth playerHealth;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Si quieres que sobreviva entre escenas, descomenta:
        // DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.OnHealthNormalizedChanged += UpdateHealth;
        }

        // Suscribirse a cambios de gatos del GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCatsChanged += UpdateCats;
            var (cur, tar) = GameManager.Instance.GetCatsProgress();
            UpdateCats(cur, tar);
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthNormalizedChanged -= UpdateHealth;

        if (GameManager.Instance != null)
            GameManager.Instance.OnCatsChanged -= UpdateCats;
    }

    void UpdateHealth(float t)
    {
        if (healthSlider) healthSlider.value = Mathf.Clamp01(t);
    }

    public void UpdateCats(int current, int target)
    {
        if (catsText) catsText.text = $"Gatos: {current} / {target}";
    }

    public void UpdateFishCount(int count)
    {
        if (fishText) fishText.text = $"Pescados: {count}";
    }
}

