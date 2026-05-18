using BasketRobbins;
using UnityEngine;

public class BallController : MonoBehaviour
{
    private GameManager gameManager_Script;

    public float throwForce = 10f;

    private Rigidbody rb;

    [Header("Scoring State (read only)")]
    [SerializeField] private bool _touchedRim;
    [SerializeField] private bool _passedTopDownward;
    [SerializeField] private bool _scoringInProgress;
    [SerializeField] private bool _approachedFromAbove;
    [SerializeField] private bool _enteredFromBelow;

    private BallSquishController squishController;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        gameManager_Script = GameManager.Instance;
        squishController = GetComponent<BallSquishController>();
    }

    public void ThrowBall()
    {
        ResetState();

        // Only allow in simulation mode
        if (!GameManager.Instance.IsSimulating()) return;

        // Prevent re-throw
        if (rb.velocity != Vector3.zero) return;

        Vector3 dir = Camera.main.transform.forward + Vector3.up * 0.3f;
        rb.AddForce(dir * throwForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.collider.CompareTag(GameConstants.Tag.Rim))
        {
            gameManager_Script.AudioPlay_BallRimHit();
        }
        else
        {
            float impactForce = collision.relativeVelocity.magnitude;

            gameManager_Script.AudioPlay_BallBounce(impactForce);
        }
    }

    public bool touchedRim
    {
        get => _touchedRim;
        set => _touchedRim = value;
    }

    public bool passedTopDownward
    {
        get => _passedTopDownward;
        set => _passedTopDownward = value;
    }

    public bool scoringInProgress
    {
        get => _scoringInProgress;
        set => _scoringInProgress = value;
    }

    public bool approachedFromAbove
    {
        get => _approachedFromAbove;
        set => _approachedFromAbove = value;
    }

    public bool enteredFromBelow
    {
        get => _enteredFromBelow;
        set => _enteredFromBelow = value;
    }

    public void ResetState()
    {
        _touchedRim = false;
        _passedTopDownward = false;
        _scoringInProgress = false;
        _approachedFromAbove = false;
        _enteredFromBelow = false;

        // Reset squish when ball resets
        if (squishController != null)
            squishController.ResetSquish();
    }

}