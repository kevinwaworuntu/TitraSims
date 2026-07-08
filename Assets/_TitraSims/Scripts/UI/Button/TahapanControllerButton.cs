using UI;
using UnityEngine;

public class TahapanControllerButton : MonoBehaviour
{
    [Tooltip("Index tahapan ini (mulai dari 0 untuk Tahap 1).")]
    public int TahapIndex;
    public GameMode ButtonGameMode;

    private TitraSims_Button _button;
    private CanvasGroup _canvasGroup;
    private const float buttonActiveAlphaValue   = 1f;
    private const float buttonInactiveAlphaValue = 0.75f;

    private void Awake()
    {
        _button       = GetComponent<TitraSims_Button>();
        _canvasGroup  = GetComponent<CanvasGroup>();
    }

    private void Start() => RefreshVisualState();

    private void OnEnable()
    {
        _button?.onClick.AddListener(OnTahapClicked);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnModeSet        += HandleProgressChanged;
            GameManager.Instance.OnTahapCompleted += HandleProgressChanged;
            GameManager.Instance.OnProgressReset  += RefreshVisualState;
        }
        RefreshVisualState();
    }

    private void OnDisable()
    {
        _button?.onClick.RemoveListener(OnTahapClicked);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnModeSet        -= HandleProgressChanged;
            GameManager.Instance.OnTahapCompleted -= HandleProgressChanged;
            GameManager.Instance.OnProgressReset  -= RefreshVisualState;
        }
    }

    private void OnTahapClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[TahapanController] GameManager.Instance is null!");
            return;
        }
        GameManager.Instance.StartTahap(TahapIndex);
        UIManager.Instance.ShowInfoPanel();
    }

    private void HandleProgressChanged(GameMode mode, int lastCompleted)
    {
        if (ButtonGameMode != mode) return;
        UpdateVisualState(GameManager.Instance.IsTahapUnlocked(mode, TahapIndex));
    }

    private void RefreshVisualState()
    {
        if (GameManager.Instance == null) return;
        UpdateVisualState(GameManager.Instance.IsTahapUnlocked(ButtonGameMode, TahapIndex));
    }

    public void UpdateVisualState(bool isInteractable)
    {
        if (_canvasGroup == null) return;
        _canvasGroup.alpha          = isInteractable ? buttonActiveAlphaValue : buttonInactiveAlphaValue;
        _canvasGroup.interactable   = isInteractable;
        _canvasGroup.blocksRaycasts = isInteractable;
    }
}
