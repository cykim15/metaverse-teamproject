using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HammerForAnvil : MonoBehaviour
{
    [SerializeField]
    private string anvilTag;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == anvilTag)
        {
            Debug.Log("±ø!");
        }
    }


}
