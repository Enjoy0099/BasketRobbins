
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

    public Vector3 ballInitialPosition;

    #endregion



    #region Private Variables

    private bool isShooting = false;

    private float currentBallAngle = 0.0f;

    #endregion

    void Start()
    {
        currentBallAngle = 0.0f;
        ballInitialPosition = ballTransform.position;

        Debug.Log("Intitial Position of ball: " + ballInitialPosition);
    }

    private IEnumerator Shoot()
    {
        isShooting = true;
        Vector3 shootDirection = transform.forward; // turret's forward

        /*var cannonball = GameObject.Instantiate(this.cannonBallPrefab);
        cannonball.transform.position = this.ballTransform.position;*/
        var rb = ballTransform.GetComponent<Rigidbody>();

        rb.isKinematic = false;


        //No negative drag ^^
        rb.drag = Mathf.Clamp(ballDrag, 0.0f, float.PositiveInfinity);
        rb.AddForce(shootVelocity * shootDirection, ForceMode.VelocityChange);

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

    //private void PlaceCrosshair()
    //{
    //    Vector3 shootDirection = transform.forward;

    //    float timeToHitGround = Trajectory.GetTimeForReachingYOnTheWayDown(ballTransform.position,
    //        shootVelocity * shootDirection,
    //        ballDrag,
    //        targetHeight);

    //    crosshair.transform.position = Trajectory.GetPosition(ballTransform.position, shootVelocity * shootDirection, ballDrag, timeToHitGround);

    //    crosshair.transform.position += Vector3.up * crosshairOffset;
    //}

    void Update()
    {
        if (samplingRate > 0.0f)
        {
            CalculateTrajectoryLine();
        }

        HandleInput();

        //PlaceCrosshair();
    }

    #region UI Functions

    public void ballResetPosition()
    {
        Debug.Log("Intitial Position of ball: " +  ballInitialPosition);
        var rb = ballTransform.GetComponent<Rigidbody>();

        rb.isKinematic = true;
        ballTransform.position = ballInitialPosition;
        ballTransform.localRotation = Quaternion.identity;
    }

    #endregion
}
