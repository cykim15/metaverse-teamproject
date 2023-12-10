using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HammerForAnvil : MonoBehaviour
{
    [SerializeField]
    private LayerMask anvilColliderLayer;

    private bool isCooldown = false;
    private float currentCooldownTime;

    [SerializeField]
    private float maxCooldownTime = 1f;
    [SerializeField]
    private float speedWeight = 1f;
    [SerializeField]
    private float angularSpeedWeight = 0.001f;
    [SerializeField]
    private float swingIntensityThreshold = 5f;

    private void Awake()
    {
        SetCooldownIs(false);
    }

    public void OnBladeTouched(Collider other)
    {
        if (1 << other.gameObject.layer == anvilColliderLayer)
        {
            StartCoroutine(Swing(other.gameObject));
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
            currentCooldownTime -= Time.deltaTime;
            yield return null;
        }

        SetCooldownIs(false);
    }

    private void SetCooldownIs(bool boolean)
    {
        isCooldown = boolean;
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
            Anvil anvil = targetObject.GetComponentInParent<Anvil>();
            if (isCooldown == false && anvil != null && anvil.coinActivate.activated == true)
            {
                anvil.TryFixWeapon();
                StartCooldownTime();
            }
        }
    }


}
