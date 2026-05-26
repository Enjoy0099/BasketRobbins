// Attach to any Canvas that spawns/enables at runtime
using UnityEngine;

public class AutoPatchOnEnable : MonoBehaviour
{
    void OnEnable()
    {
        // Singleton access — O(1), no scene search
        UIFocusFixer.Instance?.PatchAll();
    }
}