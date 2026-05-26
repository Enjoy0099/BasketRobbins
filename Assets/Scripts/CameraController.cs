using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;
using RuntimeGizmos;
using UnityEngine.PlayerLoop;

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

    private bool isDraggingUI = false;
    private bool isDraggingGizmo = false;

    public TransformGizmo gizmoScript;


    private void Start()
    {
        originalPosition = followCam.transform.position;
        originalRotation = followCam.transform.rotation;


        followCam.m_XAxis.Value = 270f;
        originalXAxis = followCam.m_XAxis.Value;
        originalYAxis = followCam.m_YAxis.Value;
    }

    void OnEnable()
    {
        GameManager.OnSimulationStart_Action += SimulationStart;
        GameManager.OnSimulationStop_Action += SimulationStop;
        CinemachineCore.GetInputAxis += GetAxisCustom;
    }

    void OnDisable()
    {
        GameManager.OnSimulationStart_Action -= SimulationStart;
        GameManager.OnSimulationStop_Action -= SimulationStop;
        CinemachineCore.GetInputAxis -= GetAxisCustom;
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

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        /*foreach (var result in raycastResults)
            Debug.Log($"Hit: {result.gameObject.name} | Layer: {result.gameObject.layer}");*/

        // Only return true if hit is actually UI layer
        foreach (var result in raycastResults)
        {
            if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }

        return false;
    }

    public void SetGizmoDragging(bool dragging)
    {
        isDraggingGizmo = dragging;
    }

    float GetAxisCustom(string axisName)
    {

        bool overUI = IsPointerOverUI();
        bool mouseDown = Input.GetMouseButton(0);
        float axisValue = Input.GetAxis(axisName);

        //Debug.Log($"OverUI:{overUI} | MouseDown:{mouseDown} | Axis:{axisValue}");

        if (isDraggingUI ||  overUI) return 0f;
        if (mouseDown) return axisValue;
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
        if (gizmoScript != null)
            isDraggingGizmo = gizmoScript.isTransforming;

        // When mouse released — clear drag state
        if (Input.GetMouseButtonUp(0))
        {
            isDraggingUI = false;
            isDraggingGizmo = false;
        }

        // When mouse pressed — check if started on UI
        if (Input.GetMouseButtonDown(0))
            isDraggingUI = IsPointerOverUI();


        if (!canMove) return;

        if (Input.GetMouseButton(0) && !isDraggingGizmo && !isDraggingUI)
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

        Vector3 dir = idleCam.transform.forward * v 
                    + idleCam.transform.right * h 
                    + idleCam.transform.up * up;
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
