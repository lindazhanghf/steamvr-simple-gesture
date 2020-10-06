using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public State[] States;
    private int m_currID = 0;

    void Start()
    {
        for (int i = 0; i < States.Length; i++)
        {
            States[i].ID = i;
        }
    }

    public void ChangeState(int newState)
    {
        ChangeState(States[newState]);
    }

    public void ChangeState(State newState)
    {
        State prevState = States[m_currID];
        prevState.OnExit();
        newState.OnEnter(prevState);
        m_currID = newState.ID;
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
