using UnityEngine;

public class StateMachine
{
    public State[] States;
    private State m_currState;

    public void Initialize(int startState = -1)
    {
        for (int i = 0; i < States.Length; i++)
        {
            States[i].ID = i;
        }

        if (startState >= 0) ChangeStateByID(startState);
    }

    public void ChangeStateByID(int nextState)
    {
        if (nextState < 0 || nextState >= States.Length) return;

        ChangeState(States[nextState]);
    }

    public void ChangeState(State nextState)
    {
        if (m_currState != null) m_currState.OnExit();
        nextState.OnEnter(m_currState);
        
        m_currState = nextState;
    }

    public int Execute()
    {
        if (m_currState == null) return -1;

        int nextState = m_currState.Execute();
        if (nextState >= 0)
        {
            ChangeStateByID(nextState);
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
        if (prevState == null)
            Debug.Log("State [" + Name + "] OnEnter");
        else
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
