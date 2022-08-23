using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public struct Command<TEnum> where TEnum : Enum
{
    public Command(List<Action<StateMachine<TEnum>.State>> requiredState, Action<StateMachine<TEnum>.State> nextState)
    {
        m_requiredState = requiredState;
        m_nextState = nextState;
    }

    List<Action<StateMachine<TEnum>.State>> m_requiredState;
    Action<StateMachine<TEnum>.State> m_nextState;


    public bool TryExecuteCommand(Action<StateMachine<TEnum>.State> targetState, out Action<StateMachine<TEnum>.State> aNewState)
    {
        if (m_requiredState.Equals(targetState))
        {
            aNewState = m_nextState;

            return true;
        }

        aNewState = null;
        return false;
    }

}

public class StateMachine<TEnum> where TEnum : Enum
{
    public enum State
    {
        Exiting, Running, Entering
    }

    private Dictionary<TEnum, Command<TEnum>> currentCommands;


    private Action<State> currentState;


    public void AddCommand(TEnum commandKey, Command<TEnum> aCommand)
    {
        if (currentCommands.ContainsKey(commandKey)) return;

        currentCommands.Add(commandKey, aCommand);
    }
    public void UpdateCurrentState()
    {
        currentState?.Invoke(State.Running);
    }

    public void ExecuteCommand(TEnum aNewCommand)
    {

        if (!currentCommands.ContainsKey(aNewCommand)) return;

        Command<TEnum> command = currentCommands[aNewCommand];
        if (command.TryExecuteCommand(currentState, out var newState))
        {
            currentState.Invoke(State.Exiting);
            currentState = newState;
            currentState.Invoke(State.Entering);
        }
    }


}



