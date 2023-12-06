using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class InventoryManager : MonoBehaviour
{
    private InputDevice? leftController;
    private InputDevice? rightController;

    private bool leftXButtonPressed = false;
    private bool leftYButtonPressed = false;
    private bool rightAButtonPressed = false;
    //private bool rightBButtonPressed = false;

    
    [System.Serializable]
    private class CollectibleItem
    {
        public string tag;
        public Sprite icon;
    }

    [System.Serializable]
    private class itemImage
    {
        public Image background;
        public Image icon;
        public GameObject slider;
    }

    [Header("Parameter")]
    [SerializeField]
    private CollectibleItem[] collectibleItems;

    [Header("Reference")]
    [SerializeField]
    private itemImage[] itemImages;
    [SerializeField]
    private XRDirectInteractor leftDirectInteractor;
    [SerializeField]
    private XRDirectInteractor rightDirectInteractor;

    private GameObject[] items;
    private int currentIndex = 0;
    private int itemCount;

    public int CurrentIndex
    {
        get
        { 
            return currentIndex;
        }
        set
        {
            currentIndex = (value % itemCount + itemCount) % itemCount;
        }
    }

    void Start()
    {
        SetupController();

        itemCount = itemImages.Length;

        items = new GameObject[itemCount];

        ActivateSlotImage(currentIndex);
    }

    void Update()
    {
        CheckButtonState(leftController, CommonUsages.primaryButton, ref leftXButtonPressed, "leftX");
        CheckButtonState(leftController, CommonUsages.secondaryButton, ref leftYButtonPressed, "leftY");
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
            SetupController();
        }

        if (controller != null && controller.Value.TryGetFeatureValue(button, out bool buttonState))
        {
            if (buttonState && !buttonPressed)
            {
                // 버튼이 처음으로 눌린 순간
                if (buttonName == "leftY")
                {
                    SelectNextItem();
                }
                else if (buttonName == "leftX") {
                    InsertOrTakeOutItem(true);
                }
                else if (buttonName == "rightA")
                {
                    InsertOrTakeOutItem(false);
                }
            }

            buttonPressed = buttonState;
        }
    }

    private void SelectNextItem()
    {
        DeactivateSlotImage(CurrentIndex);

        CurrentIndex++;
        ActivateSlotImage(CurrentIndex);
    }

    private void DeactivateSlotImage(int slot)
    {
        Color currentColor = itemImages[slot].background.color;
        currentColor.a = 1f;
        itemImages[slot].background.color = currentColor;
    }

    private void ActivateSlotImage(int slot)
    {
        Color currentColor = itemImages[slot].background.color;
        currentColor.a = 0.5f;
        itemImages[slot].background.color = currentColor;
    }

    private void InsertOrTakeOutItem(bool isLeft)
    {
        XRDirectInteractor interactor;
        if (isLeft) interactor = leftDirectInteractor;
        else interactor= rightDirectInteractor;

        // Insert
        if (items[CurrentIndex] == null && interactor.selectTarget)
        {
            GameObject grabbedObject = interactor.selectTarget.gameObject;
            if (IsCollectible(grabbedObject.tag))
            {
                // 쿨타임이 안끝난 무기는 못 넣음!
                MeleeWeapon weapon = grabbedObject.GetComponent<MeleeWeapon>();
                if (weapon != null && weapon.isCooldown == true) return;

                interactor.interactionManager.SelectExit(interactor, interactor.selectTarget);
                items[CurrentIndex] = grabbedObject;
                StartCoroutine(WaitAndDeactivate(grabbedObject));
                itemImages[CurrentIndex].icon.sprite = FindIconWithTag(grabbedObject.tag);
                itemImages[CurrentIndex].icon.enabled = true;
                
                // 무기면 내구도를 게이지에 반영
                if (weapon != null)
                {
                    itemImages[CurrentIndex].slider.SetActive(true);
                    itemImages[CurrentIndex].slider.GetComponent<Slider>().value = weapon.CurrentDurability / weapon.MaxDurability;
                }
                // 포션이면 남은 양을 게이지에 반영
                else if (grabbedObject.tag == "PotionHP" || grabbedObject.tag == "PotionStamina")
                {
                    Potion potion = grabbedObject.GetComponent<Potion>();
                    itemImages[CurrentIndex].slider.SetActive(true);
                    itemImages[CurrentIndex].slider.GetComponent<Slider>().value = potion.fillAmount / potion.RealStartingFillAmount;
                }
            }
        }

        // Take out
        else if (items[CurrentIndex] != null && !interactor.selectTarget)
        {
            items[CurrentIndex].SetActive(true);
            interactor.interactionManager.ForceSelect(interactor, items[CurrentIndex].GetComponent<XRBaseInteractable>());

            EventBackup eventBackup = items[CurrentIndex].GetComponent<EventBackup>();
            if (eventBackup != null)
            {
                eventBackup.Call();
            }

            items[CurrentIndex] = null;
            itemImages[CurrentIndex].icon.enabled = false;
            itemImages[CurrentIndex].slider.SetActive(false);
        }
    }

    private IEnumerator WaitAndDeactivate(GameObject gameObject)
    {
        yield return null;
        gameObject.SetActive(false);
    }

    private Sprite FindIconWithTag(string tag)
    {
        int i;
        for (i=0; i<collectibleItems.Length; i++)
        {
            if (collectibleItems[i].tag == tag)
            {
                break;
            }
        }

        if (i == collectibleItems.Length)
        {
            return null;
        }

        return collectibleItems[i].icon;
    }

    private bool IsCollectible(string tag)
    {
        int i;
        for (i = 0; i < collectibleItems.Length; i++)
        {
            if (collectibleItems[i].tag == tag)
            {
                break;
            }
        }

        if (i == collectibleItems.Length)
        {
            return false;
        }

        return true;
    }
}