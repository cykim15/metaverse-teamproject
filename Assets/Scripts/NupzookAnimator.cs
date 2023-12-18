using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NupzookAnimator : MonoBehaviour
{
    public UnityEvent onSpawnFire;
    public UnityEvent onDestroyFire;

    public void SpawnFire()
    {
        onSpawnFire?.Invoke();
    }

    public void DestroyFire()
    {
        onDestroyFire?.Invoke();
    }
}
