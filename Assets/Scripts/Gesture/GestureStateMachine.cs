using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureStateMachine : StateMachine
{
    public const int STATE_Idle = 0;
    public const int STATE_Point = 1;
    public const int STATE_Activation = 2;
    public const int STATE_FinishActivation = 3;
    public const int STATE_ThrowingAction = 4;
    public const int STATE_Buffer = 5;

    public GestureStateMachine(HandGestureActor handGestureActor)
    {
        States = new State[6];

        // Initialize States
        States[STATE_Idle] = new State_Idle(handGestureActor, "Idle");
        States[STATE_Point] = new State_Point(handGestureActor, "Point");
        States[STATE_Activation] = new State_Activation(handGestureActor, "Activation");
        States[STATE_FinishActivation] = new State_FinishActivation(handGestureActor, "FinishActivation");
        States[STATE_ThrowingAction] = new State_ThrowingAction(handGestureActor, "ThrowingAction");
        States[STATE_Buffer] = new State_Buffer(handGestureActor, "Buffer");

        Initialize(STATE_Idle);
    }
}

class GestureBaseState : State
{
    protected const int FullCircleAngle = 360;
    protected const float GestureTransitionBuffer_s = 0.25f;
    protected HandGestureActor actor; 
    protected HandTracker hand;
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

    public override void OnEnter(State prevState)
    {
        base.OnEnter(prevState);
        hand.EnableTraceMatch = false;
    }

    public override int Execute()
    {
        if (hand.IndexFingerPoint)
            return GestureStateMachine.STATE_Point;

        return -1;
    }
}

class State_Point : GestureBaseState
{
    InteractableObject m_currentPointing;
    private Transform finger_index_end;
    private Transform finger_index_2;
    private Coroutine m_clearCurrentPointingCoroutine;

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
            // Delay_ClearHovering();
            return GestureStateMachine.STATE_Buffer;
        }

        Collider hitObj = FindHitObject();
        if (hitObj)
        {
            InteractableObject interactableObj = hitObj.GetComponent<InteractableObject>();
            if (interactableObj == null) // The object hit is not an interactable object
            {
                Delay_ClearHovering();
                return -1;
            }

            if (m_currentPointing && m_currentPointing == interactableObj) return -1; // Pointing at the same object
            actor.StopHovering();

            // Pointing at a new object
            actor.StartHovering(interactableObj);
            m_currentPointing = interactableObj;
        }
        else
        {
            Delay_ClearHovering();
        }

        return -1;
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

    private void Delay_ClearHovering()
    {
        if (m_currentPointing == null) return;
        if (m_clearCurrentPointingCoroutine != null) return; // already running

        Debug.Log("HandGestureActor :: Delay_ClearHovering start");
        m_clearCurrentPointingCoroutine = actor.StartCoroutine(ClearHoveringCoroutine());
        m_currentPointing = null;
    }

    private IEnumerator ClearHoveringCoroutine()
    {
        yield return new WaitForSeconds(GestureTransitionBuffer_s);
        actor.StopHovering();
        m_clearCurrentPointingCoroutine = null;
    }
}

class State_Activation : GestureBaseState
{
    public State_Activation(HandGestureActor actor, string name) : base(actor, name) {}

    public override void OnEnter(State prevState)
    {
        base.OnEnter(prevState);
        hand.EnableTraceMatch = true;
        hand.CurveDetection = HandTracker.CurveType.Circle;
    }

    public override int Execute()
    {
        if (!hand.PalmOpen)
        {
            return GestureStateMachine.STATE_Buffer;
        }

        if (hand.ContinousCurveAngle > FullCircleAngle)
        {
            Debug.LogWarning("State_Activation :: activated, ContinousCurveAngle = " + hand.ContinousCurveAngle);
            return GestureStateMachine.STATE_FinishActivation;
        }

        return -1;
    }
}

