using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseAndCluesControllerLite : MonoBehaviour
{
    public static PauseAndCluesControllerLite Instance { get; private set; }

    public enum DisplayMode { SetActive, CanvasGroup }

    [Header("MODO de visualización de la tableta (elige UNO)")]
    [Tooltip("SetActive: este script debe estar FUERA del objeto que apagas.")]
    public DisplayMode displayMode = DisplayMode.SetActive;

    [Header("Tableta (modelo 3D + Canvas + Panel)")]
    [Tooltip("SET ACTIVE: raíz/contenedor a encender/apagar. (Script FUERA de este objeto)")]
    public GameObject tabletRootOrContent;
    [Tooltip("CANVAS GROUP: CanvasGroup del contenedor visible. (Script puede vivir dentro)")]
    public CanvasGroup tabletCanvasGroup;

    [Header("HUD que debe ocultarse al abrir la tableta")]
    public List<GameObject> hudRoots = new List<GameObject>(); // vida, gatitos, peces, etc.

    [Header("Entrada / Navegación")]
    public KeyCode toggleKey = KeyCode.P;
    public string mainMenuSceneName = "MainMenu";
    public bool pauseGameWhenTabletOpen = true;

    [Header("Pistas (1:1 en orden)")]
    [Tooltip("Agrega tus pistas en orden. El índice (1..N) se asigna automáticamente.")]
    public List<ClueCatPairLite> pairs = new List<ClueCatPairLite>();

    [Header("IDs de gatos (opcional)")]
    public bool autoFillIdsAsGatoi = true;       // Si dejas catId vacío, se autollenará "Gato{index}"
    public bool tolerateIdVariations = true;     // Normaliza mayúsculas/espacios/_/-

    [Header("Debug")]
    public bool verboseLogs = false;

    // ===== Estado interno =====
    private bool tabletOpen = false;
    private bool paused = false;

    // Recolección por índice (1..N)
    private readonly HashSet<int> collectedIdx = new HashSet<int>();

    // Recolección por ID normalizado (opcional)
    private readonly HashSet<string> collectedIds = new HashSet<string>();
    private readonly Dictionary<string, int> idToIndex = new Dictionary<string, int>();

    private int selectedIndex = -1; // 1..N

    // ===================== Ciclo de vida =====================
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Indexado + autolabel + auto-fill de IDs
        for (int i = 0; i < pairs.Count; i++)
        {
            var p = pairs[i];
            if (p == null) continue;

            p.index = i + 1;

            if (string.IsNullOrWhiteSpace(p.catId) && autoFillIdsAsGatoi)
                p.catId = $"Gato{p.index}";

#if UNITY_EDITOR
            if (p?.pista?.label && p.autoLabelInEditor)
                p.pista.label.text = $"Pista {p.index}";
#endif
        }

        ValidateConfig();

        // Arranque seguro
        InternalShowTablet(false);
        SetHUDVisible(true);

        WireUIOnce();
        BuildIdIndex();
        StartCatWatchers();

        if (verboseLogs) Debug.Log("[PauseAndClues] Awake OK. Mode=" + displayMode);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (verboseLogs) Debug.Log("[PauseAndClues] Key pressed: " + toggleKey);
            OnTabletToggleButton();
        }
    }

    // ===================== Mostrar/Ocultar tableta =====================
    public void OnTabletToggleButton() => SetTabletOpen(!tabletOpen);
    public void OpenTablet() => SetTabletOpen(true);
    public void CloseTablet() => SetTabletOpen(false);

    private void SetTabletOpen(bool open)
    {
        tabletOpen = open;

        InternalShowTablet(open);
        SetHUDVisible(!open);
        SetPaused(open && pauseGameWhenTabletOpen);

        if (open)
        {
            if (pairs.Count > 0) SelectPair(1);
            else RefreshAllClues();
        }

        if (verboseLogs) Debug.Log($"[PauseAndClues] Tablet {(open ? "OPEN" : "CLOSED")}");
    }

    private void InternalShowTablet(bool visible)
    {
        if (displayMode == DisplayMode.CanvasGroup)
        {
            if (!tabletCanvasGroup)
            {
                if (verboseLogs) Debug.LogError("[PauseAndClues] CanvasGroup no asignado.");
                return;
            }
            tabletCanvasGroup.alpha = visible ? 1f : 0f;
            tabletCanvasGroup.interactable = visible;
            tabletCanvasGroup.blocksRaycasts = visible;
        }
        else // SetActive
        {
            if (!tabletRootOrContent)
            {
                if (verboseLogs) Debug.LogError("[PauseAndClues] tabletRootOrContent no asignado.");
                return;
            }
            if (transform.IsChildOf(tabletRootOrContent.transform))
                Debug.LogError("[PauseAndClues] SetActive: mueve ESTE script fuera del objeto que apagas o usa CanvasGroup.");

            SafeSetActive(tabletRootOrContent, visible);
        }
    }

    private void SetHUDVisible(bool visible)
    {
        foreach (var go in hudRoots) SafeSetActive(go, visible);
    }

    private void SetPaused(bool value)
    {
        paused = value;
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnResumeButton() => SetTabletOpen(false);

    public void OnMainMenuButton()
    {
        SetTabletOpen(false);
        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
    }

    // ===================== UI Pistas =====================
    private void WireUIOnce()
    {
        foreach (var p in pairs)
        {
            if (p?.pista == null) continue;

            p.pista.SetSelected(false);
            p.pista.SetChecked(false); // palomita oculta al inicio

            if (p.pista.selectButton)
            {
                int idx = p.index;
                p.pista.selectButton.onClick.RemoveAllListeners();
                p.pista.selectButton.onClick.AddListener(() => SelectPair(idx));
            }

            if (p.pista.circleBase) p.pista.circleBase.enabled = true;

            if (p.pista.checkImage)
            {
                p.pista.checkImage.gameObject.SetActive(false);
                p.pista.checkImage.enabled = false;
                p.pista.checkImage.transform.SetAsLastSibling();
            }
            else
            {
                Debug.LogWarning($"[PauseAndClues] Falta checkImage en pista #{p.index}. Asigna la Image de la palomita en el Inspector.");
            }
        }

        if (verboseLogs) Debug.Log("[PauseAndClues] UI wired");
    }

    private void BuildIdIndex()
    {
        idToIndex.Clear();
        foreach (var p in pairs)
        {
            if (p == null) continue;
            string key = Norm(p.catId);
            if (!string.IsNullOrEmpty(key) && !idToIndex.ContainsKey(key))
                idToIndex.Add(key, p.index);
        }
        if (verboseLogs) Debug.Log($"[PauseAndClues] ID map built: {idToIndex.Count} entradas.");
    }

    public void SelectPair(int index)
    {
        selectedIndex = index;

        foreach (var p in pairs)
        {
            bool sel = (p.index == index);
            p.pista?.SetSelected(sel);
        }

        var pair = GetPair(index);
        if (pair?.pista == null) return;

        bool collected = collectedIdx.Contains(index) || (!string.IsNullOrEmpty(pair.catId) && collectedIds.Contains(Norm(pair.catId)));
        pair.pista.SetChecked(collected);

        if (verboseLogs) Debug.Log($"[PauseAndClues] SelectPair {index} (collected={collected})");
    }

    private void RefreshAllClues()
    {
        foreach (var p in pairs)
        {
            if (p?.pista == null) continue;

            bool isCollected = collectedIdx.Contains(p.index) || (!string.IsNullOrEmpty(p.catId) && collectedIds.Contains(Norm(p.catId)));

            if (p.pista.circleBase) p.pista.circleBase.enabled = true;
            p.pista.SetChecked(isCollected);

            bool isSelected = (p.index == selectedIndex);
            p.pista.SetSelected(isSelected);
        }
    }

    private ClueCatPairLite GetPair(int index) => pairs.FirstOrDefault(x => x.index == index);

    private void SafeSetActive(GameObject go, bool active)
    {
        if (go && go.activeSelf != active) go.SetActive(active);
    }

    private void ValidateConfig()
    {
        if (displayMode == DisplayMode.SetActive)
        {
            if (!tabletRootOrContent)
                Debug.LogWarning("[PauseAndClues] SetActive: asigna tabletRootOrContent.");
            else if (transform.IsChildOf(tabletRootOrContent.transform))
                Debug.LogError("[PauseAndClues] SetActive: mueve ESTE script FUERA de tabletRootOrContent o usa CanvasGroup.");
        }
        else // CanvasGroup
        {
            if (!tabletCanvasGroup)
                Debug.LogWarning("[PauseAndClues] CanvasGroup: asigna tabletCanvasGroup.");
        }
    }

    // ===================== WATCHERS: vínculo directo al GameObject del gato =====================
    private void StartCatWatchers()
    {
        foreach (var p in pairs)
        {
            if (p == null) continue;

            // Si asignaste el GameObject del gato, vigílalo:
            if (p.catObject != null)
                StartCoroutine(WatchCatAndMark(p.index, p.catObject));
        }
    }

    private IEnumerator WatchCatAndMark(int index, GameObject catGO)
    {
        if (verboseLogs) Debug.Log($"[PauseAndClues] Watcher ON para pista #{index}: {catGO.name}");

        // Espera a que el gato sea destruido (o quede null por Destroy)
        while (catGO != null)
            yield return null;

        // Si ya estaba marcado por otro camino, no repitas
        if (collectedIdx.Contains(index)) yield break;

        if (verboseLogs) Debug.Log($"[PauseAndClues] Detectado gato destruido -> pista #{index} CHECK");
        NotifyCatCollectedByIndex(index); // Marca palomita
    }

    // ===================== Notificaciones manuales (opcional) =====================
    public void NotifyCatCollectedByIndex(int index)
    {
        if (index < 1 || index > pairs.Count)
        {
            if (verboseLogs) Debug.LogWarning($"[PauseAndClues] índice fuera de rango: {index}");
            return;
        }
        if (!collectedIdx.Add(index)) return;

        var pair = GetPair(index);
        if (pair?.pista == null)
        {
            Debug.LogError($"[PauseAndClues] Pista #{index} no está configurada.");
            return;
        }
        if (!pair.pista.checkImage)
        {
            Debug.LogError($"[PauseAndClues] La Pista #{index} no tiene checkImage asignado.");
            return;
        }

        pair.pista.SetChecked(true);

        // También marca por ID, si lo hay
        if (!string.IsNullOrEmpty(pair.catId))
            collectedIds.Add(Norm(pair.catId));

        if (verboseLogs) Debug.Log($"[PauseAndClues] Cat collected: index {index} -> pista {index} CHECK");
    }

    public void NotifyCatCollected(string catIdString)
    {
        if (string.IsNullOrEmpty(catIdString)) return;

        string key = Norm(catIdString);
        bool handled = false;

        if (idToIndex.TryGetValue(key, out int idxFromId))
        {
            handled = true;
            collectedIds.Add(key);
            NotifyCatCollectedByIndex(idxFromId);
        }

        if (!handled)
        {
            int idx = ExtractIndexFromString(catIdString);
            if (idx > 0)
            {
                NotifyCatCollectedByIndex(idx);
                string expectedKey = Norm($"Gato{idx}");
                collectedIds.Add(expectedKey);
                handled = true;
            }
        }

        if (!handled)
        {
            Debug.LogWarning($"[PauseAndClues] No pude mapear ID '{catIdString}'. Asigna catObject o usa NotifyCatCollectedByIndex.");
        }
    }

    // ===================== Utilidades de ID =====================
    private string Norm(string s)
    {
        if (!tolerateIdVariations || string.IsNullOrEmpty(s)) return s;
        s = s.ToLowerInvariant();
        StringBuilder sb = new StringBuilder(s.Length);
        foreach (char ch in s)
            if (!char.IsWhiteSpace(ch) && ch != '_' && ch != '-') sb.Append(ch);
        return sb.ToString(); // "Gato 1" => "gato1"
    }

    private int ExtractIndexFromString(string s)
    {
        var m = Regex.Match(s, @"\d+");
        if (!m.Success) return -1;
        if (int.TryParse(m.Value, out int idx)) return idx;
        return -1;
    }

    // ===================== TEST de depuración =====================
    [ContextMenu("TEST ► Marcar todos los checks")]
    private void __TEST_MarcarTodos()
    {
        foreach (var p in pairs)
        {
            if (p?.pista?.checkImage)
            {
                p.pista.SetChecked(true);
            }
            else
            {
                Debug.LogWarning($"[PauseAndClues][TEST] Falta checkImage en pista #{p?.index}");
            }
        }
    }
}

