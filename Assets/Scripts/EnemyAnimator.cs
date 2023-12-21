using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [SerializeField]
    private GameObject attackCollision;

    [SerializeField]
    private AudioClip audioClipAttack1;
    [SerializeField]
    private AudioClip audioClipAttack2;
    [SerializeField]
    private AudioClip audioClipHit;
    [SerializeField]
    private AudioClip audioClipDie;

    private PlaySFX playSFX;

    private void Awake()
    {
        playSFX = GetComponent<PlaySFX>();
    }

    public void OnAttackCollision()
    {
        attackCollision.SetActive(true);
    }

    public void PlayAttack1Sound()
    {
        playSFX.Call(audioClipAttack1);
    }

    public void PlayAttack2Sound()
    {
        playSFX.Call(audioClipAttack2);
    }

    public void PlayHitSound()
    {
        playSFX.Call(audioClipHit);
    }

    public void PlayDieSound()
    {
        playSFX.Call(audioClipDie);
    }
}
