using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPViewer : MonoBehaviour
{
    [SerializeField]
    private HP hp;
    [SerializeField]
    private TextMeshProUGUI textHP;
    private Slider sliderHP;

    private void Awake()
    {
        sliderHP = GetComponent<Slider>();
    }

    private void Update()
    {
        sliderHP.value = hp.Current / hp.Max;
        textHP.text = $"Ã¼·Â {(int)hp.Current}/{hp.Max}";
    }
}
