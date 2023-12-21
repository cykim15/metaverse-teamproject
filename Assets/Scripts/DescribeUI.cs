using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using static UnityEngine.GraphicsBuffer;

public class DescribeUI : MonoBehaviour
{
    [SerializeField]
    private float startToViewDistance = 3.5f;
    //[SerializeField]
    private float startToOpaqueDistance;
    [SerializeField]
    private float startToDisableDistance = 0.2f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        startToOpaqueDistance = startToViewDistance - 0.5f;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        LookAtConstraint look = GetComponent<LookAtConstraint>();
        look.enabled = true;
        look.AddSource(new ConstraintSource { sourceTransform = Camera.main.transform, weight = 1.0f });
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, startToViewDistance);
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
        else if (distance > startToDisableDistance)
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            canvasGroup.alpha = 0f;
        }
    }
}
