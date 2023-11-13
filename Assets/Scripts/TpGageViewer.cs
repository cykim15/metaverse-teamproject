using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TpGageViewer : MonoBehaviour
{
    [SerializeField]
    private TpGage tpGage;
    [SerializeField]
    private TextMeshProUGUI textTpGage;
    private Slider sliderTpGage;

    private void Awake()
    {
        sliderTpGage = GetComponent<Slider>();
    }

    private void Update()
    {
        sliderTpGage.value = tpGage.Current / tpGage.Max;
        textTpGage.text = $"텔레포트 {(int)tpGage.Current}/{tpGage.Max}";
    }
}
