using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    private HP playerHP;
    private Stamina playerStamina;

    [SerializeField]
    private float walkingSpeed;
    [SerializeField]
    private float runningSpeed;

    public float WalkingSpeed => walkingSpeed;
    public float RunningSpeed => runningSpeed;

    private void Awake()
    {
        playerHP = GetComponent<HP>();
        playerStamina = GetComponent<Stamina>();
    }

    public void GetHit(float amount)
    {
        playerHP.Decrease(amount);

        if (playerHP.Current < 1f)
        {
            Debug.Log("»ç¸Á");
        }
        else
        {
            Debug.Log($"Ã¼·Â {amount} ±ðÀÓ");
        }
    }

    public void GetPotionEffect(string potionType, float amount)
    {
        if (potionType == "hp")
        {
            playerHP.Increase(amount);
        }
        else if (potionType == "stamina")
        {
            playerStamina.Increase(amount);
        }
    }
}
