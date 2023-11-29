using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Potion will check if it have a PotionReceiver under it when it's poured, and OnPotionPoured event will be called if
/// it does.
/// </summary>
public class PotionReceiver : MonoBehaviour
{
    [System.Serializable]
    public class PotionPouredEvent : UnityEvent<string, float> { }

    public string[] AcceptedPotionType;
    
    public PotionPouredEvent OnPotionPoured;


    public void ReceivePotion(string PotionType, float amount)
    {
        if(AcceptedPotionType.Contains(PotionType))
        {
            OnPotionPoured.Invoke(PotionType, amount);
        }                      
    }
}
