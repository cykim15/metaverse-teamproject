using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Stamina : Gage
{
    [Header("Parameter")]
    [SerializeField]
    private float normalRecovery = 1f;
    [SerializeField]
    private float decreaseWhenRun = 5f;
    [SerializeField]
    private float runThreshold = 20f;

    public float RunThreshold => runThreshold;

    [Header("Reference")]
    [SerializeField]
    private Player player;
    [SerializeField]
    private ContinuousMoveProviderBase continuousMoveProvider;

    private InputDevice? rightController;

    void Start()
    {
        SetupController();
    }

    private void Update()
    {
        bool buttonPressed = CheckButtonState(rightController, CommonUsages.secondaryButton);

        if (buttonPressed && Current > runThreshold)
        {
            continuousMoveProvider.moveSpeed = player.RunningSpeed;
        }
        else
        {
            continuousMoveProvider.moveSpeed = player.WalkingSpeed;
        }

        float playerCurrentSpeed = player.GetComponent<CharacterController>().velocity.magnitude;
        if (buttonPressed && playerCurrentSpeed >= player.WalkingSpeed - 0.1f)
        {
            Current -= decreaseWhenRun * Time.deltaTime;
        }
        else
        {
            Current += normalRecovery * Time.deltaTime;
        }

    }

    private void SetupController()
    {
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        rightController = FindController(rightControllerCharacteristics);
    }

    private InputDevice? FindController(InputDeviceCharacteristics characteristics)
    {
        InputDevice? device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (device != null && (device.Value.characteristics & characteristics) == characteristics)
        {
            return device;
        }

        return null;
    }

    private bool CheckButtonState(InputDevice? controller, InputFeatureUsage<bool> button)
    {
        if (controller == null)
        {
            SetupController();
        }

        if (controller != null && controller.Value.TryGetFeatureValue(button, out bool buttonState))
        {
            return buttonState;
        }
        return false;
    }  
}
