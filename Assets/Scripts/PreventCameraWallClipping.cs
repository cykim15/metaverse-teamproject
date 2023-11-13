using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreventCameraWallClipping : MonoBehaviour
{
    [SerializeField]
    private Transform xrRig;
    [SerializeField]
    private Transform mainCamera;
    [SerializeField]
    private GameObject blackScreen;
    [SerializeField]
    private LayerMask obstacleLayer;

    private void Awake()
    {
        blackScreen.SetActive(false);
    }

    private void Update()
    {
        if (Physics.Raycast(xrRig.position, mainCamera.position - xrRig.position, Mathf.Infinity, obstacleLayer))
        {
            blackScreen.SetActive(true);
        }
        else
        {
            blackScreen.SetActive(false);
        }
    }
}
