using UnityEngine;
using UnityEngine.EventSystems;

// Lightweight — only responds to pointer events, zero Update() cost
public class UIElementFocusFixer : MonoBehaviour,
    IPointerUpHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [HideInInspector] public bool clearOnPointerUp = true;
    [HideInInspector] public bool clearOnPointerExit = false;

    // Cached — avoid EventSystem.current lookup every call
    private EventSystem eventSystem;

    void Awake()
    {
        eventSystem = EventSystem.current;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (clearOnPointerUp) ClearFocus();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (clearOnPointerExit) ClearFocus();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ClearFocus();
    }

    private void ClearFocus()
    {
        if (eventSystem != null)
            eventSystem.SetSelectedGameObject(null);
    }
}