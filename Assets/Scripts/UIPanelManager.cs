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

        // �ʱ� �г� ����
        if (initialPanel != null && panels.Contains(initialPanel))
        {
            ShowPanel(initialPanel);
        }
        else if (panels.Count > 0 && panels[0] != null) // initialPanel�� ������ ù ��° �г��� �⺻����
        {
            ShowPanel(panels[0]); // �Ǵ� ShowPanelAtIndex(initialPanelIndex);
        }
        else
        {
            Debug.LogWarning("UIPanelManager: No panels assigned or initial panel is invalid. Deactivating all.");
            DeactivateAllPanels(); // ��� �г� ��Ȱ��ȭ
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
