using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Space(5f)]
    [Header("Manager")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private ScoreManager scoreManager;

    [Space(10f)]
    [Header("Scripts")]
    public BasketBallMovement ballMovement_Script;

    public static event Action OnSimulationStart_Action;
    public static event Action OnSimulationStop_Action;
    public static event Action OnBallResetPosition_Action;
    public static event Action OnBallShoot_Action;

    [Space(20f)]
    [Header("UI")]
    [SerializeField] private Button simulationButton;
    [SerializeField] private TextMeshProUGUI simulationButtonText;
    [SerializeField] private Button ball_ResetPosition;
    [SerializeField] private Button ball_Shoot;
    [SerializeField] private Slider ball_ShootPower;

    [Space(20f)]
    [Header("Effect")]
    [SerializeField] private ParticleSystem NormalGoalEffect_Score;
    [SerializeField] private ParticleSystem SwishGoalEffect_Score;



    private bool isSimulating = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        //AudioPlay_BGMusic();
    }

    private void Start()
    {

        SimulationToggle();

        OnSimulationStop_Action?.Invoke();
    }

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
            OnSimulationStart_Action?.Invoke();
        }
        else
        {
            OnSimulationStop_Action?.Invoke();
        }

        SimulationToggle();
    }

    private void SimulationToggle()
    {
        if(isSimulating)
        {
            ball_ResetPosition.interactable = true;
            ball_Shoot.interactable = true;
            ball_ShootPower.interactable = true;
        }
        else
        {
            ball_ResetPosition.interactable = false;
            ball_Shoot.interactable = false;
            ball_ShootPower.interactable = false;
        }
    }

    public void BallResetPosition()
    {
        OnBallResetPosition_Action?.Invoke();
    }
    
    public void BallShoot()
    {
        OnBallShoot_Action?.Invoke();
    }

    public void ScoreManage_AddScore(bool value)
    {
        scoreManager.AddScore(value);
    }

    #region Sounds
    public void AudioPlay_BGMusic()
    {
        audioManager.PlayMusic();
    }

    public void AudioPlay_Swish()
    {
        audioManager.PlaySwish();
    }

    public void AudioPlay_Normal()
    {
        audioManager.PlayNormal();
    }

    public void AudioPlay_BallBounce(float impactForce)
    {
        audioManager.PlayBounce(impactForce);
    }

    public void AudioPlay_BallRimHit()
    {
        audioManager.PlayRimHit();
    }

    #endregion


    #region Effect
    public void PlayEffect_NormalScore()
    {
        NormalGoalEffect_Score.Play();
    }
    public void PlayEffect_SwishScore()
    {
        SwishGoalEffect_Score.Play();
    }

    #endregion

}
