using UnityEngine.Audio;
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    public static AudioManager Instance => instance;

    [SerializeField]
    private Sound[] sounds;
    private Dictionary<string, Sound> dic;

    void Awake()
    {
        instance = this;
        dic = new Dictionary<string, Sound>();
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;

            dic.Add(s.name, s);
        }

        
    }

    public void Play(string name)
    {
        if (dic.TryGetValue(name, out Sound sound))
        {
            sound.source.Play();
        };
    }
}
