using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureStateMachine : StateMachine
{
    public enum StateID : int
    {
        Idle = 0,
        Point = 1,
        Activation = 2,
        FinishActivation = 3,
        FinishAction = 4,
        Cancel = 5
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
        States[(int)StateID.Activation] = new State_Activation(handGestureActor, "Activation");
        States[(int)StateID.FinishActivation] = new State();
        States[(int)StateID.FinishAction] = new State();
        States[(int)StateID.Cancel] = new State_Cancel(handGestureActor, "Cancel");

        Initialize((int)StateID.Idle);
    }
}

class GestureBaseState : State
{
    public float GestureTransitionBuffer_s = 0.25f;
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

class State_Activation : GestureBaseState
{
    public State_Activation(HandGestureActor actor, string name) : base(actor, name) {}

    public override int Execute()
    {
        if (!hand.PalmOpen)
        {
            return (int)GestureStateMachine.StateID.Cancel;
        }

        return -1;
    }
}

class State_Point : GestureBaseState
{
    InteractableObject m_currentPointing;
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
            return (int)GestureStateMachine.StateID.Cancel;
        }

        if (hand.PalmOpen && m_currentPointing && m_currentPointing.IsHovering)
        {
            // m_startActivation = true;
            hand.EnableTraceMatch = true;

            // if (m_clearCurrentPointingCoroutine != null) StopCoroutine(m_clearCurrentPointingCoroutine);
            // m_clearCurrentPointingCoroutine = null;
            return (int)GestureStateMachine.StateID.Activation;
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
            m_currentPointing = interactableObj;
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

class State_Cancel : GestureBaseState
{
    State PreviousState;
    private Coroutine m_CancelGestureCoroutine;
    private bool m_canceled;

    public State_Cancel(HandGestureActor actor, string name) : base(actor, name) {}

    public void CancelGesture()
    {
        actor.ClearCurrentPointing();
        Debug.Log("State_Cancel :: Buffer ended, Canceled");
        m_canceled = true;
    }

    public override void OnEnter(State prevState)
    {
        base.OnEnter(prevState);
        PreviousState = prevState;

        m_canceled = false;
        Debug.Log("State_Cancel :: Start CancelGestureBuffer " + m_canceled);
        m_CancelGestureCoroutine = actor.StartCoroutine(CancelGestureBuffer());
    }

    public override int Execute()
    {
        if (m_canceled)
        {
            return (int)GestureStateMachine.StateID.Idle;
        }

        // if within gesture transition time buffer, go to next state
        if (PreviousState.ID == (int)GestureStateMachine.StateID.Point)
        {
            if (hand.PalmOpen) return (int)GestureStateMachine.StateID.Activation;
        }
        if (PreviousState.ID == (int)GestureStateMachine.StateID.Activation)
        {
            if (hand.PalmOpen) return (int)GestureStateMachine.StateID.Activation;
        }

        return -1;
    }

    public override void OnExit()
    {
        base.OnExit();
        m_canceled = false;
        if (m_CancelGestureCoroutine != null) actor.StopCoroutine(m_CancelGestureCoroutine);
        m_CancelGestureCoroutine = null;
    }

    private IEnumerator CancelGestureBuffer()
    {
        yield return new WaitForSeconds(GestureTransitionBuffer_s);
        CancelGesture();
    }
}
