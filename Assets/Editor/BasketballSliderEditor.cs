// Assets/Editor/BasketballSliderEditor.cs
using TMPro;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(BasketballSlider))]
public class BasketballSliderEditor : SliderEditor
{
    private SerializedProperty clampValue;
    private SerializedProperty minAllowedValue;
    private SerializedProperty maxAllowedValue;

    private SerializedProperty componentsToDisable;
    private SerializedProperty onBecameInteractable;
    private SerializedProperty onBecameNonInteractable;
    private SerializedProperty onInteractableChanged;

    protected override void OnEnable()
    {
        base.OnEnable();
        clampValue = serializedObject.FindProperty("clampValue");
        minAllowedValue = serializedObject.FindProperty("minAllowedValue");
        maxAllowedValue = serializedObject.FindProperty("maxAllowedValue");

        componentsToDisable = serializedObject.FindProperty("componentsToDisable");
        onBecameInteractable = serializedObject.FindProperty("onBecameInteractable");
        onBecameNonInteractable = serializedObject.FindProperty("onBecameNonInteractable");
        onInteractableChanged = serializedObject.FindProperty("onInteractableChanged");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.Space();
        //EditorGUILayout.LabelField("Value Clamp", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(clampValue);

        if (clampValue.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(minAllowedValue);
            EditorGUILayout.PropertyField(maxAllowedValue);
            EditorGUI.indentLevel--;
        }


        EditorGUILayout.Space();
        //EditorGUILayout.LabelField("Disable When Non Interactable", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(componentsToDisable, true);

        EditorGUILayout.Space();
        //EditorGUILayout.LabelField("Interactable Events", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(onBecameInteractable);
        EditorGUILayout.PropertyField(onBecameNonInteractable);
        EditorGUILayout.PropertyField(onInteractableChanged);

        serializedObject.ApplyModifiedProperties();
    }
}