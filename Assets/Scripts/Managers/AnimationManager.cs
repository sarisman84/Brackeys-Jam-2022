using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [System.Serializable]
    public struct AnimationCommand
    {
        public string commandName;
        public string parameterName;
        public AnimationType commandValue;

    }

    public Animator animatorToUse;
    public List<AnimationCommand> commands;
    // Start is called before the first frame update
    public void ExecuteAnimationCommand(string commandName)
    {
        if (!animatorToUse)
        {
            Debug.LogError($"[Log]<AnimationManager/{gameObject.name}>: Cannot find an animator. Have you assign it?", this);
            return;
        }

        AnimationCommand foundCommand = commands.FirstOrDefault(c => c.commandName.Equals(commandName));

        if (foundCommand.commandValue == null)
        {
            Debug.LogError($"[Log]<AnimationManager/{gameObject.name}>: Could not find valid command [Searched command: {commandName}]", this);
            return;
        }



        switch (foundCommand.commandValue.animTypeDef)
        {
            case AnimationType.AnimTypeDef.Bool:
                animatorToUse.SetBool(foundCommand.parameterName, foundCommand.commandValue.boolValue);
                Debug.Log($"Command <{foundCommand.commandName}> assigned {foundCommand.parameterName} with a bool value of {foundCommand.commandValue.boolValue}");
                break;
            case AnimationType.AnimTypeDef.Float:
                animatorToUse.SetFloat(foundCommand.parameterName, foundCommand.commandValue.floatValue);
                Debug.Log($"Command <{foundCommand.commandName}> assigned {foundCommand.parameterName} with a float value of {foundCommand.commandValue.floatValue}");
                break;
            case AnimationType.AnimTypeDef.Int:
                animatorToUse.SetInteger(foundCommand.parameterName, foundCommand.commandValue.intValue);
                Debug.Log($"Command <{foundCommand.commandName}> assigned {foundCommand.parameterName} with a int value of {foundCommand.commandValue.intValue}");
                break;
            case AnimationType.AnimTypeDef.Trigger:
                animatorToUse.SetTrigger(foundCommand.parameterName);
                Debug.Log($"Command <{foundCommand.commandName}> triggered {foundCommand.parameterName}");
                break;
            default:
                Debug.LogWarning($"[Log]<AnimationManager/{gameObject.name}>: Command Type is not implemented!");
                break;
        }
    }
}
