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
    public static List<bool> following = new List<bool>();
    int id;

    [Header("Parameter")]
    public string enemyName;
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
    //[SerializeField]
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
    [SerializeField]
    private GameObject coinSpawnEffectPrefab;
    [SerializeField]
    private GameObject coinPrefab;

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

        id = following.Count;
        following.Add(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    private void Start()
    {
        player = Player.Instance.gameObject;
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

            // 못 찾았을 때 플레이어에게 ray를 쏴서 찾기
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            float angle = Vector3.Angle(transform.forward, (player.transform.position - transform.position).normalized);

            // 범위와 시야각 안이거나, 시야각 밖이더라도 전투거리 안에 들어오는 경우
            if ((distanceToPlayer < detectionRange && angle < detectionAngle) || distanceToPlayer < fightDistance + 0.3f)
            {
                RaycastHit hit;

                // 플레이어 발견
                if (Physics.Raycast(enemyEyes.position, player.transform.position + new Vector3(0, playerEyeHeight, 0) - enemyEyes.position, out hit, detectionRange, ~ignoreLayer) && hit.collider.gameObject == player) // eye to eye
                {
                    StopAllCoroutines();
                    navMeshAgent.speed = runningSpeed;
                    navMeshAgent.SetDestination(player.transform.position);
                    navMeshAgent.stoppingDistance = fightDistance - 0.2f; // stoppindDistance bias가 0.2 정도 존재하는 것 확인했음
                    detected = true;
                    patrolling = false;

                    if (distanceToPlayer < fightDistance + 0.3f)
                    {
                        isFighting = true;
                        textHP.enabled = true;
                        //Debug.Log("전투 시작");
                        animator.SetBool("onCombat", true);
                    }
                    following[id] = true;
                }

                // 범위와 시야각 안에서 안 보이는 경우
                else
                {
                    detected = false;
                    navMeshAgent.stoppingDistance = 0f; // 기존 waypoint로 온전히 돌아가기 위함
                }
            }

            // 범위와 시야각 밖의 경우
            else
            {
                detected = false;
                navMeshAgent.stoppingDistance = 0f; // 기존 waypoint로 온전히 돌아가기 위함
            }

            if (detected == false)
            {
                navMeshAgent.speed = walkingSpeed;
                if (waypoints.Length > 0)
                {
                    // patrolling은 설정된 경로를 따라가는지의 여부
                    // 경로를 안 따라가고 있으면 설정된 도착지로 출발
                    if (patrolling == false)
                    {
                        int beforeWaypoint = currentWaypoint - 1;
                        if (beforeWaypoint < 0) { beforeWaypoint = waypoints.Length - 1; }

                        // 플레이어를 쫓다가 다시 경로로 돌아갈 때는 대기 시간을 따로 설정함
                        // 플레이어를 쫓다가 멈춘 경우 = 이전 도착지에 없고 멈춰 있는 경우

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
                        // 경로를 따라가다가 도착하면 그만 따라가고 다음 도착지 설정
                        if (Vector3.Distance(transform.position, waypoints[currentWaypoint].transform.position) < 0.1f)
                        {
                            patrolling = false;
                            currentWaypoint++;
                            if (currentWaypoint >= waypoints.Length) { currentWaypoint = 0; }
                        }
                    }
                }

                // 이동 경로가 없는 적의 경우: 기존 위치를 지키는 것이 순찰하는 것.
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

        else // 전투
        {
            // 에러 처리, navMeshAgent가 너무 가까이 왔을 때 강제 중단
            if (navMeshAgent.enabled && Vector3.Distance(transform.position, player.transform.position) < fightDistance - 0.2f)
            {
                //Debug.Log("너무 가까이 붙음! 네비게이션 강제 종료");
                navMeshAgent.enabled = false;
            }

            if (combatCoroutineEnabled == false)
            {
                combatCoroutineEnabled = true;
                StartCoroutine(Combat());
            }


            RaycastHit hit;
            bool cannotSeePlayer = Physics.Raycast(enemyEyes.position, player.transform.position + new Vector3(0, playerEyeHeight, 0) - enemyEyes.position, out hit, detectionRange, ~ignoreLayer) && hit.collider.gameObject != player;

            // 플레이어가 전투 거리를 벗어나거나 가려졌을 때
            if (Vector3.Distance(transform.position, player.transform.position) > fightDistance + 0.3f || cannotSeePlayer)
            {
                isFighting = false;
                textHP.enabled = false;
                combatCoroutineEnabled = false;
                detected = false;
                animator.SetBool("onCombat", false);
                //Debug.Log("추적 재개");
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

    private IEnumerator WaitAndMoveToNextWaypoint(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        following[id] = false;
        navMeshAgent.destination = waypoints[currentWaypoint].transform.position;
    }

    private IEnumerator WaitAndMoveToOriginalPoint(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        following[id] = false;
        navMeshAgent.destination = originalPosition;
    }

    private IEnumerator Combat()
    {
        while (isFighting && isAlive)
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

    private IEnumerator BlinkAndDestroy()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        int blinkNums = 4;
        float blinkTime = 0.1f;

        yield return new WaitForSeconds(3f);

        for (int i = 0; i< blinkNums; i++)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }
            yield return new WaitForSeconds(blinkTime);

            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true;
            }
            yield return new WaitForSeconds(blinkTime);
        }

        yield return new WaitForSeconds(0.5f);

        int random = Random.Range(0, 2);
        if (random == 0)
        {
            Instantiate(coinSpawnEffectPrefab, transform.position, transform.rotation);
            Instantiate(coinPrefab, transform.position, transform.rotation);
        }
        gameObject.gameObject.SetActive(false);
        //Destroy(gameObject);
    }

    public void GetHit(float amount)
    {
        enemyHP.Decrease(amount);

        if (enemyHP.Current < 1f)
        {
            isAlive = false;
            following[id] = false;
            onDie?.Invoke();
            StartCoroutine("BlinkAndDestroy");
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
        directionToPlayer.y = 0f; // y 축 회전만 고려하기 위해 y 값을 0으로 설정

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
        float actualDamage = damage + Random.Range(-damageTolerance, damageTolerance);
        List<MeleeWeapon> defendingMeleeWeapons = player.GetComponent<Player>().defendingWeapons;

        if (defendingMeleeWeapons.Count == 0)
        {
            player.GetComponent<Player>().GetHit(actualDamage);
        }

        else
        {
            bool playerCanDefend = false;

            foreach (MeleeWeapon weapon in defendingMeleeWeapons)
            {
                if (weapon.CurrentDurability > 0f)
                {
                    playerCanDefend = true;
                    weapon.DefendEffect();
                    weapon.DecreaseDurability(actualDamage, true);
                }
            }

            if (playerCanDefend == false)
            {
                player.GetComponent<Player>().GetHit(actualDamage);
            }
        }
    }
}
