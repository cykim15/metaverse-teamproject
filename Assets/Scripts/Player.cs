using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class Player : MonoBehaviour
{
    public static Player Instance;

    private HP playerHP;
    private Stamina playerStamina;

    [SerializeField]
    private float walkingSpeed;
    [SerializeField]
    private float runningSpeed;

    public float WalkingSpeed => walkingSpeed;
    public float RunningSpeed => runningSpeed;

    public List<MeleeWeapon> defendingWeapons;

    public List<GameObject> grabbingObjects;

    [SerializeField]
    private AudioClip audioClipHit;

    [SerializeField]
    private PlaySFX playSFX;

    [SerializeField]
    private Image imageBloodScreen;
    [SerializeField]
    private AnimationCurve curveBloodScreen;

    [SerializeField]
    private UnityEvent onDie;

    [SerializeField]
    private float startHp;
    [SerializeField]
    private float startStamina;

    private bool isAlive = true;

    private void Awake()
    {
        Instance = this;

        playerHP = GetComponent<HP>();
        playerStamina = GetComponent<Stamina>();
        defendingWeapons = new List<MeleeWeapon>();
    }

    private void Start()
    {
        playerHP.Current = startHp;
        playerStamina.Current = startStamina;
    }

    public void GetHit(float amount)
    {
        if (isAlive == false) return;

        playerHP.Decrease(amount);

        if (playerHP.Current < 1f)
        {
            isAlive = false;
            Debug.Log("»ç¸Á");

            Color color = imageBloodScreen.color;
            color.a = 0.5f;
            imageBloodScreen.color = color;

            onDie?.Invoke();

            StopAllCoroutines();

        }
        else
        {
            playSFX.Call(audioClipHit);
            Debug.Log($"Ã¼·Â {amount} ±ðÀÓ");

            StopCoroutine("OnBloodScreen");
            StartCoroutine("OnBloodScreen");
        }
    }

    public void GetPotionEffect(string potionType, float amount)
    {
        if (isAlive == false) return;

        if (potionType == "hp")
        {
            playerHP.Increase(amount);
        }
        else if (potionType == "stamina")
        {
            playerStamina.Increase(amount);
        }
    }

    private IEnumerator OnBloodScreen()
    {
        float percent = 0;

        while (percent < 1f)
        {
            percent += Time.deltaTime;

            Color color = imageBloodScreen.color;
            color.a = Mathf.Lerp(0.5f, 0, curveBloodScreen.Evaluate(percent));
            imageBloodScreen.color = color;

            yield return null;
        }
    }
}
