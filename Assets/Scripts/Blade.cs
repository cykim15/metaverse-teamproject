using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Blade : MonoBehaviour
{
    [SerializeField]
    private UnityEvent<Collider, Vector3> onHit;

    private bool detectCollision = true;

    private void OnTriggerEnter(Collider other)
    {
        if (detectCollision)
        {
            onHit?.Invoke(other, transform.position);
        }
    }

    private void OnEnable()
    {
        detectCollision = false;
        StartCoroutine(WaitAndEnableDetectCollision());
    }

    private IEnumerator WaitAndEnableDetectCollision()
    {
        yield return new WaitForSeconds(0.1f);
        detectCollision = true;
    }
}
