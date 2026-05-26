using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

// Place on ONE GameObject only (e.g. EventSystem or GameManager)
// Never on Canvas
public class UIFocusFixer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool clearFocusOnPointerUp = true;
    [SerializeField] private bool clearFocusOnPointerExit = false;

    // Singleton — avoids FindObjectOfType calls
    public static UIFocusFixer Instance { get; private set; }

    // Track patched objects to avoid duplicate work
    private readonly HashSet<int> patchedInstanceIDs = new HashSet<int>();

    // Reusable buffer — zero allocation on repeated calls
    private static readonly List<Selectable> selectablesBuffer = new List<Selectable>();
    private static readonly List<Canvas> canvasBuffer = new List<Canvas>();

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        PatchAll();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        patchedInstanceIDs.Clear();
    }

    // Call this after spawning new UI
    public void PatchAll()
    {
        // Reuse buffer — no new List allocation
        canvasBuffer.Clear();
        selectablesBuffer.Clear();

        // Get all canvases into reused buffer
        GetAllCanvases(canvasBuffer);

        foreach (Canvas canvas in canvasBuffer)
        {
            if (canvas == null) continue;

            // Fill reused buffer
            canvas.GetComponentsInChildren(true, selectablesBuffer);

            foreach (Selectable s in selectablesBuffer)
            {
                if (s == null) continue;
                PatchSingle(s.gameObject);
            }

            selectablesBuffer.Clear();
        }

        canvasBuffer.Clear();
    }

    // Patch one specific object — call after spawning a single UI element
    public void PatchSingle(GameObject target)
    {
        if (target == null) return;

        int id = target.GetInstanceID();

        // Skip if already patched — HashSet lookup is O(1)
        if (patchedInstanceIDs.Contains(id)) return;

        patchedInstanceIDs.Add(id);

        UIElementFocusFixer fixer = target.GetComponent<UIElementFocusFixer>();
        if (fixer == null)
            fixer = target.AddComponent<UIElementFocusFixer>();

        fixer.clearOnPointerUp = clearFocusOnPointerUp;
        fixer.clearOnPointerExit = clearFocusOnPointerExit;
    }

    // Non-allocating canvas fetch using FindObjectsOfTypeNonAlloc (Unity < 2023)
    // OR FindObjectsByType with no sort (Unity 2023+)
    private void GetAllCanvases(List<Canvas> buffer)
    {
#if UNITY_2023_1_OR_NEWER
        // FindObjectsByType with None sort is fastest in 2023+
        Canvas[] found = FindObjectsByType<Canvas>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        buffer.AddRange(found);
#else
        // Legacy path
        Canvas[] found = FindObjectsOfType<Canvas>(true);
        buffer.AddRange(found);
#endif
    }
}