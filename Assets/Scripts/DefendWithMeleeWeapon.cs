using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DefendWithMeleeWeapon : MonoBehaviour
{
    private XRDirectInteractor interactor;
    [SerializeField]
    private Player player;
    [SerializeField]
    private string[] defendWeaponTags;

    private void Awake()
    {
        interactor = GetComponent<XRDirectInteractor>();
    }

    public void Enable()
    {
        if (interactor.selectTarget != null)
        {
            MeleeWeapon weapon = interactor.selectTarget.GetComponent<MeleeWeapon>();
            if (weapon != null && defendWeaponTags.Contains<string>(weapon.gameObject.tag))
            {
                weapon.defenseMode = true;
                player.defendingWeapons.Add(weapon);
            }
        }
    }

    public void Disable()
    {
        if (interactor.selectTarget != null)
        {
            MeleeWeapon weapon = interactor.selectTarget.GetComponent<MeleeWeapon>();
            if (weapon != null && defendWeaponTags.Contains<string>(weapon.gameObject.tag))
            {
                weapon.defenseMode = false;
                player.defendingWeapons.Remove(weapon);
            }
        }
    }

}
