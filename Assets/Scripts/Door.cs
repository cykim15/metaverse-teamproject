using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

public class Door : MonoBehaviour
{
    [SerializeField]
    private bool isLocked;

    private bool isActivated = true;
    private float beforeSpeed = 0f;
    private Quaternion initialRotation;

    private Rigidbody body;
    private HingeJoint joint;

    [SerializeField]
    private string doorActivateLayerName = "DoorActivate";
    [SerializeField]
    private string doorDeactivateLayerName = "DoorDeactivate";

    [SerializeField]
    private AudioClip audioClipOpen;
    [SerializeField]
    private AudioClip audioClipFix;

    [SerializeField]
    private UnityEvent onDoorActivate;

    private Vector3 jointAnchor;
    private Vector3 jointAxis;
    private JointLimits jointLimits;

    private AudioSource audioSource;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        joint = GetComponent<HingeJoint>();

        jointAnchor = joint.anchor;
        jointAxis = joint.axis;
        jointLimits = joint.limits;

        audioSource = GetComponent<AudioSource>();
        
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
        initialRotation = transform.rotation;

        
    }

    private IEnumerator WaitAndStartUpdate()
    {
        yield return new WaitForSeconds(0.5f);

        audioSource.clip = audioClipOpen;
        audioSource.Play();

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
                Quaternion rotation = transform.rotation;
                if (beforeSpeed > 0f && speed == 0f && Quaternion.Angle(rotation, initialRotation) > 1f)
                {
                    ChangeLayer(transform, doorDeactivateLayerName);
                    Destroy(joint);
                    Destroy(body);
                    isActivated = false;

                    audioSource.clip = audioClipFix;
                    audioSource.Play();
                }

                beforeSpeed = speed;
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

        onDoorActivate?.Invoke();

        StartCoroutine(WaitAndStartUpdate());
    }
}
