using UnityEngine;
using UnityEngine.UI;

public class TahapanControllerButton : MonoBehaviour
{
    [Tooltip("Index tahapan ini (mulai dari 0 untuk Tahap 1).")]
    public int TahapIndex;   
    public GameMode ButtonGameMode;
    
    private Button myButton;
    private CanvasGroup myCanvasGroup;
    private const float buttonActiveAlphaValue = 1f;
    private const float buttonInactiveAlphaValue = 0.75f;

    void Awake()
    {
        myButton = GetComponent<Button>();
        myCanvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        RefreshVisualState();
    }

    private void OnEnable()
    {
        if (myButton != null)
        {
            myButton.onClick.AddListener(() =>
            {
                OnTahapClicked();
                OnInfoButtonClicked();
            });
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnModeSet       += HandleProgressChanged;
            GameManager.Instance.OnTahapCompleted += HandleProgressChanged;
            GameManager.Instance.OnProgressReset  += RefreshVisualState;
        }
    }

    private void OnDisable()
    {
        if (myButton != null)
        {
            myButton.onClick.RemoveAllListeners();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnModeSet        -= HandleProgressChanged;
            GameManager.Instance.OnTahapCompleted -= HandleProgressChanged;
            GameManager.Instance.OnProgressReset  -= RefreshVisualState;
        }
    }

    private void HandleProgressChanged(GameMode mode, int lastCompleted)
    {
        if (ButtonGameMode != mode) return;
        UpdateVisualState(TahapIndex <= lastCompleted + 1);
    }

    private void RefreshVisualState()
    {
        if (GameManager.Instance == null) return;
        int lastCompleted = GameManager.Instance.GetLastCompletedTahapIndex(ButtonGameMode);
        UpdateVisualState(TahapIndex <= lastCompleted + 1);
    }

    private void OnTahapClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[TahapanController] GameManager.Instance is null!");
            return;
        }
        //TahapanData data = GameManager.Instance.GetCurrentTahapanData(TahapIndex);
        // if (data == null)
        // {
        //     return;
        // }
        GameManager.Instance.StartTahap(TahapIndex);
        UIManager.Instance.ShowInfoPanel();
        //UIManager.Instance.ForceHideInfoPanel();
        // GameManager.Instance.HideInfoPopup(info);
    }

    private void OnInfoButtonClicked()
    {
        //UIManager.Instance?.ShowInfoPanel();  
    }
    
    public void UpdateVisualState(bool isInteractable)
    {
        if (myCanvasGroup != null)
        {
            myCanvasGroup.alpha = isInteractable ? buttonActiveAlphaValue : buttonInactiveAlphaValue;
            myCanvasGroup.interactable = isInteractable;
            myCanvasGroup.blocksRaycasts = isInteractable;
        }
    }
}
