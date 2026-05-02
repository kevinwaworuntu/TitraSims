using UnityEngine;

[CreateAssetMenu(fileName = "InfoStyleConfig", menuName = "AR Pharma/Create Info Style Config")]
public class InfoStyleConfig : ScriptableObject
{
    [SerializeField] private InfoStyleStruct style;
    public InfoStyleStruct GetStyle() => style;
}
