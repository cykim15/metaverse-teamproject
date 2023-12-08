using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InformPlayerGrabbingObject : MonoBehaviour
{
    [SerializeField]
    private Player player;

    private XRDirectInteractor interactor;

    private GameObject grabbingObject;

    private void Awake()
    {
        interactor = GetComponent<XRDirectInteractor>();
    }

    public void Add()
    {
        grabbingObject = interactor.selectTarget.gameObject;
        player.grabbingObjects.Add(grabbingObject);
    }

    public void Remove()
    {
        player.grabbingObjects.Remove(grabbingObject);
    }
}
