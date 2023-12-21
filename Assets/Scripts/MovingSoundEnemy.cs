using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSoundEnemy : MonoBehaviour
{
    private AudioSource audioSource;
    private Player player;

    [SerializeField]
    private AudioClip audioClipMove;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        StartCoroutine("myUpdate");
    }

    private IEnumerator myUpdate()
    {
        yield return null;

        while (true)
        {
            Vector3 position1 = transform.position;

            yield return null; // 한 프레임 쉬기

            Vector3 position2 = transform.position;

            float velocity = Vector3.Distance(position1, position2) / Time.deltaTime;

            if (velocity > 0.1f)
            {
                audioSource.clip = audioClipMove;
                if (audioSource.isPlaying == false)
                {
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            else
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
