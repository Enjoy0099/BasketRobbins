using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallSquishController : MonoBehaviour
{
    [Header("Squish Settings")]
    public float squishDuration = 0.06f;
    public float restoreDuration = 0.12f;
    public float maxSquishAmount = 0.12f;
    public float minImpactSpeed = 1.5f;
    public float maxImpactSpeed = 15.0f;

    [Header("Overshoot (jelly feel)")]
    public float overshootAmount = 0.02f;
    public float overshootDuration = 0.08f;

    private Rigidbody rb;
    private Vector3 originalScale;
    private Coroutine squishCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;
    }

    void OnCollisionEnter(Collision collision)
    {
        float speed = collision.relativeVelocity.magnitude;
        if (speed < minImpactSpeed) return;

        Vector3 worldNormal = collision.GetContact(0).normal.normalized;

        float strength = Mathf.InverseLerp(minImpactSpeed, maxImpactSpeed, speed);

        float squish = maxSquishAmount * strength;

        if (squishCoroutine != null)
            StopCoroutine(squishCoroutine);

        squishCoroutine = StartCoroutine(SquishRoutine(worldNormal, squish));
    }

    private IEnumerator SquishRoutine(Vector3 worldNormal, float squishAmount)
    {
        // Phase 1: Squish
        float elapsed = 0f;
        while (elapsed < squishDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / squishDuration);
            float ease = Mathf.Sin(t * Mathf.PI * 0.5f);
            ApplySquish(worldNormal, squishAmount * ease);
            yield return null;
        }

        // Phase 2: Restore with overshoot
        elapsed = 0f;
        while (elapsed < restoreDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / restoreDuration);
            float overshoot = overshootAmount * Mathf.Sin(t * Mathf.PI);
            float squishNow = Mathf.Lerp(squishAmount, 0f, t) - overshoot;
            ApplySquish(worldNormal, squishNow);
            yield return null;
        }

        // Phase 3: Settle
        elapsed = 0f;
        while (elapsed < overshootDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / overshootDuration);
            float settle = overshootAmount * (1f - t) * Mathf.Sin(t * Mathf.PI * 2f);
            ApplySquish(worldNormal, -settle);
            yield return null;
        }

        transform.localScale = originalScale;
        squishCoroutine = null;
    }

    private void ApplySquish(Vector3 worldNormal, float amount)
    {
        // Convert world normal into local space DIRECTION only (no scale involved)
        // Use rotation only — this is the key fix
        Vector3 localNormal = Quaternion.Inverse(transform.rotation) * worldNormal;
        localNormal = localNormal.normalized;

        float compress = 1f - Mathf.Clamp(amount, -1f, 1f);
        float expand = 1f + (Mathf.Abs(amount) * 0.5f);

        // Weighted blend across local axes using the normal direction
        // No axis snapping — smooth diagonal handling
        float wx = localNormal.x * localNormal.x; // squared = always positive weight
        float wy = localNormal.y * localNormal.y;
        float wz = localNormal.z * localNormal.z;
        // wx + wy + wz = 1 because localNormal is normalized

        // Each axis: full compress if normal aligns with it, full expand if perpendicular
        float sx = Mathf.Lerp(expand, compress, wx);
        float sy = Mathf.Lerp(expand, compress, wy);
        float sz = Mathf.Lerp(expand, compress, wz);

        // Multiply onto originalScale — NO division, NO lossyScale, no shrinking bug
        transform.localScale = new Vector3(
            originalScale.x * sx,
            originalScale.y * sy,
            originalScale.z * sz
        );
    }

    public void ResetSquish()
    {
        if (squishCoroutine != null)
        {
            StopCoroutine(squishCoroutine);
            squishCoroutine = null;
        }
        transform.localScale = originalScale;
    }
}