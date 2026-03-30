using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 2f;

    private bool canMove = false;
    private bool isInUIMode = false;

    void OnEnable()
    {
        GameManager.OnSimulationStart += EnableMovement;
        GameManager.OnSimulationStop += DisableMovement;
    }

    void OnDisable()
    {
        GameManager.OnSimulationStart -= EnableMovement;
        GameManager.OnSimulationStop -= DisableMovement;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleUIMode();
        }

        if (!canMove || isInUIMode) return;

        Move();
        Look();
    }

    void Move()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S
        float up = 0;

        if (Input.GetKey(KeyCode.E)) up = 1;
        if (Input.GetKey(KeyCode.Q)) up = -1;

        Vector3 dir = transform.forward * v + transform.right * h + transform.up * up;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        transform.Rotate(Vector3.up * mouseX, Space.World);
        transform.Rotate(Vector3.left * mouseY);
    }

    void ToggleUIMode()
    {
        isInUIMode = !isInUIMode;

        if (isInUIMode)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void EnableMovement()
    {
        canMove = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void DisableMovement()
    {
        canMove = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}