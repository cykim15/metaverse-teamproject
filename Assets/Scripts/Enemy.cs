using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class Enemy : MonoBehaviour
{
    [Header("Parameter")]
    [SerializeField]
    private string enemyName;
    [SerializeField]
    private float fightDistance;
    [SerializeField]
    private float detectionRange;
    [SerializeField]
    private List<Transform> movePoints;
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float damage;
    [SerializeField]
    private float cooldownTime;
    [SerializeField]
    private float cooldownTimeTolerance;
    [SerializeField]
    private float rotationSpeed = 5f;

    [Header("Reference")]
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private Transform enemyEyes;
    [SerializeField]
    private TextMeshProUGUI textName;
    [SerializeField]
    private TextMeshProUGUI textHP;

    private NavMeshAgent navMeshAgent;
    private HP enemyHP;

    private bool isFighting = false;

    private bool isAlive = true;

    public float FightDistance => fightDistance;

    public Animator animator;


    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.stoppingDistance = fightDistance - 0.2f; // stoppindDistance bias가 0.2 정도 존재하는 것 확인했음
        enemyHP = GetComponent<HP>();
        textHP.enabled = false;
        textName.text = enemyName;
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        if (isAlive == false) return;

        if (!isFighting)
        {
            if (navMeshAgent.enabled == false) navMeshAgent.enabled = true;

            // 못 찾았을 때 플레이어에게 ray를 쏴서 찾기
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < detectionRange)
            {
                RaycastHit hit;
                if (Physics.Raycast(enemyEyes.position, player.transform.position + new Vector3(0, 1.5f, 0) - enemyEyes.position, out hit) && hit.collider.gameObject == player) // eye to eye
                {
                    //Debug.Log("찾았다!");
                    animator.SetBool("Run", true);
                    navMeshAgent.SetDestination(player.transform.position);
                }
                else
                {
                    //Debug.Log("놓쳤다!");
                    animator.SetBool("Run", false);
                } 

                if (Vector3.Distance(transform.position, player.transform.position) < fightDistance + 0.3f)
                {
                    animator.SetBool("Attack", true);
                    isFighting = true;
                    textHP.enabled = true;
                    Debug.Log("전투 시작"); 
                }
            }
        }

        else // 전투
        {
            // 에러 처리, navMeshAgent가 너무 가까이 왔을 때 강제 중단
            if (navMeshAgent.enabled && Vector3.Distance(transform.position, player.transform.position) < fightDistance - 0.2f)
            {
                Debug.Log("너무 가까이 붙음! 네비게이션 강제 종료");
                navMeshAgent.enabled = false;
            }

            // 플레이어가 전투 거리를 벗어났을 때
            if (Vector3.Distance(transform.position, player.transform.position) > fightDistance + 0.3f)
            {
                animator.SetBool("Attack", false);
                isFighting = false;
                textHP.enabled = false;
                //navMeshAgent.enabled = true;
                Debug.Log("추적 재개");
            }
            else
            {
                // 플레이어 계속 쳐다보기
                Vector3 directionToPlayer = player.transform.position - transform.position;
                directionToPlayer.y = 0f; // y 축 회전만 고려하기 위해 y 값을 0으로 설정

                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
    }

    public void CheckAlive()
    {
        if (enemyHP.Current < 1f)
        {
            isAlive = false;
            animator.SetTrigger("Die");
            Destroy(gameObject, 3f);
        }
    }

    public void Damaged(float damage)
    {
        enemyHP.DecreaseHP(damage);
        animator.SetTrigger("Hit");
        CheckAlive();
    }
}
