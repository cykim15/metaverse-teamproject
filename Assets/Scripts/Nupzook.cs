using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder.MeshOperations;

public class Nupzook : MonoBehaviour
{
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
    private float attack1Damage;
    [SerializeField]
    private float attack1DamageTolerance;
    [SerializeField]
    private float attack2Damage;
    [SerializeField]
    private float attack2DamageTolerance;
    [SerializeField]
    private float skill1Damage;

    private Player player;
    private NavMeshAgent navMeshAgent;
    [SerializeField]
    private Animator animator;
    private HP hp;

    private void Awake()
    {
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

        yield return new WaitForSeconds(2f);
        lookAtPlayer = true;
        yield return new WaitForSeconds(1f);

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
                yield return new WaitForSeconds(closeCombatCooltime + Random.Range(-closeCombatCooltimeTolerance, closeCombatCooltimeTolerance));

                CheckDistanceAndChangeCombatState();
                if (combatState != CombatState.CloseCombat) continue;

                int random = Random.Range(0, 5);
                if (random < 2) // 40%
                {
                    animator.SetTrigger("attack1");
                }
                else if (random < 4) // 40%
                {
                    animator.SetTrigger("attack2");
                }
                else // 20%
                {
                    animator.SetTrigger("skill1");
                }

            }

            else if (combatState == CombatState.FarCombat)
            {
                yield return new WaitForSeconds(farCombatCooltime + Random.Range(-farCombatCooltimeTolerance, farCombatCooltimeTolerance));

                CheckDistanceAndChangeCombatState();
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
            //animator.applyRootMotion = true;
            animator.SetBool("walk", false);
        }

        if (combatState == CombatState.Move)
        {
            lookAtPlayer = false;
            navMeshAgent.enabled = true;
            //animator.applyRootMotion = false;
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
}
