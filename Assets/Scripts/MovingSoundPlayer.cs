using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSoundPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private Player player;

    [SerializeField]
    private AudioClip audioClipWalk;
    [SerializeField]
    private AudioClip audioClipRun;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        player = Player.Instance;
        StartCoroutine("myUpdate");
    }

    private IEnumerator myUpdate()
    {
        while (true)
        {
            Vector3 position1 = transform.position;

            yield return null; // 한 프레임 쉬기

            Vector3 position2 = transform.position;

            float velocity = Vector3.Distance(position1, position2) / Time.deltaTime;

            if (velocity >= player.RunningSpeed - 0.1f)
            {
                audioSource.clip = audioClipRun;
                if (audioSource.isPlaying == false)
                {
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            else if (velocity > player.WalkingSpeed - 0.1f)
            {
                audioSource.clip = audioClipWalk;
                if (audioSource.isPlaying == false)
                {
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            else if (velocity < 0.1f)
            {
                if (audioSource.isPlaying == true)
                {
                    audioSource.Stop();
                }
            }

            yield return null;
        }
    }
}
