using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InformPlayerGrabbingObject : MonoBehaviour
{
    private XRDirectInteractor interactor;

    private GameObject grabbingObject;

    private void Awake()
    {
        interactor = GetComponent<XRDirectInteractor>();
    }

    public void Add()
    {
        grabbingObject = interactor.selectTarget.gameObject;
        Player.Instance.grabbingObjects.Add(grabbingObject);
    }

    public void Remove()
    {
        Player.Instance.grabbingObjects.Remove(grabbingObject);
    }
}
