using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CluePanelAnimator : MonoBehaviour
{
    [Header("Refs")]
    public Image image;
    public RectTransform imageRect;
    public TMP_Text text;

    [Header("Fade")]
    public bool fadeImage = true;
    public bool fadeText = true;
    public float fadeDuration = 0.25f;

    [Header("Bounce")]
    public bool bounceImage = false;
    public float bounceScale = 1.08f;
    public float bounceDuration = 0.20f;

    [Header("Typewriter")]
    public bool typewriterText = false;
    [TextArea] public string textToTypeOverride;
    public float charsPerSecond = 40f;

    private string cachedText;

    void Awake()
    {
        if (imageRect == null && image != null)
            imageRect = image.rectTransform; // autoconecta
    }

    public void Play()
    {
        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        // 1) Capturar texto ANTES de limpiar
        if (typewriterText && text)
            cachedText = string.IsNullOrEmpty(textToTypeOverride) ? text.text : textToTypeOverride;

        // 2) Estados iniciales
        if (image && fadeImage)
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);

        if (text)
        {
            if (fadeText)
                text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);
            else
            {
                var c = text.color; c.a = 1f; text.color = c;
            }
        }

        if (typewriterText && text) text.text = "";

        // 3) Fade-in simultáneo
        float t = 0f;
        Color imgFrom = image ? image.color : Color.white;
        Color imgTo = image ? new Color(imgFrom.r, imgFrom.g, imgFrom.b, 1f) : Color.white;
        Color txtFrom = text ? text.color : Color.white;
        Color txtTo = text ? new Color(txtFrom.r, txtFrom.g, txtFrom.b, 1f) : Color.white;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / fadeDuration);
            if (image && fadeImage) image.color = Color.Lerp(imgFrom, imgTo, a);
            if (text && fadeText) text.color = Color.Lerp(txtFrom, txtTo, a);
            yield return null;
        }

        // 4) Rebote
        if (bounceImage && imageRect)
        {
            Vector3 baseS = imageRect.localScale;
            Vector3 upS = baseS * bounceScale;
            float half = bounceDuration * 0.5f, tb = 0f;

            while (tb < half) { tb += Time.deltaTime; imageRect.localScale = Vector3.Lerp(baseS, upS, tb / half); yield return null; }
            tb = 0f;
            while (tb < half) { tb += Time.deltaTime; imageRect.localScale = Vector3.Lerp(upS, baseS, tb / half); yield return null; }
            imageRect.localScale = baseS;
        }

        // 5) Typewriter
        if (typewriterText && text)
        {
            string final = cachedText ?? "";
            if (final.Length == 0) yield break;

            float delay = 1f / Mathf.Max(1f, charsPerSecond);
            for (int i = 0; i < final.Length; i++)
            {
                text.text = final.Substring(0, i + 1);
                yield return new WaitForSeconds(delay);
            }
        }
    }

    [ContextMenu("DEBUG/Play Here")]
    void DebugPlay() => Play();
}
