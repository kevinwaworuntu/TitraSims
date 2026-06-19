using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InfoButton : MonoBehaviour
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (button)
            {
                button.onClick.AddListener(OnButtonClick);
            }
        }

        private void OnButtonClick()
        {
            if (UIManager.Instance)
            {
                UIManager.Instance.ToggleInfoPanel();
            }
        }
    }
}
