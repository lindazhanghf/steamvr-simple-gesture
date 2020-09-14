﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

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

    [Header("Tracking Data")]
    public bool SameSideOfBody;
    public bool OnShoulder;
    [ShowNativeProperty]
    public bool AboveHead
    {
        get { return transform.position.y > Camera.position.y; }
    }

    // TraceMatch
    public float TraceMatch_Threshold = 0.01f;
    public float MaxCircleRadius = 0.25f;
    public float MinCircleRadius = 0.05f;
    public int numFramesAllowed = 25;
    private Coroutine TraceMatchCoroutine;
    private Vector3[] frames = new Vector3[30];
    private int currFrame = 0;
    public float circleRadius;
    private float RadiusUpperBound
    {
        get { return circleRadius + TraceMatch_Threshold; }
    }
    private float RadiusLowerBound
    {
        get { return circleRadius - TraceMatch_Threshold; }
    }

    // TraceMatch - Debug
    public Transform CenterSphere;
    public Color DebugColor = Color.red;
    public float temp_Angle;
    private Material material;

    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    void OnEnable()
    {
        TraceMatchCoroutine = StartCoroutine(TraceMatch());
    }
    
    void OnDisable()
    {
        StopCoroutine(TraceMatchCoroutine);
    }

    private IEnumerator TraceMatch()
    {
        int numFramesWithinThreshold = 0;
        Vector3 originalPos = transform.position;
        while (transform.position == originalPos)
        {
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("TraceMatch : start receiving data");

        currFrame = 0;
        while (currFrame < frames.Length)
        {
            frames[currFrame] = transform.position;
            currFrame++;
            yield return new WaitForSeconds(0.03f); // 30 fps
        }
        Debug.Log("TraceMatch : collected first set of frames");

        currFrame = 0;
        while (true)
        {
            frames[currFrame] = transform.position;

            // TraceMatch
            Vector3 center = CalculateCenterOfCircle(frames[currFrame], frames[(currFrame + 10)%30], frames[(currFrame + 20)%30]);

            Vector3 v_currFrame_center = frames[currFrame] - center;
            Vector3 v_lastFrame_center = frames[currFrame == 0 ? 29 : currFrame - 1] - center;
            temp_Angle = Vector3.Angle(v_currFrame_center, v_lastFrame_center);

            if (center != Vector3.zero && Vector3.Angle(v_currFrame_center, v_lastFrame_center) > 5)
            {
                foreach (Vector3 frame in frames)
                {
                    if (WithinRadiusThreshold(Vector3.Distance(frame, center))) numFramesWithinThreshold++;
                }

                if (numFramesWithinThreshold > numFramesAllowed)
                {
                    if (numFramesWithinThreshold == 30) Debug.Log(circleRadius);
                    else Debug.Log(Hand.ToString() + " within = " + numFramesWithinThreshold);
                    material.color = DebugColor;
                }
                else
                {
                    Debug.Log(Hand.ToString() + " < 25");
                    material.color = Color.white;
                }
            }

            // Increment currFrame
            currFrame++;
            if (currFrame == frames.Length) currFrame = 0;

            yield return new WaitForSeconds(0.03f); // 30 fps
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
        circleRadius = Mathf.Sqrt(tt * uu * (vv) * iwsl2 * 0.5f);
        if (circleRadius < MinCircleRadius || circleRadius > MaxCircleRadius)
        {
            // Debug.Log("TraceMatch :: circleRadius too small");
            if (CenterSphere) CenterSphere.localPosition = Vector3.zero;
            material.color = Color.white;
            return Vector3.zero;
        }

        // result circle
        Vector3 circCenter = p1 + (u*tt*(uv) - t*uu*(tv)) * iwsl2;
        if (CenterSphere) CenterSphere.position = circCenter;
        return circCenter;
    }

    private bool WithinRadiusThreshold(float distance)
    {
        return distance < RadiusUpperBound && distance > RadiusLowerBound;
    }
}
