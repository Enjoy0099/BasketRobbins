using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Attach to Canvas — handles ALL UI focus issues
// Zero per-frame allocation, mobile friendly
public class UIFocusFixer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool clearFocusOnPointerUp = true;
    [SerializeField] private bool clearFocusOnPointerExit = false;

    [Header("Performance")]
    [SerializeField] private bool useFrameCheck = false; // only enable if still having issues
    [SerializeField] private int frameInterval = 10;   // check every 10 frames if enabled

    private int frameCounter = 0;

    void Start()
    {
        // Run ONCE at start — no per frame cost
        PatchAll();
    }

    void Update()
    {
        // Disabled by default — only enable if pointer events miss something
        if (!useFrameCheck) return;

        frameCounter++;
        if (frameCounter < frameInterval) return;
        frameCounter = 0;

        // Throttled check — runs every N frames not every frame
        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void PatchAll()
    {
        Selectable[] selectables = GetComponentsInChildren<Selectable>(true);
        foreach (Selectable s in selectables)
            Patch(s.gameObject);
    }

    public void Patch(GameObject target)
    {
        if (target.GetComponent<UIElementFocusFixer>() != null) return;

        UIElementFocusFixer fixer = target.AddComponent<UIElementFocusFixer>();
        fixer.clearOnPointerUp = clearFocusOnPointerUp;
        fixer.clearOnPointerExit = clearFocusOnPointerExit;
    }
}