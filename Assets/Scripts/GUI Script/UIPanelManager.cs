using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelManager : MonoBehaviour
{
    public List<GameObject> panels;
    public GameObject initialPanel;
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject panel in panels)
        {
            if (panel != null) panel.SetActive(false);
        };

        // 초기 패널 설정
        if (initialPanel != null && panels.Contains(initialPanel))
        {
            ShowPanel(initialPanel);
        }
        else if (panels.Count > 0 && panels[0] != null) // initialPanel이 없으면 첫 번째 패널을 기본으로
        {
            ShowPanel(panels[0]); // 또는 ShowPanelAtIndex(initialPanelIndex);
        }
        else
        {
            Debug.LogWarning("UIPanelManager: No panels assigned or initial panel is invalid. Deactivating all.");
            DeactivateAllPanels(); // 모든 패널 비활성화
        }
    }
    public void ShowPanel(GameObject panelToShow)
    {
        if (panelToShow == null || !panels.Contains(panelToShow))
        {
            Debug.LogError("Panel to show is null or not managed by UIPanelManager.", panelToShow);
            return;
        }

        foreach (GameObject panel in panels)
        {
            if (panel != null)
            {
                panel.SetActive(panel == panelToShow);
            }
        }
    }

    public void DeactivateAllPanels()
    {
        foreach (GameObject panel in panels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}
