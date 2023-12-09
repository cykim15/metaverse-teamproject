using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DescribeUI : MonoBehaviour
{
    [SerializeField]
    private float startToViewDistance = 3.5f;
    [SerializeField]
    private float startToOpaqueDistance = 3f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position, Player.Instance.transform.position);
        
        if (distance > startToViewDistance)
        {
            canvasGroup.alpha = 0f;
        }
        else if (distance > startToOpaqueDistance)
        {
            float alpha = Mathf.InverseLerp(startToViewDistance, startToOpaqueDistance, distance);
            canvasGroup.alpha = alpha;
        }
        else
        {
            canvasGroup.alpha = 1f;
        }
    }
}
