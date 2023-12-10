using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField]
    private LayerMask colliderForCoinLayer;

    private ActivateWithCoin coinUsage = null;
    private bool canUse = false;

    public void CheckUse()
    {
        if (canUse)
        {
            coinUsage.onActivate?.Invoke();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (1 << other.gameObject.layer == colliderForCoinLayer)
        {
            coinUsage = other.GetComponent<ActivateWithCoin>();
            canUse = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (1 << other.gameObject.layer == colliderForCoinLayer)
        {
            coinUsage = null;
            canUse = false;
        }
    }
}
