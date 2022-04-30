using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    public static Audio instance;

    public GameObject soundPrefab;

    public List<AudioClip> clips_pushed = new List<AudioClip>();
    public List<AudioClip> clips_zap = new List<AudioClip>();
    public List<AudioClip> clips_dash = new List<AudioClip>();

    #region vocal
    public List<AudioClip> clips_vocal_jumping = new List<AudioClip>();
    public List<AudioClip> clips_vocal_dashing = new List<AudioClip>();
    //public List<AudioClip> clips_pushMiss = new List<AudioClip>(); // maybe not this one
    public List<AudioClip> clips_vocal_pushHit = new List<AudioClip>(); // The player that is pushed - Mario style falling sound, or the player that is pushing? - "hurr"
    public List<AudioClip> clips_vocal_zapped = new List<AudioClip>();
    public List<AudioClip> clips_vocal_winVocal = new List<AudioClip>();
    public List<AudioClip> clips_vocal_die = new List<AudioClip>();
    #endregion

    private void Awake()
    {
        instance = this;
    }

    //          EXAMPLE USE:
    // AudioSource as = Audio.instance.MakeSounce(new Vector3(0,0,0));
    // as.clip = jump;
    // as.Volume = 0.2f;
    // as.Play();
    // as.GetComponent<Sound>().StartDelete(clip.length);
    public AudioSource MakeSound(Vector3 pos)
    {
        GameObject audioSource = Instantiate(soundPrefab, pos, transform.rotation, transform);
        return audioSource.GetComponent<AudioSource>();
    }

    public AudioSource MakeSound(Vector3 pos, Transform _parent)
    {
        GameObject audioSource = Instantiate(soundPrefab, pos, transform.rotation, _parent);
        return audioSource.GetComponent<AudioSource>();
    }
}
