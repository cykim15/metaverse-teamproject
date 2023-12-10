using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anvil : MonoBehaviour
{
    public MeleeWeapon fixingWeapon = null;

    [SerializeField]
    private float durabilityIncrease = 30f;
    [SerializeField]
    private float durabilityIncreaseTolerance = 10f;

    private int hitTime;

    public ActivateWithCoin coinActivate;

    private void Awake()
    {
        coinActivate.onActivate.AddListener(() => hitTime = 3);
    }

    private void OnTriggerEnter(Collider other)
    {
        MeleeWeapon weapon = other.GetComponentInParent<MeleeWeapon>();
        if (weapon != null)
        {
            fixingWeapon = weapon;
        }
    }

    private void FixWeapon()
    {
        float amount = durabilityIncrease + Random.Range(-durabilityIncreaseTolerance, durabilityIncrease);
        fixingWeapon.IncreaseDurability(amount);
        coinActivate.onDeactivate?.Invoke();
        Debug.Log($"{amount} ¼ö¸®");
    }

    public void TryFixWeapon()
    {
        hitTime--;

        if (hitTime == 0)
        {
            FixWeapon();
        }
    }
}
