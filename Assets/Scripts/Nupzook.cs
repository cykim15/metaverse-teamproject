using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.ProBuilder.MeshOperations;
using static UnityEngine.GraphicsBuffer;

public class Nupzook : MonoBehaviour
{
    public static Nupzook Instance;

    private enum CombatState { None, Move, CloseCombat, FarCombat }
    private CombatState combatState = CombatState.None;

    private bool lookAtPlayer = false;

    [SerializeField]
    private float rotationSpeed = 3f;

    [SerializeField]
    private float moveSpeed = 1f;
    [SerializeField]
    private float closeCombatDistance = 1f;
    [SerializeField]
    private float farCombatDistance = 5f;
    [SerializeField]
    private float closeCombatCooltime = 3f;
    [SerializeField]
    private float closeCombatCooltimeTolerance = 1f;
    [SerializeField]
    private float farCombatCooltime = 3f;
    [SerializeField]
    private float farCombatCooltimeTolerance = 1f;
    [SerializeField]
    private float attackDamage;
    [SerializeField]
    private float attackDamageTolerance;
    [SerializeField]
    private float skill1Damage;
    [SerializeField]
    private float skill1Radius;
    [SerializeField]
    private float throwDamage;
    [SerializeField]
    private float throwDamageTolerance;
    [SerializeField]
    private float skill2Damage;
    [SerializeField]
    private GameObject fire;
    [SerializeField]
    private float fireRange;
    [SerializeField]
    private LayerMask ignoreLayer;

    [SerializeField]
    private Transform rightHandTransform;
    [SerializeField]
    private GameObject potion1;
    [SerializeField]
    private GameObject potion2;
    [SerializeField]
    private GameObject skill1EffectPrefab;
    [SerializeField]
    private Transform skill1EffectSpawnTransform;

    private Player player;
    private NavMeshAgent navMeshAgent;

    public Animator animator;
    private HP hp;

    [SerializeField]
    private UnityEvent onCombatStart;

    private void Awake()
    {
        Instance = this;
        navMeshAgent = GetComponent<NavMeshAgent>();
        hp = GetComponent<HP>();
        navMeshAgent.enabled = false; 
    }

    private void Start()
    {
        player = Player.Instance;
    }

    // �÷��̾ ���� �濡 ������ �� ȣ��
    public void Setup()
    {
        StartCoroutine("BeforeCombat");
    }

    // Update �Լ��� �÷��̾� �Ĵٺ��� �뵵�θ� Ȱ��
    private void Update()
    {
        if (combatState == CombatState.CloseCombat || combatState == CombatState.FarCombat)
        {
            lookAtPlayer = animator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
        }
        if (lookAtPlayer)
        {
            Vector3 directionToPlayer = player.transform.position - transform.position;
            directionToPlayer.y = 0f; // y �� ȸ���� ����ϱ� ���� y ���� 0���� ����

            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private IEnumerator BeforeCombat()
    {
        yield return new WaitForSeconds(3f);

        animator.SetTrigger("standup");

        yield return new WaitForSeconds(1f);
        BGMManager.Instance.ChangeBGM(BGMManager.Bgm.Disco);
        yield return new WaitForSeconds(1f);
        lookAtPlayer = true;
        yield return new WaitForSeconds(1f);

        onCombatStart?.Invoke();
        ChangeCombatState(CombatState.Move);
        StartCoroutine("Combat");
    }    

    private IEnumerator Combat()
    {
        while (true)
        {
            if (combatState == CombatState.Move)
            {
                navMeshAgent.speed = animator.GetCurrentAnimatorStateInfo(0).IsName("Walking") ? moveSpeed : 0f;
                navMeshAgent.SetDestination(player.transform.position);
                CheckDistanceAndChangeCombatState();
            }

            else if (combatState == CombatState.CloseCombat)
            {
                float time = 0f;
                float targetTime = closeCombatCooltime + Random.Range(-closeCombatCooltimeTolerance, closeCombatCooltimeTolerance);

                while (time < targetTime)
                {
                    time += Time.deltaTime;
                    CheckDistanceAndChangeCombatState();
                    if (combatState != CombatState.CloseCombat) break;
                    yield return null;
                }

                if (combatState != CombatState.CloseCombat) continue;

                int random = Random.Range(0, 5);
                if (random < 2) // 40%
                {
                    StartCoroutine("RotateY", 45);
                    animator.SetTrigger("attack1");
                }
                else if (random < 4) // 40%
                {
                    StartCoroutine("RotateY", 80);
                    animator.SetTrigger("attack2");
                }
                else // 20%
                {
                    animator.SetTrigger("skill1");
                }

            }

            else if (combatState == CombatState.FarCombat)
            {
                // ���� �ð� ����ϰ�, ���� �ð� + ��Ÿ���� �ð��� �� ������ �ݺ���: state üũ
                float time = 0f;
                float targetTime = farCombatCooltime + Random.Range(-farCombatCooltimeTolerance, farCombatCooltimeTolerance);

                while (time < targetTime)
                {
                    time += Time.deltaTime;
                    CheckDistanceAndChangeCombatState();
                    if (combatState != CombatState.FarCombat) break;
                    yield return null;
                }

                if (combatState != CombatState.FarCombat) continue;

                int random = Random.Range(0, 5);
                if (random < 2) // 40%
                {
                    animator.SetTrigger("throw1");
                }
                else if (random < 4) // 40%
                {
                    animator.SetTrigger("throw2");
                }
                else // 20%
                {
                    StartCoroutine("RotateY", 7);
                    animator.SetTrigger("skill2");
                }
            }
            

            yield return null;
        }   
    }

    private void ChangeCombatState(CombatState combatState)
    {
        if (this.combatState == combatState) return;

        if (this.combatState == CombatState.Move)
        { 
            navMeshAgent.enabled = false;
            animator.SetBool("walk", false);
        }

        if (combatState == CombatState.Move)
        {
            lookAtPlayer = false;
            navMeshAgent.enabled = true;
            animator.SetBool("walk", true);
            this.combatState = CombatState.Move;

        }
        else if (combatState == CombatState.CloseCombat)
        {
            this.combatState = CombatState.CloseCombat;

        }
        else if (combatState == CombatState.FarCombat)
        {
            this.combatState = CombatState.FarCombat;
        }
    }

    private void CheckDistanceAndChangeCombatState()
    {
        if (DistanceToPlayer() < closeCombatDistance)
        {
            ChangeCombatState(CombatState.CloseCombat);
        }
        else if (DistanceToPlayer() < farCombatDistance)
        {
            ChangeCombatState(CombatState.FarCombat);
        }
        else
        {
            ChangeCombatState(CombatState.Move);
        }
    }

    private float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.transform.position);
    }


