using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Valve.VR;

public class HandTracker : MonoBehaviour
{
    public enum HandType : int
    {
        Left = -1,
        Right = 1,
        Any = 0,
    }

    [Header("SteamVR References")]
    public Transform Camera;
    public HandType Hand;
    private SteamVR_Action_Skeleton m_skeletonAction;

    [Header("Hand Tracking")]
    [Range(0f, 0.5f)]
    public float PalmOpenThreshold = 1f;
    public bool PalmOpen
    {
        get { return SumFingerCurls() < PalmOpenThreshold; }
    }
    private float m_sumFingerCurls;

    [Header("Tracking Data")]
    public bool SameSideOfBody;
    public bool OnShoulder;
    [ShowNativeProperty]
    public bool AboveHead
    {
        get { return transform.position.y > Camera.position.y; }
    }



    [Header("Trace Match")]
    public float TraceMatch_Threshold = 0.01f;
    public float MaxCircleRadius = 0.25f;
    public float MinCircleRadius = 0.05f;
    public int numFramesAllowed = 25;
    private Coroutine m_TraceMatchCoroutine;
    private Vector3[] m_frames = new Vector3[30];
    private int m_currFrame = 0;
    private float m_circleRadius;
    private float m_RadiusUpperBound
    {
        get { return m_circleRadius + TraceMatch_Threshold; }
    }
    private float m_RadiusLowerBound
    {
        get { return m_circleRadius - TraceMatch_Threshold; }
    }

    [Header("Trace Match - Debug")]
    public Transform CenterSphere;
    public Color DebugColor = Color.red;
    public float temp_Angle;
    private Material m_debugMaterial;

    void Awake()
    {
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

    private IEnumerator InitializeHandDataCollection()
    {
        Vector3 originalPos = transform.position;
        while (transform.position == originalPos)
        {
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("TraceMatch : start receiving data");

        m_currFrame = 0;
        while (m_currFrame < m_frames.Length)
        {
            m_frames[m_currFrame] = transform.position;
            m_currFrame++;
            yield return new WaitForSeconds(0.03f); // 30 fps
        }
        Debug.Log("TraceMatch : collected first set of m_frames");

        m_TraceMatchCoroutine = StartCoroutine(HandDataCollection());
    }

    private IEnumerator HandDataCollection()
    {
        m_currFrame = 0;
        while (true)
        {
            m_frames[m_currFrame] = transform.position;

            if (PalmOpen)
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

        Vector3 v_m_currFrame_center = m_frames[m_currFrame] - center;
        Vector3 v_lastFrame_center = m_frames[m_currFrame == 0 ? 29 : m_currFrame - 1] - center;
        temp_Angle = Vector3.Angle(v_m_currFrame_center, v_lastFrame_center);

        int numFramesWithinThreshold = 0;
        if (center != Vector3.zero && Vector3.Angle(v_m_currFrame_center, v_lastFrame_center) > 5)
        {
            foreach (Vector3 frame in m_frames)
            {
                if (WithinRadiusThreshold(Vector3.Distance(frame, center))) numFramesWithinThreshold++;
            }

            if (numFramesWithinThreshold > numFramesAllowed)
            {
                if (numFramesWithinThreshold == 30) Debug.Log(m_circleRadius);
                else Debug.Log(Hand.ToString() + " within = " + numFramesWithinThreshold);
                m_debugMaterial.color = DebugColor;
            }
            else
            {
                Debug.Log(Hand.ToString() + " < 25");
                m_debugMaterial.color = Color.white;
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
            if (CenterSphere) CenterSphere.localPosition = Vector3.zero;
            m_debugMaterial.color = Color.white;
            return Vector3.zero;
        }

        // result circle
        Vector3 circCenter = p1 + (u*tt*(uv) - t*uu*(tv)) * iwsl2;
        if (CenterSphere) CenterSphere.position = circCenter;
        return circCenter;
    }

    /// Helper Functions ///
    private float SumFingerCurls()
    {
        m_sumFingerCurls = 0;
        foreach (var fingerCurlVal in m_skeletonAction.fingerCurls)
        {
            m_sumFingerCurls += fingerCurlVal;
        }

        return m_sumFingerCurls;
    }

    private bool WithinRadiusThreshold(float distance)
    {
        return distance < m_RadiusUpperBound && distance > m_RadiusLowerBound;
    }
}
