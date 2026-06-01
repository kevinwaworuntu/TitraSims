using UnityEngine;


public class MainMenu : MonoBehaviour
{
    public void GoToModeSelection()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPanelAndAddToHistory(PanelType.PanelModeSelection);
        }
    }
    
    public void GoToHowToPlay()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPanelAndAddToHistory(PanelType.PanelHowToPlay);
        }
    }
    
    public void GoToCredits()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPanelAndAddToHistory(PanelType.PanelTeamInformation);
        }
    }
    
    public void ExitButton()
    {
        Application.Quit();
    }
    
    public void SetGameModeTBA()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMode(GameMode.TBA);
        }
    }
    
    public void SetGameModeTK()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMode(GameMode.Kompleksometri);
        }
    }

    public void ResetProgressButtonTrigger()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetAllProgress();
        }
    }
}