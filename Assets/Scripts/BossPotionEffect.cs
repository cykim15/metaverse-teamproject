using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPotionEffect : MonoBehaviour
{
    [SerializeField]
    private AudioClip audioClipBossPotion;

    void Start()
    {
        GetComponent<PlaySFX>().Call(audioClipBossPotion);
    }
}
