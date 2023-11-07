using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;

/// <summary>
/// Master script that will handle reading some input on the controller and trigger special events like Teleport or
/// activating the MagicTractorBeam
/// </summary>
public class MasterController : MonoBehaviour
{
    static MasterController s_Instance = null;
    public static MasterController Instance => s_Instance;

    public XRRig Rig => m_Rig;

    [Header("Setup")]
    public bool DisableSetupForDebug = false;
    public Transform StartingPosition;
    public GameObject TeleporterParent;
    
    [Header("Reference")]
    public XRRayInteractor RightTeleportInteractor;
    public XRRayInteractor LeftTeleportInteractor;

    public XRDirectInteractor RightDirectInteractor;
    public XRDirectInteractor LeftDirectInteractor;

    public MagicTractorBeam RightTractorBeam;
    public MagicTractorBeam LeftTractorBeam;
    
    XRRig m_Rig;
    
    InputDevice m_LeftInputDevice;
    InputDevice m_RightInputDevice;

    XRInteractorLineVisual m_RightLineVisual;
    XRInteractorLineVisual m_LeftLineVisual;

    HandPrefab m_RightHandPrefab;
    HandPrefab m_LeftHandPrefab;
    
    XRReleaseController m_RightController;
    XRReleaseController m_LeftController;

    bool m_PreviousRightClicked = false;
    bool m_PreviousLeftClicked = false;

    bool m_LastFrameRightEnable = false;
    bool m_LastFrameLeftEnable = false;

    LayerMask m_OriginalRightMask;
    LayerMask m_OriginalLeftMask;
    
    List<XRBaseInteractable> m_InteractableCache = new List<XRBaseInteractable>(16);


    /// edited ///
    Gradient invalidRay;
    Gradient validRay;
    public LocomotionSystem locomotionSystem;
    //////////////

    void Awake()
    {
        s_Instance = this;
        m_Rig = GetComponent<XRRig>();
       
    }

    void OnEnable()
    {
         InputDevices.deviceConnected += RegisterDevices;
    }

    void OnDisable()
    {
        InputDevices.deviceConnected -= RegisterDevices;
    }

    void Start()
    {
        m_RightLineVisual = RightTeleportInteractor.GetComponent<XRInteractorLineVisual>();
        m_RightLineVisual.enabled = false;

        m_LeftLineVisual = LeftTeleportInteractor.GetComponent<XRInteractorLineVisual>();
        m_LeftLineVisual.enabled = false;

        m_RightController = RightTeleportInteractor.GetComponent<XRReleaseController>();
        m_LeftController = LeftTeleportInteractor.GetComponent<XRReleaseController>();

        m_OriginalRightMask = RightTeleportInteractor.interactionLayerMask;
        m_OriginalLeftMask = LeftTeleportInteractor.interactionLayerMask;

        /// edited ///
        invalidRay = m_RightLineVisual.invalidColorGradient;
        validRay = m_RightLineVisual.validColorGradient;
        //////////////
        
        if (!DisableSetupForDebug)
        {
            transform.position = StartingPosition.position;
            transform.rotation = StartingPosition.rotation;
            
            if(TeleporterParent != null)
                TeleporterParent.SetActive(false);
        }
        
        InputDeviceCharacteristics leftTrackedControllerFilter = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left;
        List<InputDevice> foundControllers = new List<InputDevice>();
        
        InputDevices.GetDevicesWithCharacteristics(leftTrackedControllerFilter, foundControllers);

        if (foundControllers.Count > 0)
            m_LeftInputDevice = foundControllers[0];
        
        
        InputDeviceCharacteristics rightTrackedControllerFilter = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right;

        InputDevices.GetDevicesWithCharacteristics(rightTrackedControllerFilter, foundControllers);

        if (foundControllers.Count > 0)
            m_RightInputDevice = foundControllers[0];

        if (m_Rig.currentTrackingOriginMode != TrackingOriginModeFlags.Floor)
            m_Rig.cameraYOffset = 1.8f;


    }

