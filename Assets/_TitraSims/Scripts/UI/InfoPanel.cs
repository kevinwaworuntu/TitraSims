using TMPro;
using UnityEngine;

namespace UI
{
    public class InfoPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI TitleText;
        [SerializeField] private TextMeshProUGUI DescriptionText;
    
        public void SetTitleText(string text) => TitleText.text = text;
        public void SetDescriptionText(string text) => DescriptionText.text = text;
    }
}
