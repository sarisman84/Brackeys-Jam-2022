using UnityEngine;

public class PlayerInitPos : MonoBehaviour
{
    void Start() 
    {
        CharacterController cr = PollingStation.Instance.player.GetComponent<CharacterController>();
        cr.enabled = false;
        cr.transform.position = transform.position;
        cr.enabled = true;
    }
}
