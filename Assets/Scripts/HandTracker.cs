using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class HandTracker : MonoBehaviour
{
    public enum HandType : int
    {
        Left = 0,
        Right = 1,
        Any = 100,
    }

    [Header("SteamVR References")]
    public Transform Camera;
    public HandType Hand;

    [Header("Tracking Data")]
    public bool SameSideOfBody;
    public bool OnShoulder;
    [ShowNativeProperty]
    public bool AboveHead
    {
        get { return transform.position.y > Camera.position.y; }
    }

    void OnTriggerEnter(Collider other)
    {
        GestureTrigger gestureTrigger = other.GetComponent<GestureTrigger>();
        if (gestureTrigger == null) return;

        if (gestureTrigger.Orientation != GestureSystem.Orientation.Default)
        {
            SameSideOfBody = (int)Hand == (int) gestureTrigger.Orientation;
        }

        if (gestureTrigger.Part == GestureSystem.BodyPart.Shoulder) OnShoulder = true;
    }

    void OnTriggerExit(Collider other)
    {
        GestureTrigger gestureTrigger = other.GetComponent<GestureTrigger>();
        if (gestureTrigger == null) return;

        if (gestureTrigger.Part == GestureSystem.BodyPart.Shoulder) OnShoulder = false;
    }}
