using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationManager : MonoBehaviour {
    [System.Serializable]
    public struct AnimationCommand {
        public string commandName;
        public string parameterName;
        public AnimationType commandValue;

    }





    public Animator animatorToUse;
    public List<AnimationCommand> commands;


    [Header("Debug")]
    public bool showDebug;




    public void ExecuteAnimCommand(string commandName, AnimationType.AnimTypeDef type, object value)
    {
        if (!animatorToUse)
        {
            if (showDebug)
                Debug.LogError($"[Log]<AnimationManager/{gameObject.name}>: Cannot find an animator. Have you assign it?", this);
            return;
        }

        AnimationCommand foundCommand = commands.FirstOrDefault(c => c.commandName.Equals(commandName));

        if (foundCommand.commandValue == null)
        {
            if (showDebug)
                Debug.LogError($"[Log]<AnimationManager/{gameObject.name}>: Could not find valid command [Searched command: {commandName}]", this);
            return;
        }

        switch (foundCommand.commandValue.animTypeDef)
        {
            case AnimationType.AnimTypeDef.Bool:
                animatorToUse.SetBool(foundCommand.parameterName, (bool)value);
                if (showDebug)
                    Debug.Log($"Command <{foundCommand.commandName}> assigned {foundCommand.parameterName} with a bool value of {foundCommand.commandValue.boolValue}");
                break;
            case AnimationType.AnimTypeDef.Float:
                animatorToUse.SetFloat(foundCommand.parameterName, (float)value);
                if (showDebug)
                    Debug.Log($"Command <{foundCommand.commandName}> assigned {foundCommand.parameterName} with a float value of {foundCommand.commandValue.floatValue}");
                break;
            case AnimationType.AnimTypeDef.Int:
                animatorToUse.SetInteger(foundCommand.parameterName, (int)value);
                if (showDebug)
                    Debug.Log($"Command <{foundCommand.commandName}> assigned {foundCommand.parameterName} with a int value of {foundCommand.commandValue.intValue}");
                break;
            case AnimationType.AnimTypeDef.Trigger:
                animatorToUse.SetTrigger(foundCommand.parameterName);
                if (showDebug)
                    Debug.Log($"Command <{foundCommand.commandName}> triggered {foundCommand.parameterName}");
                break;
            default:
                if (showDebug)
                    Debug.LogWarning($"[Log]<AnimationManager/{gameObject.name}>: Command Type is not implemented!");
                break;
        }

    }


    // Start is called before the first frame update
    public void ExecuteAnimCommand(string commandName)
    {
        if (!animatorToUse)
        {
            if (showDebug)
                Debug.LogError($"[Log]<AnimationManager/{gameObject.name}>: Cannot find an animator. Have you assign it?", this);
            return;
        }

        AnimationCommand foundCommand = commands.FirstOrDefault(c => c.commandName.Equals(commandName));

        if (foundCommand.commandValue == null)
        {
            if (showDebug)
                Debug.LogError($"[Log]<AnimationManager/{gameObject.name}>: Could not find valid command [Searched command: {commandName}]", this);
            return;
        }



        switch (foundCommand.commandValue.animTypeDef)
        {
            case AnimationType.AnimTypeDef.Bool:
                animatorToUse.SetBool(foundCommand.parameterName, foundCommand.commandValue.boolValue);
                if (showDebug)
                    Debug.Log($"Command <{foundCommand.commandName}> assigned {foundCommand.parameterName} with a bool value of {foundCommand.commandValue.boolValue}");
                break;
            case AnimationType.AnimTypeDef.Float:
                animatorToUse.SetFloat(foundCommand.parameterName, foundCommand.commandValue.floatValue);
                if (showDebug)
                    Debug.Log($"Command <{foundCommand.commandName}> assigned {foundCommand.parameterName} with a float value of {foundCommand.commandValue.floatValue}");
                break;
            case AnimationType.AnimTypeDef.Int:
                animatorToUse.SetInteger(foundCommand.parameterName, foundCommand.commandValue.intValue);
                if (showDebug)
                    Debug.Log($"Command <{foundCommand.commandName}> assigned {foundCommand.parameterName} with a int value of {foundCommand.commandValue.intValue}");
                break;
            case AnimationType.AnimTypeDef.Trigger:
                animatorToUse.SetTrigger(foundCommand.parameterName);
                if (showDebug)
                    Debug.Log($"Command <{foundCommand.commandName}> triggered {foundCommand.parameterName}");
                break;
            default:
                if (showDebug)
                    Debug.LogWarning($"[Log]<AnimationManager/{gameObject.name}>: Command Type is not implemented!");
                break;
        }
    }
}
