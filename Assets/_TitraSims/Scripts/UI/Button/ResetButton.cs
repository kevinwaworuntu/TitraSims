using UnityEngine;

namespace UI
{
    public class ResetButton : TitraSims_Button
    {
        private MainMenu mainMenu;

        private void Start() => mainMenu = FindAnyObjectByType<MainMenu>();

        protected override void OnClick()
        {
            if (mainMenu) mainMenu.ResetProgressButtonTrigger();
        }
    }
}
