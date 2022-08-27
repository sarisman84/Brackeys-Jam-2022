using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New Audio Clip", menuName = "New Custom Audio Clip", order = 0)]
public class CustomAudioClip : ScriptableObject {

    public AudioMixerGroup channel;
    public AudioClip clip;
    public bool playOnAwake;
    public bool loop;
    public float initialVolume;
    public float initialPitch;
    public float clipStartTimestamp;
    public float clipEndTimestamp;


}
