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
            if (coinUsage != null && coinUsage.activated == false)
            {
                canUse = true;
            }

            Chest chest = coinUsage.GetComponentInParent<Chest>();
            if (chest != null)
            {
                chest.collidingCoin = this;
            }
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (1 << other.gameObject.layer == colliderForCoinLayer)
        {
            Chest chest = coinUsage.GetComponentInParent<Chest>();
            if (chest != null)
            {
                chest.collidingCoin = null;
            }
            coinUsage = null;
            canUse = false;
        }
    }
}
