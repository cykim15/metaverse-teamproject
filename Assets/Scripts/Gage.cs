using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gage: MonoBehaviour
{
    [SerializeField]
    private float max;
    private float current;

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

    public void Increase(float value) { Current += value; }
    public void Decrease(float value)
    {
        Current -= value;
    }
}
