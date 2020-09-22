using UnityEngine;

public class InteractableBehavior : MonoBehaviour
{
    public InteractableObject interactableObject;

    protected virtual void Awake()
    {
        if (interactableObject == null)  interactableObject = GetComponent<InteractableObject>();
    }

    protected virtual void Start()
    {
        if (interactableObject == null)
        {
            Debug.Log("[Warning] InteractableBehavior :: No InteractableObject found for this InteractableBehavior");
            return;
        }
        interactableObject.OnInteractionEvents.Add(OnInteraction);
        interactableObject.OnActivationEvents.Add(OnActivation);
        interactableObject.OnStartHoveringEvents.Add(OnStartHovering);
        interactableObject.OnStopHoveringEvents.Add(OnStopHovering);
    }

    protected virtual void OnInteraction() { }

    protected virtual void OnActivation() { }

    protected virtual void OnStartHovering() { }

    protected virtual void OnStopHovering() { }
}
