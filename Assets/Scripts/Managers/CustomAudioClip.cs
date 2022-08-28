using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New Audio Clip", menuName = "New Custom Audio Clip", order = 0)]
public class CustomAudioClip : ScriptableObject {

    public AudioMixerGroup channel;
    public AudioClip clip;
    public bool playOnAwake;
    public bool loop;
    public float initialVolume = 1.0f;
    public float initialPitch = 1.0f;
    public float clipStartTimestamp = -1.0f;
    public float clipEndTimestamp = -1.0f;


}
