using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Attach this to your Canvas/UI GameObject.
/// Wire up the Slider and (optionally) a TMP_Text label in the Inspector.
/// Point BasketBallMovement to your BasketBallMovement component.
/// </summary>
public class PowerSliderUI : MonoBehaviour
{
    [Header("References")]
    public BasketballSlider powerSlider;                  // Unity UI Slider (0 → 1)
    public BasketBallMovement ballMovement_Script;     // The movement script

    /*[Header("Optional Label")]
    public TMP_Text powerLabel;                 // e.g. "Power: 75%"*/


    [Header("Slider Color")]
    [SerializeField] private Color sliderEnabled_Color;
    [SerializeField] private Color sliderDisabled_Color;
    [SerializeField] private Color sliderHandle_Color;

    private Image fillImage;
    private Image handleImage;
    private Material handleMaterial;

    private void Awake()
    {
        CacheComponents();

    }

    private void CacheComponents()
    {
        fillImage = powerSlider.fillRect.GetComponent<Image>();

        handleImage = powerSlider.handleRect.GetComponent<Image>();

        if (handleImage.material != null)
        {
            handleMaterial = new Material(handleImage.material);
            handleImage.material = handleMaterial;
        }

        DebugMaterialProperties(handleMaterial);
    }

    private void DebugMaterialProperties(Material mat)
    {

#if UNITY_EDITOR
        int propCount = UnityEditor.ShaderUtil.GetPropertyCount(mat.shader);
        for (int i = 0; i < propCount; i++)
        {
            var type = UnityEditor.ShaderUtil.GetPropertyType(mat.shader, i);
            var name = UnityEditor.ShaderUtil.GetPropertyName(mat.shader, i);
            var desc = UnityEditor.ShaderUtil.GetPropertyDescription(mat.shader, i);

        }
#endif
    }

    void OnEnable()
    {
        powerSlider.onBecameNonInteractable.AddListener(OnSliderDisabled);
        powerSlider.onBecameInteractable.AddListener(OnSliderEnabled);
        powerSlider.onInteractableChanged.AddListener(OnSliderInteractableChanged);
    }

    void OnDisable()
    {
        powerSlider.onBecameNonInteractable.RemoveListener(OnSliderDisabled);
        powerSlider.onBecameInteractable.RemoveListener(OnSliderEnabled);
        powerSlider.onInteractableChanged.RemoveListener(OnSliderInteractableChanged);
    }

    void Start()
    {
        powerSlider.minValue = 0f;
        powerSlider.maxValue = 1f;
        powerSlider.value = ballMovement_Script.power;

        powerSlider.onValueChanged.AddListener(OnSliderChanged);

        // Refresh label immediately
        //RefreshLabel(ballMovement.power);
    }

    void OnDestroy()
    {
        if (powerSlider != null)
            powerSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        ballMovement_Script.SetPower(value);   // single source of truth
        //RefreshLabel(value);
    }

    /*private void RefreshLabel(float value)
    {
        if (powerLabel == null) return;

        float velocity = Mathf.Lerp(ballMovement.minShootVelocity, ballMovement.maxShootVelocity, value);
        powerLabel.text = $"Power: {Mathf.RoundToInt(value * 100f)}%  ({velocity:F1} m/s)";
    }*/

    void OnSliderDisabled()
    {
        SetFillColor(sliderDisabled_Color);
        SetHandleColor(WithAlpha(sliderHandle_Color, sliderDisabled_Color.a));
    }

    void OnSliderEnabled()
    {
        SetFillColor(sliderEnabled_Color);
        SetHandleColor(WithAlpha(sliderHandle_Color, sliderEnabled_Color.a));
    }

    private void SetFillColor(Color color)
    {
        if (fillImage == null) return;
        fillImage.color = color;
    }

    private Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    private void SetHandleColor(Color color)
    {
        if (handleImage == null) return;

        if (handleMaterial != null)
        {
            handleMaterial.SetColor("_BaseColor", color);
            handleMaterial.SetColor("_Color", color);
        }
            
        else
            handleImage.color = color; // fallback if no custom material
    }


    void OnSliderInteractableChanged(bool isInteractable)
    {
        //Debug.Log("Slider interactable: " + isInteractable);
    }
}