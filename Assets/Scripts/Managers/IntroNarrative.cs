using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroNarrative : MonoBehaviour
{
    public Text dialogueText;
    public string[] messages;
    private int currentMessage = 0;

    void Start()
    {
        ShowMessage();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentMessage++;
            if (currentMessage < messages.Length)
                ShowMessage();
            else
                SceneManager.LoadScene("MainMenu");
        }
    }

    void ShowMessage()
    {
        dialogueText.text = messages[currentMessage];
    }
}
