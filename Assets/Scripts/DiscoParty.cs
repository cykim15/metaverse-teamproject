using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoParty : MonoBehaviour
{
    [SerializeField]
    private float hueMin = 0f;
    [SerializeField]
    private float hueMax = 1f;
    [SerializeField]
    private float saturationMin = 0.7f;
    [SerializeField]
    private float saturationMax = 1f;
    [SerializeField]
    private float valueMin = 0.7f;
    [SerializeField]
    private float valueMax = 1f;
    [SerializeField]
    private float lightTime;
    [SerializeField]
    private float noLightTime;
    [SerializeField]
    private float timeTolerance;
    [SerializeField]
    private Light[] lights;

    private Color GetRandomColor()
    {
        return Random.ColorHSV(hueMin, hueMax, saturationMin, saturationMax, valueMin, valueMax);
    }

    public void Call()
    {
        foreach (Light light in lights)
        {
            StartCoroutine("PartyTonight", light);
        }
    }

    private IEnumerator PartyTonight(Light light)
    {
        while (true)
        {
            light.color = GetRandomColor();

            yield return new WaitForSeconds(lightTime + Random.Range(-timeTolerance, timeTolerance));

            light.color = Color.black;

            yield return new WaitForSeconds(noLightTime + Random.Range(-timeTolerance, timeTolerance));
        }
    }


}
