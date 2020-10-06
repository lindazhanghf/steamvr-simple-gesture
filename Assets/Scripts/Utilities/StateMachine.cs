using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public State[] States;
    private int m_currID = 0;

    public void Initialize(int startState = -1)
    {
        for (int i = 0; i < States.Length; i++)
        {
            States[i].ID = i;
        }

        if (startState > 0) ChangeState(startState);
    }

    public void ChangeState(int nextState)
    {
        if (nextState < 0 || nextState >= States.Length) return;

        ChangeState(States[nextState]);
    }

    public void ChangeState(State nextState)
    {
        State prevState = States[m_currID];
        prevState.OnExit();
        nextState.OnEnter(prevState);
        m_currID = nextState.ID;
    }

    public int Execute()
    {
        int nextState = States[m_currID].Execute();
        if (nextState >= 0)
        {
            ChangeState(nextState);
        } 
        return nextState;
    }
}

public class State
{
    public string Name;
    public int ID;

    public virtual void OnEnter(State prevState)
    {
        Debug.Log("State [" + Name + "] OnEnter :: prevState = " + prevState.Name);
    }

    /// <returns>ID of the next state; -1 if staying in current state</returns>
    public virtual int Execute()
    {
        return -1;
    }

    public virtual void OnExit()
    {
        Debug.Log("State [" + Name + "] OnExit");
    }
}
