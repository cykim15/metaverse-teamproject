using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GageViewer : MonoBehaviour
{
    protected string gageName;
    protected Gage gage;
    [SerializeField]
    private TextMeshProUGUI text;
    [SerializeField]
    private Slider slider;

    protected virtual void Update()
    {
        slider.value = gage.Current / gage.Max;
        text.text = $"{gageName} {(int)gage.Current}/{gage.Max}";
    }
}
