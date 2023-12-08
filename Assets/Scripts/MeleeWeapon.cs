using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.HID;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Parameter")]
    [SerializeField]
    private float maxCooldownTime;
    [SerializeField]
    private float maxDurability = 100f;
    [SerializeField, Tooltip("휘두르는 세기 = a * speed + b * angularSpeed 일 때, a의 값에 해당합니다.")]
    private float speedWeight;
    [SerializeField, Tooltip("휘두르는 세기 = a * speed + b * angularSpeed 일 때, b의 값에 해당합니다.")]
    private float angularSpeedWeight;
    [SerializeField, Tooltip("휘두르는 세기가 해당 값을 넘어야 공격으로 판정됩니다.")]
    private float swingIntensityThreshold;
    [SerializeField, Tooltip("실제 데미지는 휘두르는 세기에 해당 값을 곱하여 계산됩니다.")]
    private float damageWeight;
    [SerializeField, Tooltip("공격할 때 가한 데미지에 해당 값을 곱한 값만큼 무기의 내구도가 줄어듭니다.")]
    private float durabilityAttackWeight;
    [SerializeField, Tooltip("방어할 때 받은 데미지에 해당 값을 곱한 값만큼 무기의 내구도가 줄어듭니다.")]
    private float durabilityDefendWeight;
    [SerializeField]
    private bool throwingWeapon = false;
    [SerializeField]
    private float throwingDamageWeight;

    private float currentCooldownTime;
    public bool isCooldown;
    public bool defenseMode = false;
    private float currentDurability;

    public float MaxDurability => maxDurability;
    public float CurrentDurability => currentDurability;


    [Header("Reference")]
    [SerializeField]
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

    private Vector3 originalUILocalPosition;
    private Quaternion originalUILocalRotation;
    private Vector3 oppositeUILocalPosition;
    private Quaternion oppositeUILocalRotation;



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


    public void OnBladeTouched(Collider other)
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

        // 근접
        if (player.grabbingObjects.Contains(gameObject))
        {
            StartCoroutine(nameof(Swing), collidedObject);
        }

        // 투척
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
            // 양면 중 보이는 면에 쿨타임이 나타나도록
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

        yield return null; // 한 프레임 쉬기

        Vector3 position2 = transform.position;
        Quaternion rotation2 = transform.rotation;

        float velocity = Vector3.Distance(position1, position2) / Time.deltaTime;
        float angularVelocity = Quaternion.Angle(rotation1, rotation2) / Time.deltaTime;

        float swingIntensity = speedWeight * velocity + angularSpeedWeight * angularVelocity;
        Debug.Log($"속도: {velocity}, 각속도: {angularVelocity}, 계산된 세기: {swingIntensity}");

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

                targetObject.GetComponent<Enemy>().GetHit(damage);
                //Debug.Log($"적에게 {damage.ToString("F2")}의 데미지 입힘");
                DecreaseDurability(damage, false);
                StartCooldownTime();
            }
            else
            {
                //Debug.Log($"다음 공격까지 {currentCooldownTime.ToString("F2")}초 남음");
            }
        }
    }

    private void ChangeBladeTransparency(float alpha)
    {
        Material material = bladeRenderer.material;
        Color objectColor = material.color;
        objectColor.a = alpha;
        material.color = objectColor;
    }

    public void DecreaseDurability(float damage, bool isDefense)
    {
        float weight = isDefense ? durabilityDefendWeight : durabilityAttackWeight;
        Debug.Log($"내구도 {damage * weight} 줄음");
        currentDurability -= damage * weight;

        if (currentDurability < 0)
        {
            currentDurability = 0;
        }

        ChangeBladeTransparency(currentDurability / maxDurability);
    }

}
