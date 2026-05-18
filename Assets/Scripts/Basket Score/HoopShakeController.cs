using System.Collections;
using UnityEngine;

public class HoopShakeController : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeStrength = 0.08f;  // max tilt angle in degrees
    public float shakeDuration = 0.6f;
    public float shakeDecay = 4.0f;   // how fast it settles back

    [Header("References")]
    public Transform rimTransform;   // drag your RimMesh here

    private Vector3 rimOriginalLocalPos;
    private Quaternion rimOriginalLocalRot;
    private Coroutine shakeCoroutine;

    void Start()
    {
        if (rimTransform == null)
            rimTransform = transform;

        rimOriginalLocalPos = rimTransform.localPosition;
        rimOriginalLocalRot = rimTransform.localRotation;
    }

    // Called by HoopRimTrigger with the world-space hit point
    public void TriggerShake(Vector3 hitPoint, float impactStrength)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeRoutine(hitPoint, impactStrength));
    }

    private IEnumerator ShakeRoutine(Vector3 hitPoint, float impactStrength)
    {
        // Direction from rim center to hit point (horizontal only)
        Vector3 rimCenter = rimTransform.position;
        Vector3 toHit = hitPoint - rimCenter;
        toHit.y = 0f;
        Vector3 hitDir = toHit.normalized;

        // Tilt axis is perpendicular to hit direction (so rim tips toward hit side)
        Vector3 tiltAxis = Vector3.Cross(hitDir, Vector3.up).normalized;

        float strength = shakeStrength * Mathf.Clamp01(impactStrength);
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shakeDuration;

            // Oscillating tilt that decays over time
            float angle = strength * Mathf.Sin(t * Mathf.PI * 6f) * (1f - t);

            rimTransform.localRotation = rimOriginalLocalRot *
                Quaternion.AngleAxis(angle, tiltAxis);

            // Slight vertical dip on hit side
            float dip = Mathf.Abs(angle) * 0.01f;
            Vector3 offset = hitDir * dip + Vector3.down * Mathf.Abs(dip);
            rimTransform.localPosition = rimOriginalLocalPos + offset;

            yield return null;
        }

        // Snap back cleanly
        rimTransform.localRotation = rimOriginalLocalRot;
        rimTransform.localPosition = rimOriginalLocalPos;
        shakeCoroutine = null;
    }
}