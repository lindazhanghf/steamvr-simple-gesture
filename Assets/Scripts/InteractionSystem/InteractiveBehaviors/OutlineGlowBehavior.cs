using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineGlowBehavior : InteractableBehavior
{
    static Color s_DefaultColor = Color.magenta;

    [Header("Outline")]
    public bool enableOutline = true;
    [SerializeField] private Color m_touchColor = Color.green;

    [Header("Glowing Effect")]
    [Tooltip("Make the outline to glow slowly to indicate interactivity")]
    [SerializeField] private bool m_enableGlow = true;
    [SerializeField] private Color m_glowColor = new Color(105 / 255f, 139 / 255f, 1f);
    private Coroutine m_ChangeOutlineCoroutine;

    [Header("Additional Models")]
    [SerializeField] private bool m_enableOutlineOnChildren = true;

    private Material m_outlineMat;
    private List<MeshRenderer> m_singleMaterialMesh;
    private List<MeshRenderer> m_multiMaterialMesh;
    private Color m_color;
    private float m_currOutlineThickness;

    protected override void Awake()
    {
        base.Awake();

        m_outlineMat = Resources.Load<Material>("ItemPickupOutline");
    }

    protected override void Start()
    {
        base.Start();

        m_singleMaterialMesh = new List<MeshRenderer>();
        m_multiMaterialMesh = new List<MeshRenderer>();

        List<MeshRenderer> allMeshes = new List<MeshRenderer>();
        // allMeshes.AddRange(_additionalMeshes);
        if (m_enableOutlineOnChildren)
            allMeshes.AddRange(GetComponentsInChildren<MeshRenderer>());

        foreach (MeshRenderer renderer in allMeshes)
        {
            if (renderer == null) continue;

            if (renderer.materials.Length == 1)
            {
                m_singleMaterialMesh.Add(renderer);
            }
            else
            {
                m_multiMaterialMesh.Add(CreateOutlineObject(renderer.gameObject));
            }
        }

        EnableOutline();
        SetOutlineThickness(0f);
        if (m_enableGlow) SetOutlineColor(m_glowColor);
    }

    protected override void OnStartHovering()
    {
        base.OnStartHovering();
        if (!enableOutline) return;

        Debug.Log("OnStartHovering" + gameObject.name);

        // SetOutlineColor(m_touchColor, 0.0025f);
        ChangeOutlineCoroutine(0.5f, m_touchColor, 0.0025f, s_DefaultColor);
    }

    protected override void OnStopHovering()
    {
        base.OnStopHovering();

        Debug.Log("OnStopHovering" + gameObject.name);
        // DisableOutline();
        ChangeOutlineCoroutine(0.5f, m_touchColor, 0f, s_DefaultColor);
    }

    private void EnableOutline()
    {
        foreach (MeshRenderer m in m_singleMaterialMesh)
        {
             m.materials = new Material[] { m.materials[0], m_outlineMat };
        }

        foreach (MeshRenderer m in m_multiMaterialMesh)
        {
            m.gameObject.SetActive(true);
        }
    }

    private void DisableOutline()
    {
        foreach (MeshRenderer m in m_singleMaterialMesh)
        {
            m.materials = new Material[] { m.materials[0] };
        }

        foreach (MeshRenderer m in m_multiMaterialMesh)
        {
            m.gameObject.SetActive(false);
        }
    }

    private void SetOutlineColor(Color color, float thickness = 0f)
    {
        m_color = color;

        foreach (MeshRenderer m in m_singleMaterialMesh)
        {
            m.materials[1].SetColor("g_vOutlineColor", color);
            m.materials[1].SetFloat("g_flOutlineWidth", thickness);
        }

        foreach (MeshRenderer m in m_multiMaterialMesh)
        {
            foreach (Material mat in m.materials)
            {
                mat.SetColor("g_vOutlineColor", color);
                mat.SetFloat("g_flOutlineWidth", thickness);
            }
        }
    }

    private void SetOutlineThickness(float thickness)
    {
        m_currOutlineThickness = thickness;

        foreach (MeshRenderer m in m_singleMaterialMesh)
        {
            m.materials[1].SetFloat("g_flOutlineWidth", thickness);
        }

        foreach (MeshRenderer m in m_multiMaterialMesh)
        {
            foreach (Material mat in m.materials)
            {
                mat.SetFloat("g_flOutlineWidth", thickness);
            }
        }
    }

    /// <param name="startColor">If s_DefaultColor (or Color.magenta) is provided, current color will be used instead.</param>
    /// <param name="startThickness">If no value is provided, current outline thickness will be used.</param>
    private void ChangeOutlineCoroutine(float timer,
        Color targetColor, float targetThickness,
        Color startColor, float startThickness = -1) // Default start values is current value
    {
        StopChangeOutlineCoroutine();
        m_ChangeOutlineCoroutine = StartCoroutine(ChangeOutline(timer, targetColor, targetThickness, startColor, startThickness));
    }

    private void StopChangeOutlineCoroutine()
    {
        Debug.Log("StopChangeOutlineCoroutine " + gameObject.name);
        if (m_ChangeOutlineCoroutine != null)
        {
            StopCoroutine(m_ChangeOutlineCoroutine);
            m_ChangeOutlineCoroutine = null;
        }
    }

    private IEnumerator ChangeOutline(float timer,
        Color targetColor, float targetThickness,
        Color startColor, float startThickness)
    {
        if (startThickness < 0) startThickness = m_currOutlineThickness;
        bool blendColor = (startColor != Color.magenta && startColor != targetColor);
        if (!blendColor)
        {
            SetOutlineColor(targetColor);
        }

        float time = 0;
        while (time < timer)
        {
            float lerpAmount = time / timer;
            SetOutlineThickness(Mathf.Lerp(startThickness, targetThickness, lerpAmount));
            if (blendColor)
            {
                SetOutlineColor(Color.Lerp(startColor, targetColor, lerpAmount));
            }

            time += Time.deltaTime;
            yield return null;
        }

        // SetOutlineThickness(targetThickness);
        // SetOutlineColor(targetColor);
        // Debug.Log(m_currOutlineThickness);
    }

    private MeshRenderer CreateOutlineObject(GameObject obj)
    {
        GameObject outlineObj = new GameObject(obj.name + " (Outline)", new System.Type[] { typeof(MeshFilter), typeof(MeshRenderer) });
        outlineObj.transform.SetParent(obj.transform, false);

        outlineObj.GetComponent<MeshFilter>().mesh = obj.GetComponent<MeshFilter>().mesh;

        MeshRenderer rend = outlineObj.GetComponent<MeshRenderer>();
        Material[] outlines = new Material[obj.GetComponent<MeshRenderer>().materials.Length];
        for (int i = 0; i < outlines.Length; i++)
        {
            outlines[i] = m_outlineMat;
        }
        rend.materials = outlines;

        return rend;
    }
}