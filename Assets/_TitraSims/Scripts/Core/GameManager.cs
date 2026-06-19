using System;
using Config;
using UnityEngine;
using Vuforia;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Mode Saat Ini")]
    public GameMode currentMode;

    [Header("Marker Mapping")]
    public GameObject[] markerTBAMapping;
    public GameObject[] markerTKMapping;
    
    [Header("Runtime State")]
    private int currentAttemptingTahapIndex = -1;

    private const string LAST_COMPLETED_TAHAP_TBA_KEY = "LastCompletedTahapTBA";
    private const string LAST_COMPLETED_TAHAP_KOMP_KEY = "LastCompletedTahapKomp";

    [Header("Config Data")]
    [SerializeField] private AnimationConfig animationConfig;

    public AnimationConfig AnimationConfig => animationConfig;

    // ── Events ────────────────────────────────────────────────────────────────
    // Subscribers (UIManager, TahapanControllerButton, …) react to these.
    // GameManager never references those classes directly.
    public event Action<GameMode, int> OnModeSet;        // (mode, lastCompletedForMode)
    public event Action                OnTahapStarted;
    public event Action<GameMode, int> OnTahapCompleted; // (mode, newLastCompleted)
    public event Action                OnProgressReset;
    // ─────────────────────────────────────────────────────────────────────────

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
        if (VuforiaBehaviour.Instance != null)
        {
            VuforiaBehaviour.Instance.enabled = isActive;
        }
    }
    
    public int GetLastCompletedTahapIndex()
    {
        return PlayerPrefs.GetInt(CurrentProgressKey, -1);
    }

    /// <summary>Returns the last completed tahap index for any given mode (not just the current one).</summary>
    public int GetLastCompletedTahapIndex(GameMode mode)
    {
        string key = mode == GameMode.TBA ? LAST_COMPLETED_TAHAP_TBA_KEY : LAST_COMPLETED_TAHAP_KOMP_KEY;
        return PlayerPrefs.GetInt(key, -1);
    }

    public void SetMode(GameMode mode)
    {
        currentMode = mode;
        currentAttemptingTahapIndex = -1;
        OnModeSet?.Invoke(currentMode, GetLastCompletedTahapIndex());
    }

    public void StartTahap(int tahapIndex)
    {
        int lastCompleted = GetLastCompletedTahapIndex();

        if (tahapIndex > lastCompleted + 1)
        {
            return;
        }

        SetCurrentMarkerActive(tahapIndex, true);
        currentAttemptingTahapIndex = tahapIndex;
        SetARCameraActive(true);
        OnTahapStarted?.Invoke();
    }

    public void BackFromCurrentTahap()
    {
        if (currentAttemptingTahapIndex < 0)
        {
            return;
        }
        SetCurrentMarkerActive(currentAttemptingTahapIndex, false);
        currentAttemptingTahapIndex = -1;
        SetARCameraActive(false);
    }
    public void CompleteCurrentTahap()
    {
        if (currentAttemptingTahapIndex < 0)
        {
            return;
        }
        SetCurrentMarkerActive(currentAttemptingTahapIndex, false);
        if (PlayerPrefs.GetInt(CurrentProgressKey, -1) <= currentAttemptingTahapIndex)
        {
            PlayerPrefs.SetInt(CurrentProgressKey, currentAttemptingTahapIndex);
            PlayerPrefs.Save();
        }
        
        currentAttemptingTahapIndex = -1;
        SetARCameraActive(false);
        OnTahapCompleted?.Invoke(currentMode, GetLastCompletedTahapIndex());
    }
    public string GetInfoTitle()
    {
        int idx = currentAttemptingTahapIndex;
        if (currentMode == GameMode.TBA)
        {
            return (idx >= 0 && idx < InfoTextBank.TBA_Title.Length) ? InfoTextBank.TBA_Title[idx] : "";
        }
        return (idx >= 0 && idx < InfoTextBank.TK_Title.Length) ? InfoTextBank.TK_Title[idx] : "";
    }
    
    public string GetInfoText()
    {
        int idx = currentAttemptingTahapIndex;
        if (currentMode == GameMode.TBA)
        {
            return (idx >= 0 && idx < InfoTextBank.TBA.Length) ? InfoTextBank.TBA[idx] : "";
        }
        return (idx >= 0 && idx < InfoTextBank.TK.Length) ? InfoTextBank.TK[idx] : ""; 
    }
    
    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey(LAST_COMPLETED_TAHAP_TBA_KEY);
        PlayerPrefs.DeleteKey(LAST_COMPLETED_TAHAP_KOMP_KEY);
        PlayerPrefs.Save();

        currentAttemptingTahapIndex = -1;
        SetARCameraActive(false);
        OnProgressReset?.Invoke();
    }

    private void SetCurrentMarkerActive(int index, bool active)
    {
        switch (currentMode)
        {
            case GameMode.TBA:
                if (index < markerTBAMapping.Length) markerTBAMapping[index].SetActive(active);
                break;
            case GameMode.Kompleksometri:
                if (index < markerTKMapping.Length) markerTKMapping[index].SetActive(active);
                break;
        }
    }
}
