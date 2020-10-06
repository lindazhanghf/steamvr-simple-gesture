using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureStateMachine : StateMachine
{
    public enum StateID : int
    {
        Idle,
        Point,
        Activation,
        FinishActivation,
        FinishAction,
        Cancel
    }

    // State[] states = new State[6];
    // HandGestureActor m_actor;

    public GestureStateMachine(HandGestureActor handGestureActor)
    {
        // m_actor = handGestureActor;
        States = new State[6];

        // Initialize States
        States[(int)StateID.Idle] = new State_Idle(handGestureActor, "Idle");
        States[(int)StateID.Point] = new State_Point(handGestureActor, "Point");
        // States[(int)StateID.Activation] = new State("Activation");
        // States[(int)StateID.FinishActivation] = new State("FinishActivation");
        // States[(int)StateID.FinishAction] = new State("FinishAction");
        // States[(int)StateID.Cancel] = new State("Cancel");
    }
}

class GestureBaseState : State
{
    public HandGestureActor actor; 
    public HandTracker hand;
    public GestureBaseState(HandGestureActor handGestureActor, string name)
    {
        Name = name;
        actor = handGestureActor;
        hand = handGestureActor.TrackingHand;
    }
}

class State_Idle : GestureBaseState
{
    public State_Idle(HandGestureActor actor, string name) : base(actor, name) {}

    public override int Execute()
    {
        if (hand.IndexFingerPoint)
            return (int)GestureStateMachine.StateID.Point;

        return -1;
    }
}

class State_Point : GestureBaseState
{
    GameObject m_currentPointing;
    private Transform finger_index_end;
    private Transform finger_index_2;

    public State_Point(HandGestureActor actor, string name) : base(actor, name)
    {
        finger_index_end = actor.finger_index_end;
        finger_index_2 = finger_index_end.parent;
    }

    public override void OnEnter(State prevState)
    {
        base.OnEnter(prevState);
        hand.IndexFingerTip.gameObject.SetActive(true);
    }

    public override void OnExit()
    {
        base.OnExit();
        hand.IndexFingerTip.gameObject.SetActive(false);
    }

    public override int Execute()
    {
        if (!hand.IndexFingerPoint)
        {
            Delay_ClearCurrentPointing();
            return (int)GestureStateMachine.StateID.Idle;
        }

        Collider hitObj = FindHitObject();
        if (hitObj)
        {
            InteractableObject interactableObj = hitObj.GetComponent<InteractableObject>();
            if (interactableObj == null) // The object hit is not an interactable object
            {
                Delay_ClearCurrentPointing();
                return -1;
            }

            if (m_currentPointing && m_currentPointing == interactableObj) return -1; // Pointing at the same object
            actor.ClearCurrentPointing();

            // Pointing at a new object
            actor.StartHovering(interactableObj);
            m_currentPointing = interactableObj.gameObject;
        }
        else
        {
            Delay_ClearCurrentPointing();
        }

        return -1;
    }

    private void Delay_ClearCurrentPointing()
    {
        m_currentPointing = null;
        actor.Delay_ClearCurrentPointing();
    }

    private Collider FindHitObject()
    {
        RaycastHit raycastHit;
        if (Physics.Raycast(finger_index_2.position, finger_index_end.position - finger_index_2.position, out raycastHit, 400))
        {
            if (actor.DebugHit) actor.DebugHit.transform.position = raycastHit.point;
            return raycastHit.collider;
        }

        return null;
    }
}