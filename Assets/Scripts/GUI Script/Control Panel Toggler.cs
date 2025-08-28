using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPanelToggler : MonoBehaviour
{
    public CanvasGroup targetCanvasGroup;
    private bool isPanelVisible = false;
    // Start is called before the first frame update
    void Start()
    {
        SetPanelVisibility(false);
    }
    public void TogglePanelVisibility()
    {
        isPanelVisible = !isPanelVisible;
        SetPanelVisibility(isPanelVisible);
    }

    private void SetPanelVisibility(bool isVisible)
    {
        if(targetCanvasGroup == null)
        {
            Debug.LogError("Target Canvas Group missing!");
            return;
        }

        if(isVisible)
        {
            targetCanvasGroup.alpha = 1;
            targetCanvasGroup.interactable = true;
            targetCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            targetCanvasGroup.alpha = 0;
            targetCanvasGroup.interactable = false;
            targetCanvasGroup.blocksRaycasts = false;
        }
    }
}
