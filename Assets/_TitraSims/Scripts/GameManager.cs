using Config;
using UnityEngine;
// using Vuforia;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Mode Saat Ini")]
    public GameMode currentMode;

    [Header("Tahapan Data")]
    public TahapanData[] tahapanTBA;
    public TahapanData[] tahapanKompleksometri;

    [Header("Marker Mapping")]
    public GameObject[] markerTBAMapping;
    public GameObject[] markerTKMapping;
    
    [Header("Runtime State")]
    public int currentAttemptingTahapIndex = -1;

    private const string LAST_COMPLETED_TAHAP_TBA_KEY = "LastCompletedTahapTBA";
    private const string LAST_COMPLETED_TAHAP_KOMP_KEY = "LastCompletedTahapKomp";

    [Header("Config Data")] 
    [SerializeField] private AnimationConfig animationConfig;
    [SerializeField] private InfoStyleConfig styleConfigDefault;
    [SerializeField] private InfoStyleConfig styleConfigTBA;
    [SerializeField] private InfoStyleConfig styleConfigTK;

    public AnimationConfig AnimationConfig => animationConfig;
    private string CurrentProgressKey
    {
        get
        {
            return currentMode == GameMode.TBA
                ? LAST_COMPLETED_TAHAP_TBA_KEY
                : LAST_COMPLETED_TAHAP_KOMP_KEY;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SetARCameraActive(false);
    }
    
    public void SetARCameraActive(bool isActive)
    {
        // if (VuforiaBehaviour.Instance != null)
        // {
        //     VuforiaBehaviour.Instance.enabled = isActive;
        // }
    }
    
    public int GetLastCompletedTahapIndex()
    {
        return PlayerPrefs.GetInt(CurrentProgressKey, -1);
    }
    
    public void SetMode(GameMode mode)
    {
        currentMode = mode;
        currentAttemptingTahapIndex = -1;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ForceHideInfoPanel();
            var panelToOpen = (currentMode == GameMode.TBA)
                ? UIManager.Instance.panelTBA
                : UIManager.Instance.panelTK;

            UIManager.Instance.ShowPanelAndAddToHistory(panelToOpen);
            UIManager.Instance.UpdateTahapButtonStates();
        }
    }
    
    public void StartTahap(int tahapIndex)
    {
        int lastCompleted = GetLastCompletedTahapIndex();
        
        if (tahapIndex > lastCompleted + 1)
        {
            return;
        }
        
        switch (currentMode)
        {
            case GameMode.TBA:
                if(tahapIndex < markerTBAMapping.Length) markerTBAMapping[tahapIndex].SetActive(true);
                break;
            case GameMode.Kompleksometri:
                if(tahapIndex < markerTKMapping.Length) markerTKMapping[tahapIndex].SetActive(true);
                break;
        }
        currentAttemptingTahapIndex = tahapIndex;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ForceHideInfoPanel();
            UIManager.Instance.ShowPanelAndAddToHistory(UIManager.Instance.panelScanAR);
        }

        SetARCameraActive(true);
    }

    public void BackFrromCurrentTahap()
    {
        if (currentAttemptingTahapIndex < 0)
        {
            return;
        }
        switch (currentMode)
        {
            case GameMode.TBA:
                if(currentAttemptingTahapIndex < markerTBAMapping.Length) markerTBAMapping[currentAttemptingTahapIndex].SetActive(false);
                break;
            case GameMode.Kompleksometri:
                if(currentAttemptingTahapIndex < markerTKMapping.Length) markerTKMapping[currentAttemptingTahapIndex].SetActive(false);
                break;
        }
        animationConfig.GenericAnimController[animationConfig.GetAnimGenericClipEntryName()] = null;
        currentAttemptingTahapIndex = -1;
        SetARCameraActive(false);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ForceHideInfoPanel();
            UIManager.Instance.GoBack();
            UIManager.Instance.UpdateTahapButtonStates();
        }
    }
    public void CompleteCurrentTahap()
    {
        if (currentAttemptingTahapIndex < 0)
        {
            return;
        }
        switch (currentMode)
        {
            case GameMode.TBA:
                if(currentAttemptingTahapIndex < markerTBAMapping.Length) markerTBAMapping[currentAttemptingTahapIndex].SetActive(false);
                break;
            case GameMode.Kompleksometri:
                if(currentAttemptingTahapIndex < markerTKMapping.Length) markerTKMapping[currentAttemptingTahapIndex].SetActive(false);
                break;
        }
        animationConfig.GenericAnimController[animationConfig.GetAnimGenericClipEntryName()] = null;
        if (PlayerPrefs.GetInt(CurrentProgressKey, -1) <= currentAttemptingTahapIndex)
        {
            PlayerPrefs.SetInt(CurrentProgressKey, currentAttemptingTahapIndex);
            PlayerPrefs.Save();
        }
        
        currentAttemptingTahapIndex = -1;
        SetARCameraActive(false);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ForceHideInfoPanel();
            UIManager.Instance.GoBack();
            UIManager.Instance.UpdateTahapButtonStates();
        }
    }

    public void ShowInfoPopup(GameObject infoPanel, string infoText)
    {
        if (infoPanel == null) return;

        var textComponent = infoPanel.GetComponentInChildren<TextMeshProUGUI>(true);
        if (textComponent != null)
        {
            
            if (string.IsNullOrWhiteSpace(infoText))
            {
                int idx = currentAttemptingTahapIndex;
                if (currentMode == GameMode.TBA)
                    infoText = (idx >= 0 && idx < InfoTextBank.TBA.Length) ? InfoTextBank.TBA[idx] : "";
                else
                    infoText = (idx >= 0 && idx < InfoTextBank.TK.Length) ? InfoTextBank.TK[idx] : "";
            }
            textComponent.text = infoText;
            textComponent.overflowMode = TextOverflowModes.Overflow;
            switch (currentMode)
            {
                case GameMode.TBA:
                    RetrieveTextStyle(textComponent, styleConfigTBA);
                    break;
                case GameMode.Kompleksometri:
                    RetrieveTextStyle(textComponent, styleConfigTK);
                    break;
                default:
                    RetrieveTextStyle(textComponent, styleConfigDefault);
                    break;
            }
        }

        infoPanel.SetActive(true);
    }
    
    public void RetrieveTextStyle(TextMeshProUGUI textComponent, InfoStyleConfig config)
    {
        if (config == null)
        {
            Debug.Break();
            return;
        }
        InfoStyleStruct style = config.GetStyle();
        
        textComponent.font = style.Font;
        textComponent.fontSize = style.FontSize;
        textComponent.alignment = style.Alignment;
        textComponent.fontStyle = style.IsBold ? FontStyles.Bold : FontStyles.Normal;
        textComponent.lineSpacing = style.LineSpacing;
        textComponent.paragraphSpacing = style.ParagraphSpacing;
        textComponent.margin = new Vector4(style.MarginLeft, style.MarginTop, style.MarginRight, style.MarginBottom);    
    }

    public void HideInfoPopup(GameObject infoPanel)
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);  
        }
    }
    
    public TahapanData GetCurrentTahapanData(int index)
    {
        if (currentMode == GameMode.TBA)
        {
            if (tahapanTBA != null && index >= 0 && index < tahapanTBA.Length)
                return tahapanTBA[index];
        }
        else 
        {
            if (tahapanKompleksometri != null && index >= 0 && index < tahapanKompleksometri.Length)
                return tahapanKompleksometri[index];
        }

        return null;
    }
    
    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey(LAST_COMPLETED_TAHAP_TBA_KEY);
        PlayerPrefs.DeleteKey(LAST_COMPLETED_TAHAP_KOMP_KEY);
        PlayerPrefs.Save();

        currentAttemptingTahapIndex = -1;
        SetARCameraActive(false);
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ForceHideInfoPanel();
            UIManager.Instance.UpdateTahapButtonStates();
        }
    }
}
