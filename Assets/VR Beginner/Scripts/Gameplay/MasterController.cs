using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
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
    [SerializeField]
    float wallClippingBoxSize;
    [SerializeField]
    float obstaclePreventTeleportationDistance;

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
    TpGage playerTeleportationGage;
    
    [SerializeField]
    LayerMask obstacleLayerMask;
    private Vector3 boxSize;
    //////////////

    void Awake()
    {
        s_Instance = this;
        m_Rig = GetComponent<XRRig>();
        boxSize = new Vector3(wallClippingBoxSize, wallClippingBoxSize, wallClippingBoxSize);
       
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
        playerTeleportationGage = GetComponent<TpGage>();
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
        /*
        Bounds bounds = new Bounds(RightDirectInteractor.transform.position, boxSize);
        Collider[] colliders = Physics.OverlapBox(RightDirectInteractor.transform.position, boxSize / 2f, Quaternion.identity, obstacleLayerMask);
        if (colliders.Length > 0)
        {
            Debug.Log("hi");
            return;
        }
        Debug.Log("sex");*/

        Vector2 axisInput;
        m_RightInputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out axisInput);
        
        m_RightLineVisual.enabled = axisInput.y > 0.5f;
        
        RightTeleportInteractor.interactionLayerMask = m_LastFrameRightEnable ? m_OriginalRightMask : new LayerMask();


        bool canTeleport = false;
        /// edited ///
        if (m_RightLineVisual.enabled)
        {
            RightTeleportInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);
            
            if (hit.collider != null)
            {
                int hitLayer = hit.collider.gameObject.layer;
                string hitLayerName = LayerMask.LayerToName(hitLayer);
                if (hitLayerName == "Teleporter" && CanTeleportConsideringEnemies(hit.point) && CanTeleportConsideringObstacles(hit.point))
                {
                    m_RightLineVisual.invalidColorGradient = validRay;
                    canTeleport = true;
                }
                else
                {
                    m_RightLineVisual.invalidColorGradient = invalidRay;
                }
            }
     

        }
        
        //////////////
        
        if (axisInput.y <= 0.5f && m_PreviousRightClicked)
        {
            /// edited ///
            //m_RightController.Select();
            RightTeleportInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);

            float distance = Vector3.Distance(transform.position, hit.point);
            if (playerTeleportationGage.CanTeleport(distance))
            {
                TeleportRequest teleportRequest = new TeleportRequest();
                teleportRequest.destinationPosition = hit.point;
                teleportRequest.destinationRotation = transform.rotation;
                locomotionSystem.GetComponent<TeleportationProvider>().QueueTeleportRequest(teleportRequest);
                playerTeleportationGage.ConsumeGage(distance);
            }
            //////////////
        }

        /*
        if (axisInput.y <= -0.5f)
        {
            if(!RightTractorBeam.IsTracting)
                RightTractorBeam.StartTracting();
        }
        else if(RightTractorBeam.IsTracting)
        {
            RightTractorBeam.StopTracting();
        }*/

        //if the right animator is null, we try to get it. It's not the best performance wise but no other way as setup
        //of the model by the Interaction Toolkit is done on the first update.
        if (m_RightHandPrefab == null)
        {
            m_RightHandPrefab = RightDirectInteractor.GetComponentInChildren<HandPrefab>();
        }

        /// edited ///
        //m_PreviousRightClicked = axisInput.y > 0.5f;
        m_PreviousRightClicked = canTeleport;
        //////////////

        if (m_RightHandPrefab != null)
        {
            m_RightHandPrefab.Animator.SetBool("Pointing", axisInput.y > 0.5f);
        }

        m_LastFrameRightEnable = m_RightLineVisual.enabled;
    }

    void LeftTeleportUpdate()
    {
        /*
        Bounds bounds = new Bounds(LeftDirectInteractor.transform.position, boxSize);
        Collider[] colliders = Physics.OverlapBox(LeftDirectInteractor.transform.position, boxSize / 2f, Quaternion.identity, obstacleLayerMask);
        if (colliders.Length > 0) return;*/

        Vector2 axisInput;
        m_LeftInputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out axisInput);

        m_LeftLineVisual.enabled = axisInput.y > 0.5f;

        LeftTeleportInteractor.interactionLayerMask = m_LastFrameRightEnable ? m_OriginalRightMask : new LayerMask();


        bool canTeleport = false;
        /// edited ///
        if (m_LeftLineVisual.enabled)
        {
            LeftTeleportInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);

            if (hit.collider != null)
            {
                int hitLayer = hit.collider.gameObject.layer;
                string hitLayerName = LayerMask.LayerToName(hitLayer);
                if (hitLayerName == "Teleporter" && CanTeleportConsideringEnemies(hit.point) && CanTeleportConsideringObstacles(hit.point))
                {
                    m_LeftLineVisual.invalidColorGradient = validRay;
                    canTeleport = true;
                }
                else
                {
                    m_LeftLineVisual.invalidColorGradient = invalidRay;
                }
            }


        }

        //////////////

        if (axisInput.y <= 0.5f && m_PreviousLeftClicked)
        {
            /// edited ///
            //m_LeftController.Select();
            LeftTeleportInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);

            float distance = Vector3.Distance(transform.position, hit.point);
            if (playerTeleportationGage.CanTeleport(distance))
            {
                TeleportRequest teleportRequest = new TeleportRequest();
                teleportRequest.destinationPosition = hit.point;
                teleportRequest.destinationRotation = transform.rotation;
                locomotionSystem.GetComponent<TeleportationProvider>().QueueTeleportRequest(teleportRequest);
                playerTeleportationGage.ConsumeGage(distance);
            }
            //////////////
        }

        /*
        if (axisInput.y <= -0.5f)
        {
            if(!LeftTractorBeam.IsTracting)
                LeftTractorBeam.StartTracting();
        }
        else if(LeftTractorBeam.IsTracting)
        {
            LeftTractorBeam.StopTracting();
        }*/

        //if the left animator is null, we try to get it. It's not the best performance wise but no other way as setup
        //of the model by the Interaction Toolkit is done on the first update.
        if (m_LeftHandPrefab == null)
        {
            m_LeftHandPrefab = LeftDirectInteractor.GetComponentInChildren<HandPrefab>();
        }

        /// edited ///
        //m_PreviousLeftClicked = axisInput.y > 0.5f;
        m_PreviousLeftClicked = canTeleport;
        //////////////

        if (m_LeftHandPrefab != null)
        {
            m_LeftHandPrefab.Animator.SetBool("Pointing", axisInput.y > 0.5f);
        }

        m_LastFrameLeftEnable = m_LeftLineVisual.enabled;
    }

    private bool CanTeleportConsideringEnemies(Vector3 targetPosition)
    {
        float closestDistance = float.MaxValue;
        GameObject closestEnemy = null;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(targetPosition, enemy.transform.position);

            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy == null) return true;

        if (closestDistance < closestEnemy.GetComponent<Enemy>().FightDistance) return false;
        else return true;
    }

    private bool CanTeleportConsideringObstacles(Vector3 targetPosition)
    {
        float closestDistance = float.MaxValue;
        GameObject closestObstacle = null;

        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacles");

        foreach (GameObject obstacle in obstacles)
        {
            Vector3 closestPointOnCollider = obstacle.GetComponent<Collider>().ClosestPointOnBounds(targetPosition);

            float distanceToCollider = Vector3.Distance(targetPosition, closestPointOnCollider);

            if (distanceToCollider < closestDistance)
            {
                closestDistance = distanceToCollider;
                closestObstacle = obstacle;
            }
        }

        if (closestObstacle == null) return true;

        if (closestDistance < obstaclePreventTeleportationDistance) return false;
        else return true;
    }
}
