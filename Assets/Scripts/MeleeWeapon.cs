using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.HID;
using UnityEngine.Events;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Parameter")]
    [SerializeField]
    private float maxCooldownTime;
    [SerializeField]
    private float maxDurability = 100f;
    [SerializeField, Tooltip("�ֵθ��� ���� = a * speed + b * angularSpeed �� ��, a�� ���� �ش��մϴ�.")]
    private float speedWeight;
    [SerializeField, Tooltip("�ֵθ��� ���� = a * speed + b * angularSpeed �� ��, b�� ���� �ش��մϴ�.")]
    private float angularSpeedWeight;
    [SerializeField, Tooltip("�ֵθ��� ���Ⱑ �ش� ���� �Ѿ�� �������� �����˴ϴ�.")]
    private float swingIntensityThreshold;
    [SerializeField, Tooltip("���� �������� �ֵθ��� ���⿡ �ش� ���� ���Ͽ� ���˴ϴ�.")]
    private float damageWeight;
    [SerializeField, Tooltip("������ �� ���� �������� �ش� ���� ���� ����ŭ ������ �������� �پ��ϴ�.")]
    private float durabilityAttackWeight;
    [SerializeField, Tooltip("����� �� ���� �������� �ش� ���� ���� ����ŭ ������ �������� �پ��ϴ�.")]
    private float durabilityDefendWeight;
    [SerializeField]
    private bool throwingWeapon = false;
    [SerializeField]
    private float throwingDamageWeight;
    [SerializeField]
    private UnityEvent onAttack;

    private float swordDamageWeightToGolem = 0.5f;
    private float hammerDamageWeightToGolem = 1.2f;

    private float currentCooldownTime;
    public bool isCooldown;
    public bool defenseMode = false;
    private float currentDurability;

    public float MaxDurability => maxDurability;
    public float CurrentDurability => currentDurability;


    [Header("Reference")]
    //[SerializeField]
    private Player player;
    [SerializeField]
    private Renderer bladeRenderer;
    [SerializeField]
    LayerMask enemyLayer;
    [SerializeField]
    private GameObject cooldownUI;
    [SerializeField]
    private Image imageFill;
    [SerializeField]
    private TextMeshProUGUI textCooldownTime;
    [SerializeField]
    private GameObject effectPrefab;

    private Vector3 originalUILocalPosition;
    private Quaternion originalUILocalRotation;
    private Vector3 oppositeUILocalPosition;
    private Quaternion oppositeUILocalRotation;

    private Vector3 effectPosition;

    [SerializeField]
    private GameObject defendEffectPrefab;
    [SerializeField]
    private Transform defendEffectPoint;
    [SerializeField]
    private AudioClip audioClipDefend;

    private void Awake()
    {
        SetCooldownIs(false);
        originalUILocalPosition = cooldownUI.GetComponent<RectTransform>().localPosition;
        originalUILocalRotation = cooldownUI.GetComponent<RectTransform>().localRotation;
        oppositeUILocalPosition = originalUILocalPosition;
        oppositeUILocalPosition.z *= -1;
        oppositeUILocalRotation = Quaternion.Euler(0f, 180f, 0f) * originalUILocalRotation;

        currentDurability = maxDurability;
    }

    private void Start()
    {
        player = Player.Instance;
        onAttack.AddListener(SpawnEffect);
    }


    public void OnBladeTouched(Collider other, Vector3 bladePosition)
    {
        if (currentDurability == 0f)
        {
            return;
        }

        GameObject collidedObject = other.gameObject;
        int collidedLayer = collidedObject.layer;

        if (1 << collidedLayer != enemyLayer)
        {
            return;
        }

        effectPosition = other.ClosestPointOnBounds(bladePosition);

        // ����
        if (player.grabbingObjects.Contains(gameObject))
        {
            StartCoroutine(nameof(Swing), collidedObject);
        }

        // ��ô
        else
        {
            if (throwingWeapon)
            {
                StartCoroutine(nameof(Swing), collidedObject);
            }
        }
    }


    private void StartCooldownTime()
    {
        if (isCooldown == true)
        {
            return;
        }

        StartCoroutine(nameof(OnCooldownTime), maxCooldownTime);
    }

    private IEnumerator OnCooldownTime(float maxCooldownTime)
    {
        currentCooldownTime = maxCooldownTime;

        SetCooldownIs(true);

        while (currentCooldownTime > 0)
        {
            // ��� �� ���̴� �鿡 ��Ÿ���� ��Ÿ������
            Vector3 cameraPosition = MasterController.Instance.Rig.cameraGameObject.transform.position;
            RaycastHit hit;
            if (Physics.Raycast(cameraPosition, transform.position - cameraPosition, out hit, Mathf.Infinity, 1 << gameObject.layer))
            {
                if (transform.InverseTransformPoint(hit.point).z > 0)
                {
                    cooldownUI.GetComponent<RectTransform>().localPosition = oppositeUILocalPosition;
                    cooldownUI.GetComponent<RectTransform>().localRotation = oppositeUILocalRotation;
                }
                else
                {
                    cooldownUI.GetComponent<RectTransform>().localPosition = originalUILocalPosition;
                    cooldownUI.GetComponent<RectTransform>().localRotation = originalUILocalRotation;
                }
            }

            imageFill.fillAmount = currentCooldownTime / maxCooldownTime;

            textCooldownTime.text = currentCooldownTime.ToString("F1");
            currentCooldownTime -= Time.deltaTime;
            yield return null;
        }

        SetCooldownIs(false);
    }

    private void SetCooldownIs(bool boolean)
    {
        isCooldown = boolean;
        cooldownUI.SetActive(boolean);
    }

    private IEnumerator Swing(GameObject targetObject)
    {
        Vector3 position1 = transform.position;
        Quaternion rotation1 = transform.rotation;

        yield return null; // �� ������ ����

        Vector3 position2 = transform.position;
        Quaternion rotation2 = transform.rotation;

        float velocity = Vector3.Distance(position1, position2) / Time.deltaTime;
        float angularVelocity = Quaternion.Angle(rotation1, rotation2) / Time.deltaTime;

        float swingIntensity = speedWeight * velocity + angularSpeedWeight * angularVelocity;
        //Debug.Log($"�ӵ�: {velocity}, ���ӵ�: {angularVelocity}, ���� ����: {swingIntensity}");

        if (swingIntensity >= swingIntensityThreshold)
        {
            if (isCooldown == false)
            {
                float damage;

                if (throwingWeapon && player.grabbingObjects.Contains(gameObject) == false)
                {
                    damage = throwingDamageWeight * swingIntensity;
                }
                else
                {
                    damage = damageWeight * swingIntensity;
                }

                Enemy enemy = targetObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    if (gameObject.tag == "Sword" && enemy.enemyName == "��") damage *= swordDamageWeightToGolem;
                    if (gameObject.tag == "Hammer" && enemy.enemyName == "��") damage *= hammerDamageWeightToGolem;

                    enemy.GetHit(damage);   
                }

                Nupzook boss = targetObject.GetComponent<Nupzook>();
                if (boss != null)
                {
                    boss.GetHit(damage);
                }
                DecreaseDurability(damage, false);
                onAttack?.Invoke();
                StartCooldownTime();

            }
            else
            {
                //Debug.Log($"���� ���ݱ��� {currentCooldownTime.ToString("F2")}�� ����");
            }
        }
    }

    public void ChangeBladeTransparency(float alpha)
    {
        Material material = bladeRenderer.material;
        Color objectColor = material.color;
        objectColor.a = alpha;
        material.color = objectColor;
    }

    public void DecreaseDurability(float damage, bool isDefense)
    {
        float weight = isDefense ? durabilityDefendWeight : durabilityAttackWeight;
        Debug.Log($"������ {damage * weight} ����");
        currentDurability -= damage * weight;

        if (currentDurability < 0)
        {
            currentDurability = 0;
        }

        ChangeBladeTransparency(currentDurability / maxDurability);
    }

    public void IncreaseDurability(float amount)
    {
        currentDurability += amount;

        if (currentDurability > maxDurability)
        {
            currentDurability = maxDurability;
        }

        ChangeBladeTransparency(currentDurability / maxDurability);
    }

    private void SpawnEffect()
    {
        Instantiate(effectPrefab, effectPosition, Quaternion.identity);
    }

    public void DefendEffect()
    {
        Instantiate(defendEffectPrefab, defendEffectPoint);
        GetComponent<PlaySFX>().Call(audioClipDefend);
    }
}
