using UnityEngine;


public class MainMenu : MonoBehaviour
{

    public void StartButton()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPanelAndAddToHistory(UIManager.Instance.panelPilihMode);
        }
    }


    public void HowToPlayButton()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPanelAndAddToHistory(UIManager.Instance.panelHowToPlay);
        }
    }


    public void InfoButton()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPanelAndAddToHistory(UIManager.Instance.panelInformasiTim);
        }
    }


    public void ExitButton()
    {
        Debug.Log("Aplikasi akan ditutup.");
        Application.Quit();
    }




    public void PilihModeTBA()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMode(GameMode.TBA);
        }
    }


    public void PilihModeKompleksometri()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMode(GameMode.Kompleksometri);
        }
    }




    public void BackButtonTrigger()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.GoBack();
        }
    }

    public void ResetProgressButtonTrigger()
    {
        if (GameManager.Instance != null)
        {

            GameManager.Instance.ResetAllProgress();


            Debug.Log("Tombol Reset ditekan. Progres dihapus.");
        }
    }
}