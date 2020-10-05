using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandGestureActor : InteractionActor {
    [Header("HandGestureActor")]
    public HandTracker TrackingHand;
    public int FullCircleAngle = 360; // in degree
    public int ThrowCurveAngle = 90; // in degree
    [Range(0, 5f)]
    public float GestureTransitionBuffer_s = 0.25f;
    public Transform finger_index_end;

    [Header("Debug")]
    public GameObject DebugHit;

    private InteractableObject m_currentPointing;
    private Transform finger_index_2;
    private bool m_startActivation;
    private bool m_startThrowing;
    private Coroutine m_clearCurrentPointingCoroutine;

    void Start()
    {
        finger_index_2 = finger_index_end.parent;
    }

    void Update()
    {
        if (TrackingHand == null || !TrackingHand.IsTracking) return;

        if (m_startThrowing)
        {
            if (TrackingHand.ContinousCurveAngle > ThrowCurveAngle && TrackingHand.PalmOpen)
            {
                var direction = TrackingHand.GetLastCircleDirection();
                Debug.LogError("THROWN----------------- " + direction);

                var rb = m_currentPointing.gameObject.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.AddForce(Vector3.Normalize(direction) * 50);

                ClearCurrentPointing();
                TrackingHand.MaxCircleRadius = 0.25f;
                TrackingHand.RadiusThreshold = 0.03f;
                m_startThrowing = false;
                TrackingHand.EnableTraceMatch = false;
            }
        }

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

        if (m_currentPointing && m_currentPointing.IsActivated)
        {
            Debug.Log("HandGestureActor :: detect throwing...");

            // Demo only
            if (TrackingHand.Fist)
            {
                m_startThrowing = true;
                TrackingHand.MaxCircleRadius = 0.6f;
                TrackingHand.RadiusThreshold = 0.2f;
                // ClearCurrentPointing();
            }
            return;
        }

        if (m_currentPointing && m_currentPointing.IsHovering && TrackingHand.PalmOpen)
        {
            m_startActivation = true;
            TrackingHand.EnableTraceMatch = true;

            if (m_clearCurrentPointingCoroutine != null) StopCoroutine(m_clearCurrentPointingCoroutine);
            m_clearCurrentPointingCoroutine = null;
            return;
        }
        
        if (TrackingHand.IndexFingerPoint)
        {
            TrackingHand.IndexFingerTip.gameObject.SetActive(true);

            Collider hitObj = FindHitObject();
            if (hitObj)
            {
                InteractableObject interactableObj = hitObj.GetComponent<InteractableObject>();
                // if (m_debuging) Debug.Log("HandGestureActor :: HIIIIIIITING " + hitObj.name + " " + interactableObj != null);
                if (interactableObj == null) // The object hit is not an interactable object
                {
                    Delay_ClearCurrentPointing();
                    return;
                }

                if (m_currentPointing)
                {
                    if (m_currentPointing == interactableObj) return; // Pointing at the same object

                    Invoke_StopHovering(m_currentPointing);
                    if (m_clearCurrentPointingCoroutine != null) StopCoroutine(m_clearCurrentPointingCoroutine);
                    m_clearCurrentPointingCoroutine = null;
                }

                Invoke_StartHovering(interactableObj);
                m_currentPointing = interactableObj;
            }
            else
            {
                Delay_ClearCurrentPointing();
            }
        }
        else
        {
            TrackingHand.IndexFingerTip.gameObject.SetActive(false);
            Delay_ClearCurrentPointing();
        }
    }

    /* ClearCurrentPointing - START */
    private void Delay_ClearCurrentPointing()
    {
        if (m_currentPointing == null) return;
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
    private void ClearCurrentPointing()
    {
        if (m_currentPointing)
        {
            if (m_debuging) Debug.LogWarning("HandGestureActor :: ClearCurrentPointing & Invoke_StopHovering");
            Invoke_StopHovering(m_currentPointing);
            m_currentPointing = null;
        }
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