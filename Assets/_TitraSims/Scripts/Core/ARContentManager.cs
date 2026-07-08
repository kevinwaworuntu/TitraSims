using DG.Tweening;
using Gameplay;
using UI;
using System.Collections;
using UnityEngine;

public class ARContentManager : MonoBehaviour
{
    protected TahapanInteractionController tahapanInteractionController;
    private float autoHideNarationPanel = 3;
    
    protected virtual void Awake()
    {
        tahapanInteractionController = GetComponent<TahapanInteractionController>();
    }

    protected virtual void OnEnable()
    {
        var parentObject = transform.parent.gameObject;
        if (!parentObject)
        {
            return;
        }
        DefaultObserverEventHandler defaultObserverEventHandler = parentObject.GetComponent<DefaultObserverEventHandler>();
        if (defaultObserverEventHandler)
        {
            defaultObserverEventHandler.OnTargetFound.AddListener(OnTargetFound);
            defaultObserverEventHandler.OnTargetLost.AddListener(OnTargetLost);
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
        if (!UIManager.Instance)
        {
            return;
        }
        ToggleNavigationButtons(false, false);
    }

    protected virtual void OnDisable()
    {
        var parentObject = transform.parent.gameObject;
        if (!parentObject)
        {
            return;
        }
        DefaultObserverEventHandler defaultObserverEventHandler = parentObject.GetComponent<DefaultObserverEventHandler>();
        if (defaultObserverEventHandler)
        {
            defaultObserverEventHandler.OnTargetFound.RemoveListener(OnTargetFound);
            defaultObserverEventHandler.OnTargetLost.RemoveListener(OnTargetLost);
        }
        if (tahapanInteractionController != null)
        {
            tahapanInteractionController.OnStartWaitingForPlayerInputToContinue -= OnStartWaitingForPlayerInputToContinueHandler;
            tahapanInteractionController.OnFinishPlayingInteraction -= OnFinishPlayingTahapanInteractionHandler;
            tahapanInteractionController.OnInteractionComplete -= OnInteractionCompleteHandler;
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

            StartCoroutine(AutoHideNarationPanel());
           
        }
    }
  
    IEnumerator AutoHideNarationPanel() // Todo : Add detection also if player intended to reopen
    {
        yield return new WaitForSeconds(autoHideNarationPanel);
        UIManager.Instance.ForceHideNarationPanel();
    }
    
    public void OnTargetLost()
    {
        if(GameManager.Instance.IsCurrentTahapCompleted())
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
            StartCoroutine(AutoHideNarationPanel());
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