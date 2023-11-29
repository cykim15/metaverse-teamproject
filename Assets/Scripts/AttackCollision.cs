using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollision : MonoBehaviour
{
    [SerializeField]
    private Enemy enemy;

    private void OnEnable()
    {
        StartCoroutine(nameof(AutoDisable));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemy.HitPlayer();
            gameObject.SetActive(false);
        }
    }

    private IEnumerator AutoDisable()
    {
        yield return new WaitForSeconds(0.1f);

        gameObject.SetActive(false);
    }
}
