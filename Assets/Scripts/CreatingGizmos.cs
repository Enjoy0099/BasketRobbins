using UnityEngine;

public class CreatingGizmos : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;      // Forward/backward
    public float strafeSpeed = 7f;     // Left/right
    public float mouseSensitivity = 2f; // Mouse rotation speed
    public bool invertY = false;        // Invert mouse Y for pitch

    [Header("Visuals")]
    public GameObject border; // Child object for selection border
    public GameObject arrows; // Parent object containing arrow indicators

    private bool isSelected = false;
    private float yaw;
    private float pitch;

    void Start()
    {
        if (border) border.SetActive(true);

        // Hide border and arrows initially
        if (border) border.SetActive(false);
        if (arrows) arrows.SetActive(false);

        // Initialize rotation
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void Update()
    {
        HandleSelection();
        HandleMovementAndRotation();
    }

    void HandleSelection()
    {
        // Select plane with left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                    isSelected = true;
                else
                    isSelected = false;
            }
            else
            {
                isSelected = false;
            }

            // Toggle visuals
            if (border) border.SetActive(isSelected);
            if (arrows) arrows.SetActive(isSelected);
        }
    }

    void HandleMovementAndRotation()
    {
        if (!isSelected) return; // Only allow control when selected

        // --- Keyboard Movement ---
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.I)) moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.K)) moveDirection -= transform.forward;
        if (Input.GetKey(KeyCode.J)) moveDirection -= transform.right;
        if (Input.GetKey(KeyCode.L)) moveDirection += transform.right;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // --- Mouse Rotation (right button) ---
        if (Input.GetMouseButton(1)) // 1 = right mouse button
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * mouseSensitivity;
            pitch += (invertY ? 1 : -1) * mouseY * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -80f, 80f);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}