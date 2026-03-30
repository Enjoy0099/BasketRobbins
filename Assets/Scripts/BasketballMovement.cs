
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BasketballMovement : MonoBehaviour
{

    #region Public Variables

    public float shootingCooldown = 1.0f;
    public float turnSpeed = 3.0f;
    public float ballUpDownSpeed = 5.0f;
    public float targetHeight = 0.0f;
    public float samplingRate = 0.2f;
    public float ballDrag = 0.2f;
    public float crosshairOffset = 0.01f;

    public float shootVelocity = 15.0f;

    public GameObject cannonBallPrefab;

    public GameObject turnTable;
    public GameObject crosshair;

    public LineRenderer lineRenderer;

    public Transform ballTransform;

    public Vector2 ballUpDownRange;

    public Vector3 ballInitialPosition;

    #endregion



    #region Private Variables

    private bool isShooting = false;

    private float currentBallAngle = 0.0f;

    private float ballRadius;

    private bool canBallMove = false;

    #endregion

    private void OnEnable()
    {
        GameManager.OnSimulationStart += SimulationStart;
        GameManager.OnSimulationStop += SimulationStop;
    }

    private void OnDisable()
    {
        GameManager.OnSimulationStart -= SimulationStart;
        GameManager.OnSimulationStop -= SimulationStop;
    }

    void SimulationStart()
    {
        canBallMove = true;
        ballResetPosition();
    }

    void SimulationStop()
    {
        canBallMove = false;
        ballResetPosition();
    }

    void Start()
    {
        currentBallAngle = 0.0f;
        ballInitialPosition = ballTransform.position;

        ballRadius = ballTransform.GetComponent<SphereCollider>().radius;
    }

    private IEnumerator Shoot()
    {
        isShooting = true;
        Vector3 shootDirection = transform.forward; // turret's forward

        /*var cannonball = GameObject.Instantiate(this.cannonBallPrefab);
        cannonball.transform.position = this.ballTransform.position;*/
        var rb = ballTransform.GetComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.WakeUp();


        //No negative drag ^^
        rb.drag = Mathf.Clamp(ballDrag, 0.0f, float.PositiveInfinity);
        rb.velocity = Vector3.zero;
        rb.AddForce(shootVelocity * shootDirection, ForceMode.VelocityChange);

        yield return new WaitForSeconds(shootingCooldown);
    }

    private void HandleInput()
    {
        if (!canBallMove) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        transform.Rotate(Vector3.up, horizontal * turnSpeed * Time.deltaTime);


        currentBallAngle += vertical * this.ballUpDownSpeed * Time.deltaTime;
        currentBallAngle = Mathf.Clamp(currentBallAngle, ballUpDownRange.y, ballUpDownRange.x);

        Vector3 rot = transform.localEulerAngles;
        rot.x = currentBallAngle;

        transform.localEulerAngles = rot;


        if (!isShooting && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Shoot());
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("SPACE PRESSED");
        }
    }

    private void CalculateTrajectoryLine()
    {
        List<Vector3> lrPositions = ListPool<Vector3>.Get();

        Vector3 previousPosition = transform.position;
        Vector3 currentPosition = previousPosition;
        float currentTime = 0.0f;

        lrPositions.Add(currentPosition);

        while (true)
        {
            currentTime += samplingRate;
            currentPosition = Trajectory.GetPosition(transform.position, shootVelocity * transform.forward, ballDrag, currentTime);

            //hit any collider.
            if (Physics.Linecast(previousPosition, currentPosition, out RaycastHit hit))
            {
                lrPositions.Add(hit.point);
                break;
            }

            //if the line has reached target height
            if (currentPosition.y <= targetHeight)
            {
                lrPositions.Add(currentPosition);
                break;
            }

            lrPositions.Add(currentPosition);
            previousPosition = currentPosition;

            if (lrPositions.Count > 100) break;

        }

        lineRenderer.positionCount = lrPositions.Count;

        lineRenderer.SetPositions(lrPositions.ToArray());

        ListPool<Vector3>.Release(lrPositions);
    }

    private void PlaceCrosshair()
    {
        Vector3 shootDirection = transform.forward;

        Vector3 previousPosition = ballTransform.position;
        Vector3 currentPosition = previousPosition;
        float currentTime = 0f;

        Vector3 hitPoint = Vector3.zero;
        Vector3 hitNormal = Vector3.up; // default for flat ground

        while (true)
        {
            currentTime += samplingRate;
            currentPosition = Trajectory.GetPosition(ballTransform.position, shootVelocity * shootDirection, ballDrag, currentTime);

            Vector3 segmentDir = currentPosition - previousPosition;
            float segmentLength = segmentDir.magnitude;

            if (segmentLength > 0f)
            {
                // SphereCast for collision considering ball radius
                if (Physics.SphereCast(previousPosition, ballRadius, segmentDir.normalized, out RaycastHit hit, segmentLength))
                {
                    hitPoint = hit.point;
                    hitNormal = hit.normal;
                    break;
                }
            }

            // Stop if reaching target height
            if (currentPosition.y <= targetHeight)
            {
                hitPoint = currentPosition;
                hitNormal = Vector3.up; // flat ground
                break;
            }

            previousPosition = currentPosition;

            if (currentTime > 5f) break; // safety
        }

        // Place crosshair slightly above the surface
        crosshair.transform.position = hitPoint + Vector3.up * crosshairOffset;

        // Rotate crosshair to align with surface normal
        crosshair.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal) * Quaternion.Euler(90f, 0f, 0f);
    }

    void Update()
    {
        if(isShooting)
            return;

        if (samplingRate > 0.0f)
        {
            CalculateTrajectoryLine();
        }

        HandleInput();

        //PlaceCrosshair();
    }

    public void ballResetPosition()
    {
        var rb = ballTransform.GetComponent<Rigidbody>();

        rb.isKinematic = true;
        ballTransform.position = ballInitialPosition;
        ballTransform.localRotation = Quaternion.identity;

        isShooting = false;
    }
}