    // ������ Ʋ���� �ִϸ��̼� ������..
    private IEnumerator RotateY(float amount)
    {
        Vector3 currentRotation = transform.rotation.eulerAngles;
        Vector3 targetRotation = new Vector3(currentRotation.x, currentRotation.y + amount, currentRotation.z);

        while (Quaternion.Angle(transform.rotation, Quaternion.Euler(targetRotation)) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(targetRotation), rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void SpawnFire()
    {
        ParticleSystem[] fireParticleSystems = fire.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem particleSystem in fireParticleSystems)
        {
            particleSystem.Play();
            StartCoroutine("FireDamage");
        }
    }

    public void DestroyFire()
    {
        ParticleSystem[] fireParticleSystems = fire.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem particleSystem in fireParticleSystems)
        {
            particleSystem.Stop();
            StopCoroutine("FireDamage");
        }
    }

    public void Throw()
    {
        float damage = throwDamage + Random.Range(-throwDamageTolerance, throwDamageTolerance);

        GameObject potion;
        if (Random.Range(0, 2) == 0) potion = potion1;
        else potion = potion2;

        Instantiate(potion, rightHandTransform.position, rightHandTransform.rotation);
        potion.GetComponent<BossPotion>().damage = damage;
    }

    private IEnumerator FireDamage()
    {
        while (true)
        {
            RaycastHit hit;
            if (Physics.Raycast(fire.transform.position, fire.transform.forward, out hit, fireRange, ~ignoreLayer) && hit.collider.gameObject == player.gameObject)
            {
                yield return new WaitForSeconds(0.5f);
                player.GetHit(skill2Damage); 
            }
            yield return null;
        }
    }

    public void HitPlayer()
    {
        float actualDamage = attackDamage + Random.Range(-attackDamageTolerance, attackDamageTolerance);
        List<MeleeWeapon> defendingMeleeWeapons = player.GetComponent<Player>().defendingWeapons;

        if (defendingMeleeWeapons.Count == 0)
        {
            player.GetHit(actualDamage);
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
                player.GetHit(actualDamage);
            }
        }
    }

    public void GetHit(float amount)
    {
        hp.Decrease(amount);
        DestroyFire();

        if (hp.Current < 1f)
        {
            animator.applyRootMotion = true;
            animator.SetTrigger("die");
            StopAllCoroutines();
            lookAtPlayer = false;

        }
        else
        {
            animator.SetTrigger("gethit");
        }
    }

    public void Skill1()
    {
        Instantiate(skill1EffectPrefab, skill1EffectSpawnTransform.position, Quaternion.identity);
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer < skill1Radius)
        {
            player.GetHit(skill1Damage);
        }
    }
}
