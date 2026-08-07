using DG.Tweening;
using Gameplay;
using UI;
using System.Collections;
using UnityEngine;
using Vuforia;

public class ARContentManager : MonoBehaviour
{
    protected TahapanInteractionController tahapanInteractionController;
    private float autoHideNarationPanel = 3;
    private int interactByQty;
    private int targetInteractByQty = 2;

    private ObserverBehaviour observerBehaviour;
    private bool isTargetFound;
    private Coroutine autoHideNarationRoutine;

    protected virtual void Awake()
    {
        tahapanInteractionController = GetComponent<TahapanInteractionController>();
    }

    protected virtual void OnEnable()
    {
        // Each spawn starts un-latched, so it computes its own found/lost edge instead of
        // inheriting the state of the ImageTarget's shared DefaultObserverEventHandler.
        isTargetFound = false;

        observerBehaviour = ResolveObserverBehaviour();
        if (observerBehaviour)
        {
            observerBehaviour.OnTargetStatusChanged += OnObserverStatusChanged;
        }
        if (!tahapanInteractionController)
        {
            tahapanInteractionController = GetComponent<TahapanInteractionController>();
        }
        if (tahapanInteractionController)
        {
            tahapanInteractionController.OnStartWaitingForPlayerInputToContinue += OnStartWaitingForPlayerInputToContinueHandler;
            tahapanInteractionController.OnFinishPlayingInteraction += OnFinishPlayingTahapanInteractionHandler;
            tahapanInteractionController.OnInteractionComplete += OnInteractionCompleteHandler;
        }
        ToggleNavigationButtons(false, false);
    }

    protected virtual void Start()
    {
        // Vuforia only raises a status change on a transition. Seed from the live status here —
        // after every OnEnable (ours and any subclass's) has run — so a target that is already
        // tracked at spawn time still starts the interaction.
        if (observerBehaviour)
        {
            EvaluateTargetStatus(observerBehaviour.TargetStatus.Status);
        }
    }

    protected virtual void OnDisable()
    {
        if (observerBehaviour)
        {
            observerBehaviour.OnTargetStatusChanged -= OnObserverStatusChanged;
        }
        observerBehaviour = null;

        if (tahapanInteractionController != null)
        {
            tahapanInteractionController.OnStartWaitingForPlayerInputToContinue -= OnStartWaitingForPlayerInputToContinueHandler;
            tahapanInteractionController.OnFinishPlayingInteraction -= OnFinishPlayingTahapanInteractionHandler;
            tahapanInteractionController.OnInteractionComplete -= OnInteractionCompleteHandler;
        }
    }

    private ObserverBehaviour ResolveObserverBehaviour()
    {
        var parent = transform.parent;
        return parent ? parent.GetComponent<ObserverBehaviour>() : null;
    }

