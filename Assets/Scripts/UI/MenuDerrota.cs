using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuDerrota : MonoBehaviour
{
    public void IntentarDeNuevo()
    {
        SceneManager.LoadScene("Calle");
    }

    public void RegresarAlMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
