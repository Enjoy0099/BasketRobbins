using UnityEngine;

public class BallController : MonoBehaviour
{
    public float throwForce = 10f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ThrowBall()
    {
        // Only allow in simulation mode
        if (!GameManager.instance.IsSimulating()) return;

        // Prevent re-throw
        if (rb.velocity != Vector3.zero) return;

        Vector3 dir = Camera.main.transform.forward + Vector3.up * 0.3f;
        rb.AddForce(dir * throwForce, ForceMode.Impulse);
    }
}