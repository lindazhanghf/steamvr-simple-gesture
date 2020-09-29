using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandGestureActor : InteractionActor {
    public HandTracker TrackingHand;

    private InteractableObject m_currentPointing;

    void Update()
    {
        if (TrackingHand == null || !TrackingHand.IsTracking) return;

        if (TrackingHand.IndexFingerPoint)
        {
            TrackingHand.IndexFingerTip.gameObject.SetActive(true);

            Collider hitObj = FindHitObject();
            if (hitObj)
            {
                InteractableObject interactableObj = hitObj.GetComponent<InteractableObject>();
                if (m_debuging) Debug.Log("HIIIIIIITING " + hitObj.name + " " + interactableObj != null);
                if (interactableObj == null) // The object hit is not an interactable object
                {
                    ClearCurrentHovering();
                    return;
                }
                else
                {
                    Invoke_StartHovering(interactableObj);
                    m_currentPointing = interactableObj;
                }
            }
        }
        else
        {
            TrackingHand.IndexFingerTip.gameObject.SetActive(false);
            ClearCurrentHovering();
        }
    }

    private void ClearCurrentHovering()
    {
        if (m_currentPointing)
        {
            Invoke_StopHovering(m_currentPointing);
            m_currentPointing = null;
        }
    }

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