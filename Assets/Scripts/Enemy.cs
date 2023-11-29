using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.Events;
using System.Runtime.CompilerServices;
using UnityEngine.InputSystem.HID;

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
    private float detectionAngle = 90f;

    [System.Serializable]
    private class Waypoint
    {
        public Transform transform;
        public float waitTime;
    }

    [SerializeField]
    private Waypoint[] waypoints;
    [SerializeField]
    private float waitTimeAfterMissingPlayer;
    [SerializeField]
    private float walkingSpeed;
    [SerializeField]
    private float runningSpeed;
    [SerializeField]
    private float attack1Damage;
    [SerializeField]
    private float attack2Damage;
    [SerializeField]
    private float attack1DamageTolerance;
    [SerializeField]
    private float attack2DamageTolerance;
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
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private LayerMask ignoreLayer;

    [Header("Events")]
    [SerializeField]
    private UnityEvent onAttack1;
    [SerializeField]
    private UnityEvent onAttack2;
    [SerializeField]
    private UnityEvent onGetHit;
    [SerializeField]
    private UnityEvent onDie;

    private NavMeshAgent navMeshAgent;
    private HP enemyHP;

    private bool isFighting = false;

    private bool isAlive = true;

    private float damage;

    private float damageTolerance;

    private int currentWaypoint = 0;

    private bool detected = false;

    private bool patrolling = false;

    private float velocity;

    private bool combatCoroutineEnabled = false;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    

    private float playerEyeHeight = 1.5f;

    private void Awake()
    {
        navMeshAgent = GetComponentInChildren<NavMeshAgent>();
        navMeshAgent.speed = walkingSpeed;
        enemyHP = GetComponent<HP>();
        textHP.enabled = false;
        textName.text = enemyName;

        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }
    private void Update()
    {
        if (isAlive == false) return;

        if (!isFighting)
        {
            /*if (gameObject.tag == "Test")
                Debug.Log($"patrolling : {patrolling}, detected : {detected}");*/

            if (navMeshAgent.enabled == false) navMeshAgent.enabled = true;

            animator.SetFloat("moveSpeed", MappingToAnimatorSpeed(navMeshAgent.velocity.magnitude));

            // �� ã���� �� �÷��̾�� ray�� ���� ã��
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            float angle = Vector3.Angle(transform.forward, (player.transform.position - transform.position).normalized);

            // ������ �þ߰� ���̰ų�, �þ߰� ���̴��� �����Ÿ� �ȿ� ������ ���
            if ((distanceToPlayer < detectionRange && angle < detectionAngle) || distanceToPlayer < fightDistance + 0.3f)
            {
                RaycastHit hit;

                // �÷��̾� �߰�
                if (Physics.Raycast(enemyEyes.position, player.transform.position + new Vector3(0, playerEyeHeight, 0) - enemyEyes.position, out hit, detectionRange, ~ignoreLayer) && hit.collider.gameObject == player) // eye to eye
                {
                    StopAllCoroutines();
                    navMeshAgent.speed = runningSpeed;
                    navMeshAgent.SetDestination(player.transform.position);
                    navMeshAgent.stoppingDistance = fightDistance - 0.2f; // stoppindDistance bias�� 0.2 ���� �����ϴ� �� Ȯ������
                    detected = true;
                    patrolling = false;

                    if (distanceToPlayer < fightDistance + 0.3f)
                    {
                        isFighting = true;
                        textHP.enabled = true;
                        Debug.Log("���� ����");
                        animator.SetBool("onCombat", true);
                    }
                }

                // ������ �þ߰� �ȿ��� �� ���̴� ���
                else
                {
                    detected = false;
                    navMeshAgent.stoppingDistance = 0f; // ���� waypoint�� ������ ���ư��� ����
                }  
            }

            // ������ �þ߰� ���� ���
            else
            {
                detected = false;
                navMeshAgent.stoppingDistance = 0f; // ���� waypoint�� ������ ���ư��� ����
            }

            if (detected == false)
            {
                navMeshAgent.speed = walkingSpeed;
                if (waypoints.Length > 0)
                {
                    // patrolling�� ������ ��θ� ���󰡴����� ����
                    // ��θ� �� ���󰡰� ������ ������ �������� ���
                    if (patrolling == false)
                    {
                        int beforeWaypoint = currentWaypoint - 1;
                        if (beforeWaypoint < 0) { beforeWaypoint = waypoints.Length - 1; }

                        // �÷��̾ �Ѵٰ� �ٽ� ��η� ���ư� ���� ��� �ð��� ���� ������
                        // �÷��̾ �Ѵٰ� ���� ��� = ���� �������� ���� ���� �ִ� ���

                        if (Vector3.Distance(transform.position, waypoints[beforeWaypoint].transform.position) > 0.1f && navMeshAgent.velocity.magnitude < 0.1f)
                        {
                            StartCoroutine(WaitAndMoveToNextWaypoint(waitTimeAfterMissingPlayer));
                            patrolling = true;
                        }

                        else if (Vector3.Distance(transform.position, waypoints[beforeWaypoint].transform.position) < 0.1f)
                        {
                            StartCoroutine(WaitAndMoveToNextWaypoint(waypoints[beforeWaypoint].waitTime));
                            patrolling = true;
                        }
                    }

                    else
                    {
                        // ��θ� ���󰡴ٰ� �����ϸ� �׸� ���󰡰� ���� ������ ����
                        if (Vector3.Distance(transform.position, waypoints[currentWaypoint].transform.position) < 0.1f)
                        {
                            patrolling = false;
                            currentWaypoint++;
                            if (currentWaypoint >= waypoints.Length) { currentWaypoint = 0; }
                        }
                    }
                }

                // �̵� ��ΰ� ���� ���� ���: ���� ��ġ�� ��Ű�� ���� �����ϴ� ��.
                else
                {
                    if (patrolling == false)
                    {
                        if (Vector3.Distance(transform.position, originalPosition) > 0.1f && navMeshAgent.velocity.magnitude < 0.1f)
                        {
                            StartCoroutine(WaitAndMoveToOriginalPoint(waitTimeAfterMissingPlayer));
                            patrolling = true;
                        }

                        else if (Vector3.Distance(transform.position, originalPosition) < 0.1f)
                        {
                            patrolling = true;
                        }
                    }

                    else
                    {
                        if (Quaternion.Angle(transform.rotation, originalRotation) > 0.1f && Vector3.Distance(transform.position, originalPosition) < 0.1f)
                        {
                            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, rotationSpeed * Time.deltaTime);
                        }
                    }
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

            if (combatCoroutineEnabled == false)
            {
                combatCoroutineEnabled = true;
                StartCoroutine(Combat());
            }

            
            RaycastHit hit;
            bool cannotSeePlayer = Physics.Raycast(enemyEyes.position, player.transform.position + new Vector3(0, playerEyeHeight, 0) - enemyEyes.position, out hit, detectionRange, ~ignoreLayer) && hit.collider.gameObject != player;

            // �÷��̾ ���� �Ÿ��� ����ų� �������� ��
            if (Vector3.Distance(transform.position, player.transform.position) > fightDistance + 0.3f || cannotSeePlayer)
            {
                isFighting = false;
                textHP.enabled = false;
                combatCoroutineEnabled = false;
                detected = false;
                animator.SetBool("onCombat", false);
                Debug.Log("���� �簳");
            }
            else
            {
                // �÷��̾� ��� �Ĵٺ���
                Vector3 directionToPlayer = player.transform.position - transform.position;
                directionToPlayer.y = 0f; // y �� ȸ���� ����ϱ� ���� y ���� 0���� ����

                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
    }

    private IEnumerator WaitAndMoveToNextWaypoint(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        navMeshAgent.destination = waypoints[currentWaypoint].transform.position;
    }

    private IEnumerator WaitAndMoveToOriginalPoint(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        navMeshAgent.destination = originalPosition;
    }

    private IEnumerator Combat()
    {
        while (isFighting)
        {
            if (Random.Range(0, 2) == 0)
            {
                damage = attack1Damage;
                damageTolerance = attack1DamageTolerance;
                onAttack1?.Invoke();
            }
            else
            {
                damage = attack2Damage;
                damageTolerance = attack2DamageTolerance;
                onAttack2?.Invoke();
            }

            yield return new WaitForSeconds(cooldownTime + Random.Range(-cooldownTimeTolerance, cooldownTimeTolerance));
        }
    }

    public void GetHit(float amount)
    {
        enemyHP.Decrease(amount);

        if (enemyHP.Current < 1f)
        {
            isAlive = false;
            onDie?.Invoke();
            Destroy(gameObject, 3f);
        }
        else
        {
            onGetHit?.Invoke();
            if (detected == false)
            {
                StartCoroutine(LookAtPlayerSlowly());
            }
        }
    }

    private IEnumerator LookAtPlayerSlowly()
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;
        directionToPlayer.y = 0f; // y �� ȸ���� ����ϱ� ���� y ���� 0���� ����

        navMeshAgent.enabled = false;
        yield return new WaitForSeconds(1f);
        

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);

            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                yield return null;
            }
        }

        navMeshAgent.enabled = true;
    }

    private float MappingToAnimatorSpeed(float speed)
    {
        if (speed >= 0 && speed < walkingSpeed)
        {
            return Mathf.Lerp(0f, 0.5f, speed / walkingSpeed); 
        }
        else if (speed >= walkingSpeed)
        {
            return Mathf.Lerp(0.5f, 1.0f, (speed - walkingSpeed) / (runningSpeed - walkingSpeed));
        }
        else
        {
            return 0f;
        }
    }

    public void HitPlayer()
    {
        player.GetComponent<Player>().GetHit(damage + Random.Range(-damageTolerance, damageTolerance));
    }
}
