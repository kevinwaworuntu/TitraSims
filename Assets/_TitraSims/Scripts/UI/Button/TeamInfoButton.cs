using UnityEngine;

namespace UI
{
    public class TeamInfoButton : TitraSims_Button
    {
        private MainMenu mainMenu;

        private void Start() => mainMenu = FindAnyObjectByType<MainMenu>();

        protected override void OnClick()
        {
            if (mainMenu) mainMenu.GoToCredits();
        }
    }
}
