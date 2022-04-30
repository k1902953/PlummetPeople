using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventReciever : MonoBehaviour
{
    public GameObject playerRoot;
    private PlayerPush playerPush;

    private void Awake()
    {
        playerPush = playerRoot.GetComponent<PlayerPush>();
    }

    void Event_StartPush()
    {
        // call push script: start raycast
        playerPush.SetDoPushRaycast(true);
    }

    void Event_EndPush()
    {
        // call push script: end raycast
        playerPush.SetDoPushRaycast(false);
    }

    void Event_Dash()
    {
        AudioSource audio = Audio.instance.MakeSound(transform.position);
        audio.clip = Audio.instance.clips_dash[Random.Range(0, Audio.instance.clips_dash.Count)];
        audio.volume = 0.15f;
        audio.spatialBlend = 1f;
        audio.Play();
        audio.GetComponent<Sound>().StartDelete(audio.clip.length);
    }

    void Event_DashVocal()
    {
        AudioSource audio2 = Audio.instance.MakeSound(transform.position, transform);
        audio2.clip = Audio.instance.clips_vocal_dashing[Random.Range(0, Audio.instance.clips_vocal_dashing.Count)];
        audio2.volume = 0.4f;
        audio2.spatialBlend = 1f;
        audio2.Play();
        audio2.GetComponent<Sound>().StartDelete(audio2.clip.length);
    }

    void Event_Jump()
    {
        AudioSource audio = Audio.instance.MakeSound(transform.position, transform);
        audio.clip = Audio.instance.clips_vocal_jumping[Random.Range(0, Audio.instance.clips_vocal_jumping.Count)];
        audio.volume = 0.4f;
        audio.spatialBlend = 1f;
        audio.Play();
        audio.GetComponent<Sound>().StartDelete(audio.clip.length);
    }
}
