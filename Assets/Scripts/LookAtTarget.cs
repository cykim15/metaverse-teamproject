using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    private void Update()
    {
        if (target != null)
        {
            Vector3 lookDirection = target.position - transform.position;

            Quaternion rotation = Quaternion.LookRotation(lookDirection);

            transform.rotation = rotation;

            transform.Rotate(Vector3.up, 180f);
        }
    }
}
