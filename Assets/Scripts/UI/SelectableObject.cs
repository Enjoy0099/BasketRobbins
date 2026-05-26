using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    public Canvas targetCanvas;

    private void Awake()
    {
        Deselect();
    }

    public void Select()
    {
        ShowUI(true);
    }

    public void Deselect()
    {
        ShowUI(false);
    }


    private void ShowUI(bool show)
    {
        if (targetCanvas != null)
        {
            targetCanvas.enabled = show;
        }
    }
}
