using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Escenas")]
    [Tooltip("Nombre exacto de la escena del menú principal en Build Settings")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("Orden exacto de tus niveles jugables")]
    public string[] levelOrder = { "Calle", "Parque", "Escuela" };

    [Tooltip("Nombre exacto de la escena de victoria")]
    public string winSceneName = "WinScene";

    [Header("Progreso por nivel")]
    public int targetCatsPerLevel = 5;
    [SerializeField] private int catsCollectedThisLevel = 0;

    [Header("UI (solo MainMenu)")]
    public GameObject controlsPanel;
    public GameObject creditsPanel;

    // --- NUEVO: estado por nivel ---
    [Header("Estado del jugador (por nivel)")]
    [SerializeField] private int fishCount = 0;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    // --- NUEVO: pausa ---
    private bool isPaused = false;

    // (actual, objetivo)
    public event Action<int, int> OnCatsChanged;
    // NUEVO
    public event Action<int> OnFishChanged;
    public event Action<float, float> OnHealthChanged;
    public event Action<bool> OnPauseChanged;

    // --- Singleton ---
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- Por escena cargada ---
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Siempre arrancamos sin pausa al cargar
        SetPaused(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (IsLevel(scene.name))
        {
            // Reset de progreso por nivel
            catsCollectedThisLevel = 0;
            fishCount = 0;
            currentHealth = maxHealth;

            OnCatsChanged?.Invoke(catsCollectedThisLevel, targetCatsPerLevel);
            OnFishChanged?.Invoke(fishCount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnPauseChanged?.Invoke(false);
        }

        if (scene.name == mainMenuSceneName)
        {
            TryAutowireMenuPanels();
            SetActiveSafe(controlsPanel, false);
            SetActiveSafe(creditsPanel, false);
        }
    }

    // =====================================================================
    //                        PROGRESO DE GATOS / NIVELES
    // =====================================================================
    public void RegisterCatCollected()
    {
        catsCollectedThisLevel++;
        OnCatsChanged?.Invoke(catsCollectedThisLevel, targetCatsPerLevel);

        // Llamamos a UIManager para actualizar el UI
        if (UIManager.Instance != null)
        {
            var (current, target) = GetCatsProgress();
            UIManager.Instance.UpdateCats(current, target);
        }

        if (catsCollectedThisLevel >= targetCatsPerLevel)
            LoadNextAccordingToOrder();
    }

    void LoadNextAccordingToOrder()
    {
        string current = SceneManager.GetActiveScene().name;
        int idx = IndexInOrder(current);

        if (idx >= 0)
        {
            int nextIdx = idx + 1;
            if (nextIdx < levelOrder.Length)
            {
                string next = levelOrder[nextIdx];
                if (CanLoad(next)) SceneManager.LoadScene(next);
                else Debug.LogWarning($"[GameManager] La escena '{next}' no está en Build Settings.");
            }
            else
            {
                if (!string.IsNullOrEmpty(winSceneName) && CanLoad(winSceneName))
                    SceneManager.LoadScene(winSceneName);
                else
                    Debug.LogWarning("[GameManager] WinScene no está configurada o no existe en Build Settings.");
            }
        }
        else
        {
            Debug.LogWarning($"[GameManager] La escena actual '{current}' no está en levelOrder.");
        }
    }

    bool IsLevel(string sceneName) => IndexInOrder(sceneName) >= 0;

    int IndexInOrder(string sceneName)
    {
        for (int i = 0; i < levelOrder.Length; i++)
            if (levelOrder[i] == sceneName) return i;
        return -1;
    }

    bool CanLoad(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }

    public (int current, int target) GetCatsProgress() =>
        (catsCollectedThisLevel, targetCatsPerLevel);

    // =====================================================================
    //                             PECES (FISH)
    // =====================================================================
    public int GetFishCount() => fishCount;

    public void AddFish(int amount = 1)
    {
        fishCount = Mathf.Max(0, fishCount + amount);
        OnFishChanged?.Invoke(fishCount);

        // Llamamos a UIManager para actualizar el UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateFishCount(fishCount);
        }
    }
    public bool SpendFish(int amount = 1)
    {
        if (fishCount < amount) return false;
        fishCount -= amount;
        OnFishChanged?.Invoke(fishCount);
        return true;
    }

    // =====================================================================
    //                               SALUD
    // =====================================================================
    public float GetMaxHealth() => maxHealth;
    public float GetCurrentHealth() => currentHealth;

    public void SetMaxHealth(float newMax, bool fillToMax = true)
    {
        maxHealth = Mathf.Max(1f, newMax);
        if (fillToMax) currentHealth = maxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }





    // =====================================================================
    //                               PAUSA
    // =====================================================================
    public void TogglePause() => SetPaused(!isPaused);

    public void SetPaused(bool value)
    {
        isPaused = value;
        Time.timeScale = isPaused ? 0f : 1f;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.None;
        Cursor.visible = true;
        OnPauseChanged?.Invoke(isPaused);
    }

    public bool IsPaused() => isPaused;

    // =====================================================================
    //                            MENÚ PRINCIPAL
    // =====================================================================
    public void StartGame()
    {
        if (levelOrder == null || levelOrder.Length == 0)
        {
            Debug.LogError("[GameManager] levelOrder vacío.");
            return;
        }

        string firstLevel = levelOrder[0];
        if (CanLoad(firstLevel)) SceneManager.LoadScene(firstLevel);
        else Debug.LogError($"[GameManager] '{firstLevel}' no está en Build Settings.");
    }

    public void QuitGame()
    {
        Debug.Log("[GameManager] Quit");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ShowControls()
    {
        SetActiveSafe(controlsPanel, true);
        SetActiveSafe(creditsPanel, false);
    }

    public void HideControls()
    {
        SetActiveSafe(controlsPanel, false);
    }

    public void ShowCredits()
    {
        SetActiveSafe(creditsPanel, true);
        SetActiveSafe(controlsPanel, false);
    }

    public void HideCredits()
    {
        SetActiveSafe(creditsPanel, false);
    }

    public void CloseAllPanels()
    {
        SetActiveSafe(controlsPanel, false);
        SetActiveSafe(creditsPanel, false);
    }

    public void GoToMainMenu()
    {
        if (CanLoad(mainMenuSceneName)) SceneManager.LoadScene(mainMenuSceneName);
        else Debug.LogError($"[GameManager] '{mainMenuSceneName}' no está en Build Settings.");
    }

    // =====================================================================
    //                             UTILIDADES
    // =====================================================================
    void TryAutowireMenuPanels()
    {
        if (!controlsPanel)
        {
            var go = GameObject.Find("ControlsPanel");
            if (go) controlsPanel = go;
        }
        if (!creditsPanel)
        {
            var go2 = GameObject.Find("CreditsPanel");
            if (go2) creditsPanel = go2;
        }
    }

    void SetActiveSafe(GameObject go, bool value)
    {
        if (go && go.activeSelf != value) go.SetActive(value);
    }

    void Update()
    {
        // Tecla P para abrir/cerrar pausa
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

#if UNITY_EDITOR
        // DEBUG opcional: avanzar de nivel con F9
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Log("[GameManager] DEBUG: Avanzar nivel (F9).");
            LoadNextAccordingToOrder();
        }
#endif
    }
}
