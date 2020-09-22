using System;
using System.Collections;
using UnityEngine;
using Valve.VR;

[CreateAssetMenu(fileName = "FingerGestureSetting", menuName = "GestureSystem/FingerGestureSetting")]
public class FingerGestureSetting : ScriptableObject
{
    [Serializable]
    public struct FingerSetting
    {
        // public string name; TODO: display finger name in editor
        public SteamVR_Skeleton_FingerIndexEnum Index;
        [Range(0f, 1f)]
        public float FingerStraightThreshold;
        [Range(0f, 1f)]
        public float FingerCurlThreshold;
    }

    public FingerSetting[] FingerSettings = new FingerSetting[5];

    public bool IsCurl(float curlVal, SteamVR_Skeleton_FingerIndexEnum finger)
    {
        return curlVal > FingerSettings[(int)finger].FingerStraightThreshold;
    }

    public bool IsStraight(float curlVal, SteamVR_Skeleton_FingerIndexEnum finger)
    {
        return curlVal < FingerSettings[(int)finger].FingerStraightThreshold;
    }
}
