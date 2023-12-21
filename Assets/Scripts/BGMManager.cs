using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;
    public enum Bgm { None, Normal, Detected, Boss, Disco}
    private Bgm currentBgm;

    [SerializeField]
    private AudioClip audioClipNormalBGM;
    [SerializeField]
    private AudioClip audioClipDetectedBGM;
    [SerializeField]
    private AudioClip audioClipBossBGM;
    [SerializeField]
    private AudioClip audioClipDiscoBGM;

    [HideInInspector]
    public AudioSource audioSource;

    private bool bossRoomUnlocked = false;

    private float normalTime;

    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;

        ChangeBGM(Bgm.Normal);
    }

    private void Update()
    {
        if (bossRoomUnlocked == false)
        {
            bool someEnemyIsFollowing = false;
            foreach (bool following in Enemy.following)
            {
                someEnemyIsFollowing = someEnemyIsFollowing || following;
            }
            if (someEnemyIsFollowing == true)
            {
                ChangeBGM(Bgm.Detected);
            }
            else
            {
                ChangeBGM(Bgm.Normal);
            }
        }
    }

    public void BossTrigger()
    {
        bossRoomUnlocked = true;
        ChangeBGM(Bgm.Boss);
    }

    public void ChangeBGM(Bgm bgm)
    {
        if (currentBgm == bgm) return;

        if (currentBgm == Bgm.Normal) normalTime = audioSource.time;

        currentBgm = bgm;

        if (currentBgm == Bgm.None)
        {
            audioSource.Stop();
        }
        else
        {
            audioSource.clip = GetAudioClipByBgm(bgm);

            if (bgm == Bgm.Normal) 
            {
                audioSource.time = normalTime;
                StartCoroutine(FadeIn());
            }

            else
            {
                StopAllCoroutines();
                audioSource.time = 0f;
                audioSource.Play();
            }
        }
    }

    private AudioClip GetAudioClipByBgm(Bgm bgm)
    {
        switch (bgm)
        {
            case Bgm.Normal:
                return audioClipNormalBGM;
            case Bgm.Detected:
                return audioClipDetectedBGM;
            case Bgm.Boss:
                return audioClipBossBGM;
            case Bgm.Disco:
                return audioClipDiscoBGM;
            default:
                return null;
        }
    }

    private IEnumerator FadeIn(float duration = 1.0f)
    {
        float startVolume = 0.0f;
        float currentTime = 0.0f;

        audioSource.Play();

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 1, currentTime / duration);
            yield return null;
        }

        audioSource.volume = 1;
    }


}
