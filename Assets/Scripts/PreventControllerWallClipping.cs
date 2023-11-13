using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PreventControllerWallClipping : MonoBehaviour
{
    [SerializeField]
    private Transform xrRig;
    [SerializeField]
    private Transform mainCamera;
    [SerializeField]
    private Transform endPoint;
    [SerializeField]
    private XRDirectInteractor directInteractor;
    [SerializeField]
    private XRRayInteractor rayInteractor;
    [SerializeField]
    private LayerMask obstacleLayer;

    Vector3 cameraHeightRigPosition;

    private void Update()
    {
        cameraHeightRigPosition = xrRig.position;
        cameraHeightRigPosition.y = mainCamera.position.y;

        if (Physics.Raycast(cameraHeightRigPosition, endPoint.position - cameraHeightRigPosition, Vector3.Distance(cameraHeightRigPosition, endPoint.position), obstacleLayer))
        {
            Transform interactionTransform = null;
            if (directInteractor.selectTarget != null)
            {
                interactionTransform = directInteractor.selectTarget.transform;
            }
            directInteractor.enabled = false;
            rayInteractor.enabled = false;
            if (interactionTransform != null) interactionTransform.position = xrRig.position;
        }
        else
        {
            directInteractor.enabled = true;
            rayInteractor.enabled = true;
        }
    }
}
