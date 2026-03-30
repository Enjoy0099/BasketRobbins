using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static event Action OnSimulationStart;
    public static event Action OnSimulationStop;



    [SerializeField] private Button simulationButton;
    [SerializeField] private TextMeshProUGUI simulationButtonText;

    

    private bool isSimulating = false;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            DontDestroyOnLoad(instance);
    }

    public void ToggleSimulation()
    {
        isSimulating = !isSimulating;

        simulationButtonText.text = isSimulating ? "Stop" : "Simulate";

        if (isSimulating)
        {
            OnSimulationStart?.Invoke();
            Debug.Log(OnSimulationStart?.GetInvocationList().Length ?? 0);
        }
        else
        {
            OnSimulationStop?.Invoke();
        }
    }

    private void Start()
    {
        OnSimulationStop?.Invoke();
    }

}