[Serializable]
public class ClueCatPairLite
{
    [HideInInspector] public int index; // 1..N

    [Header("Vínculo directo (recomendado)")]
    [Tooltip("Asigna aquí el GameObject del gato en la escena. Al destruirse, se marca la palomita.")]
    public GameObject catObject;

    [Header("Vínculo por ID (opcional)")]
    [Tooltip("Si lo dejas vacío y autoFillIdsAsGatoi = true, se usará 'Gato{index}'. Se toleran 'Gato 1', 'gato_1', etc.")]
    public string catId;

    [Header("UI de la pista")]
    public bool autoLabelInEditor = false;
    public ClueSlotLite pista;
}

[Serializable]
public class ClueSlotLite
{
    public Button selectButton; // opcional: clic en la pista para seleccionarla
    public TMP_Text label;        // texto de la pista
    public Image circleBase;   // círculo vacío
    public Image checkImage;   // palomita

    public void SetSelected(bool value)
    {
        // Si quieres resaltar el texto:
        // if (label) label.fontStyle = value ? FontStyles.Bold : FontStyles.Normal;
    }

    public void SetChecked(bool value)
    {
        if (!checkImage)
        {
            Debug.LogError("[PauseAndClues] checkImage no asignado en esta PISTA (revisa el Inspector del par).");
            return;
        }

        // Activa/Desactiva de verdad
        checkImage.gameObject.SetActive(value);
        checkImage.enabled = value;

        // Garantiza alpha/orden al encender
        if (value)
        {
            var c = checkImage.color;
            if (c.a < 0.99f) checkImage.color = new Color(c.r, c.g, c.b, 1f);
            checkImage.transform.SetAsLastSibling();
        }
    }
}
