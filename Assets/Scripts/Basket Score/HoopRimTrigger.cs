using BasketRobbins;
using UnityEngine;

public class HoopRimTrigger : MonoBehaviour
{
    [Header("References")]
    public HoopShakeController shakeController;

    [Header("Impact Settings")]
    public float minImpactVelocity = 1.0f;  // ignore tiny touches
    public float maxImpactVelocity = 15.0f; // clamp for strength calc
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag(GameConstants.Tag.Ball)) return;

        BallController ball = collision.collider.GetComponent<BallController>();
        if (ball != null)
            ball.touchedRim = true;

        // Calculate impact strength from velocity
        float speed = collision.relativeVelocity.magnitude;
        if (speed < minImpactVelocity) return;

        float strength = Mathf.InverseLerp(minImpactVelocity, maxImpactVelocity, speed);

        // Use actual contact point for directional shake
        Vector3 hitPoint = collision.GetContact(0).point;

        if (shakeController != null)
            shakeController.TriggerShake(hitPoint, strength);
    }
}
