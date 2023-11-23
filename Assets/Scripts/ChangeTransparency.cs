using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTransparency : MonoBehaviour
{
    private Material material;

    private void Awake()
    {
        Renderer myRenderer = GetComponent<Renderer>();
        material = myRenderer.material;

    }

    public void Call(float alphaValue)
    {
        Color objectColor = material.color;
        objectColor.a = alphaValue;
        material.color = objectColor;
    }
}
