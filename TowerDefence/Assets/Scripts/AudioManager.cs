using UnityEngine.Audio;
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField]
    private Sound[] sounds;
    private Dictionary<string, Sound> dic;

    private int ticks;

    void Awake()
    {
        Instance = this;
        dic = new Dictionary<string, Sound>();
        foreach (Sound s in sounds)
        {
            s.sources = new AudioSource[s.instances];
            for (int i = 0; i < s.instances; i++)
            {
                s.sources[i] = gameObject.AddComponent<AudioSource>();
                s.sources[i].clip = s.clip;
                s.sources[i].volume = s.volume;
                s.sources[i].pitch = s.pitch;
                
            }

            dic.Add(s.name, s);
        }
    }

    public void Play(string name, float delay = 0)
    {
        if (dic.TryGetValue(name, out Sound sound))
        {
            sound.ReadySource().PlayDelayed(delay);
        };
    }

    public void PlayWithRandomPitch(string name, float delay = 0)
    {
        if (dic.TryGetValue(name, out Sound sound))
        {
            sound.RandomizePitch();
            AudioSource source = sound.ReadySource();
            source.pitch = sound.RandomizePitch();
            source.PlayDelayed(delay);
        };
    }


    public void Tick()
    {
        ticks++;
        
        if (ticks % 4 == 0)
        {
            Play("kick");
            Play("hat");


        }

        if (ticks % 4 == 1)
        {
            Play("kick");

        }

        if (ticks % 4 == 2)
        {
            Play("kick");
          


        }

        if (ticks % 4 == 3)
        {
            Play("kick");

        }




    }

    public void InterTick(float t)
    {

    }
    [ContextMenu("Load Sounds")]
    private void LoadSounds()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sounds");
        Debug.Log(clips);
        sounds = new Sound[clips.Length];

        for (int i = 0; i < clips.Length; i++)
        {
            sounds[i] = new Sound()
            {
                name = clips[i].name,
                clip = clips[i],
            };
        }
    }
    
}
