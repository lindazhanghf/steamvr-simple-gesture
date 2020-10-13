using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandGestureActor : InteractionActor {
    [Header("HandGestureActor")]
    public HandTracker TrackingHand;
    public int FullCircleAngle = 360; // in degree
    public Transform finger_index_end;

    [Header("Debug")]
    public GameObject DebugHit;

    protected GestureStateMachine m_GestureStateMachine;
    private Transform finger_index_2;

    void Start()
    {
        finger_index_2 = finger_index_end.parent;
        StartCoroutine(WaitForTrackingHand());
    }

    IEnumerator WaitForTrackingHand()
    {
        while (!TrackingHand.IsTracking) yield return new WaitForSeconds(1f);
        Debug.Log("TrackingHand is enabled :: TrackingHand.IsTracking - " + TrackingHand.Hand.ToString());

        m_GestureStateMachine = new GestureStateMachine(this);
    }

    void Update()
    {
        if (m_GestureStateMachine == null) return;

        m_GestureStateMachine.Execute();

        /* The following code should only be executed on one hand (right) */
        if (TrackingHand.Hand == HandTracker.HandType.Left) return;

        /// [Gesture] UNDO / halt
        if (HandTracker.LeftHand.PalmForward && HandTracker.LeftHand.PalmOpen 
            && HandTracker.RightHand.PalmForward && HandTracker.RightHand.PalmOpen)
        {
            // TODO: check if there is an action that can be undone
            Debug.Log("HandGestureActor :: [Gesture] UNDO");
        }
    }

    public void StartHovering(InteractableObject newInteractableObj)
    {
        if (m_debuging) Debug.LogWarning("HandGestureActor :: StartHovering");
        Invoke_StartHovering(newInteractableObj);
    }

    public void StopHovering()
    {
        if (m_currentObject)
        {
            if (m_debuging) Debug.LogWarning("HandGestureActor :: StopHovering");
            Invoke_StopHovering(m_currentObject);
        }
    }

    public void Activate()
    {
        Invoke_Activation();
    }

    public void ThrowAction(Vector3 throwDirection)
    {
        Invoke_Interaction();
        Invoke_StopHovering(m_currentObject);
    }

    private Collider FindHitObject()
    {
        RaycastHit raycastHit;
        // Debug.DrawRay(TrackingHand.IndexFingerTip.position, Vector3.forward * 10, Color.red, 10);

        if (Physics.Raycast(finger_index_2.position, finger_index_end.position - finger_index_2.position, out raycastHit, 400))
        {
            if (DebugHit) DebugHit.transform.position = raycastHit.point;
            return raycastHit.collider;
        }
        return null;
    }
}