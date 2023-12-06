using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaminaViewer : GageViewer
{
    [SerializeField]
    private Stamina stamina;
    [SerializeField]
    private Image fill;
    [SerializeField]
    private Color underRunThresholdColor;

    private Color originalColor;

    private void Awake()
    {
        gage = stamina;
        gageName = "스태미나";
        originalColor = fill.color;
    }

    protected override void Update()
    {
        base.Update();
        if (stamina.Current < stamina.RunThreshold)
        {
            fill.color = underRunThresholdColor;
        }
        else
        {
            fill.color = originalColor;
        }
    }
}
