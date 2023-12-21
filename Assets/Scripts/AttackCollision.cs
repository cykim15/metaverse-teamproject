using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollision : MonoBehaviour
{
    [SerializeField]
    private Enemy enemy;
    [SerializeField]
    private Nupzook boss;

    private void OnEnable()
    {
        StartCoroutine(nameof(AutoDisable));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemy?.HitPlayer();
            boss?.HitPlayer();
            gameObject.SetActive(false);
        }
    }

    private IEnumerator AutoDisable()
    {
        yield return new WaitForSeconds(0.1f);

        gameObject.SetActive(false);
    }
}
