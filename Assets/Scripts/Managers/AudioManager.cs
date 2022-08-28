using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class AudioManager : MonoBehaviour {


    public CustomAudioClip[] audioClips;

    struct Player {
        AudioSource audioSource;
        public void OnComplete(System.Action<AudioManager> onCompleteEvent, AudioManager manager)
        {

            manager.StartCoroutine(CheckState(audioSource, onCompleteEvent, manager));
        }


        IEnumerator CheckState(AudioSource source, System.Action<AudioManager> onCompleteEvent, AudioManager manager)
        {
            float dur = source.clip.length;
            for (; ; )
            {
                yield return null;
                if (source.time >= dur)
                {
                    Debug.Log($"[Log]<AudioManager/Player/{audioSource.clip.name}>: On Complete Event called for {audioSource.clip}.");
                    onCompleteEvent?.Invoke(manager);
                    break;
                }
            }
            yield return null;
        }
    }

    public Dictionary<CustomAudioClip, AudioSource> sources { get; private set; }


    private void Awake()
    {
        RegisterInspector();
    }

    private void RegisterInspector()
    {
        if (sources != null)
        {
            foreach (var item in sources)
            {
                Destroy(item.Value);
            }
            sources.Clear();
        }
        else
        {
            sources = new Dictionary<CustomAudioClip, AudioSource>();
        }


        for (int i = 0; i < audioClips.Length; i++)
        {
            CreatePlayer(audioClips[i]);
        }
    }

    public AudioSource GetSourceOfClip(string clipName)
    {
        if (sources.Any(x => x.Key.name.Contains(clipName)))
        {
            return sources.FirstOrDefault(x => x.Key.name.Contains(clipName)).Value;
        }

        return null;
    }

    private void Update()
    {

    }



    public AudioManager Play(string clipName, bool solo = false)
    {
        if (solo)
            StopAll();
        GetSourceOfClip(clipName)?.Play();

        return this;

    }


    public AudioManager Play(string clipName, Vector3 position, bool solo = false)
    {
        if (solo)
            StopAll();
        var source = GetSourceOfClip(clipName);

        if (!source) return this;

        source.transform.position = position;
        source.transform.localRotation = Quaternion.identity;

        source.Play();



        return this;
    }

    public void OnComplete(System.Action<AudioManager> onEventCallback)
    {

    }


    public void Stop(string clipName)
    {
        GetSourceOfClip(clipName)?.Stop();
    }


    void StopAll()
    {
        foreach (var item in sources)
        {
            item.Value.Stop();
        }
    }

    void CreatePlayer(CustomAudioClip clip)
    {

        GameObject go = new GameObject(clip.name);
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;


        AudioSource audioSource = go.AddComponent<AudioSource>();

        audioSource.outputAudioMixerGroup = clip.channel;
        audioSource.clip = clip.clip;
        audioSource.playOnAwake = clip.playOnAwake;
        audioSource.loop = clip.loop;
        audioSource.volume = clip.initialVolume;
        audioSource.pitch = clip.initialPitch;

        if (clip.clipStartTimestamp >= 0)
            audioSource.SetScheduledStartTime(clip.clipStartTimestamp);
        if (clip.clipEndTimestamp >= 0)
            audioSource.SetScheduledEndTime(clip.clipEndTimestamp);



        if (clip.playOnAwake)
            audioSource.Play();


        sources.Add(clip, audioSource);
    }
}
