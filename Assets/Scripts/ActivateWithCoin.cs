using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActivateWithCoin : MonoBehaviour
{
    public bool activated = false;
    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    private void Awake()
    {
        onActivate.AddListener(() => activated = true);
        onDeactivate.AddListener(() => activated = false);
    }
}
