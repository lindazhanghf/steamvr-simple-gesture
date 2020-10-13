using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Valve.VR;

public class HandTracker : MonoBehaviour
{
    public static HandTracker LeftHand { get; private set; }
    public static HandTracker RightHand { get; private set; }
    public enum HandType : int
    {
        Left = -1,
        Right = 1,
        Any = 0,
    }

    public bool IsTracking { get; private set; }

    [Header("SteamVR References")]
    public HandType Hand;
    private SteamVR_Action_Skeleton m_skeletonAction;


    [Header("Hand Gesture")]
    public FingerGestureSetting FingerSetting;
    public Transform Wrist;
    [Range(0f, 5f)]
    public float PalmOpenThreshold = 1f; // sum of 5 fingers curl value
    public bool PalmOpen
    {
        get { return SumFingerCurls() < PalmOpenThreshold; }
    }
    public bool PalmForward => Wrist.eulerAngles.x > 275f && Wrist.eulerAngles.x < 295f; // TODO: use head direction
    public bool IndexFingerPoint
    {
        get 
        {
            return (FingerSetting.IsStraight(m_skeletonAction.indexCurl, SteamVR_Skeleton_FingerIndexEnum.index)
            && FingerSetting.IsCurl(m_skeletonAction.thumbCurl, SteamVR_Skeleton_FingerIndexEnum.thumb)
            && FingerSetting.IsCurl(m_skeletonAction.middleCurl, SteamVR_Skeleton_FingerIndexEnum.middle)
            && FingerSetting.IsCurl(m_skeletonAction.ringCurl, SteamVR_Skeleton_FingerIndexEnum.ring)
            && FingerSetting.IsCurl(m_skeletonAction.pinkyCurl, SteamVR_Skeleton_FingerIndexEnum.pinky));
        }
    }
    public bool Fist
    {
        get 
        {
            return (FingerSetting.IsCurl(m_skeletonAction.indexCurl, SteamVR_Skeleton_FingerIndexEnum.index)
            && FingerSetting.IsCurl(m_skeletonAction.thumbCurl, SteamVR_Skeleton_FingerIndexEnum.thumb)
            && FingerSetting.IsCurl(m_skeletonAction.middleCurl, SteamVR_Skeleton_FingerIndexEnum.middle)
            && FingerSetting.IsCurl(m_skeletonAction.ringCurl, SteamVR_Skeleton_FingerIndexEnum.ring)
            && FingerSetting.IsCurl(m_skeletonAction.pinkyCurl, SteamVR_Skeleton_FingerIndexEnum.pinky));
        }
    }
    private float m_sumFingerCurls;


    [Header("Trace Match")]
    public float RadiusThreshold = 0.01f;
    public float MaxCircleRadius = 0.25f;
    public float MinCircleRadius = 0.05f;
    public int NumFramesAllowed = 25;
    private bool m_enableTraceMatch;
    public bool EnableTraceMatch
    {
        set
        {
            m_enableTraceMatch = value;
            if (!value) ResetDebug();
        }
    }

    private Coroutine m_TraceMatchCoroutine;
    private Vector3[] m_frames = new Vector3[30];
    private int m_currFrame = 0;
    private float m_circleRadius;
    private float m_RadiusUpperBound
    {
        get { return m_circleRadius + RadiusThreshold; }
    }
    private float m_RadiusLowerBound
    {
        get { return m_circleRadius - RadiusThreshold; }
    }


    [Header("Trace Match - Curve")]
    public CurveType CurveDetection;
    public enum CurveType
    {
        Circle, NonCircle
    }
    public float ContinousCurveAngle { get; private set; }
    private Vector3 m_curveLastFrame = Vector3.zero;
    private Vector3 m_curveStartCenter = Vector3.zero;
    private int m_curveStartFrameIndex;


    [Header("Trace Match - Debug ")]
    public Transform IndexFingerTip;
    public Transform CenterSphere;
    public Color DebugColor = Color.red;
    private float m_currentCurveAngle;
    private Material m_debugMaterial;
    private bool m_debugReset;


    void Awake()
    {
        if (Hand == HandType.Left) LeftHand = this;
        else if (Hand == HandType.Right) RightHand = this;

        m_debugMaterial = GetComponent<MeshRenderer>().material;

        // TODO: get using SteamVR_Input_Sources
        m_skeletonAction = GetComponentInParent<SteamVR_Behaviour_Skeleton>().skeletonAction;
    }

    void OnEnable()
    {
        StartCoroutine(InitializeHandDataCollection());
    }

    void OnDisable()
    {
        if (m_TraceMatchCoroutine != null) StopCoroutine(m_TraceMatchCoroutine);
    }

    public static HandTracker GetHandByType(HandType type)
    {
        if (type == HandType.Left) return LeftHand;
        if (type == HandType.Right) return RightHand;
        return null;
    }

