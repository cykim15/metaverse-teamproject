using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class CoinManager : MonoBehaviour
{
    private InputDevice? leftController;
    private InputDevice? rightController;

    private bool leftXButtonPressed = false;
    //private bool leftYButtonPressed = false;
    private bool rightAButtonPressed = false;
    //private bool rightBButtonPressed = false;

    private int coin = 0;

    [Header("Reference")]
    [SerializeField]
    private LayerMask coinLayer;
    [SerializeField]
    private TextMeshProUGUI coinText;
    [SerializeField]
    private XRDirectInteractor leftDirectInteractor;
    [SerializeField]
    private XRDirectInteractor rightDirectInteractor;

    public int Coin => coin;

    void Start()
    {
        SetupController();
    }

    void Update()
    {
        CheckButtonState(leftController, CommonUsages.primaryButton, ref leftXButtonPressed, "leftX");
        //CheckButtonState(leftController, CommonUsages.secondaryButton, ref leftYButtonPressed, "leftY");
        CheckButtonState(rightController, CommonUsages.primaryButton, ref rightAButtonPressed, "rightA");
        //CheckButtonState(rightController, CommonUsages.secondaryButton, ref rightBButtonPressed, "rightB");
    }

    private void SetupController()
    {
        InputDeviceCharacteristics leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;

        leftController = FindController(leftControllerCharacteristics);
        rightController = FindController(rightControllerCharacteristics);
    }

    private InputDevice? FindController(InputDeviceCharacteristics characteristics)
    {
        InputDevice? device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (device != null && (device.Value.characteristics & characteristics) == characteristics)
        {
            return device;
        }

        device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (device != null && (device.Value.characteristics & characteristics) == characteristics)
        {
            return device;
        }

        return null;
    }

    private void CheckButtonState(InputDevice? controller, InputFeatureUsage<bool> button, ref bool buttonPressed, string buttonName)
    {
        if (controller == null)
        {
            InputDeviceCharacteristics leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
            InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;

            leftController = FindController(leftControllerCharacteristics);
            rightController = FindController(rightControllerCharacteristics);
        }

        if (controller != null && controller.Value.TryGetFeatureValue(button, out bool buttonState))
        {
            if (buttonState && !buttonPressed)
            {
                // 버튼이 처음으로 눌린 순간
                if (buttonName == "leftX")
                {
                    CollectCoin(true);
                }
                else if (buttonName == "rightA")
                {
                    CollectCoin(false);
                }
            }

            buttonPressed = buttonState;
        }
    }


    private void CollectCoin(bool isLeft)
    {
        XRDirectInteractor interactor;
        if (isLeft) interactor = leftDirectInteractor;
        else interactor = rightDirectInteractor;

        // Insert
        if (interactor.selectTarget)
        {
            GameObject grabbedObject = interactor.selectTarget.gameObject;
            if (coinLayer == 1 << grabbedObject.layer)
            {
                interactor.interactionManager.SelectExit(interactor, interactor.selectTarget);
                StartCoroutine(WaitAndDeactivate(grabbedObject));
                IncreaseCoin(1);
            }
        }
    }

    private IEnumerator WaitAndDeactivate(GameObject gameObject)
    {
        yield return null;
        gameObject.SetActive(false);
    }

    public void IncreaseCoin(int amount)
    {
        coin += amount;
        UpdateUI();
    }

    public void DecreaseCoin(int amount)
    {
        coin -= amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        coinText.text = $"코인: {coin}개";
    }
}
