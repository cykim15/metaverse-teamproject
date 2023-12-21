using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySFX : MonoBehaviour
{
    private float minPitch = 0.9f;
    private float maxPitch = 1.1f;
    private AudioSource target;

    private void Awake()
    {
        target = GetComponent<AudioSource>();
    }

    public void Call(AudioClip clip)
    {
        target.clip = clip;
        target.pitch = Random.Range(minPitch, maxPitch);
        target.Play();
    }
}
