using System;
using Config;
using UnityEngine;
using UnityEngine.Serialization;
using Vuforia;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Mode Saat Ini")]
    public GameMode currentMode;

    [FormerlySerializedAs("markerTBAMapping")] [Header("Marker Object Mapping")]
    public GameObject[] markerTBAPrefabsMapping;
    [FormerlySerializedAs("markerTKMapping")] public GameObject[] markerTKPrefabsMapping;
    
    [FormerlySerializedAs("marker")]
    [Header("Marker Mapping")]
    [SerializeField] private GameObject[] markerTBA;
    [FormerlySerializedAs("marker")]
    [Header("Marker Mapping")]
    [SerializeField] private GameObject[] markerTK;
    
    [Header("Runtime State")]
    private int currentAttemptingTahapIndex = -1;

    private const string LAST_COMPLETED_TAHAP_TBA_KEY = "LastCompletedTahapTBA";
    private const string LAST_COMPLETED_TAHAP_KOMP_KEY = "LastCompletedTahapKomp";

    [Header("Development Build Unlock Override")]
    [Tooltip("Enabled: progress persists via PlayerPrefs, as normal.\nDisabled: ignores PlayerPrefs — each tahapan's unlock state comes from the checkbox masks below. Use for development/test builds.")]
    [SerializeField] private bool usePlayerPrefsForProgress = true;
    [Tooltip("Bitmask: bit N set = tahapan N is unlocked. Edit via the Tahap Progress Debug window's checkbox dropdown.")]
    [SerializeField] private int devUnlockedMaskTBA = 1;
    [SerializeField] private int devUnlockedMaskKomp = 1;

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

    private GameObject currentActiveMarkerObject;
    private bool isTahapCompleted;
   
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
        return GetLastCompletedTahapIndex(currentMode);
    }

    /// <summary>Returns the last completed tahap index for any given mode (not just the current one).</summary>
    public int GetLastCompletedTahapIndex(GameMode mode)
    {
        if (!usePlayerPrefsForProgress)
        {
            int mask = mode == GameMode.TBA ? devUnlockedMaskTBA : devUnlockedMaskKomp;
            return HighestSetBitIndex(mask);
        }

        string key = mode == GameMode.TBA ? LAST_COMPLETED_TAHAP_TBA_KEY : LAST_COMPLETED_TAHAP_KOMP_KEY;
        return PlayerPrefs.GetInt(key, -1);
    }

    /// <summary>Whether a marker prefab is assigned for the given tahap index. Only relevant when usePlayerPrefsForProgress is disabled.</summary>
    public bool HasMarkerPrefabAssigned(GameMode mode, int tahapIndex)
    {
        GameObject[] mapping = mode == GameMode.TBA ? markerTBAPrefabsMapping : markerTKPrefabsMapping;
        return mapping != null && tahapIndex >= 0 && tahapIndex < mapping.Length && mapping[tahapIndex] != null;
    }

    /// <summary>Whether a specific tahapan can be entered right now, under either the PlayerPrefs (sequential) or dev override (checkbox mask) model.</summary>
    public bool IsTahapUnlocked(GameMode mode, int tahapIndex)
    {
        if (tahapIndex < 0) return false;

        if (!usePlayerPrefsForProgress)
        {
            return HasMarkerPrefabAssigned(mode, tahapIndex);
        }

        return tahapIndex <= GetLastCompletedTahapIndex(mode) + 1;
    }

    private static int HighestSetBitIndex(int mask)
    {
        int highest = -1;
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0) highest = i;
        }
        return highest;
    }

    /// <summary>Directly sets the last completed tahap index for a given mode, unlocking every tahap up to and including it.</summary>
    public void SetLastCompletedTahapIndex(GameMode mode, int index)
    {
        if (!usePlayerPrefsForProgress)
        {
            Debug.LogWarning("[GameManager] usePlayerPrefsForProgress is disabled — tahapan unlock is fixed by devUnlockedMaskTBA/Komp and won't change.");
            return;
        }

        string key = mode == GameMode.TBA ? LAST_COMPLETED_TAHAP_TBA_KEY : LAST_COMPLETED_TAHAP_KOMP_KEY;
        PlayerPrefs.SetInt(key, index);
        PlayerPrefs.Save();

        if (mode == currentMode)
        {
            OnModeSet?.Invoke(currentMode, index);
        }
    }

    public void SetMode(GameMode mode)
    {
        currentMode = mode;
        currentAttemptingTahapIndex = -1;
        OnModeSet?.Invoke(currentMode, GetLastCompletedTahapIndex());
    }

    public void StartTahap(int tahapIndex)
    {
        if (!IsTahapUnlocked(currentMode, tahapIndex))
        {
            return;
        }

        if (!SpawnMarkerObject(tahapIndex))
        {
            return;
        }
        currentAttemptingTahapIndex = tahapIndex;
        SetARCameraActive(true);
        OnTahapStarted?.Invoke();
        isTahapCompleted = false;
    }

    public bool SpawnMarkerObject(int tahapIndex)
    {
       
        GameObject markerImageObject = null;
        GameObject markerPrefab = null;
        switch (currentMode)
        {
            case GameMode.TBA:
                if ((tahapIndex >= markerTBA.Length || markerTBA[tahapIndex] == null)  || (tahapIndex >= markerTBAPrefabsMapping.Length || markerTBAPrefabsMapping[tahapIndex] == null))
                {
                    return false;
                }
                markerImageObject = markerTBA[tahapIndex];
                markerPrefab = markerTBAPrefabsMapping[tahapIndex];
                break;
            case GameMode.Kompleksometri:
                if ((tahapIndex >= markerTK.Length || markerTK[tahapIndex] == null)  || (tahapIndex >= markerTKPrefabsMapping.Length || markerTKPrefabsMapping[tahapIndex] == null))
                {
                    return false;
                }
                markerImageObject = markerTK[tahapIndex];
                markerPrefab = markerTKPrefabsMapping[tahapIndex];
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
   
        currentActiveMarkerObject = Instantiate(markerPrefab, markerImageObject.transform, false);
        currentActiveMarkerObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        currentActiveMarkerObject.transform.localScale = Vector3.one;
        return true;
    }

    public bool DestroyMarkerObject()
    {
        if (currentActiveMarkerObject)
        {
            Destroy(currentActiveMarkerObject);
            currentActiveMarkerObject = null;
            return true;
        }
        
        return false;
    }

    public void BackFromCurrentTahap()
    {
        if (currentAttemptingTahapIndex < 0)
        {
            return;
        }

        if (!DestroyMarkerObject())
        {
            Debug.LogError("Failed to destroy marker object!");
        }
        currentAttemptingTahapIndex = -1;
        SetARCameraActive(false);
    }
    public void CompleteCurrentTahap()
    {
        if (currentAttemptingTahapIndex < 0)
        {
            return;
        }
        if (!DestroyMarkerObject())
        {
            Debug.LogError("Failed to destroy marker object!");
        }
        if (usePlayerPrefsForProgress && PlayerPrefs.GetInt(CurrentProgressKey, -1) <= currentAttemptingTahapIndex)
        {
            PlayerPrefs.SetInt(CurrentProgressKey, currentAttemptingTahapIndex);
            PlayerPrefs.Save();
        }
        
        currentAttemptingTahapIndex = -1;
        SetARCameraActive(false);
        OnTahapCompleted?.Invoke(currentMode, GetLastCompletedTahapIndex());
        isTahapCompleted = true;
    }

    public bool IsCurrentTahapCompleted()
    {
        return isTahapCompleted;
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
        if (usePlayerPrefsForProgress)
        {
            PlayerPrefs.DeleteKey(LAST_COMPLETED_TAHAP_TBA_KEY);
            PlayerPrefs.DeleteKey(LAST_COMPLETED_TAHAP_KOMP_KEY);
            PlayerPrefs.Save();
        }

        currentAttemptingTahapIndex = -1;
        SetARCameraActive(false);
        OnProgressReset?.Invoke();
    }

#if UNITY_EDITOR
    public int MarkerTBACount  => markerTBAPrefabsMapping?.Length ?? 0;
    public int MarkerKompCount => markerTKPrefabsMapping?.Length ?? 0;
    public GameObject CurrentActiveMarkerObject => currentActiveMarkerObject;
    public bool UsePlayerPrefsForProgress => usePlayerPrefsForProgress;

    public GameObject[] GetMarkerMapping(GameMode mode)
    {
        return mode == GameMode.TBA ? markerTBAPrefabsMapping : markerTKPrefabsMapping;
    }

    public bool DebugSpawnTahap(int tahapIndex, GameMode mode)
    {
        DestroyMarkerObject();
        GameObject[] mapping = mode == GameMode.TBA ? markerTBAPrefabsMapping : markerTKPrefabsMapping;
        if (mapping == null || tahapIndex >= mapping.Length || mapping[tahapIndex] == null) return false;
        if (markerTBA == null || tahapIndex >= markerTBA.Length || markerTBA[tahapIndex] == null) return false;
        currentActiveMarkerObject = Instantiate(mapping[tahapIndex], markerTBA[tahapIndex].transform, false);
        currentActiveMarkerObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        currentActiveMarkerObject.transform.localScale = Vector3.one;
        currentAttemptingTahapIndex = tahapIndex;
        isTahapCompleted = false;
        OnTahapStarted?.Invoke();
        return true;
    }
#endif
}
