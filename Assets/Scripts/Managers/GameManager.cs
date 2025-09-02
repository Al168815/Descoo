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

    public event Action<int, int> OnCatsChanged; // (actual, objetivo)

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
        // Asegurar que el tiempo está corriendo
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Si estoy en un nivel jugable, reinicio conteo de gatos y notifico al UI
        if (IsLevel(scene.name))
        {
            catsCollectedThisLevel = 0;
            OnCatsChanged?.Invoke(catsCollectedThisLevel, targetCatsPerLevel);
        }

        // Si estoy en el menú principal, intento auto-asignar paneles si no están
        if (scene.name == mainMenuSceneName)
        {
            TryAutowireMenuPanels();
            // Por defecto, ocultos
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
                // Último nivel → WinScene
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
    //                            MENÚ PRINCIPAL
    // =====================================================================

    // Llamar desde el botón "Iniciar"
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

    // Llamar desde el botón "Salir"
    public void QuitGame()
    {
        Debug.Log("[GameManager] Quit");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Llamar desde el botón "Controles"
    public void ShowControls()
    {
        SetActiveSafe(controlsPanel, true);
        SetActiveSafe(creditsPanel, false);
    }

    public void HideControls()
    {
        SetActiveSafe(controlsPanel, false);
    }

    // Llamar desde el botón "Créditos"
    public void ShowCredits()
    {
        SetActiveSafe(creditsPanel, true);
        SetActiveSafe(controlsPanel, false);
    }

    public void HideCredits()
    {
        SetActiveSafe(creditsPanel, false);
    }

    // Llamar desde botones "Cerrar" dentro de cada panel, si los pones
    public void CloseAllPanels()
    {
        SetActiveSafe(controlsPanel, false);
        SetActiveSafe(creditsPanel, false);
    }

    // Opcional para un botón "Volver al Menú" desde niveles
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
            var go = GameObject.Find("CreditsPanel");
            if (go) creditsPanel = go;
        }
    }

    void SetActiveSafe(GameObject go, bool value)
    {
        if (go && go.activeSelf != value) go.SetActive(value);
    }

    // --- DEBUG opcional: avanzar de nivel con F9 ---
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Log("[GameManager] DEBUG: Avanzar nivel (F9).");
            LoadNextAccordingToOrder();
        }
#endif
    }
}
