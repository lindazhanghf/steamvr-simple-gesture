using System;
using UnityEngine;

public abstract class InteractionActor : MonoBehaviour
{
    [Header("InteractionActor")]
    protected InteractableObject m_currentObject;

    protected bool m_debuging = false;

 #if UNITY_EDITOR || DEVELOPMENT_BUILD 
    [SerializeField] private bool EnableDebuging = false;
    protected virtual void Awake()
    {
        m_debuging = EnableDebuging;
    }
 #endif

    public void Unsubsribe(InteractableObject toUnsubscribe = null)
    {
        if (m_currentObject == toUnsubscribe)
        {
            Invoke_StopHovering(toUnsubscribe);
        }
    }

    protected virtual void Invoke_Interaction()
    {
        if (m_currentObject == null) return;

        try
        {
            m_currentObject.Interaction();
        }
        catch (Exception e) { Debug.LogError("InteractionActor [" + gameObject.name + "] :: " + e); }
    }

    protected virtual void Invoke_EndInteraction()
    {
        if (m_currentObject == null) return;

        try
        {
            m_currentObject.EndInteraction();
        }
        catch (Exception e) { Debug.LogError("InteractionActor [" + gameObject.name + "] :: " + e); }
    }

    protected virtual void Invoke_StartHovering(InteractableObject interactableObject)
    {
        // Prevent Invoke_StartHovering() being called again without properly OnTriggerExit()
        if (m_currentObject != null && m_currentObject == interactableObject)
        {
            return;
        }

        try
        {
            // The first object that the user touches becomes m_currentObject
            if (m_currentObject == null)
            {
                if (m_debuging) Debug.LogWarning("InteractionActor :: new m_currentObject: " + interactableObject.name);
            }
            else
            {
                m_currentObject.StopHovering();
            }

            m_currentObject = interactableObject;
            interactableObject.StartHovering(this);
        }
        catch (Exception e) { Debug.LogError("InteractionActor [" + gameObject.name + "] :: " + e); }
    }

    protected virtual void Invoke_StopHovering(InteractableObject interactableObject)
    {
        if (m_currentObject == interactableObject)
        {
            m_currentObject = null;
        }

        try
        {
            interactableObject.StopHovering();
        }
        catch (Exception e) { Debug.LogError("InteractionActor [" + gameObject.name + "] :: " + e); }
    }
}
