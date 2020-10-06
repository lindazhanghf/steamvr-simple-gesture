using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureStateMachine : StateMachine
{
    enum StateID : int
    {
        Idle,
        Point,
        Activation,
        FinishActivation,
        FinishAction,
        Cancel
    }

    State[] states = new State[6];
    HandGestureActor m_actor;

    public GestureStateMachine(HandGestureActor handGestureActor)
    {
        m_actor = handGestureActor;

        // Initialize States
        states[(int)StateID.Idle] = new State_Idle(handGestureActor, "Idle");
        // states[(int)StateID.Point] = new State("Point");
        // states[(int)StateID.Activation] = new State("Activation");
        // states[(int)StateID.FinishActivation] = new State("FinishActivation");
        // states[(int)StateID.FinishAction] = new State("FinishAction");
        // states[(int)StateID.Cancel] = new State("Cancel");
    }
}

class GestureBaseState : State
{
    public HandGestureActor actor; 
    public HandTracker hand;
    public GestureBaseState(HandGestureActor handGestureActor, string name)
    {
        Name = name;
        handGestureActor = actor;
        hand = actor.TrackingHand;
    }
}

class State_Idle : GestureBaseState
{
    public State_Idle(HandGestureActor actor, string name) : base(actor, name) {}

    public override bool Execute()
    {
        if (hand.IndexFingerPoint)
            return true;

        return false;
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
        hand.IndexFingerTip.gameObject.SetActive(true);
    }

    public override int OnExit()
    {
        hand.IndexFingerTip.gameObject.SetActive(false);
        return 0;
    }

    public override bool Execute()
    {
        if (!hand.IndexFingerPoint)
        {
            Delay_ClearCurrentPointing();
            return false;
        }

        Collider hitObj = FindHitObject();
        if (hitObj)
        {
            InteractableObject interactableObj = hitObj.GetComponent<InteractableObject>();
            if (interactableObj == null) // The object hit is not an interactable object
            {
                Delay_ClearCurrentPointing();
                return true;
            }

            if (m_currentPointing && m_currentPointing == interactableObj) return true; // Pointing at the same object
            actor.ClearCurrentPointing();

            // Pointing at a new object
            actor.StartHovering(interactableObj);
            m_currentPointing = interactableObj.gameObject;
        }
        else
        {
            Delay_ClearCurrentPointing();
        }

        return true;
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