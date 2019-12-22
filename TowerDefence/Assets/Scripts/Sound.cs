using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;

    public int bpm;

    public float loopStart;
    public float loopEnd;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;

    [Range(.1f, 3)]
    public float pitch;

    [HideInInspector]
    public AudioSource source;

}