class State_FinishActivation : GestureBaseState
{
    float lastActivatedAngle;
    public State_FinishActivation(HandGestureActor actor, string name) : base(actor, name) {}

    public override void OnEnter(State prevState)
    {
        base.OnEnter(prevState);
        
        if (prevState.ID == GestureStateMachine.STATE_Activation)
        {
            actor.Activate();
            lastActivatedAngle = hand.ContinousCurveAngle;
        }
    }

    public override int Execute()
    {
        if (hand.ContinousCurveAngle > lastActivatedAngle + FullCircleAngle)
        {
            Debug.LogWarning("State_Activation :: activated AGAIN, ContinousCurveAngle = " + hand.ContinousCurveAngle);
            actor.Activate(); // keep activating
            lastActivatedAngle = hand.ContinousCurveAngle;
        }
        else if (hand.ContinousCurveAngle == 0)
        {
            lastActivatedAngle = 0;
        }

        if (hand.IndexFingerPoint) // user might change target
        {
            // return GestureStateMachine.STATE_Buffer; // TODO:
        }

        if (hand.Fist)
        {
            return GestureStateMachine.STATE_ThrowingAction;
        }

        return -1;
    }
}

class State_ThrowingAction : GestureBaseState
{
    private Vector3 m_throwingStartPos;

    public State_ThrowingAction(HandGestureActor actor, string name) : base(actor, name) {}

    public override void OnEnter(State prevState)
    {
        base.OnEnter(prevState);
        hand.CurveDetection = HandTracker.CurveType.NonCircle;
        m_throwingStartPos = hand.transform.position;
    }

    public override void OnExit()
    {
        base.OnExit();
        hand.CurveDetection = HandTracker.CurveType.Circle;
    }

    public override int Execute()
    {
        if (!hand.Fist)
        {
            // TODO: If hand released without throwing, go back to activation state
            // if (hand.ContinousCurveAngle == 0) 
            //     return GestureStateMachine.STATE_FinishActivation;

            Vector3 throwDirection = hand.transform.position - m_throwingStartPos;
            actor.ThrowAction(throwDirection);
            return GestureStateMachine.STATE_Idle;
        }

        return -1;
    }
}

class State_Buffer : GestureBaseState
{
    State PreviousState;
    private Coroutine m_BufferGestureCoroutine;
    private bool m_canceled;

    public State_Buffer(HandGestureActor actor, string name) : base(actor, name) {}

    public void CancelGesture()
    {
        Debug.Log("State_Buffer :: Buffer ended, gesture canceled");
        m_canceled = true;

        actor.StopHovering();
        hand.EnableTraceMatch = false;
    }

    public override void OnEnter(State prevState)
    {
        base.OnEnter(prevState);
        PreviousState = prevState;

        m_canceled = false;
        Debug.Log("State_Buffer :: Start GestureTransitionBuffer coroutine");
        m_BufferGestureCoroutine = actor.StartCoroutine(GestureTransitionBuffer());
    }

    public override int Execute()
    {
        if (m_canceled)
        {
            return GestureStateMachine.STATE_Idle;
        }

        // if within gesture transition time buffer, go to next state
        switch (PreviousState.ID)
        {
            case GestureStateMachine.STATE_Point:
                if (hand.PalmOpen) return GestureStateMachine.STATE_Activation;
                break;
            case GestureStateMachine.STATE_Activation:
                if (hand.PalmOpen) return GestureStateMachine.STATE_Activation;
                break;
            default:
                break;
        }

        return -1;
    }

    public override void OnExit()
    {
        base.OnExit();
        m_canceled = false;
        if (m_BufferGestureCoroutine != null) actor.StopCoroutine(m_BufferGestureCoroutine);
        m_BufferGestureCoroutine = null;
    }

    private IEnumerator GestureTransitionBuffer()
    {
        yield return new WaitForSeconds(GestureTransitionBuffer_s);
        CancelGesture();
    }
}
