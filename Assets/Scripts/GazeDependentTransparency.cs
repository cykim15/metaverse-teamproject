using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GazeDependentTransparency : MonoBehaviour
{
    [SerializeField]
    private new Camera camera;
    [SerializeField]
    private float thresholdAngle = 30f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        Vector3 dir = transform.position - camera.transform.position;
        float angle = Vector3.Angle(camera.transform.forward, dir);

        if (angle < thresholdAngle)
        {
            canvasGroup.alpha = 1 - angle / thresholdAngle;
        }
        else
        {
            canvasGroup.alpha = 0f;
        }
        
    }
}
