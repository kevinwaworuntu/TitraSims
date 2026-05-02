using TMPro;
using UnityEngine;

namespace UI
{
    public class NarationPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        public void SetTitleText(string text) => titleText.text = text;
        public void SetDescriptionText(string text) => descriptionText.text = text;
        public void SetVisibility(bool visible) => gameObject.SetActive(visible);
    }
}