using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureSystem : MonoBehaviour
{
    public enum HandSetting : int
    {
        Left = 0,
        Right = 1,
        Any = 100,
        Opposite = -1
    }
    public enum Orientation : int
    {
        Left = 0,
        Right = 1,
        Default = 0
    }

    public enum BodyPart
    {
        Body, Shoulder
    }

    [Serializable]
    public struct HandStat
    {
        public HandSetting Setting;
        public bool SameSideOfBody;
        public bool OnShoulder;
        public bool AboveHead;
    }
    [Serializable]
    public struct GestureSet
    {
        public string Name;
        public HandSetting Setting;
        public HandStat[] KeyFrames;
        public  int _keyFrameIndex;
    }

    public GestureSet[] Gestures;

    public HandTracker[] Hands; // Make sure left hand at index 0, right hand at index 1!

    private HandTracker[] m_handLeft;
    private HandTracker[] m_handRight;

    void Start()
    {
        m_handLeft = new HandTracker[] { Hands[(int)HandSetting.Left] };
        m_handRight = new HandTracker[] { Hands[(int)HandSetting.Right] };
    }

    void Update()
    {
        for (int i = 0; i < Gestures.Length; i++)
        {
            bool match = true;
            int frameIndex = Gestures[i]._keyFrameIndex;

            foreach (HandTracker hand in GetHandTrackersToCheck(Gestures[i].Setting))
            {
                // if (hand.SameSideOfBody == Gestures[i].KeyFrames[frameIndex].SameSideOfBody &&
                //     hand.OnShoulder == Gestures[i].KeyFrames[frameIndex].OnShoulder &&
                //     hand.AboveHead == Gestures[i].KeyFrames[frameIndex].AboveHead)
                // { }
                // else
                // {
                //     match = false;
                //     break;
                // }
            }

            if (match)
            {
                Gestures[i]._keyFrameIndex = frameIndex + 1;
            }

            if (Gestures[i]._keyFrameIndex == Gestures[i].KeyFrames.Length) // Gesture Complete!!
            {
                Debug.Log("GestureSystem :: Check Gestures :: !!!!!!!!!!!! Gesture Matched:" + Gestures[i].Name);
                Gestures[i]._keyFrameIndex = 0;
            }
        }
    }

    private HandTracker[] GetHandTrackersToCheck(HandSetting setting)
    {
        switch (setting)
        {
            case HandSetting.Right:
                return m_handRight;
            case HandSetting.Left:
                return m_handLeft;
            default:
                return Hands;
        }
    }
}
