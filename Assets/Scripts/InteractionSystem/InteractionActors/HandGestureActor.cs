using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandGestureActor : InteractionActor {
    [Header("HandGestureActor")]
    public HandTracker TrackingHand;
    [Range(0, 5f)]
    public float GestureTransitionBuffer_s = 0.5f;

    private InteractableObject m_currentPointing;
    private bool m_startActivation;

    private Coroutine m_clearCurrentPointingCoroutine;
    // protected Coroutine clearCurrentPointingCoroutine
    // {
    //     set {
    //         if (value == null) StopCoroutine(m_clearCurrentPointingCoroutine);
    //         m_clearCurrentPointingCoroutine = value;
    //     }
    // }

    void Update()
    {
        if (TrackingHand == null || !TrackingHand.IsTracking) return;

        if (m_startActivation)
        {
            if (TrackingHand.PalmOpen)
            {
                // Check if full circle
                Debug.Log("HandGestureActor :: START Activation - Drawing circles");
            }
            else
            {
                TrackingHand.EnableTraceMatch = false;
                m_startActivation = false;
                Debug.Log("HandGestureActor :: STOP Activation");
            }
        }

        if (m_currentPointing && m_currentPointing.IsHovering && TrackingHand.PalmOpen)
        {
            m_startActivation = true;
            TrackingHand.EnableTraceMatch = true;
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
            if (m_debuging) Debug.Log("HandGestureActor :: ClearCurrentPointing & Invoke_StopHovering");
            Invoke_StopHovering(m_currentPointing);
            m_currentPointing = null;
        }
    }
    /* ClearCurrentPointing - END */

    private Collider FindHitObject()
    {
        RaycastHit raycastHit;
        // Debug.DrawRay(TrackingHand.IndexFingerTip.position, Vector3.forward * 10, Color.red, 10);

        if (Physics.Raycast(TrackingHand.IndexFingerTip.position, Vector3.forward, out raycastHit, 400))
        {
            return raycastHit.collider;
        }
        return null;
    }
}