using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPViewer : GageViewer
{
    [SerializeField]
    private HP hp;

    private void Awake()
    {
        gage = hp;
        gageName = "Ã¼·Â";
    }
}
