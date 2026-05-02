using System;
using Gameplay;
using UI;
using UnityEngine;

public class ARContentManager : MonoBehaviour
{
    protected TahapanInteractionController tahapanInteractionController;

    protected virtual void Awake()
    {
        tahapanInteractionController = GetComponent<TahapanInteractionController>();
    }

    protected virtual void OnEnable()
    {
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
        if (tahapanInteractionController != null)
        {
            tahapanInteractionController.OnStartWaitingForPlayerInputToContinue -= OnStartWaitingForPlayerInputToContinueHandler;
            tahapanInteractionController.OnFinishPlayingInteraction -= OnFinishPlayingTahapanInteractionHandler;
            tahapanInteractionController.OnInteractionComplete -= OnInteractionCompleteHandler;
        }
    }

    public void OnTargetFound()
    {
        if (!UIManager.Instance || UIManager.Instance.IsPanelInfoActive())
        {
            return;
        }
        if (!tahapanInteractionController)
        {
            return;
        }
        tahapanInteractionController.StartInteraction();
    }

    public void OnTargetLost()
    {
        if (!UIManager.Instance)
        {
            return;
        }
        UIManager.Instance.HideAllARPopups();
    }
    
    protected virtual void OnStartWaitingForPlayerInputToContinueHandler()
    {
        if (!UIManager.Instance)
        {
            return;
        }
        ToggleNavigationButtons(true, false);
        
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
        UIManager.Instance.btnNextInteraction.onClick.AddListener(tahapanInteractionController.PlayerInteractToFinishInteraction);
        UIManager.Instance.btnNextInteraction.onClick.AddListener(() =>
        {
            ToggleNavigationButtons(false, false);
        });
    }
    
    protected void OnFinishPlayingTahapanInteractionHandler()
    {
        if (!tahapanInteractionController)
        {
            return;
        }
        tahapanInteractionController.ContinueInteraction();
    }
    
    protected void OnInteractionCompleteHandler()
    {
        if (!UIManager.Instance)
        {
            return;
        }
        UIManager.Instance.NarationPanel.gameObject.SetActive(false);
        ToggleNavigationButtons(false, true);

        UIManager.Instance.btnCompleteTahapan.onClick.RemoveAllListeners();
        UIManager.Instance.btnCompleteTahapan.onClick.AddListener(() => 
        {
            GameManager.Instance.CompleteCurrentTahap();
            ContextualButtonController.Instance?.DestroyButtons();
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