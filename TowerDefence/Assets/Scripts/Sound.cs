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
    public float volume = .75f;

    [Range(-3f, 3)]
    public float pitch = 1;

    [HideInInspector]
    public AudioSource[] sources;

    public int instances = 1;
   
    public AnimationCurve pitchDistribution;

    [ContextMenu("Randomize Pitch")]
    public float RandomizePitch()
    {
        float r = Random.value;
        r = pitchDistribution.Evaluate(r);
        return pitch = r;
    }

    public AudioSource ReadySource()
    {
        foreach(AudioSource source in sources)
        {
            if (!source.isPlaying)
            {
                return source;

            }


        }
        return sources[0];
    }
}
