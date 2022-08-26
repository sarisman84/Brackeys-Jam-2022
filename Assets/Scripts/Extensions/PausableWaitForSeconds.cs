using UnityEngine;

public class PausableWaitForSeconds : CustomYieldInstruction
{
    private float waitingTime;

    public PausableWaitForSeconds(float delay) {
        waitingTime = delay;
    }

    public override bool keepWaiting {//queried each frame after Update and before LateUpdate
        get {
            if(PollingStation.Instance.runtimeManager.currentState == RuntimeManager.RuntimeState.Playing)
                waitingTime -= Time.deltaTime;
            return waitingTime > 0;
        }
    }
}
