using UnityEngine;
using UnityEngine.UI;

public class TahapanController : MonoBehaviour
{
    [Tooltip("Index tahapan ini (mulai dari 0 untuk Tahap 1).")]
    public int tahapIndex;   

    private Button myButton;
    private CanvasGroup myCanvasGroup;

    void Awake()
    {
        myButton = GetComponent<Button>();
        myCanvasGroup = GetComponent<CanvasGroup>();
        
        // ToDo : Remove logging from release build
        if (myButton == null)
        {
            Debug.LogError($"[TahapanController] Button component missing on {gameObject.name}!");
        }
        if (myCanvasGroup == null)
        {
            Debug.LogError($"[TahapanController] CanvasGroup component missing on {gameObject.name}. Tambahkan CanvasGroup untuk kontrol alpha & interaksi.");
        }
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
        TahapanData data = GameManager.Instance.GetCurrentTahapanData(tahapIndex);
        if (data == null)
        {
            return;
        }
        GameManager.Instance.StartTahap(tahapIndex);
        GameManager.Instance.HideInfoPopup(data.panelInfo);
    }

    private void OnInfoButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            UIManager.Instance.ShowInfoPanel();  
        }
    }
    
    public void UpdateVisualState(bool isInteractable, float alpha)
    {
        if (myCanvasGroup != null)
        {
            myCanvasGroup.alpha = alpha;
            myCanvasGroup.interactable = isInteractable;
            myCanvasGroup.blocksRaycasts = isInteractable;
        }
    }
}
