using System;
using UnityEngine;

public abstract class InteractionActor : MonoBehaviour
{
    [Header("InteractionActor")]
    protected InteractableObject m_currentObject;

    protected bool m_debuging = false;

 #if UNITY_EDITOR || DEVELOPMENT_BUILD 
    public bool EnableDebuging = false;
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
        // The first object that the user touches becomes m_currentObject
        if (m_currentObject == null)
        {
            m_currentObject = interactableObject;
            if (m_debuging) Debug.LogWarning("InteractionActor :: new m_currentObject: " + m_currentObject.name);
        }
        // Sometimes OnTriggerEnter() might be called twice without properly OnTriggerExit()
        else if (m_currentObject == interactableObject)
        {
            return;
        }

        try
        {
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
