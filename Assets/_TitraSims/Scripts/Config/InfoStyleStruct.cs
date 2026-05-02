using System;
using TMPro;
using UnityEngine;

[Serializable]
public struct InfoStyleStruct
{
    public TMP_FontAsset Font;                 
    public float FontSize;                     
    public TextAlignmentOptions Alignment;     
    public bool IsBold;                          
    public float LineSpacing;                  
    public float ParagraphSpacing;       
    
    [Header("Margin")]
    public float MarginLeft;
    public float MarginTop;
    public float MarginRight;
    public float MarginBottom;  
}
