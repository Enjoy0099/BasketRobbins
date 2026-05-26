using UnityEngine;

/// <summary>
/// Example: How to use TrajectorySystem3D with your existing projectile launcher.
/// 
/// SETUP:
///   1. Add TrajectorySystem3D component to your launcher GameObject
///   2. Attach this script to the same GameObject
///   3. Assign your existing LineRenderer to the 'lineRenderer' field in Inspector
///      (or leave null to use the physics-based trajectory calculation below)
///   4. Adjust Launch Force, Gravity Scale, etc. to match your game
/// </summary>
public class TrajectoryExample : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Your existing LineRenderer (optional). If assigned, 3D dots mirror its points.")]
    public LineRenderer lineRenderer;

    [Tooltip("The 3D trajectory system (assign the TrajectorySystem3D component here).")]
    public TrajectorySystem3D trajectorySystem;

    [Header("Projectile Settings")]
    public float launchForce = 15f;
    public float gravityScale = -9.81f;

    [Header("Trajectory Resolution")]
    [Tooltip("How many physics steps to simulate for the preview.")]
    [Range(10, 80)]
    public int simulationSteps = 30;

    [Tooltip("Time step per simulation step (smaller = more accurate but more points).")]
    public float timeStep = 0.1f;

    private bool _isAiming = false;

    // ─────────────────────────────────────────────
    // OPTION A: You already have a LineRenderer
    // Just pass its points to the 3D system each frame
    // ─────────────────────────────────────────────

    private void Update()
    {
        //_isAiming = true;
        if (Input.GetMouseButtonDown(0))
            _isAiming = true;

        if (Input.GetMouseButtonUp(0))
        {
            _isAiming = false;
            trajectorySystem.HideTrajectory();
            return;
        }

        if (!_isAiming) return;

        // --- OPTION A: Mirror from existing LineRenderer ---
        if (lineRenderer != null && lineRenderer.positionCount > 1)
        {
            trajectorySystem.ShowTrajectoryFromLineRenderer(lineRenderer);
            return;
        }

        // --- OPTION B: Calculate trajectory from scratch ---
        Vector3 launchVelocity = transform.forward * launchForce;
        Vector3[] points = CalculateTrajectoryPoints(transform.position, launchVelocity);
        trajectorySystem.ShowTrajectory(points);
    }

    // ─────────────────────────────────────────────
    // OPTION B: Calculate trajectory points yourself
    // Use this if you don't have a LineRenderer yet
    // ─────────────────────────────────────────────

    private Vector3[] CalculateTrajectoryPoints(Vector3 startPos, Vector3 startVelocity)
    {
        Vector3[] points = new Vector3[simulationSteps];
        Vector3 pos = startPos;
        Vector3 vel = startVelocity;
        Vector3 gravity = Vector3.up * gravityScale;

        for (int i = 0; i < simulationSteps; i++)
        {
            points[i] = pos;

            // Simple Euler integration (same as what Unity's physics uses internally)
            vel += gravity * timeStep;
            pos += vel * timeStep;

            // Optional: stop early if we hit something
            if (Physics.Linecast(points[i], pos, out RaycastHit hit))
            {
                // Trim array to hit point for accuracy
                System.Array.Resize(ref points, i + 1);
                points[i] = hit.point;
                break;
            }
        }

        return points;
    }

    // ─────────────────────────────────────────────
    // OPTION C: Call from outside (e.g., your gun script)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Call this from your weapon/launcher script:
    ///   trajectoryExample.UpdateTrajectory(muzzlePosition, muzzleVelocity);
    /// </summary>
    public void UpdateTrajectory(Vector3 origin, Vector3 velocity)
    {
        Vector3[] points = CalculateTrajectoryPoints(origin, velocity);
        trajectorySystem.ShowTrajectory(points);
    }

    /// <summary>
    /// Call this when the player fires or stops aiming.
    /// </summary>
    public void ClearTrajectory()
    {
        trajectorySystem.HideTrajectory();
    }
}