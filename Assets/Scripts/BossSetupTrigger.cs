using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossSetupTrigger : MonoBehaviour
{
    public UnityEvent onPlayerEntered;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Player.Instance.gameObject)
        {
            onPlayerEntered?.Invoke();
            Destroy(gameObject);
        }
    }
}
