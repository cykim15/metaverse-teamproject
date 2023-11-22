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
    [SerializeField, Tooltip("휘두르는 세기 = a * speed + b * angularSpeed 일 때, a의 값에 해당합니다.")]
    private float speedWeight;
    [SerializeField, Tooltip("휘두르는 세기 = a * speed + b * angularSpeed 일 때, b의 값에 해당합니다.")]
    private float angularSpeedWeight;
    [SerializeField, Tooltip("휘두르는 세기가 이 값을 넘어야 공격으로 판정됩니다.")]
    private float swingIntensityThreshold;
    [SerializeField, Tooltip("실제 데미지는 휘두르는 세기에 해당 값을 곱하여 계산됩니다.")]
    private float damageWeight;

    private float currentCooldownTime;
    private bool isCooldown;

    [Header("Reference")]
    [SerializeField]
    private Collider blade;
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
        oppositeUILocalPosition.x *= -1;
        oppositeUILocalRotation = Quaternion.Euler(0f, 180f, 0f) * originalUILocalRotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collidedObject = collision.collider.gameObject;
        int collidedLayer = collidedObject.layer;
        if (LayerMask.LayerToName(collidedLayer) == "Enemy")
        {
            StartCoroutine(nameof(Swing), collidedObject);
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
                if (transform.InverseTransformPoint(hit.point).x > 0)
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

        float swingIntensity = velocity + angularVelocity / 1000;

        if (swingIntensity >= swingIntensityThreshold)
        {
            if (isCooldown == false)
            {
                float damage = damageWeight * swingIntensity;
                targetObject.GetComponent<Enemy>().Damaged(damage);
                //Debug.Log($"적에게 {damage.ToString("F2")}의 데미지 입힘");
                StartCooldownTime();
            }
            else
            {
                //Debug.Log($"다음 공격까지 {currentCooldownTime.ToString("F2")}초 남음");
            }
        }
    }
}
