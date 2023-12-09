using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventBackup : MonoBehaviour
{
    [SerializeField]
    private UnityEvent selectEntered;

    public void Call()
    {
        selectEntered?.Invoke();
        //StartCoroutine(WaitAndInvokeEvent());
    }

    public IEnumerator WaitAndInvokeEvent()
    {
        yield return new WaitForSeconds(0.1f);
        selectEntered?.Invoke();
    }
}
