using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TpGage : MonoBehaviour
{
    [SerializeField]
    private float max = 100;
    private float current;
    [SerializeField]
    private float normalRecovery = 1.0f;
    [SerializeField]
    private float costWeight = 1.0f;

    public float Max => max;
    public float Current
    {
        get 
        {
            return current;

        }
        set
        {
            current = value;
            current = Mathf.Clamp(current, 0, max);
        }
    }  

    private void Awake()
    {
        Current = max;
    }

    private void Update()
    {
        Current += Time.deltaTime * normalRecovery;
    }

    public bool CanTeleport(float distance)
    {
        return Current > costWeight * distance;
    }

    public void ConsumeGage(float distance)
    {
        Current -= costWeight * distance;
    }
}
