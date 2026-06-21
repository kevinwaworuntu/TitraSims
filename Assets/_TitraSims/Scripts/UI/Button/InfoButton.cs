namespace UI
{
    public class InfoButton : TitraSims_Button
    {
        protected override void OnClick()
        {
            if (UIManager.Instance) UIManager.Instance.ToggleInfoPanel();
        }
    }
}