    void RegisterDevices(InputDevice connectedDevice)
    {
        if (connectedDevice.isValid)
        {
            if ((connectedDevice.characteristics & InputDeviceCharacteristics.HeldInHand) == InputDeviceCharacteristics.HeldInHand)
            {
                if ((connectedDevice.characteristics & InputDeviceCharacteristics.Left) == InputDeviceCharacteristics.Left)
                {
                    m_LeftInputDevice = connectedDevice;
                }
                else if ((connectedDevice.characteristics & InputDeviceCharacteristics.Right) == InputDeviceCharacteristics.Right)
                {
                    m_RightInputDevice = connectedDevice;
                }
            }
        }
    }
    
    void Update()
    {
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
            Application.Quit();
        
        RightTeleportUpdate();
        LeftTeleportUpdate();
    }

    void RightTeleportUpdate()
    {
        Vector2 axisInput;
        m_RightInputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out axisInput);
        
        m_RightLineVisual.enabled = axisInput.y > 0.5f;
        
        RightTeleportInteractor.interactionLayerMask = m_LastFrameRightEnable ? m_OriginalRightMask : new LayerMask();


        bool canTeleport = false;
        /// edited ///
        if (m_RightLineVisual.enabled)
        {
            RightTeleportInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);
            int hitLayer = hit.collider.gameObject.layer;
            string hitLayerName = LayerMask.LayerToName(hitLayer);
            if (hitLayerName == "Teleporter")
            {
                m_RightLineVisual.invalidColorGradient = validRay;
                canTeleport = true;
            }
            else
            {
                m_RightLineVisual.invalidColorGradient = invalidRay;
            }

        }
        
        //////////////
        
        if (axisInput.y <= 0.5f && m_PreviousRightClicked)
        {
            /// edited ///
            //m_RightController.Select();
            RightTeleportInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);
            TeleportRequest teleportRequest = new TeleportRequest();
            teleportRequest.destinationPosition = hit.point;
            teleportRequest.destinationRotation = transform.rotation;
            locomotionSystem.GetComponent<TeleportationProvider>().QueueTeleportRequest(teleportRequest);
            //////////////
        }

        if (axisInput.y <= -0.5f)
        {
            if(!RightTractorBeam.IsTracting)
                RightTractorBeam.StartTracting();
        }
        else if(RightTractorBeam.IsTracting)
        {
            RightTractorBeam.StopTracting();
        }

        //if the right animator is null, we try to get it. It's not the best performance wise but no other way as setup
        //of the model by the Interaction Toolkit is done on the first update.
        if (m_RightHandPrefab == null)
        {
            m_RightHandPrefab = RightDirectInteractor.GetComponentInChildren<HandPrefab>();
        }

        //m_PreviousRightClicked = axisInput.y > 0.5f;
        m_PreviousRightClicked = canTeleport; // edited

        if (m_RightHandPrefab != null)
        {
            m_RightHandPrefab.Animator.SetBool("Pointing", m_PreviousRightClicked);
        }

        m_LastFrameRightEnable = m_RightLineVisual.enabled;
    }

    void LeftTeleportUpdate()
    {
        Vector2 axisInput;
        m_LeftInputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out axisInput);
        
        m_LeftLineVisual.enabled = axisInput.y > 0.5f;
        
        LeftTeleportInteractor.interactionLayerMask = m_LastFrameLeftEnable ? m_OriginalLeftMask : new LayerMask();
        
        if (axisInput.y <= 0.5f && m_PreviousLeftClicked)
        {
            m_LeftController.Select();
        }
        
        if (axisInput.y <= -0.5f)
        {
            if(!LeftTractorBeam.IsTracting)
                LeftTractorBeam.StartTracting();
        }
        else if(LeftTractorBeam.IsTracting)
        {
            LeftTractorBeam.StopTracting();
        }
        
        //if the left animator is null, we try to get it. It's not the best performance wise but no other way as setup
        //of the model by the Interaction Toolkit is done on the first update.
        if (m_LeftHandPrefab == null)
        {
            m_LeftHandPrefab = LeftDirectInteractor.GetComponentInChildren<HandPrefab>();
        }

        m_PreviousLeftClicked = axisInput.y > 0.5f;
        
        if (m_LeftHandPrefab != null)
            m_LeftHandPrefab.Animator.SetBool("Pointing", m_PreviousLeftClicked);
        
        m_LastFrameLeftEnable = m_LeftLineVisual.enabled;
    }
}
