using UnityEngine;

public class ObjectUIBinding : MonoBehaviour
{
    // Drag the target 3D object this UI belongs to
    public Transform boundObject;

    public bool IsBoundTo(Transform target)
    {
        return boundObject != null && boundObject == target;
    }

    public Transform GetBoundObject()
    {
        return boundObject;
    }
}