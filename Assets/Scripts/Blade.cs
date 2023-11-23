using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Blade : MonoBehaviour
{
    [SerializeField]
    private UnityEvent<Collider> onHit;

    private void OnTriggerEnter(Collider other)
    {
        onHit?.Invoke(other);
    }
}
