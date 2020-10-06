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

    public bool Execute()
    {
        return States[m_currID].Execute();
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

    public virtual bool Execute()
    {
        return true;
    }

    public virtual int OnExit()
    {
        Debug.Log("State [" + Name + "] OnExit");
        return 0;
    }
}
