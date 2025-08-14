using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuVictoria : MonoBehaviour
{
    public void RegresarAlMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
