// Assets/Scripts/UI/BasketballSlider.cs
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BasketballSlider : Slider
{
    [Header("Disable When Non Interactable")]
    [SerializeField]
    private Behaviour[] componentsToDisable;

    [Header("Interactable Events")]
    public UnityEvent onBecameInteractable;
    public UnityEvent onBecameNonInteractable;

    // Optional: if you need to pass the bool state directly
    public UnityEvent<bool> onInteractableChanged;

    private bool lastInteractableState = true;

    [Header("Value Clamp")]
    [SerializeField] private bool clampValue = true;

    [Range(0f, 1f)]
    [SerializeField] private float minAllowedValue = 0.1f;

    [Range(0f, 1f)]
    [SerializeField] private float maxAllowedValue = 0.9f;

    protected override void Set(float input, bool sendCallback = true)
    {
        if (clampValue)
        {
            input = Mathf.Clamp(input, minAllowedValue, maxAllowedValue);
        }

        base.Set(input, sendCallback);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        lastInteractableState = base.IsInteractable();
        RefreshComponents(lastInteractableState);
    }

    public override bool IsInteractable()
    {
        bool state = base.IsInteractable();

        if (state != lastInteractableState)
        {
            lastInteractableState = state;
            RefreshComponents(state);
            onInteractableChanged?.Invoke(state);

            if (state)
                onBecameInteractable?.Invoke();
            else
                onBecameNonInteractable?.Invoke();
        }

        return state;
    }

    private void RefreshComponents()
    {
        RefreshComponents(base.IsInteractable());
    }

    private void RefreshComponents(bool state)
    {
        // ── null check added — array is empty during AddComponent ──
        if (componentsToDisable == null) return;

        foreach (Behaviour comp in componentsToDisable)
        {
            if (comp != null)
                comp.enabled = state;
        }
    }

}