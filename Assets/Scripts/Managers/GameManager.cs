using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private int catsCollected = 0;
    private int totalCatsToCollect = 5;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void RegisterCatCollected()
    {
        catsCollected++;
        if (catsCollected >= totalCatsToCollect)
        {
            LoadNextScene();
        }
    }

    public void ResetCats()
    {
        catsCollected = 0;
    }

    void LoadNextScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        string nextScene = "";
        if (currentScene == "Calle") nextScene = "Parque";
        else if (currentScene == "Parque") nextScene = "Escuela";
        else if (currentScene == "Escuela") nextScene = "WinScene";
        catsCollected = 0;
        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene);
    }

    public void TriggerGameOver()
    {
        SceneManager.LoadScene("GameOverScene");
    }
}
