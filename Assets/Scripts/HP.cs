using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HP : MonoBehaviour
{
    [SerializeField]
    private float max = 100;
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

    public Animator animator;

    private void Awake()
    {
        Current = max;
        animator = GetComponent<Animator>();
    }

    public void IncreaseHP(float value) { Current += value; }
    public void DecreaseHP(float value)
    { 
        Current -= value;
    }
}