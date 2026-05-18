// Assets/Editor/BasketballSliderMigrator.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public static class BasketballSliderMigrator
{
    [MenuItem("Tools/Migrate All Sliders to BasketballSlider")]
    static void MigrateAll()
    {
        Slider[] sliders = Object.FindObjectsByType<Slider>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        int count = 0;
        foreach (Slider s in sliders)
        {
            if (s is BasketballSlider) continue;
            MigrateOne(s);
            count++;
        }

        Debug.Log($"Migrated {count} Slider(s) to BasketballSlider.");
    }

    [MenuItem("Tools/Migrate Selected Sliders to BasketballSlider")]
    static void MigrateSelected()
    {
        GameObject[] selected = Selection.gameObjects;

        if (selected.Length == 0)
        {
            Debug.LogWarning("Nothing selected.");
            return;
        }

        int count = 0;
        foreach (GameObject go in selected)
        {
            Slider s = go.GetComponent<Slider>();

            if (s == null)
            {
                Debug.LogWarning($"{go.name} has no Slider — skipped.");
                continue;
            }

            if (s is BasketballSlider)
            {
                Debug.LogWarning($"{go.name} is already a BasketballSlider — skipped.");
                continue;
            }

            MigrateOne(s);
            count++;
        }

        Debug.Log($"Migrated {count} Slider(s) to BasketballSlider.");
    }

    static void MigrateOne(Slider old)
    {
        GameObject go = old.gameObject;

        // ── 1. Record all Slider values ──────────────────────────
        float minValue = old.minValue;
        float maxValue = old.maxValue;
        float value = old.value;
        bool wholeNumbers = old.wholeNumbers;
        Slider.Direction direction = old.direction;
        RectTransform fillRect = old.fillRect;
        RectTransform handleRect = old.handleRect;
        Slider.SliderEvent onValueChanged = old.onValueChanged;

        // Selectable (base class) fields
        Navigation navigation = old.navigation;
        Selectable.Transition transition = old.transition;
        ColorBlock colors = old.colors;
        SpriteState spriteState = old.spriteState;
        AnimationTriggers animTriggers = old.animationTriggers;
        bool interactable = old.interactable;
        Graphic targetGraphic = old.targetGraphic;

        // ── 2. Register undo so Ctrl+Z works ──────────────────────
        Undo.RegisterFullObjectHierarchyUndo(go, "Migrate to BasketballSlider");

        // ── 3. Remove old component ───────────────────────────────
        Object.DestroyImmediate(old);

        // ── 4. Add your custom component ─────────────────────────
        BasketballSlider nb = go.AddComponent<BasketballSlider>();

        // ── 5. Re-apply all values ────────────────────────────────
        nb.minValue = minValue;
        nb.maxValue = maxValue;
        nb.value = value;
        nb.wholeNumbers = wholeNumbers;
        nb.direction = direction;
        nb.fillRect = fillRect;
        nb.handleRect = handleRect;
        nb.onValueChanged = onValueChanged;

        nb.navigation = navigation;
        nb.transition = transition;
        nb.colors = colors;
        nb.spriteState = spriteState;
        nb.animationTriggers = animTriggers;
        nb.interactable = interactable;
        nb.targetGraphic = targetGraphic;

        // ── 6. Mark scene dirty so Unity saves it ─────────────────
        EditorUtility.SetDirty(go);
    }
}