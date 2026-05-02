using UnityEngine;
using System.Collections.Generic;
using UI;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Main Panels")]
    public GameObject panelHomePage;
    public GameObject panelPilihMode;
    public GameObject panelTBA; 
    public GameObject panelTK;  
    public GameObject panelScanAR;
    public GameObject panelHowToPlay;
    public GameObject panelInformasiTim;
    public GameObject panelInfo;

    [FormerlySerializedAs("panelNarasi")] public NarationPanel NarationPanel; 

    [Header("Tahapan Controllers (TBA)")]
    public TahapanController[] tahapanButtonsTBA;

    [Header("Tahapan Controllers (TK)")]
    public TahapanController[] tahapanButtonsTK;

    [Header("AR Popup References")]
    public GameObject[] allARPopups;
    public Button btnARNext;
    public Button btnARPrev;

    public Button btnInfo;  
    public Button btnNextInteraction; 
    public Button btnCompleteTahapan; 

    [Header("Animation Controls")]
    public Button btnPlayAnimation;
    public Button btnStopAnimation;

    private Stack<GameObject> panelHistory = new Stack<GameObject>();
    private GameObject currentActivePanel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        ShowPanelAndAddToHistory(panelHomePage);
        if (btnInfo != null)
        {
            btnInfo.onClick.AddListener(() => ShowInfoPanel());
        }
        HideAllARPopups();
    }
    
    public void ForceHideInfoPanel()
    {
        if (panelInfo != null && panelInfo.activeSelf)
            panelInfo.SetActive(false);
    }

    public void ShowPanelAndAddToHistory(GameObject panelToShow)
    {
        ForceHideInfoPanel();
        if (currentActivePanel != null && currentActivePanel != panelToShow && currentActivePanel != panelHomePage &&
        currentActivePanel != panelInfo)
        {
            panelHistory.Push(currentActivePanel);
        }
        SetOnlyOnePanelActive(panelToShow);
        currentActivePanel = panelToShow;
    }

    // ToDo : Panel Info cukup satu saja, datanya yang unique
    public void ShowInfoPanel()
    {
        if (GameManager.Instance != null)
        {
            TahapanData data = GameManager.Instance.GetCurrentTahapanData(
                GameManager.Instance.currentAttemptingTahapIndex
            );

            if (data != null && data.panelInfo != null)
            {
                
                GameManager.Instance.ShowInfoPopup(data.panelInfo, null);
            }
        }
    }

    // ToDo : Panel Info cukup satu saja, datanya yang unique
    public void CloseInfoPanel()
    {
        if (GameManager.Instance != null)
        {
            TahapanData data = GameManager.Instance.GetCurrentTahapanData(
                GameManager.Instance.currentAttemptingTahapIndex
            );

            if (data != null && data.panelInfo != null)
            {
                GameManager.Instance.HideInfoPopup(data.panelInfo);
            }
        }
    }
    
    private void SetOnlyOnePanelActive(GameObject panelToShow)
    {
        if (panelHomePage != null) panelHomePage.SetActive(false);
        if (panelPilihMode != null) panelPilihMode.SetActive(false);
        if (panelTBA != null) panelTBA.SetActive(false);
        if (panelTK != null) panelTK.SetActive(false); 
        if (panelScanAR != null) panelScanAR.SetActive(false);
        if (panelHowToPlay != null) panelHowToPlay.SetActive(false);
        if (panelInformasiTim != null) panelInformasiTim.SetActive(false);
        if (panelInfo != null) panelInfo.SetActive(false);

        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
            Debug.Log("UIManager: Showing Panel -> " + panelToShow.name);
        }
        else
        {
            Debug.LogWarning("UIManager: Panel tujuan ShowPanelAndAddToHistory adalah null!");
        }
    }

    public void GoBack()
    {
        if (currentActivePanel == panelScanAR && GameManager.Instance != null)
        {
            GameManager.Instance.SetARCameraActive(false);
            HideAllARPopups();
        }

        ForceHideInfoPanel();

        if (panelHistory.Count > 0)
        {
            GameObject previousPanel = panelHistory.Pop();
            SetOnlyOnePanelActive(previousPanel);
            currentActivePanel = previousPanel;
        }
        else
        {
            SetOnlyOnePanelActive(panelHomePage);
            currentActivePanel = panelHomePage;
        }
    }
    
    public void ShowARPopup(GameObject popupToShow)
    {
        HideAllARPopups();
        if (popupToShow != null)
        {
            popupToShow.SetActive(true);
            if (panelScanAR != null) panelScanAR.SetActive(true);
            Debug.Log($"UIManager: Menampilkan Popup AR -> {popupToShow.name}");
        }
    }

    public void HideAllARPopups()
    {
        if (allARPopups != null)
        {
            foreach (var popup in allARPopups)
            {
                if (popup != null) popup.SetActive(false);
            }
        }
        if (btnARNext != null) btnARNext.onClick.RemoveAllListeners();
        if (btnARPrev != null) btnARPrev.onClick.RemoveAllListeners();
    }
    
    public void UpdateTahapButtonStates()
    {
        if (GameManager.Instance == null) return;

        int lastCompletedIndex = GameManager.Instance.GetLastCompletedTahapIndex();

        
        TahapanController[] buttonsToUpdate = (GameManager.Instance.currentMode == GameMode.TBA)
                                              ? tahapanButtonsTBA
                                              : tahapanButtonsTK;

        if (buttonsToUpdate == null) return;

        for (int i = 0; i < buttonsToUpdate.Length; i++)
        {
            TahapanController btn = buttonsToUpdate[i];
            if (btn == null) continue;

            bool isInteractable = (i <= lastCompletedIndex + 1);
            float alpha = isInteractable ? 1f : 0.75f;

            btn.UpdateVisualState(isInteractable, alpha);
        }
        Debug.Log($"UIManager: Status tombol tahapan untuk mode {GameManager.Instance.currentMode} diperbarui.");
    }
    
    public void SetButtonNavigationVisibility(bool enabled)
    { 
        if(btnARNext) btnARNext.gameObject.SetActive(enabled);
        if(btnARPrev) btnARPrev.gameObject.SetActive(enabled);
    }
    
    public void SetButtonAnimationVisibility(bool enabled)
    { 
        btnPlayAnimation.gameObject.SetActive(enabled);
        btnStopAnimation.gameObject.SetActive(enabled);
    }
    
    public bool IsPanelInfoActive()
    {
        return panelInfo.activeInHierarchy;
    }
}