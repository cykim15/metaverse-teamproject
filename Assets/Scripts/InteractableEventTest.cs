using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableEventTest : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void OnHoverEntered()
    {
        meshRenderer.material.color = Color.red;
        Debug.Log($"{gameObject.name} - OnHoverEntered");
    }

    public void OnSelectEntered()
    {
        meshRenderer.material.color = Color.yellow;
        Debug.Log($"{gameObject.name} - OnSelectEntered");
    }

    public void OnFocusEntered()
    {
        meshRenderer.material.color = Color.blue;
        Debug.Log($"{gameObject.name} - OnFocusEntered");
    }

    public void OnActivated()
    {
        meshRenderer.material.color = Color.green;
        Debug.Log($"{gameObject.name} - OnActivated");
    }

    public void Reset()
    {
        transform.position = new Vector3(0, 1, 1);
        meshRenderer.material.color = Color.white;
    }
}
