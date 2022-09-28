using System;
using System.Collections.Generic;

public class State
{
    public List<FSMAction> behaviours;
    public Action onAbruptExit;
}

public class FSM
{
    #region PRIVATE_FIELDS
    private int currentState;
    private int[,] relations;
    private Dictionary<int, State> behaviours;
    #endregion

    public FSM(int states, int flags)
    {
        currentState = -1;

        relations = new int[states, flags];
        for (int i = 0; i < states; i++)
        {
            for (int j = 0; j < flags; j++)
            {
                relations[i, j] = -1;
            }
        }

        behaviours = new Dictionary<int, State>();
    }

    public void ForceCurrentState(int state)
    {
        currentState = state;
    }

    public void SetRelation(int sourceState, int flag, int destinationState)
    {
        relations[sourceState, flag] = destinationState;
    }

    public void SetFlag(int flag)
    {
        if (relations[currentState, flag] != -1)
        {
            currentState = relations[currentState, flag];
        }
    }

    public int GetCurrentState()
    {
        return currentState;
    }

    public void SetBehaviour(int state, FSMAction behaviour, Action onAbruptExit = null)
    {
        State newState = new State
        {
            behaviours = new List<FSMAction> { behaviour },
            onAbruptExit = onAbruptExit
        };
        List<FSMAction> newBehaviours = new List<FSMAction> { behaviour };

        if (behaviours.ContainsKey(state))
        {
            behaviours[state] = newState;
        }
        else
        {
            behaviours.Add(state, newState);
        }
    }

    public void AddBehaviour(int state, FSMAction behaviour, Action onAbruptExit = null)
    {
        if (behaviours.ContainsKey(state))
        {
            behaviours[state].behaviours.Add(behaviour);
        }
        else
        {
            State newState = new State
            {
                behaviours = new List<FSMAction> { behaviour },
                onAbruptExit = onAbruptExit
            };

            behaviours.Add(state, newState);
        }
    }

    public void Exit()
    {
        if (behaviours.ContainsKey(currentState))
        {
            List<FSMAction> actions = behaviours[currentState].behaviours;
            if (actions != null)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    if (actions[i] != null)
                    {
                        actions[i].AbruptExit();
                    }
                }
            }            
            
            Action onExit = behaviours[currentState].onAbruptExit;
            onExit?.Invoke();
        }
    }

    public void Update()
    {
        if (behaviours.ContainsKey(currentState))
        {
            List<FSMAction> actions = behaviours[currentState].behaviours;
            if (actions != null)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    if (actions[i] != null)
                    {
                        actions[i].Execute();
                    }
                }
            }
        }
    }
}