    private void OnObserverStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        EvaluateTargetStatus(targetStatus.Status);
    }

    private void EvaluateTargetStatus(Status status)
    {
        // Mirrors the Tracked_ExtendedTracked filter the scene's ImageTargets are configured with.
        bool isFound = status == Status.TRACKED || status == Status.EXTENDED_TRACKED;
        if (isFound == isTargetFound)
        {
            return;
        }
        isTargetFound = isFound;

        if (isFound)
        {
            OnTargetFound();
        }
        else
        {
            OnTargetLost();
        }
    }

    public void OnTargetFound()
    {
        if (UIManager.Instance) // Todo : Revisit move to better place
        {
            UIManager.Instance.SetScanMarkerTextVisibility(false);
        }
        AudioManager.Instance?.SFX.PlayNonInterrupt(SoundType.Gameplay_MarkerFound);
        if (!tahapanInteractionController)
        {
            return;
        }
        tahapanInteractionController.StartInteraction(); // Todo : need revisit, wrong gated logic
        
        if (UIManager.Instance) // Todo : Revisit move to better place
        {
            UIManager.Instance.ForceHideInfoPanel();
            UIManager.Instance.SetButtonNarationVisibility(true);

            RestartAutoHideNarationPanel();

        }
    }

    private void RestartAutoHideNarationPanel()
    {
        if (autoHideNarationRoutine != null)
        {
            StopCoroutine(autoHideNarationRoutine);
        }
        autoHideNarationRoutine = StartCoroutine(AutoHideNarationPanel());
    }

    IEnumerator AutoHideNarationPanel() // Todo : Add detection also if player intended to reopen
    {
        yield return new WaitForSeconds(autoHideNarationPanel);
        autoHideNarationRoutine = null;
        if (UIManager.Instance)
        {
            UIManager.Instance.ForceHideNarationPanel();
        }
    }

    public void OnTargetLost()
    {
        // Prompt the player to re-scan while the tahapan is still in progress. Once it is
        // completed the flow moves on and the prompt would be noise.
        if (GameManager.Instance && !GameManager.Instance.IsCurrentTahapCompleted())
        {
            if (UIManager.Instance) // Todo : Revisit move to better place
            {
                UIManager.Instance.SetScanMarkerTextVisibility(true);
            }
        }
        AudioManager.Instance?.SFX.PlayNonInterrupt(SoundType.Gameplay_MarkerLost);
        if (!UIManager.Instance)
        {
            return;
        }
        UIManager.Instance.HideAllARPopups();
    }

    //TODO :MOVE THIS SOMEWHERE ELSE
    public void RequestContinue()
    {
        OnStartWaitingForPlayerInputToContinueHandler();
    }
    
    public void RequestContinueByQty()
    {
        interactByQty ++;
        if (interactByQty < targetInteractByQty)
        {
            return;
        }
        OnStartWaitingForPlayerInputToContinueHandler();
        interactByQty = 0;
    }

    public void SetContinueByQtyTarget(int qty)
    {
        targetInteractByQty = qty;
    }
    
    protected virtual void OnStartWaitingForPlayerInputToContinueHandler()
    {
        if (!UIManager.Instance)
        {
            return;
        }
        ToggleNavigationButtons(true, false);
        UIManager.Instance.SetButtonNarationVisibility(false);

        if (!tahapanInteractionController)
        {
            return;
        }
        if (tahapanInteractionController.IsPlayingLastIndex())
        {
            OnInteractionCompleteHandler();
            return;
        }
        UIManager.Instance.btnNextInteraction.onClick.RemoveAllListeners();
        UIManager.Instance.btnNextInteraction.onClick.AddListener(() =>
        {
            var rt = UIManager.Instance.btnNextInteraction.GetComponent<RectTransform>();
            UIAnimator.ButtonPress(rt).OnComplete(() =>
            {
                tahapanInteractionController.PlayerInteractToFinishInteraction();
                ToggleNavigationButtons(false, false);
            });
            RestartAutoHideNarationPanel();
        });
    }
    
    protected void OnFinishPlayingTahapanInteractionHandler()
    {
        if (!tahapanInteractionController)
        {
            return;
        }
        tahapanInteractionController.ContinueInteraction();

        if (UIManager.Instance)
        {
            UIManager.Instance.SetButtonNarationVisibility(true);
        }
    }
    
    protected void OnInteractionCompleteHandler()
    {
        if (!UIManager.Instance)
        {
            return;
        }
        
        // Todo : move this to better place
        UIManager.Instance.SetScanMarkerTextVisibility(false);
        UIManager.Instance.SetButtonNarationVisibility(false);
            
        UIManager.Instance.GetPanelByType(PanelType.PanelNaration)?.SetActive(false);
        ToggleNavigationButtons(false, true);

        UIManager.Instance.btnCompleteTahapan.onClick.RemoveAllListeners();
        UIManager.Instance.btnCompleteTahapan.onClick.AddListener(() =>
        {
            var rt = UIManager.Instance.btnCompleteTahapan.GetComponent<RectTransform>();
            UIAnimator.ButtonPress(rt).OnComplete(() =>
            {
                GameManager.Instance.CompleteCurrentTahap();
                ContextualButtonController.Instance?.DestroyButtons();
            });
        });
    }
    
    private void ToggleNavigationButtons(bool nextActive, bool completeActive)
    {
        if (!UIManager.Instance)
        {
            return;
        }
        UIManager.Instance.btnNextInteraction.gameObject.SetActive(nextActive);
        UIManager.Instance.btnCompleteTahapan.gameObject.SetActive(completeActive);
    }
}