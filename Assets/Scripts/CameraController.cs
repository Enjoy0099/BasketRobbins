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
        followCam.Priority = 20;
        idleCam.Priority = 0;

        ResetCamera();
        followCam.enabled = true;
    }

    void SimulationStop()
    {
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


}
