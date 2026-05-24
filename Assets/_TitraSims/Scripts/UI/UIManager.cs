using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Serializable]
    public struct PanelMapping
    {
        public PanelType Type;
        public GameObject Panel;
    }

    [Header("Main Panels")]
    public PanelMapping[] Panels;
    private Dictionary<PanelType, GameObject> PanelsDict = new();
    
    [Header("Tahapan Controllers Button")] 
    private List<TahapanControllerButton> tahapanButtons = new();
    
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

    // Cache
    private Stack<PanelType> panelHistory = new();
    private PanelType currentActivePanelType;
    
    // Getters
    public PanelType CurrentActivePanelType => currentActivePanelType;
    public event Action<PanelType> OnPanelTypeChanged;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        foreach (var panelMapping in Panels)
        {
            PanelsDict[panelMapping.Type] = panelMapping.Panel;
        }
        tahapanButtons.AddRange(FindObjectsByType<TahapanControllerButton>());
        
        ShowPanelAndAddToHistory(PanelType.PanelHomePage);
        if (btnInfo != null) btnInfo.onClick.AddListener(() => ShowInfoPanel());
        HideAllARPopups();
    }

    public void ShowPanelAndAddToHistory(PanelType panelType)
    {
        ForceHideInfoPanel();
        if (currentActivePanelType == panelType) return;
        if (currentActivePanelType != PanelType.PanelHomePage && currentActivePanelType != PanelType.PanelInfo) panelHistory.Push(panelType);

        currentActivePanelType = panelType;
        OnPanelTypeChanged?.Invoke(panelType);
        SetOnlyOnePanelActive(panelType);
    }

    public GameObject GetPanelByType(PanelType targetType)
    {
        return PanelsDict[targetType];
    }
    
    private void SetOnlyOnePanelActive(PanelType panelToShow)
    {
        foreach (var kvp in PanelsDict)
        {
            kvp.Value?.SetActive(false);
        }
        GetPanelByType(panelToShow)?.SetActive(true);
    }

    public void GoBack()
    {
        if (currentActivePanelType == PanelType.PanelScanAR && GameManager.Instance != null)
        {
            HideAllARPopups();
        }

        ForceHideInfoPanel();

        if (panelHistory.Count > 0)
        {
            var previousPanel = panelHistory.Pop();
            SetOnlyOnePanelActive(previousPanel);
            currentActivePanelType = previousPanel;
        }
        else
        {
            SetOnlyOnePanelActive(PanelType.PanelHomePage);
            currentActivePanelType = PanelType.PanelHomePage;
        }
        OnPanelTypeChanged?.Invoke(currentActivePanelType);
    }

    // public void ShowARPopup(GameObject popupToShow)
    // {
    //     HideAllARPopups();
    //     if (popupToShow != null)
    //     {
    //         popupToShow.SetActive(true);
    //         if (panelScanAR != null) panelScanAR.SetActive(true);
    //         Debug.Log($"UIManager: Menampilkan Popup AR -> {popupToShow.name}");
    //     }
    // }

    public void HideAllARPopups() // ToDo
    {
        if (allARPopups != null)
            foreach (var popup in allARPopups)
            {
                popup?.SetActive(false);
            }

        if (btnARNext != null) btnARNext.onClick.RemoveAllListeners();
        if (btnARPrev != null) btnARPrev.onClick.RemoveAllListeners();
    }

    public void UpdateTahapButtonStates(int lastCompletedIndex, GameMode currentMode)
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        foreach (TahapanControllerButton tahapanControllerButton in tahapanButtons)
        {
            if (tahapanControllerButton.ButtonGameMode != currentMode)
            {
                continue;
            }
            var isInteractable = tahapanControllerButton.TahapIndex <= lastCompletedIndex + 1;
            tahapanControllerButton.UpdateVisualState(isInteractable);
        }
    }

    public void SetButtonNavigationVisibility(bool enabled)
    {
        if (btnARNext) btnARNext.gameObject.SetActive(enabled);
        if (btnARPrev) btnARPrev.gameObject.SetActive(enabled);
    }

    public void SetButtonAnimationVisibility(bool enabled)
    {
        btnPlayAnimation.gameObject.SetActive(enabled);
        btnStopAnimation.gameObject.SetActive(enabled);
    }

    #region Info Panel

    public bool IsPanelInfoActive()
    {
        return GetPanelByType(PanelType.PanelInfo).activeInHierarchy;
    }

    public void ForceHideInfoPanel()
    {
        GetPanelByType(PanelType.PanelInfo)?.SetActive(false);
    }
    
    // ToDo : Panel Info cukup satu saja, datanya yang unique
    public void ShowInfoPanel()
    {
        if (GameManager.Instance != null)
        {
            var data = GameManager.Instance.GetCurrentTahapanData(
                GameManager.Instance.currentAttemptingTahapIndex
            );

            if (data != null && data.panelInfo != null) GameManager.Instance.ShowInfoPopup(data.panelInfo, null);
        }
    }

    // // ToDo : Panel Info cukup satu saja, datanya yang unique
    // public void CloseInfoPanel()
    // {
    //     if (GameManager.Instance != null)
    //     {
    //         var data = GameManager.Instance.GetCurrentTahapanData(
    //             GameManager.Instance.currentAttemptingTahapIndex
    //         );
    //
    //         if (data != null && data.panelInfo != null) GameManager.Instance.HideInfoPopup(data.panelInfo);
    //     }
    // }
    #endregion
}