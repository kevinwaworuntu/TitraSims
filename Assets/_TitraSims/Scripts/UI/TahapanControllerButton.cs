using UnityEngine;
using UnityEngine.UI;

public class TahapanControllerButton : MonoBehaviour
{
    [Tooltip("Index tahapan ini (mulai dari 0 untuk Tahap 1).")]
    public int TahapIndex;   
    public GameMode ButtonGameMode;
    
    private Button myButton;
    private CanvasGroup myCanvasGroup;
    private const float buttonActiveAplhaValue = 1;
    private const float buttonInactiveAlphaValue = 0.75f;
    
    void Awake()
    {
        myButton = GetComponent<Button>();
        myCanvasGroup = GetComponent<CanvasGroup>();
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
    }

    private void OnDisable()
    {
        if (myButton != null)
        {
            myButton.onClick.RemoveAllListeners(); 
        }
    }

    private void OnTahapClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[TahapanController] GameManager.Instance is null!");
            return;
        }
        TahapanData data = GameManager.Instance.GetCurrentTahapanData(TahapIndex);
        if (data == null)
        {
            return;
        }
        GameManager.Instance.StartTahap(TahapIndex);
        GameManager.Instance.HideInfoPopup(data.panelInfo);
    }

    private void OnInfoButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            UIManager.Instance.ShowInfoPanel();  
        }
    }
    
    public void UpdateVisualState(bool isInteractable)
    {
        if (myCanvasGroup != null)
        {
            myCanvasGroup.alpha = isInteractable ? buttonActiveAplhaValue : buttonInactiveAlphaValue;
            myCanvasGroup.interactable = isInteractable;
            myCanvasGroup.blocksRaycasts = isInteractable;
        }
    }
}
