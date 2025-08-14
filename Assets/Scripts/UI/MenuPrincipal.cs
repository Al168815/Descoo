using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public void Jugar()
    {
        SceneManager.LoadScene("Calle");
    }

    public void Salir()
    {
        Application.Quit();
    }
}
