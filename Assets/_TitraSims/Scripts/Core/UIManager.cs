using System;
using System.Collections.Generic;
using Config;
using TMPro;
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
    
    [Header("Panel Scanning AR")] 
    public GameObject[] allARPopups;
    public Button btnARNext;
    public Button btnARPrev;
    
    public Button btnNextInteraction;
    public Button btnCompleteTahapan;

    [Header("Animation Controls")]
    [SerializeField] private Button btnPlayAnimation;
    [SerializeField] private Button btnStopAnimation;

    [Header("Info Style Config")]
    [SerializeField] private InfoStyleConfig styleConfigDefault;

    // Cache
    private Stack<PanelType> panelHistory = new();
    private PanelType currentActivePanelType;
    
    // Getters
    public PanelType CurrentActivePanelType => currentActivePanelType;
    public event Action<PanelType> OnPanelTypeChanged;
    public Button PlayAnimationButton => btnPlayAnimation;
    public Button StopAnimationButton => btnStopAnimation;
    
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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnModeSet       += HandleModeSet;
            GameManager.Instance.OnTahapStarted  += HandleTahapStarted;
            GameManager.Instance.OnTahapCompleted += HandleTahapCompleted;
            GameManager.Instance.OnProgressReset  += HandleProgressReset;
        }

        ShowPanelAndAddToHistory(PanelType.PanelHomePage);
      
        HideAllARPopups();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnModeSet        -= HandleModeSet;
        GameManager.Instance.OnTahapStarted   -= HandleTahapStarted;
        GameManager.Instance.OnTahapCompleted -= HandleTahapCompleted;
        GameManager.Instance.OnProgressReset  -= HandleProgressReset;
    }

    public void ShowPanelAndAddToHistory(PanelType panelType, bool hidePanelUnderStack = true)
    {
        ForceHideInfoPanel();
        if (currentActivePanelType == panelType && currentActivePanelType != panelType) return;
        if (currentActivePanelType != PanelType.PanelHomePage && currentActivePanelType != PanelType.PanelInfo) panelHistory.Push(currentActivePanelType);

        currentActivePanelType = panelType;
        OnPanelTypeChanged?.Invoke(panelType);

        if (hidePanelUnderStack) SetOnlyOnePanelActive(panelType);
        else SetPanelToActive(panelType);
    }

    public GameObject GetPanelByType(PanelType targetType)
    {
        PanelsDict.TryGetValue(targetType, out var panel);
        return panel;
    }
    
    private void SetOnlyOnePanelActive(PanelType panelToShow)
    {
        foreach (var kvp in PanelsDict)
        {
            if (kvp.Value != null) kvp.Value.SetActive(false);
        }
        var panel = GetPanelByType(panelToShow);
        if (panel != null) panel.SetActive(true);
    }

    private void SetPanelToActive(PanelType panelToShow)
    {
        var panel = GetPanelByType(panelToShow);
        if (panel != null) panel.SetActive(true);
    }

    public void GoBack()
    {
        if (currentActivePanelType == PanelType.PanelScanAR)
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

    private void HandleModeSet(GameMode mode, int lastCompleted)
    {
        ForceHideInfoPanel();
        PanelType panelToOpen = mode == GameMode.TBA ? PanelType.PanelTBA : PanelType.PanelTK;
        ShowPanelAndAddToHistory(panelToOpen);
    }

    private void HandleTahapStarted()
    {
        ForceHideInfoPanel();
        ShowPanelAndAddToHistory(PanelType.PanelScanAR);
    }

    private void HandleTahapCompleted(GameMode mode, int lastCompleted)
    {
        ForceHideInfoPanel();
        GoBack();
    }

    private void HandleProgressReset()
    {
        ForceHideInfoPanel();
    }

    public void SetButtonNavigationVisibility(bool enabled)
    {
        if (btnARNext) btnARNext.gameObject.SetActive(enabled);
        if (btnARPrev) btnARPrev.gameObject.SetActive(enabled);
    }

    public void SetButtonAnimationVisibility(bool isVisible)
    {
        if (btnPlayAnimation) btnPlayAnimation.gameObject.SetActive(isVisible);
        if (btnStopAnimation) btnStopAnimation.gameObject.SetActive(isVisible);
    }

    #region Info Panel

    public bool IsPanelInfoActive()
    {
        return GetPanelByType(PanelType.PanelInfo).activeSelf;
    }

    public void ToggleInfoPanel()
    {
        if (IsPanelInfoActive())
        {
            ForceHideInfoPanel();
        }
        else
        {
            ForceHideNarationPanel();
            ShowInfoPanel();
        }
        // Todo use observer pattern at least
        SetButtonNarationVisibility(!Instance.IsPanelInfoActive());
    }

    //Todo : Move to better place
    [SerializeField] private Button narationButton;
    public void SetButtonNarationVisibility(bool enabled)
    {
        narationButton.interactable = enabled;
    }
    
    public void ForceHideInfoPanel()
    {
        GetPanelByType(PanelType.PanelInfo)?.SetActive(false);
    }
    
    public void ShowInfoPanel()
    {
        if (GameManager.Instance != null)
        {
            var panelInfoGameObject = GetPanelByType(PanelType.PanelInfo);
            var infoPanel = panelInfoGameObject.GetComponent<InfoPanel>();
            if (infoPanel != null)
            {
                infoPanel.SetTitleText(GameManager.Instance.GetInfoTitle());
                infoPanel.SetDescriptionText(GameManager.Instance.GetInfoText());
                //ApplyInfoTextStyle(infoPanel, GameManager.Instance.currentMode);
            }
        }
        GetPanelByType(PanelType.PanelInfo)?.SetActive(true);
        
        // Todo use observer pattern at least
        SetButtonNarationVisibility(!Instance.IsPanelInfoActive());
    }

    private void ApplyInfoTextStyle(TextMeshProUGUI textComponent, GameMode mode)
    {
        InfoStyleStruct style = styleConfigDefault.GetStyle();
        if (style.Font != null) textComponent.font = style.Font;
        textComponent.fontSize        = style.FontSize;
        textComponent.alignment       = style.Alignment;
        textComponent.fontStyle       = style.IsBold ? FontStyles.Bold : FontStyles.Normal;
        textComponent.lineSpacing     = style.LineSpacing;
        textComponent.paragraphSpacing = style.ParagraphSpacing;
        textComponent.margin          = new Vector4(style.MarginLeft, style.MarginTop, style.MarginRight, style.MarginBottom);
    }

    #endregion
    
    #region Naration Panel

    public bool IsPanelNarationActive()
    {
        return GetPanelByType(PanelType.PanelNaration).activeSelf;
    }

    public void ToggleNarationPanel()
    {
        if (IsPanelNarationActive())
        {
            ForceHideNarationPanel();
        }
        else
        {
            ForceHideInfoPanel();
            ShowNarationPanel();
        }
    }

    public void ForceHideNarationPanel()
    {
        GetPanelByType(PanelType.PanelNaration)?.SetActive(false);
    }
    
    public void ShowNarationPanel()
    {
        GetPanelByType(PanelType.PanelNaration)?.SetActive(true);
    }
    #endregion
 
    [SerializeField] private GameObject ScanMarkerTextGO;
    // TODO : TEMP Move to proper place
    public void SetScanMarkerTextVisibility(bool visible)
    {
        ScanMarkerTextGO.SetActive(visible);
    }
    
}