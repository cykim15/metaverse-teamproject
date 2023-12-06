using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DefendWithMeleeWeapon : MonoBehaviour
{
    private XRDirectInteractor interactor;

    private void Awake()
    {
        interactor = GetComponent<XRDirectInteractor>();
    }

    public void Enable()
    {
        if (interactor != null)
        {
            MeleeWeapon weapon = interactor.GetComponent<MeleeWeapon>();
            if (weapon != null)
            {
                weapon.defenseMode = true;
            }
        }
    }

    public void Disable()
    {
        if (interactor != null)
        {
            MeleeWeapon weapon = interactor.GetComponent<MeleeWeapon>();
            if (weapon != null)
            {
                weapon.defenseMode = false;
            }
        }
    }
}
