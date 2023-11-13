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

    private bool detected = false;

    private bool isFighting = false;

    private bool isAlive = true;

    public float FightDistance => fightDistance;


    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.stoppingDistance = fightDistance - 0.2f; // stoppindDistance bias�� 0.2 ���� �����ϴ� �� Ȯ������
        enemyHP = GetComponent<HP>();
        textHP.enabled = false;
        textName.text = enemyName;
    }
    private void Update()
    {
        if (isAlive == false) return;
        /*
        // �������� ���� ����
        if (!isFighting)
        {
            // �� ã���� �� �÷��̾�� ray�� ���� ã��
            if (!detected)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                if (distanceToPlayer < detectionRange)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, player.transform.position - transform.position, out hit, detectionRange))
                    {
                        if (hit.collider.gameObject == player)
                        {
                            detected = true;
                            Debug.Log("ã�Ҵ�! ���� ����");
                        }
                    }
                }
            }

            // ã���� �� ����
            else
            {
                navMeshAgent.SetDestination(player.transform.position);

                // ���� �Ÿ��� �������� �� ���� ����
                if (Vector3.Distance(transform.position, player.transform.position) < fightDistance)
                {
                    //navMeshAgent.isStopped = true;
                    isFighting = true;
                    textHP.enabled = true;
                    Debug.Log("���� ����");
                }
            }
        }
        
        else
        {
            // ���� �Ÿ��� ����� �� �ٽ� ����
            if (Vector3.Distance(transform.position, player.transform.position) > fightDistance + 0.1f)
            {
                //navMeshAgent.isStopped = false;
                isFighting = false;
                textHP.enabled = false;
                Debug.Log("���� �簳");
            }
        }
        */

        // �������� �ִ� ����
        if (!isFighting)
        {
            if (navMeshAgent.enabled == false) navMeshAgent.enabled = true;

            // �� ã���� �� �÷��̾�� ray�� ���� ã��
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < detectionRange)
            {
                RaycastHit hit;
                if (Physics.Raycast(enemyEyes.position, player.transform.position + new Vector3(0, 1.5f, 0) - enemyEyes.position, out hit) && hit.collider.gameObject == player) // eye to eye
                {
                    //Debug.Log("ã�Ҵ�!");
                    navMeshAgent.SetDestination(player.transform.position);
                }
                else
                {
                    //Debug.Log("���ƴ�!");
                } 

                if (Vector3.Distance(transform.position, player.transform.position) < fightDistance + 0.3f)
                {
                    isFighting = true;
                    textHP.enabled = true;
                    Debug.Log("���� ����");
                }
            }
        }

        else // ����
        {
            // ���� ó��, navMeshAgent�� �ʹ� ������ ���� �� ���� �ߴ�
            if (navMeshAgent.enabled && Vector3.Distance(transform.position, player.transform.position) < fightDistance - 0.2f)
            {
                Debug.Log("�ʹ� ������ ����! �׺���̼� ���� ����");
                navMeshAgent.enabled = false;
            }

            // �÷��̾ ���� �Ÿ��� ����� ��
            if (Vector3.Distance(transform.position, player.transform.position) > fightDistance + 0.3f)
            {
                isFighting = false;
                textHP.enabled = false;
                //navMeshAgent.enabled = true;
                Debug.Log("���� �簳");
            }
            else
            {
                // �÷��̾� ��� �Ĵٺ���
                Vector3 directionToPlayer = player.transform.position - transform.position;
                directionToPlayer.y = 0f; // y �� ȸ���� �����ϱ� ���� y ���� 0���� ����

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
            isAlive = true;
            Destroy(gameObject, 3f);
        }
    }
}