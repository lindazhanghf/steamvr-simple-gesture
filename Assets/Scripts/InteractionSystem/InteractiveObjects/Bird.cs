using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : InteractableObject {
    public GameObject BirdStand;
    public GameObject BirdFly;

    private Rigidbody rb;
    private Vector3 idlePosition;
    private Quaternion idleRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        idlePosition = transform.position;
        idleRotation = transform.rotation;
    }

    public override void Interaction()
    {
        base.Interaction();
    }

    void OnTriggerEnter(Collider c)
    {
        Target t = c.GetComponent<Target>();
        if (t)
        {
            t.EndInteraction();
        }
        EndInteraction();
    }

    public override void EndInteraction()
    {
        base.EndInteraction();
        rb.isKinematic = true;
        BirdStand.SetActive(true);
        BirdFly.SetActive(false);
        transform.position = idlePosition;
        transform.rotation = idleRotation;
    }

    public override void Activation()
    {
        base.Activation();
        BirdStand.SetActive(false);
        BirdFly.SetActive(true);
    }

    public override void StartHovering(InteractionActor newActor = null)
    {
        base.StartHovering();
    }

    public override void StopHovering()
    {
        base.StopHovering();
    }
}