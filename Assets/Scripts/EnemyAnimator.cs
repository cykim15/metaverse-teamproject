using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [SerializeField]
    private GameObject attackCollision;

    public void OnAttackCollision()
    {
        attackCollision.SetActive(true);
    }
}