    private IEnumerator InitializeHandDataCollection()
    {
        Vector3 originalPos = transform.position;
        while (transform.position == originalPos)
        {
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("TraceMatch : start receiving data");
        IsTracking = true;

        m_currFrame = 0;
        while (m_currFrame < m_frames.Length)
        {
            m_frames[m_currFrame] = transform.position;
            m_currFrame++;
            yield return new WaitForSeconds(0.03f); // 30 fps
        }
        Debug.Log("TraceMatch : finished collecting first set of frames");

        m_TraceMatchCoroutine = StartCoroutine(HandDataCollection());
    }

    private IEnumerator HandDataCollection()
    {
        m_currFrame = 0;
        while (true)
        {
            m_frames[m_currFrame] = transform.position;

            if (m_enableTraceMatch)
            {
                TraceMatch();
            }

            // Increment m_currFrame
            m_currFrame++;
            if (m_currFrame == m_frames.Length) m_currFrame = 0;

            yield return new WaitForSeconds(0.03f); // 30 fps
        }
    }

    private void TraceMatch()
    {
        Vector3 center = CalculateCenterOfCircle(m_frames[m_currFrame], m_frames[(m_currFrame + 10)%30], m_frames[(m_currFrame + 20)%30]);

        Vector3 v_currFrame_center = m_frames[m_currFrame] - center;
        Vector3 v_lastFrame_center = m_frames[m_currFrame == 0 ? 29 : m_currFrame - 1] - center; // Earliest frame in this set
        m_currentCurveAngle = Vector3.Angle(v_currFrame_center, v_lastFrame_center);

        int numFramesWithinThreshold = 0;
        if (center != Vector3.zero && Vector3.Angle(v_currFrame_center, v_lastFrame_center) > 5)
        {
            foreach (Vector3 frame in m_frames)
            {
                if (RadiusWithinThreshold(Vector3.Distance(frame, center))) numFramesWithinThreshold++;
            }

            if (numFramesWithinThreshold > NumFramesAllowed)
            {
                if (numFramesWithinThreshold == 30) Debug.Log(m_circleRadius);
                else Debug.Log(Hand.ToString() + " within = " + numFramesWithinThreshold);
                m_debugMaterial.color = DebugColor;

                CalculateContinousCurve(center, v_currFrame_center);
            }
            else
            {
                Debug.Log(Hand.ToString() + " < 25");
                ResetDebug();
            }
        }
    }

    /// <summary>
    /// Finding the circumscribed circle using 3 points in 3D space
    /// Reference: https://stackoverflow.com/a/13992781 
    /// </summary>
    private Vector3 CalculateCenterOfCircle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // triangle "edges"
        Vector3 t = p2-p1;
        Vector3 u = p3-p1;
        Vector3 v = p3-p2;

        // triangle normal
        Vector3 w = Vector3.Cross(t, u); 
        float wsl = w.sqrMagnitude;

        // area of the triangle is too small
        if (Mathf.Approximately(wsl, 0f)) return Vector3.zero;

        // helpers
        float iwsl2 = 1.0f / (2.0f * wsl);
        float tt = Vector3.Dot(t, t);
        float uu = Vector3.Dot(u, u);
        float vv = Vector3.Dot(v, v);
        float uv = Vector3.Dot(u, v);
        float tv = Vector3.Dot(t, v);

        // NOTE: radius of the circle is saved in the class variable for other calculation
        m_circleRadius = Mathf.Sqrt(tt * uu * (vv) * iwsl2 * 0.5f);
        if (m_circleRadius < MinCircleRadius || m_circleRadius > MaxCircleRadius)
        {
            // Debug.Log("TraceMatch :: m_circleRadius too small");
            ResetDebug();
            return Vector3.zero;
        }

        // result circle
        Vector3 circCenter = p1 + (u*tt*(uv) - t*uu*(tv)) * iwsl2;
        if (CenterSphere) CenterSphere.position = circCenter;
        m_debugReset = false;
        return circCenter;
    }

    private void CalculateContinousCurve(Vector3 centerPos, Vector3 currFrameVector)
    {
        if (m_currFrame % 5 != 0) return; // Check ContinousCurve every 5 frames
        Debug.Log(m_currFrame);

        if (m_curveStartFrameIndex < 0) // Start new curve
        {
            ContinousCurveAngle = m_currentCurveAngle;

            m_curveStartCenter = centerPos;
            m_curveStartFrameIndex = m_currFrame;
            m_curveLastFrame = m_frames[m_currFrame];
            return;
        }

        // Curve is not continous anymore (center of curve has shifted)
        if (!DistanceWithinThreshold(m_curveStartCenter, centerPos))
        {
            ResetContinousCurve();
            return;
        }

        Vector3 lastFrameVector = m_curveLastFrame - centerPos;
        ContinousCurveAngle += Vector3.Angle(lastFrameVector, currFrameVector);
    }
    
    private void ResetContinousCurve()
    {
        Debug.Log("HandTracker :: ResetContinousCurve, last ContinousCurveAngle = " + ContinousCurveAngle);
        m_curveStartFrameIndex = -1;
        ContinousCurveAngle = 0f;
    }

    private void ResetDebug()
    {
        if (m_debugReset) return;

        Debug.Log("HandTracker :: ResetDebug");
        m_debugMaterial.color = Color.white;
        if (CenterSphere)
        {
            CenterSphere.localPosition = Vector3.zero;
        }

        m_debugReset = true;
        ResetContinousCurve();
    }

    /// Helper Functions ///
    public Vector3 GetLastCircleDirection()
    {
        return m_curveLastFrame = m_curveStartCenter;
    }

    private float SumFingerCurls()
    {
        m_sumFingerCurls = 0;
        foreach (var fingerCurlVal in m_skeletonAction.fingerCurls)
        {
            m_sumFingerCurls += fingerCurlVal;
        }

        return m_sumFingerCurls;
    }

    private bool RadiusWithinThreshold(float radius)
    {
        return radius < m_RadiusUpperBound && radius > m_RadiusLowerBound;
    }

    private bool DistanceWithinThreshold(Vector3 v1, Vector3 v2)
    {
        return Vector3.Distance(v1, v2) < RadiusThreshold;
    }
}
