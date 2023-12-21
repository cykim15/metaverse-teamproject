using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActivateWithCoin : MonoBehaviour
{
    public bool activated = false;
    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    [SerializeField]
    private AudioClip audioClipActivate;
    
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        onActivate.AddListener(() => activated = true);
        onActivate.AddListener(PlayActivateSound);
        onDeactivate.AddListener(() => activated = false);
    }

    private void PlayActivateSound()
    {
        audioSource.clip = audioClipActivate;
        audioSource.Play();
    }
}
