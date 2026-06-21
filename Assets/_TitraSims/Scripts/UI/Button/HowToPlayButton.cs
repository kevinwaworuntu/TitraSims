using UnityEngine;

namespace UI
{
    public class HowToPlayButton : TitraSims_Button
    {
        private MainMenu mainMenu;

        private void Start() => mainMenu = FindAnyObjectByType<MainMenu>();

        protected override void OnClick()
        {
            if (mainMenu) mainMenu.GoToHowToPlay();
        }
    }
}
