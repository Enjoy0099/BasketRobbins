using UnityEngine;
using UnityEngine.EventSystems;

// Auto-added by UIFocusFixer — do not add manually
public class UIElementFocusFixer : MonoBehaviour,
    IPointerUpHandler,
    IPointerExitHandler
{
    [HideInInspector] public bool clearOnPointerUp = true;
    [HideInInspector] public bool clearOnPointerExit = false;

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!clearOnPointerUp) return;
        ClearFocus();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!clearOnPointerExit) return;
        ClearFocus();
    }

    private void ClearFocus()
    {
        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}