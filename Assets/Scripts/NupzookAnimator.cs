using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NupzookAnimator : MonoBehaviour
{
    public UnityEvent onSpawnFire;
    public UnityEvent onDestroyFire;
    public UnityEvent onThrow;
    public UnityEvent onSkill1;

    [SerializeField]
    private GameObject attackCollision;

    [SerializeField]
    private AudioClip audioClipAttack;
    [SerializeField]
    private AudioClip audioClipSkill1;

    private AudioSource audioSource;

    private PlaySFX playSFX;

    private void Awake()
    {
        playSFX = GetComponent<PlaySFX>();
        audioSource = GetComponent<AudioSource>();
    }

    public void OnAttackCollision()
    {
        attackCollision.SetActive(true);
    }

    public void PlayAttackSound()
    {
        playSFX.Call(audioClipAttack);
    }

    public void SpawnFire()
    {
        onSpawnFire?.Invoke();
    }

    public void DestroyFire()
    {
        onDestroyFire?.Invoke();
    }

    public void Throw()
    {
        onThrow?.Invoke();
    }

    public void Skill1()
    {
        onSkill1?.Invoke();
    }

    public void PlaySkill1Sound()
    {
        audioSource.clip = audioClipSkill1;
        audioSource.Play();
    }
}
