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
    [SerializeField]
    private GameObject normalEffectPrefab;
    [SerializeField]
    private GameObject fixEffectPrefab;
    [SerializeField]
    private AudioClip audioClipNormal;
    [SerializeField]
    private AudioClip audioClipFix;

    private AudioSource audioSource;

    private int hitTime;

    public ActivateWithCoin coinActivate;

    private GameObject effect;

    private void Awake()
    {
        coinActivate.onActivate.AddListener(() => hitTime = 3);
        audioSource = GetComponent<AudioSource>();
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

    public void TryFixWeapon(Vector3 effectPosition)
    {
        hitTime--;

        if (hitTime > 0)
        {
            audioSource.clip = audioClipNormal;
            effect = normalEffectPrefab;
        }
        else
        {
            audioSource.clip = audioClipFix;
            effect = fixEffectPrefab;
            FixWeapon();
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        audioSource.Play();
        Instantiate(effect, effectPosition, Quaternion.identity);
    }
}
