using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : InteractableObject {

    public override void Interaction()
    {
        base.Interaction();
    }

    public override void EndInteraction()
    {
        base.EndInteraction();
        gameObject.SetActive(false);
    }

    public override void Activation()
    {
        base.Activation();
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