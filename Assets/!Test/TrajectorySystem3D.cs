using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3D Trajectory System
/// Replaces flat LineRenderer with 3D dot markers along the trajectory path.
/// Supports spheres, cylinders, cubes, and custom meshes.
/// Optimized for mobile via object pooling.
/// 
/// HOW TO USE:
///   1. Attach this script to any GameObject (e.g., "TrajectoryManager")
///   2. Assign a Material in the Inspector (or leave null for default)
///   3. Call ShowTrajectory(points) with your trajectory Vector3 array
///   4. Call HideTrajectory() to hide it
/// </summary>
public class TrajectorySystem3D : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // INSPECTOR SETTINGS
    // ─────────────────────────────────────────────

    [Header("Dot Shape")]
    [Tooltip("Choose the shape of each dot along the trajectory.")]
    public DotShape dotShape = DotShape.Sphere;

    [Tooltip("Custom mesh to use when DotShape is set to CustomMesh.")]
    public Mesh customMesh;

    [Header("Dot Size")]
    [Tooltip("Scale of the dot at the START (launch point). Bigger = more visible.")]
    public float startScale = 0.18f;

    [Tooltip("Scale of the dot at the END (landing point). Smaller = depth effect.")]
    public float endScale = 0.06f;

    [Tooltip("Uniform multiplier applied on top of start/end scale. Good for quick tweaks.")]
    public float globalScaleMultiplier = 1.0f;

    [Header("Dot Density")]
    [Tooltip("Number of dots to place between each pair of trajectory points. " +
             "Higher = smoother curve. Recommended: 1-3 for mobile, up to 5 for PC.")]
    [Range(1, 10)]
    public int dotsPerSegment = 2;

    [Tooltip("Maximum total dots rendered at once. Hard cap for mobile safety.")]
    [Range(5, 100)]
    public int maxDots = 40;

    [Header("Dot Rotation")]
    [Tooltip("Rotate each dot so its local axis points along the trajectory direction.\n" +
             "Turn ON for Capsule, Cylinder, Arrow meshes.\n" +
             "Leave OFF for Sphere, Cube (rotation doesn't matter for those).")]
    public bool alignToDirection = true;

    [Tooltip("Unity's Capsule/Cylinder stand upright on their Y-axis by default.\n" +
             "This offset rotates the mesh BEFORE aligning to the path.\n" +
             "Capsule/Cylinder: use (90, 0, 0).  Custom mesh: adjust as needed.")]
    public Vector3 rotationOffset = new Vector3(90f, 0f, 0f);

    [Header("Dot Appearance")]
    [Tooltip("Material applied to all dots. Assign a simple Unlit or Mobile/Diffuse material for best mobile perf.")]
    public Material dotMaterial;

    [Tooltip("Color at the start of the trajectory.")]
    public Color startColor = new Color(1f, 0.9f, 0.2f, 1f); // warm yellow

    [Tooltip("Color at the end of the trajectory.")]
    public Color endColor = new Color(1f, 0.3f, 0.1f, 0.4f); // faded red

    [Tooltip("If true, each dot's color gradually changes from startColor to endColor.")]
    public bool useColorGradient = true;

    [Header("Animation")]
    [Tooltip("If true, dots pulse in/out gently for a lively feel.")]
    public bool animatePulse = false;

    [Tooltip("Speed of the pulse animation.")]
    public float pulseSpeed = 3f;

    [Tooltip("How much the scale changes during pulse (0 = no pulse, 0.3 = 30% scale change).")]
    [Range(0f, 0.5f)]
    public float pulseAmount = 0.15f;

    [Header("Mobile Optimisation")]
    [Tooltip("Pre-warms the object pool on Start so no allocations happen during gameplay.")]
    public bool prewarmPool = true;

    [Tooltip("Shadow casting mode. Set to Off for best mobile performance.")]
    public UnityEngine.Rendering.ShadowCastingMode shadowCasting =
        UnityEngine.Rendering.ShadowCastingMode.Off;

    [Tooltip("If true, dots receive lighting. Turn off for Unlit shaders.")]
    public bool receiveLighting = false;

    // ─────────────────────────────────────────────
    // ENUMS
    // ─────────────────────────────────────────────

    public enum DotShape
    {
        Sphere,
        Cube,
        Cylinder,
        Quad,           // flat billboard — cheapest on mobile
        CustomMesh
    }

    // ─────────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────────

    private List<GameObject> _pool = new List<GameObject>();   // object pool
    private List<GameObject> _active = new List<GameObject>(); // currently visible dots
    private GameObject _poolRoot;                               // parent for pooled objects
    private Mesh _resolvedMesh;                                // mesh used for dots

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    private void Start()
    {
        // Create a hidden parent so pooled objects don't clutter the hierarchy root
        _poolRoot = new GameObject("TrajectoryDotPool");
        _poolRoot.transform.SetParent(transform);

        _resolvedMesh = ResolveMesh();

        if (prewarmPool)
            PrewarmPool(maxDots);
    }

    private void Update()
    {
        if (!animatePulse || _active.Count == 0) return;

        float pulseFactor = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

        for (int i = 0; i < _active.Count; i++)
        {
            if (_active[i] == null) continue;
            // Keep original base scale and apply pulse on top
            Vector3 baseScale = _active[i].transform.localScale;
            // We stored base scale in the dot's name tag; simpler: re-derive from lerp
            float t = _active.Count > 1 ? (float)i / (_active.Count - 1) : 0f;
            float baseSize = Mathf.Lerp(startScale, endScale, t) * globalScaleMultiplier;
            float pulsedSize = baseSize * pulseFactor;
            _active[i].transform.localScale = Vector3.one * pulsedSize;
        }
    }

    // ─────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────

    /// <summary>
    /// Show the 3D trajectory along the given world-space points.
    /// Call this every frame (or whenever points change) while aiming.
    /// </summary>
    /// <param name="trajectoryPoints">
    /// Array of Vector3 positions — the same array you'd pass to LineRenderer.SetPositions().
    /// </param>
    public void ShowTrajectory(Vector3[] trajectoryPoints)
    {
        if (trajectoryPoints == null || trajectoryPoints.Length < 2)
        {
            HideTrajectory();
            return;
        }

        // Build the full list of world positions to place dots at
        List<Vector3> dotPositions = BuildDotPositions(trajectoryPoints);

        // Clamp to max
        int count = Mathf.Min(dotPositions.Count, maxDots);

        // Deactivate excess dots from previous frame
        ReturnExcessToPool(count);

        // Place / activate dots
        for (int i = 0; i < count; i++)
        {
            float t = count > 1 ? (float)i / (count - 1) : 0f;

            GameObject dot = GetOrCreateDot(i);
            dot.transform.position = dotPositions[i];

            // Scale: big at start, small at end
            float size = Mathf.Lerp(startScale, endScale, t) * globalScaleMultiplier;
            dot.transform.localScale = Vector3.one * size;

            if (alignToDirection)
            {
                // Use vector to next dot; for the last dot use vector FROM previous
                Vector3 dir = i < count - 1
                    ? (dotPositions[i + 1] - dotPositions[i])
                    : (dotPositions[i] - dotPositions[i - 1]);

                if (dir.sqrMagnitude > 0.0001f)
                {
                    // LookRotation points Z-axis along dir.
                    // rotationOffset corrects for the mesh's natural axis:
                    //   Capsule/Cylinder stand on Y  → offset (90, 0, 0)
                    //   Sphere/Cube                  → offset doesn't matter
                    //   Custom mesh                  → adjust until it looks right
                    dot.transform.rotation =
                        Quaternion.LookRotation(dir.normalized) *
                        Quaternion.Euler(rotationOffset);
                }
            }

            // Color gradient
            if (useColorGradient)
            {
                Color c = Color.Lerp(startColor, endColor, t);
                SetDotColor(dot, c);
            }

            dot.SetActive(true);

            if (!_active.Contains(dot))
                _active.Add(dot);
        }
    }

    /// <summary>
    /// Show trajectory from a LineRenderer directly.
    /// Reads the LineRenderer's existing positions — no duplication needed.
    /// </summary>
    public void ShowTrajectoryFromLineRenderer(LineRenderer lr)
    {
        if (lr == null) return;
        int count = lr.positionCount;
        Vector3[] pts = new Vector3[count];
        lr.GetPositions(pts);
        ShowTrajectory(pts);
    }

    /// <summary>
    /// Hide all trajectory dots immediately.
    /// </summary>
    public void HideTrajectory()
    {
        ReturnExcessToPool(0);
        _active.Clear();
    }

    /// <summary>
    /// Change dot shape at runtime and rebuild the pool mesh.
    /// </summary>
    public void SetDotShape(DotShape shape)
    {
        dotShape = shape;
        _resolvedMesh = ResolveMesh();
        // Update all pooled objects' meshes
        foreach (var go in _pool)
        {
            if (go == null) continue;
            var mf = go.GetComponent<MeshFilter>();
            if (mf) mf.sharedMesh = _resolvedMesh;
        }
    }

    // ─────────────────────────────────────────────
    // INTERNAL HELPERS
    // ─────────────────────────────────────────────

    /// <summary>
    /// Inserts extra interpolated positions between each pair of input points
    /// based on dotsPerSegment, giving a denser dot trail on curved paths.
    /// </summary>
    private List<Vector3> BuildDotPositions(Vector3[] pts)
    {
        var result = new List<Vector3>();

        for (int i = 0; i < pts.Length - 1; i++)
        {
            result.Add(pts[i]);

            for (int s = 1; s < dotsPerSegment; s++)
            {
                float frac = (float)s / dotsPerSegment;
                result.Add(Vector3.Lerp(pts[i], pts[i + 1], frac));
            }
        }

        result.Add(pts[pts.Length - 1]); // always include last point
        return result;
    }

    /// <summary>
    /// Returns a dot from the pool or creates a new one if the pool is empty.
    /// </summary>
    private GameObject GetOrCreateDot(int index)
    {
        // If we already have an active dot at this index, reuse it
        if (index < _active.Count)
            return _active[index];

        // Otherwise grab from pool
        if (_pool.Count > 0)
        {
            var pooled = _pool[_pool.Count - 1];
            _pool.RemoveAt(_pool.Count - 1);
            return pooled;
        }

        // Nothing in pool — create fresh
        return CreateDotObject();
    }

    /// <summary>
    /// Deactivates dots beyond the needed count and moves them back to the pool.
    /// </summary>
    private void ReturnExcessToPool(int keepCount)
    {
        for (int i = keepCount; i < _active.Count; i++)
        {
            if (_active[i] == null) continue;
            _active[i].SetActive(false);
            _pool.Add(_active[i]);
        }

        if (keepCount < _active.Count)
            _active.RemoveRange(keepCount, _active.Count - keepCount);
    }

    /// <summary>
    /// Allocates pool objects ahead of time to avoid GC spikes during gameplay.
    /// </summary>
    private void PrewarmPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var dot = CreateDotObject();
            dot.SetActive(false);
            _pool.Add(dot);
        }
    }

    /// <summary>
    /// Creates a single dot GameObject with MeshFilter + MeshRenderer.
    /// </summary>
    private GameObject CreateDotObject()
    {
        var go = new GameObject("TrajDot");
        go.transform.SetParent(_poolRoot.transform);

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = _resolvedMesh;

        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = dotMaterial != null
            ? dotMaterial
            : CreateDefaultMaterial();
        mr.shadowCastingMode = shadowCasting;
        mr.receiveShadows = receiveLighting;

        // Initial color
        SetDotColor(go, startColor);

        return go;
    }

    /// <summary>
    /// Sets the color on a dot's MaterialPropertyBlock (no material instancing = faster).
    /// </summary>
    private void SetDotColor(GameObject dot, Color color)
    {
        var mr = dot.GetComponent<MeshRenderer>();
        if (mr == null) return;

        var block = new MaterialPropertyBlock();
        mr.GetPropertyBlock(block);
        block.SetColor("_Color", color);
        mr.SetPropertyBlock(block);
    }

    /// <summary>
    /// Returns the Unity primitive mesh matching the chosen DotShape.
    /// </summary>
    private Mesh ResolveMesh()
    {
        if (dotShape == DotShape.CustomMesh && customMesh != null)
            return customMesh;

        PrimitiveType primitive = dotShape switch
        {
            DotShape.Sphere => PrimitiveType.Sphere,
            DotShape.Cube => PrimitiveType.Cube,
            DotShape.Cylinder => PrimitiveType.Cylinder,
            DotShape.Quad => PrimitiveType.Quad,
            _ => PrimitiveType.Sphere,
        };

        // Grab the shared mesh from a temp primitive (never shown in scene)
        var temp = GameObject.CreatePrimitive(primitive);
        var mesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Destroy(temp);
        return mesh;
    }

    /// <summary>
    /// Fallback material if none is assigned — uses the simplest built-in shader.
    /// </summary>
    private Material CreateDefaultMaterial()
    {
        // Try mobile-friendly unlit first, fall back to Standard
        var mat = new Material(Shader.Find("Mobile/Unlit (Supports Lightmap)")
                   ?? Shader.Find("Unlit/Color")
                   ?? Shader.Find("Standard"));
        mat.color = startColor;
        return mat;
    }

    // ─────────────────────────────────────────────
    // CLEANUP
    // ─────────────────────────────────────────────

    private void OnDestroy()
    {
        if (_poolRoot != null)
            Destroy(_poolRoot);
    }
}