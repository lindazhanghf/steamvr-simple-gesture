using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandGestureActor : InteractionActor {
    public HandTracker TrackingHand;

    void Update()
    {
        if (TrackingHand == null || !TrackingHand.IsTracking) return;

        if (TrackingHand.IndexFingerPoint)
        {
            Debug.Log(FindHitObject());
            TrackingHand.IndexFingerTip.gameObject.SetActive(true);
        }
        else
        {
            TrackingHand.IndexFingerTip.gameObject.SetActive(false);
        }
    }

    private Vector3 FindHitObject()
    {
        Debug.Log(transform.position);
        RaycastHit raycastHit;
        Debug.DrawRay(TrackingHand.IndexFingerTip.position, Vector3.forward * 10, Color.red, 10);

        if (Physics.Raycast(TrackingHand.IndexFingerTip.position, Vector3.forward, out raycastHit, 400))
        {
            return raycastHit.point;
        }
        return Vector3.zero;
    }
}