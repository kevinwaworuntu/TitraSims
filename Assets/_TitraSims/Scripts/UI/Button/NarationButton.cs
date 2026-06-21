namespace UI
{
    public class NarationButton : TitraSims_Button
    {
        protected override void OnClick()
        {
            if (UIManager.Instance) UIManager.Instance.ToggleNarationPanel();
        }
    }
}
