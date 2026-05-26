using UnityEngine;
using UnityEngine.UI;

namespace BasketRobbins
{
    public class WindSliderUI : MonoBehaviour
    {
        [Header("References")]
        public BasketballSlider windSlider;
        public WindZone windZone_Script;

        private Image fillImage;
        private Image handleImage;
        private Material handleMaterial;

        private void Awake()
        {
            CacheComponents();

        }

        private void CacheComponents()
        {
            fillImage = windSlider.fillRect.GetComponent<Image>();

            handleImage = windSlider.handleRect.GetComponent<Image>();

            if (handleImage.material != null)
            {
                handleMaterial = new Material(handleImage.material);
                handleImage.material = handleMaterial;
            }
        }

        void OnEnable()
        {
            windSlider.onInteractableChanged.AddListener(OnSliderInteractableChanged);
        }

        void OnDisable()
        {
            windSlider.onInteractableChanged.RemoveListener(OnSliderInteractableChanged);
        }

        void Start()
        {
            windSlider.minValue = 0f;
            windSlider.maxValue = 1f;
            windSlider.value = windZone_Script.windPower;

            windSlider.onValueChanged.AddListener(OnSliderChanged);

            // Refresh label immediately
            //RefreshLabel(ballMovement.power);
        }

        void OnDestroy()
        {
            if (windSlider != null)
                windSlider.onValueChanged.RemoveListener(OnSliderChanged);
        }

        private void OnSliderChanged(float value)
        {
            windZone_Script.SetWindPower(value);   // single source of truth
                                                   //RefreshLabel(value);
        }


        void OnSliderInteractableChanged(bool isInteractable)
        {
            //Debug.Log("Slider interactable: " + isInteractable);
        }
    }
}

