using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionMaterial : MonoBehaviour
{
    private Color newEmissionColor;

    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Material[] newMaterials;

    public bool Enabled => enabled;

    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        originalMaterials = new Material[renderers.Length];
        newMaterials = new Material[renderers.Length];
        newEmissionColor = InteractableManager.Instance.interactableEmissionColor;

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = new Material(renderers[i].material);
            newMaterials[i] = new Material(renderers[i].material);

            newMaterials[i].SetInt("_ReceiveShadows", 0);
            newMaterials[i].EnableKeyword("_EMISSION");
            newMaterials[i].SetColor("_EmissionColor", newEmissionColor);
        }
    }

    public void Enable()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = newMaterials[i];
        }

        MeleeWeapon weapon = GetComponent<MeleeWeapon>();

        if (weapon != null)
        {
            weapon.ChangeBladeTransparency(weapon.CurrentDurability / weapon.MaxDurability);
        }
    }

    public void Disable()
    {
        if (gameObject.activeSelf == true)
        {
            StartCoroutine("WaitAndDisable");
        }
        else
        {
            /*
            gameObject.SetActive(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material = originalMaterials[i];
            }

            MeleeWeapon weapon = GetComponent<MeleeWeapon>();

            if (weapon != null)
            {
                weapon.ChangeBladeTransparency(weapon.CurrentDurability / weapon.MaxDurability);
            }

            gameObject.SetActive(false);*/
        }
        
    }

    private IEnumerator WaitAndDisable()
    {
        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = originalMaterials[i];
        }

        MeleeWeapon weapon = GetComponent<MeleeWeapon>();

        if (weapon != null)
        {
            weapon.ChangeBladeTransparency(weapon.CurrentDurability / weapon.MaxDurability);
        }
    }
}
