using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandGestureActor : InteractionActor {
    [Header("HandGestureActor")]
    public HandTracker TrackingHand;
    public int FullCircleAngle = 360; // in degree
    [Range(0, 5f)]
    public float GestureTransitionBuffer_s = 0.25f;
    public Transform finger_index_end;

    [Header("Debug")]
    public GameObject DebugHit;

    protected GestureStateMachine m_GestureStateMachine;
    private InteractableObject m_currInteractiveObject;
    private Transform finger_index_2;
    private bool m_startActivation;
    private Coroutine m_clearCurrentPointingCoroutine;

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
        m_GestureStateMachine.Initialize((int)GestureStateMachine.StateID.Idle);
    }

    void Update()
    {
        if (m_GestureStateMachine == null) return;

        m_GestureStateMachine.Execute();
        return;

        if (m_startActivation)
        {
            if (TrackingHand.PalmOpen)
            {
                // Debug.Log("HandGestureActor :: START Activation - Drawing circles");

                if (TrackingHand.ContinousCurveAngle > FullCircleAngle)
                {
                    Invoke_Activation();
                    m_startActivation = false;
                    Debug.LogWarning("HandGestureActor :: DONE Activation !!!!!!!!!!!!! " + TrackingHand.ContinousCurveAngle);
                }
            }
            else
            {
                TrackingHand.EnableTraceMatch = false;
                m_startActivation = false;
                Debug.LogWarning("HandGestureActor :: STOP Activation");
            }

            return;
        }

        if (m_currInteractiveObject && m_currInteractiveObject.IsActivated)
        {
            Debug.Log("HandGestureActor :: detect throwing...");
            return;
        }

        if (m_currInteractiveObject && m_currInteractiveObject.IsHovering && TrackingHand.PalmOpen)
        {
            m_startActivation = true;
            TrackingHand.EnableTraceMatch = true;

            if (m_clearCurrentPointingCoroutine != null) StopCoroutine(m_clearCurrentPointingCoroutine);
            m_clearCurrentPointingCoroutine = null;
            return;
        }
    }

    public void StartHovering(InteractableObject newInteractableObj)
    {
        m_currInteractiveObject = newInteractableObj;
        Invoke_StartHovering(newInteractableObj);
    }

    /* ClearCurrentPointing - START */
    public void ClearCurrentPointing()
    {
        if (m_currInteractiveObject)
        {
            if (m_debuging) Debug.LogWarning("HandGestureActor :: ClearCurrentPointing & Invoke_StopHovering");
            Invoke_StopHovering(m_currInteractiveObject);
            m_currInteractiveObject = null;
        }

        if (m_clearCurrentPointingCoroutine != null) StopCoroutine(m_clearCurrentPointingCoroutine);
        m_clearCurrentPointingCoroutine = null;
    }
    public void Delay_ClearCurrentPointing()
    {
        if (m_currInteractiveObject == null) return;
        if (m_clearCurrentPointingCoroutine != null) return; // already running

        if (m_debuging) Debug.Log("HandGestureActor :: Delay_ClearCurrentPointing -- delaying for " + GestureTransitionBuffer_s);
        m_clearCurrentPointingCoroutine = StartCoroutine(ClearCurrentPointingCoroutine());
    }
    private IEnumerator ClearCurrentPointingCoroutine()
    {
        yield return new WaitForSeconds(GestureTransitionBuffer_s);
        ClearCurrentPointing();
        m_clearCurrentPointingCoroutine = null;
    }
    /* ClearCurrentPointing - END */

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