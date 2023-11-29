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

    [Header("Reference")]
    [SerializeField]
    private Player player;
    [SerializeField]
    private ContinuousMoveProviderBase continuousMoveProvider;

    private InputDevice? rightController;
    private bool beforeButtonPressed = false;

    void Start()
    {
        SetupController();
    }

    private void Update()
    {
        bool buttonPressed = CheckButtonState(rightController, CommonUsages.secondaryButton);
        if (buttonPressed != beforeButtonPressed) // 버튼 입력이 바뀔 경우에만 continuous move provider에 접근하여 속도 변경
        {
            continuousMoveProvider.moveSpeed = buttonPressed ? player.RunningSpeed : player.WalkingSpeed;
        }

        float playerCurrentSpeed = player.GetComponent<CharacterController>().velocity.magnitude;
        if (buttonPressed && Mathf.Abs(player.RunningSpeed - playerCurrentSpeed) < 0.1f)
        {
            Current -= decreaseWhenRun * Time.deltaTime;
        }
        else
        {
            Current += normalRecovery * Time.deltaTime;
        }

        beforeButtonPressed = buttonPressed;

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
