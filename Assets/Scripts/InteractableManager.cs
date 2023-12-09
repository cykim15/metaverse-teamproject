using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableManager : MonoBehaviour
{
    public static InteractableManager Instance;

    public Color interactableEmissionColor;

    private void Awake()
    {
        Instance = this;
    }
}
