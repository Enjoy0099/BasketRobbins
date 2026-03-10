
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Turret : MonoBehaviour
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


    #endregion

    #region Private Variables

    private bool isShooting = false;

    private float currentBallAngle = 0.0f;

    #endregion

    void Start()
    {
        currentBallAngle = 0.0f;
    }

    private IEnumerator Shoot()
    {
        isShooting = true;

        /*var cannonball = GameObject.Instantiate(this.cannonBallPrefab);
        cannonball.transform.position = this.ballTransform.position;*/
        var rb = ballTransform.GetComponentInChildren<Rigidbody>();

        rb.isKinematic = false;


        //No negative drag ^^
        rb.drag = Mathf.Clamp(ballDrag, 0.0f, float.PositiveInfinity);
        rb.AddForce(shootVelocity * ballTransform.forward, ForceMode.VelocityChange);

        yield return new WaitForSeconds(shootingCooldown);

        isShooting = false;
    }

    private void HandleInput()
    {
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
    }

    private void CalculateTrajectoryLine()
    {
        List<Vector3> lrPositions = ListPool<Vector3>.Get();

        Vector3 currentPosition = transform.position;
        float currentTime = 0.0f;

        while (currentPosition.y > targetHeight)
        {
            currentPosition = Trajectory.GetPosition(transform.position, shootVelocity * transform.forward, ballDrag, currentTime);
            currentTime += samplingRate;
            lrPositions.Add(currentPosition);
        }

        lineRenderer.positionCount = lrPositions.Count;

        lineRenderer.SetPositions(lrPositions.ToArray());

        ListPool<Vector3>.Release(lrPositions);
    }

    private void PlaceCrosshair()
    {

        float timeToHitGround = Trajectory.GetTimeForReachingYOnTheWayDown(this.ballTransform.position,
            this.shootVelocity * this.ballTransform.forward,
            this.ballDrag,
            this.targetHeight);

        this.crosshair.transform.position = Trajectory.GetPosition(this.ballTransform.position,
            this.shootVelocity * this.ballTransform.forward,
            this.ballDrag,
            timeToHitGround);

        this.crosshair.transform.position += Vector3.up * this.crosshairOffset;
    }

    void Update()
    {
        if (samplingRate > 0.0f)
        {
            CalculateTrajectoryLine();
        }

        HandleInput();

        //PlaceCrosshair();
    }
}
