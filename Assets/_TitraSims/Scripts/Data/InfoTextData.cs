using UnityEngine;
using System;

namespace Data
{
    [CreateAssetMenu(fileName = "InfoTextData", menuName = "AR Pharma/Create Info Text Data", order = 0)]
    public class InfoTextData : ScriptableObject
    {
        [Serializable]
        private struct InfoTextStruct
        {
            public string Key; // Known Issue : Slow for lookup
            [TextArea(1,10)] public string Value;
        }
        [SerializeField] private InfoTextStruct[] textData;

        public bool TryGetTextDataByKey(string key, out string value)
        {
            foreach (InfoTextStruct element in textData)
            {
                if (element.Key == key)
                {
                    value = element.Value;
                    return true;
                }
            }
            value = null;
            return false;
        }
    }
}