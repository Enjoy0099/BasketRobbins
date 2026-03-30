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
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /*public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleSimulation();
        }
    }*/

    public bool IsSimulating()
    {
        return isSimulating;
    }

    public void ToggleSimulation()
    {
        isSimulating = !isSimulating;

        simulationButtonText.text = isSimulating ? "Stop" : "Simulate";

        if (isSimulating)
        {
            OnSimulationStart?.Invoke();
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
