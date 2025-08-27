using UnityEngine;

public class SceneBootstrap : MonoBehaviour
{
    public GameObject gameManagerPrefab;

    void Awake()
    {
        if (GameManager.Instance == null && gameManagerPrefab != null)
        {
            Instantiate(gameManagerPrefab);
        }
    }
}
