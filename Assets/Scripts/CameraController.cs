using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineFreeLook followCam;
    public CinemachineVirtualCamera idleCam;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float originalXAxis;
    private float originalYAxis;

    //--------------------------

    public float moveSpeed = 10f;
    public float lookSpeed = 2f;

    private bool canMove = false;

    //--------------------------

    private void Start()
    {
        originalPosition = followCam.transform.position;
        originalRotation = followCam.transform.rotation;

        originalXAxis = followCam.m_XAxis.Value;
        originalYAxis = followCam.m_YAxis.Value;
    }

    void OnEnable()
    {
        GameManager.OnSimulationStart += SimulationStart;
        GameManager.OnSimulationStop += SimulationStop;

        CinemachineCore.GetInputAxis = GetAxisCustom;
    }

    void OnDisable()
    {
        GameManager.OnSimulationStart -= SimulationStart;
        GameManager.OnSimulationStop -= SimulationStop;

        CinemachineCore.GetInputAxis = null;
    }

    void SimulationStart()
    {
        canMove = false;

        followCam.Priority = 20;
        idleCam.Priority = 0;

        ResetCamera();
        followCam.enabled = true;
    }

    void SimulationStop()
    {
        canMove = true;

        followCam.Priority = 0;
        idleCam.Priority = 20;

        followCam.enabled = false;
        ResetCamera();
    }

    float GetAxisCustom(string axisName)
    {
        if (Input.GetMouseButton(0))
        {
            return Input.GetAxis(axisName);
        }

        return 0f;
    }

    void ResetCamera()
    {
        // Reset axis (MOST IMPORTANT)
        followCam.m_XAxis.Value = originalXAxis;
        followCam.m_YAxis.Value = originalYAxis;

        // Reset transform (extra safety)
        followCam.transform.position = originalPosition;
        followCam.transform.rotation = originalRotation;
    }

    //------------------------------------------------------------------------------------------------

    void Update()
    {
        if (!canMove) return;

        if (Input.GetMouseButton(0))
        {
            Move();
            Look();
        }
            
    }

    void Move()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S
        float up = 0;

        if (Input.GetKey(KeyCode.E)) up = 1;
        if (Input.GetKey(KeyCode.Q)) up = -1;

        Vector3 dir = idleCam.transform.forward * v + idleCam.transform.right * h + idleCam.transform.up * up;
        idleCam.transform.position += dir * moveSpeed * Time.deltaTime;
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        idleCam.transform.Rotate(Vector3.up * mouseX, Space.World);
        idleCam.transform.Rotate(Vector3.left * mouseY);
    }

    //------------------------------------------------------------------------------------------------


}
