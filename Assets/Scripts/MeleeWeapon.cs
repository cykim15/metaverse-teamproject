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
    [SerializeField, Tooltip("�ֵθ��� ���� = a * speed + b * angularSpeed �� ��, a�� ���� �ش��մϴ�.")]
    private float speedWeight;
    [SerializeField, Tooltip("�ֵθ��� ���� = a * speed + b * angularSpeed �� ��, b�� ���� �ش��մϴ�.")]
    private float angularSpeedWeight;
    [SerializeField, Tooltip("�ֵθ��� ���Ⱑ �� ���� �Ѿ�� �������� �����˴ϴ�.")]
    private float swingIntensityThreshold;
    [SerializeField, Tooltip("���� �������� �ֵθ��� ���⿡ �ش� ���� ���Ͽ� ���˴ϴ�.")]
    private float damageWeight;

    private float currentCooldownTime;
    private bool isCooldown;

    [Header("Reference")]
    [SerializeField]
    private ChangeTransparency changeBladeTransparency;
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
    }


    public void OnBladeTouched(Collider other)
    {
        GameObject collidedObject = other.gameObject;
        int collidedLayer = collidedObject.layer;
        if (1 << collidedLayer == enemyLayer)
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
            changeBladeTransparency.Call(1 - imageFill.fillAmount);

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
        changeBladeTransparency.Call(boolean ? 0.0f : 1.0f);

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

        float swingIntensity = velocity + angularVelocity / 1000;

        if (swingIntensity >= swingIntensityThreshold)
        {
            if (isCooldown == false)
            {
                float damage = damageWeight * swingIntensity;
                targetObject.GetComponent<Enemy>().Damaged(damage);
                //Debug.Log($"������ {damage.ToString("F2")}�� ������ ����");
                StartCooldownTime();
            }
            else
            {
                //Debug.Log($"���� ���ݱ��� {currentCooldownTime.ToString("F2")}�� ����");
            }
        }
    }
}
