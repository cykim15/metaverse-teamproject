using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Door : MonoBehaviour
{
    [SerializeField]
    private bool isLocked;

    private bool isActivated = true;
    private float beforeSpeed = 0f;

    private Rigidbody body;
    private HingeJoint joint;

    [SerializeField]
    private string doorActivateLayerName = "DoorActivate";
    [SerializeField]
    private string doorDeactivateLayerName = "DoorDeactivate";

    private Vector3 jointAnchor;
    private Vector3 jointAxis;
    private JointLimits jointLimits;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        joint = GetComponent<HingeJoint>();

        jointAnchor = joint.anchor;
        jointAxis = joint.axis;
        jointLimits = joint.limits;
        
        if (isLocked)
        {
            ChangeLayer(transform, doorDeactivateLayerName);
            Destroy(joint);
            Destroy(body);
        }
        else
        {
            StartCoroutine(WaitAndStartUpdate());
        }

        
    }

    private IEnumerator WaitAndStartUpdate()
    {
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            Update_();
            yield return null;
        }
    }

    private void Update_()
    {
        if (isLocked == false)
        {
            if (isActivated && body != null)
            {
                float speed = body.velocity.magnitude;
                if (beforeSpeed > 0f && speed == 0f)
                {
                    ChangeLayer(transform, doorDeactivateLayerName);
                    Destroy(joint);
                    Destroy(body);
                    isActivated = false;
                }

                beforeSpeed = body.velocity.magnitude;
            }
        }
    }

    private void ChangeLayer(Transform target, string layerName)
    {
        target.gameObject.layer = LayerMask.NameToLayer(layerName);
    }

    public void Unlock()
    {
        ChangeLayer(transform, doorActivateLayerName);

        body = gameObject.AddComponent<Rigidbody>();
        joint = gameObject.AddComponent<HingeJoint>();

        joint.anchor = jointAnchor;
        joint.axis = jointAxis;
        joint.limits = jointLimits;
        joint.useLimits = true;     

        isLocked = false;

        StartCoroutine(WaitAndStartUpdate());
    }
}
