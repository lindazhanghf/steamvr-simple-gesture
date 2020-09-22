using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineGlowBehavior : InteractableBehavior
{
    [Header("Outline")]
    public bool enableOutline = true;
    [SerializeField] private Color m_touchColor = Color.green;

    [Header("Glowing Effect")]
    [Tooltip("Make the outline to glow slowly to indicate interactivity")]
    [SerializeField] private bool m_enableGlow = true;
    [SerializeField] private Color m_glowColor = new Color(105 / 255f, 139 / 255f, 1f);

    [Header("Additional Models")]
    [SerializeField] private bool m_enableOutlineOnChildren = true;

    private Material m_outlineMat;
    private List<MeshRenderer> m_singleMaterialMesh;
    private List<MeshRenderer> m_multiMaterialMesh;
    private float m_outlineThickness;
    private bool m_touched;

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

        if (m_enableGlow)
        {
            EnableOutline();
            SetOutlineColor(m_glowColor);
        }
    }

    protected override void OnStartHovering()
    {
        base.OnStartHovering();
        if (!enableOutline) return;

        EnableOutline();
        SetOutlineColor(m_touchColor, 0.0025f);
        m_touched = true;
    }

    protected override void OnStopHovering()
    {
        base.OnStopHovering();

        DisableOutline();
    }

    void Update()
    {
        if (interactableObject && interactableObject.IsHovering) return;

        if (m_enableGlow)
        {
            m_outlineThickness = 0.001f + (Mathf.Sin(Time.time * 3) * 0.001f); // Ping pong between 0.001 and 0.002
            SetOutlineThickness(m_outlineThickness);
        }
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