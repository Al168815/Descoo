using UnityEngine;
using UnityEngine.UI;  // Necesario para usar Image
using TMPro;

public class LevelHUD : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI catsCounter;  // Contador de gatos (ya no se usará aquí)
    [SerializeField] private TextMeshProUGUI fishCounter;  // Contador de pescados (ya no se usará aquí)
    [SerializeField] private Image healthBarImage;         // Barra de salud como imagen

    [SerializeField] private GameObject pausePanel;        // Panel de pausa
    [SerializeField] private Button tabletButton;          // Botón tableta para pausa

    void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            // Solo suscríbete a los eventos que afectan a la barra de salud
            GameManager.Instance.OnHealthChanged += HandleHealthChanged;
        }

        if (tabletButton != null)
            tabletButton.onClick.AddListener(OnTabletPressed);
    }

    void Start()
    {
        // Asegúrate de que GameManager.Instance no sea null
        var gm = GameManager.Instance;
        if (gm == null) return;

        // Obtén el estado inicial de la salud
        HandleHealthChanged(gm.GetCurrentHealth(), gm.GetMaxHealth());  // Actualiza la barra de salud
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            // Desuscríbete de los eventos
            GameManager.Instance.OnHealthChanged -= HandleHealthChanged;
        }

        if (tabletButton != null)
            tabletButton.onClick.RemoveListener(OnTabletPressed);
    }

    void HandleHealthChanged(float current, float max)
    {
        if (healthBarImage != null)
        {
            // Cambia el ancho de la barra de salud según el porcentaje de vida
            float healthPercentage = current / max;
            healthBarImage.fillAmount = healthPercentage; // fillAmount de 0 a 1
        }
    }

    void OnTabletPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.TogglePause();  // Alterna el estado de pausa
    }
}
