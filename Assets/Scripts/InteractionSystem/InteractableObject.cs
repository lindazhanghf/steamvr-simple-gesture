using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 #if UNITY_EDITOR
using UnityEditor;
 #endif

// [RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour {
    public bool IsUsable = true;
    // Public getter variables
    public bool IsHovering { get; private set; }
    public bool IsActivated { get; private set; }
    public int ActivationDegree { get; private set; }

    /// InteractableEvents: used for InteractableBehavior to listen for these events
    public delegate void InteractableEvent();
    public List<InteractableEvent> OnInteractionEvents = new List<InteractableEvent>();
    public List<InteractableEvent> OnActivationEvents = new List<InteractableEvent>();
    public List<InteractableEvent> OnStartHoveringEvents = new List<InteractableEvent>();
    public List<InteractableEvent> OnStopHoveringEvents = new List<InteractableEvent>();

    protected InteractionActor m_currentActor;

    /// Call the InteractionActor to unsubscribe itself
    protected virtual void OnDisable()
    {
        if (IsHovering && m_currentActor != null)
        {
            m_currentActor.Unsubsribe(this);
        }
    }

    public virtual void Interaction()
    {
        if (!IsUsable) return;

        InvokeInteractableEvents(OnInteractionEvents);
    }

    public virtual void EndInteraction() {}

    public virtual void Activation()
    {
        IsActivated = true;
        InvokeInteractableEvents(OnActivationEvents);
    }

    public virtual void StartHovering(InteractionActor newActor = null)
    {
        m_currentActor = newActor;
        IsHovering = true;

        InvokeInteractableEvents(OnStartHoveringEvents);
    }

    public virtual void StopHovering()
    {
        m_currentActor = null;
        IsHovering = false;
        IsActivated = false;

        InvokeInteractableEvents(OnStopHoveringEvents);
    }

    private void InvokeInteractableEvents(List<InteractableEvent> events)
    {
        if (events == null) return;

        foreach (InteractableEvent interactableEvent in events)
        {
            if (interactableEvent != null)
            {
                try
                {
                    interactableEvent();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[" + gameObject.name + "] InteractableEvent-error: " + e);
                }
            }
        }
    }
}

 #if UNITY_EDITOR
[CustomEditor(typeof(InteractableObject))]
public class InteractableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        InteractableObject myScript = (InteractableObject)target;
        if (GUILayout.Button("Debug Interaction"))
        {
            myScript.Interaction();
        }
    }
}
 #endif