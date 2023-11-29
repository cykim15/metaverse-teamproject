using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaminaViewer : GageViewer
{
    [SerializeField]
    private Stamina stamina;

    private void Awake()
    {
        gage = stamina;
        gageName = "스태미나";
    }
}
