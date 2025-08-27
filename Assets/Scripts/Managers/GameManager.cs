using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Progreso por nivel")]
    public int targetCatsPerLevel = 5;
    [SerializeField] private int catsCollectedThisLevel = 0;

    // Orden exacto de tus escenas jugables
    // (asegúrate de que estos nombres coincidan tal cual con tus escenas)
    [Header("Orden de niveles")]
    public string[] levelOrder = { "Calle", "Parque", "Escuela" };
    public string winSceneName = "WinScene";

    public event Action<int, int> OnCatsChanged; // (actual, objetivo)

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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reinicia conteo sólo si la escena actual es uno de los niveles jugables
        if (IsLevel(scene.name))
        {
            catsCollectedThisLevel = 0;
            OnCatsChanged?.Invoke(catsCollectedThisLevel, targetCatsPerLevel);
        }
    }

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

        // Si estoy en un nivel listado
        if (idx >= 0)
        {
            // ¿hay siguiente nivel en el arreglo?
            int nextIdx = idx + 1;
            if (nextIdx < levelOrder.Length)
            {
                string next = levelOrder[nextIdx];
                if (CanLoad(next)) SceneManager.LoadScene(next);
                else Debug.LogWarning($"[GameManager] La escena '{next}' no está en Build Settings.");
            }
            else
            {
                // ya fue el último (Escuela) → WinScene
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
}
