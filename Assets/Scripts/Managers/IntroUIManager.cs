using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class IntroUIManager : MonoBehaviour
{
    [Header("Scroll y contenido")]
    public ScrollRect scrollRect;        // Scroll de la tablet
    public RectTransform contentPanel;   // Contenedor de todo el contenido

    [Header("Texto e imágenes")]
    public Text newsText;                // Texto principal de la campaña
    public Image logoDescoo;             // Logo de la campaña
    public Image[] photos;               // Fotos de gatitos y entorno

    [Header("Botón final")]
    public Button startButton;           // Botón: leer testimonios y comenzar rescate
    public string firstLevelScene = "Calle"; // Nivel 1

    [Header("Animación de fade")]
    public float fadeDuration = 1f;      // Tiempo para que aparezcan imágenes/texto

    void Start()
    {
        // Inicializa todo en invisible
        if (logoDescoo) SetAlpha(logoDescoo, 0f);
        foreach (var img in photos)
            SetAlpha(img, 0f);

        if (newsText) SetAlpha(newsText, 0f);

        // Desactiva el botón hasta el final
        if (startButton) startButton.gameObject.SetActive(false);

        // Inicia la corutina de animación
        StartCoroutine(ShowIntroSequence());
    }

    IEnumerator ShowIntroSequence()
    {
        // Fade-in del logo
        if (logoDescoo) yield return StartCoroutine(FadeImage(logoDescoo, 1f));

        // Fade-in del texto
        if (newsText) yield return StartCoroutine(FadeText(newsText, 1f));

        // Fade-in de las fotos secuencialmente
        foreach (var img in photos)
        {
            if (img) yield return StartCoroutine(FadeImage(img, 1f));
            yield return new WaitForSeconds(0.3f); // pequeño delay entre fotos
        }

        // Al final, activa el botón
        if (startButton) startButton.gameObject.SetActive(true);
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(StartRescue);
    }

    void StartRescue()
    {
        SceneManager.LoadScene(firstLevelScene);
    }

    #region Fade Helpers

    void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;
        var c = g.color;
        c.a = a;
        g.color = c;
    }

    IEnumerator FadeImage(Image img, float targetAlpha)
    {
        if (!img) yield break;
        float t = 0f;
        float startAlpha = img.color.a;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            SetAlpha(img, a);
            yield return null;
        }
        SetAlpha(img, targetAlpha);
    }

    IEnumerator FadeText(Text txt, float targetAlpha)
    {
        if (!txt) yield break;
        float t = 0f;
        float startAlpha = txt.color.a;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            var c = txt.color;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            txt.color = c;
            yield return null;
        }
        var final = txt.color;
        final.a = targetAlpha;
        txt.color = final;
    }

    #endregion
}
