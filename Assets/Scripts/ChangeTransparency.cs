using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTransparency : MonoBehaviour
{
    static public void Call (GameObject gameObject, float alpha)
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            Material material = renderer.material;
            Color objectColor = material.color;
            objectColor.a = alpha;
            material.color = objectColor;
        }
    }
}
