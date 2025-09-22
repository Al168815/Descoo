using UnityEngine;
using System.Collections;

public class CluesSequenceManager : MonoBehaviour
{
    [Header("Raíces")]
    public CanvasGroup introPanelGroup;   // opcional: para ocultar Intro
    public CanvasGroup panelPistasGroup;  // CanvasGroup del PanelPistas (raíz con decoración)
    public GameObject[] cluePanels;       // Clue1..Clue5 en ORDEN

    private int currentIndex = -1;

    void Awake()
    {
        // Estado inicial del panel de pistas oculto
        if (panelPistasGroup)
        {
            panelPistasGroup.alpha = 0f;
            panelPistasGroup.interactable = false;
            panelPistasGroup.blocksRaycasts = false;
        }
        ShowOnly(-1); // todos los ClueX ocultos
    }

    // Llamar desde botón "Ver pistas" en Intro
    public void OpenFromIntro()
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        // Abre el contenedor de pistas
        StartCoroutine(FadeCanvas(panelPistasGroup, 0f, 1f, 0.25f, true, () =>
        {
            GoToIndex(0); // ir a la primera pista
        }));

        // Oculta Intro (si existe)
        if (introPanelGroup)
            StartCoroutine(FadeCanvas(introPanelGroup, 1f, 0f, 0.25f, false));
    }

    public void Next()
    {
        if (cluePanels == null || cluePanels.Length == 0) return;
        if (currentIndex + 1 < cluePanels.Length)
            GoToIndex(currentIndex + 1);
        // Si ya estás en la última, no hace nada.
    }

    public void Prev()
    {
        if (cluePanels == null || cluePanels.Length == 0) return;
        if (currentIndex - 1 >= 0)
            GoToIndex(currentIndex - 1);
    }

    // Cierra pistas y vuelve al Intro
    public void CloseAndBackToIntro()
    {
        StartCoroutine(FadeCanvas(panelPistasGroup, 1f, 0f, 0.25f, false, () =>
        {
            ShowOnly(-1);
            if (introPanelGroup)
                StartCoroutine(FadeCanvas(introPanelGroup, 0f, 1f, 0.25f, true));
        }));
    }

    // ---------------- Interno ----------------

    private void GoToIndex(int index)
    {
        if (cluePanels == null || cluePanels.Length == 0)
        {
            Debug.LogWarning("[CluesSequence] 'cluePanels' está vacío.");
            return;
        }
        if (index < 0 || index >= cluePanels.Length)
        {
            Debug.LogWarning($"[CluesSequence] Índice {index} fuera de rango.");
            return;
        }

        ShowOnly(index);

        // Busca el animador en el panel o en sus hijos (aunque estén inactivos)
        var anim = cluePanels[index].GetComponentInChildren<CluePanelAnimator>(true);
        if (anim != null)
        {
            anim.Play();
        }
        else
        {
            Debug.LogWarning($"[CluesSequence] No encontré CluePanelAnimator en {cluePanels[index].name}.");
        }
    }

    private void ShowOnly(int index)
    {
        currentIndex = index;
        if (cluePanels == null) return;

        for (int i = 0; i < cluePanels.Length; i++)
        {
            if (cluePanels[i]) cluePanels[i].SetActive(i == index);
        }
    }

    private IEnumerator FadeCanvas(CanvasGroup g, float from, float to, float dur, bool interactable, System.Action done = null)
    {
        if (!g) { done?.Invoke(); yield break; }

        g.alpha = from;
        g.interactable = false;
        g.blocksRaycasts = false;

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            g.alpha = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }

        g.alpha = to;
        g.interactable = interactable;
        g.blocksRaycasts = interactable;

        done?.Invoke();
    }
}
